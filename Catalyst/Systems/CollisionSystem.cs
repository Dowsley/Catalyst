using System;
using System.Collections.Generic;
using System.Linq;
using Catalyst.Core;
using Catalyst.Entities;
using Catalyst.Globals;
using Microsoft.Xna.Framework;

namespace Catalyst.Systems;

/// <summary>
/// Implements Axis-Aligned Bounding Box (AABB) collision detection and resolution.
/// Handles entity movement and sliding against solid tiles.
/// </summary>
/// <remarks>
/// <p>
/// Key strategies to prevent common issues like player being inside wall, which leads to "wall climbing":
/// </p>
/// <p>
/// 1. Accurate <c>IsOnFloor</c> check: Determines if an entity is on the ground by checking 
///    a thin line just below its bottom edge across its width.
/// </p>
/// <p>
/// 2. <c>CollisionResolutionEpsilon</c>: When a collision is resolved (vertical or horizontal),
///    the entity is positioned a tiny distance away from the collision surface. This prevents
///    floating-point inaccuracies from causing micro-overlaps that could lead to the entity
///    being pushed up or down along walls.
/// </p>
/// <p>
/// Known limitations: If entity is moving two fast, it CAN go through walls. Will eventually be fixed when needed.
/// </p>
/// </remarks>
public class CollisionSystem(World worldRef, bool debug=false)
{
    private const float CollisionResolutionEpsilon = 0.01f;
    private const float CheckOffsetY = 1.0f; // How many pixels below the entity's bottom to check
    private const float GroundClearancePixels = 2.0f; 
    private const float MinimumEffectiveMovementThreshold = 0.0001f; // Movements smaller than this are considered negligible

    public bool IsOnFloor(Entity entity)
    {
        float entityBottomY = entity.CollisionShape.Bottom;
        float checkY = entityBottomY + CheckOffsetY;

        float entityLeftX = entity.CollisionShape.Left;
        float entityRightX = entity.CollisionShape.Right;

        Point startGrid = worldRef.WorldToGrid(new Vector2(entityLeftX, checkY));
        Point endGrid = worldRef.WorldToGrid(new Vector2(entityRightX, checkY));

        int gridCheckY = startGrid.Y; 

        for (int x = startGrid.X; x <= endGrid.X; x++)
        {
            if (worldRef.IsPositionSolid(x, gridCheckY))
                return true;
        }
        return false;
    }

    public void MoveAndSlide(Entity entity)
    {
        float intendedHorizontalMovementThisFrame = entity.Velocity.X; // Store X velocity before move
        float intendedVerticalMovementThisFrame = entity.Velocity.Y;

        var possibleMoveOffset = ComputePossibleMove(entity); 
        
        entity.Position += possibleMoveOffset;

        bool onFloor = IsOnFloor(entity);
        bool hitCeiling = intendedVerticalMovementThisFrame < 0 && possibleMoveOffset.Y > intendedVerticalMovementThisFrame + CollisionResolutionEpsilon / 2;
        if (onFloor || hitCeiling)
        {
            entity.Velocity.Y = 0; 
        }
        
        // Check if horizontal movement was impeded significantly
        bool hitWall = (intendedHorizontalMovementThisFrame > 0 && possibleMoveOffset.X < intendedHorizontalMovementThisFrame - CollisionResolutionEpsilon / 2) ||
                       (intendedHorizontalMovementThisFrame < 0 && possibleMoveOffset.X > intendedHorizontalMovementThisFrame + CollisionResolutionEpsilon / 2);
        switch (hitWall)
        {
            case true: // Clamp horizontal velocity if it's very small (and a wall wasn't just hit)
            case false when MathF.Abs(entity.Velocity.X) < Settings.MinimumVelocity:
                entity.Velocity.X = 0; // If wall hit, zero out X velocity
                break;
        }
    }
    
    /// <summary>
    /// Computes the possible movement offset for the given entity based on its velocity.
    /// </summary>
    /// <remarks>
    /// Expects <paramref name="entity"/>.Velocity to be expressed in units per frame.
    /// This method evaluates potential collisions and adjusts the movement accordingly.
    /// </remarks>
    /// <param name="entity">The entity whose movement is being evaluated.</param>
    /// <returns>The adjusted movement offset accounting for possible collisions.</returns>
    private Vector2 ComputePossibleMove(Entity entity)
    {
        var moveOffset = new Vector2(entity.Velocity.X, entity.Velocity.Y);
        
        // First handle vertical movement only
        var verticalMove = HandleVerticalMovement(entity, moveOffset.Y);
        // Then handle horizontal movement with the vertical position already adjusted
        var tempPos = entity.Position;
        entity.Position = tempPos + new Vector2(0, verticalMove);
        var horizontalMove = HandleHorizontalMovement(entity, moveOffset.X);
        entity.Position = tempPos; // Restore original position
        
        return new Vector2(horizontalMove, verticalMove);
    }
    
    private float HandleVerticalMovement(Entity entity, float moveY)
    {
        if (MathF.Abs(moveY) < MinimumEffectiveMovementThreshold)
            return 0;
            
        var colShapeFinalVertical = entity.CollisionShape;
        colShapeFinalVertical.Position.Y += moveY;
        
        var topLeftWorld = colShapeFinalVertical.TopLeft;
        var topRightWorld = colShapeFinalVertical.TopRight;
        var bottomLeftWorld = colShapeFinalVertical.BottomLeft;
        var bottomRightWorld = colShapeFinalVertical.BottomRight;
        
        var topLeftGrid = worldRef.WorldToGrid(topLeftWorld);
        var topRightGrid = worldRef.WorldToGrid(topRightWorld);
        var bottomLeftGrid = worldRef.WorldToGrid(bottomLeftWorld);
        // var bottomRightGrid = worldRef.WorldToGrid(bottomRightWorld);
        
        // Check vertical movement
        var verticalTiles = new List<Point>();
        var leftGrid = topLeftGrid.X;
        var rightGrid = topRightGrid.X;
        var topGrid = topLeftGrid.Y;
        var bottomGrid = bottomLeftGrid.Y;
        
        for (int x = leftGrid; x <= rightGrid; x++)
        {
            for (int y = topGrid; y <= bottomGrid; y++)
            {
                var tilePos = new Point(x, y);
                if (worldRef.IsPositionSolid(x, y))
                {
                    verticalTiles.Add(tilePos);
                    if (debug)
                        worldRef.DebugCollidedTiles.Enqueue(tilePos);
                }
                else
                {
                    if (debug)
                        worldRef.DebugCheckedTiles.Enqueue(tilePos);
                }
            }
        }
        
        if (verticalTiles.Count == 0)
            return moveY;
            
        // Resolve vertical collision
        if (moveY > 0) // moving down - should snap to top of topmost block
        {
            var topMost = verticalTiles.OrderBy(p => p.Y).First();
            var worldOriginOfTopMostTile = worldRef.GridToWorld(topMost);
            float penetration = colShapeFinalVertical.Bottom - worldOriginOfTopMostTile.Y;
            float allowedMove = moveY - Math.Max(0, penetration); 
            return Math.Max(0, allowedMove - CollisionResolutionEpsilon);
        }
        else // moving up - should snap to bottom of bottommost block
        {
            var bottomMost = verticalTiles.OrderByDescending(p => p.Y).First();
            var worldOriginOfBottomMostTile = worldRef.GridToWorld(bottomMost);
            var bottomOfTile = worldOriginOfBottomMostTile.Y + Settings.TileSize;
            float penetration = bottomOfTile - colShapeFinalVertical.Top;
            float allowedMove = moveY + Math.Max(0, penetration); 
            return Math.Min(0, allowedMove + CollisionResolutionEpsilon);
        }
    }
    
    private float HandleHorizontalMovement(Entity entity, float moveX)
    {
        if (MathF.Abs(moveX) < MinimumEffectiveMovementThreshold)
            return 0;

        // Create a temporary collision shape representing the entity's position after the intended horizontal move.
        // The entity's Y position is already adjusted from HandleVerticalMovement.
        var tempShape = new CollisionShape(
            entity.CollisionShape.Position + new Vector2(moveX, 0),
            entity.CollisionShape.Size
        );
        
        // Determine the world Y-coordinate scan range for horizontal collisions.
        float scanTopWorldY = tempShape.Top;
        float scanBottomWorldY = tempShape.Bottom - GroundClearancePixels;

        // If clearance makes the scan height negative (entity is shorter than clearance),
        // then perform the check without clearance to avoid missing collisions.
        if (scanBottomWorldY < scanTopWorldY)
        {
            scanBottomWorldY = tempShape.Bottom;
        }

        // Convert the X-extents of the tempShape to grid coordinates.
        var tempShapeTopLeftGrid = worldRef.WorldToGrid(tempShape.TopLeft);
        var tempShapeTopRightGrid = worldRef.WorldToGrid(tempShape.TopRight);
        int xLoopStartGrid = tempShapeTopLeftGrid.X;
        int xLoopEndGrid = tempShapeTopRightGrid.X;

        // Convert the world Y scan range to grid Y scan range for the loop.
        // Use any valid X within the shape for this conversion (e.g., tempShape.Left).
        int yLoopStartGrid = worldRef.WorldToGrid(new Vector2(tempShape.Left, scanTopWorldY)).Y;
        int yLoopEndGrid = worldRef.WorldToGrid(new Vector2(tempShape.Left, scanBottomWorldY)).Y;

        // Ensure yLoopEndGrid is not less than yLoopStartGrid after conversion,
        // which could happen with very small scan ranges or specific WorldToGrid behaviors.
        if (yLoopEndGrid < yLoopStartGrid) 
        {
            yLoopEndGrid = yLoopStartGrid; // At least scan one row of grid cells.
        }
        
        var horizontalTiles = new List<Point>();
        for (int x = xLoopStartGrid; x <= xLoopEndGrid; x++)
        {
            for (int y = yLoopStartGrid; y <= yLoopEndGrid; y++)
            {
                var tilePos = new Point(x, y);
                if (worldRef.IsPositionSolid(x, y))
                {
                    horizontalTiles.Add(tilePos);
                    if (debug)
                        worldRef.DebugCollidedTiles.Enqueue(tilePos);
                }
                else
                {
                    if (debug)
                        worldRef.DebugCheckedTiles.Enqueue(tilePos);
                }
            }
        }
        
        if (horizontalTiles.Count == 0)
            return moveX;
            
        // Resolve horizontal collision
        if (moveX > 0) // moving right - should snap to left of leftmost block
        {
            var leftMost = horizontalTiles.OrderBy(p => p.X).First();
            var worldOriginOfLeftMostTile = worldRef.GridToWorld(leftMost);
            float penetration = tempShape.Right - worldOriginOfLeftMostTile.X;
            float allowedMove = moveX - Math.Max(0, penetration);
            return Math.Max(0, allowedMove - CollisionResolutionEpsilon);
        }
        else // moving left - should snap to right of rightmost block
        {
            var rightMost = horizontalTiles.OrderByDescending(p => p.X).First();
            var worldOriginOfRightMostTile = worldRef.GridToWorld(rightMost);
            var rightOfTile = worldOriginOfRightMostTile.X + Settings.TileSize;
            float penetration = rightOfTile - tempShape.Left;
            float allowedMove = moveX + Math.Max(0, penetration);
            return Math.Min(0, allowedMove + CollisionResolutionEpsilon);
        }
    }
}