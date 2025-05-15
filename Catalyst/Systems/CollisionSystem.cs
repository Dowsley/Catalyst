using System;
using System.Collections.Generic;
using System.Linq;
using Catalyst.Core;
using Catalyst.Core.Extensions;
using Catalyst.Entities;
using Catalyst.Globals;
using Microsoft.Xna.Framework;

namespace Catalyst.Systems;

public class CollisionSystem(World worldRef)
{
    private readonly World _worldRef = worldRef;

    public bool IsOnFloor(Entity entity)
    {
        var feetPos = worldRef.WorldToGrid(entity.Position + new Vector2(0, entity.CollisionShape.Size.Y));
        bool isGrounded = worldRef.IsPositionSolid(feetPos.X, feetPos.Y);
        return isGrounded;
    }

    public void MoveAndSlide(Entity entity)
    {
        var possibleMoveOffset = ComputePossibleMove(entity);
        
        entity.Position += possibleMoveOffset;
        if (IsOnFloor(entity))
        {
            entity.Velocity.X *= Settings.GroundFriction;
            entity.Velocity.Y = 0;
        }
        else
        {
            entity.Velocity.X *= Settings.AirResistance;
        }
        
        // Clamp horizontal velocity
        if (MathF.Abs(entity.Velocity.X) < Settings.MinimumVelocity)
        {
            entity.Velocity.X = 0;
        }
    }
    
    /* Gets possible movement given the intended velocity from the entity */
    public Vector2 ComputePossibleMove(Entity entity)
    {
        var moveOffset = new Vector2(entity.Velocity.X, entity.Velocity.Y);
        
        var colShapeFinalVertical = entity.CollisionShape;
        colShapeFinalVertical.Position.Y += moveOffset.Y;

        var topLeftWorld = colShapeFinalVertical.TopLeft;
        var topRightWorld = colShapeFinalVertical.TopRight;
        var bottomLeftWorld = colShapeFinalVertical.BottomLeft;
        var bottomRightWorld = colShapeFinalVertical.BottomRight;
        
        var topLeftGrid = worldRef.WorldToGrid(topLeftWorld);
        var topRightGrid = worldRef.WorldToGrid(topRightWorld);
        var bottomLeftGrid = worldRef.WorldToGrid(bottomLeftWorld);
        var bottomRightGrid = worldRef.WorldToGrid(bottomRightWorld);

        var topGrid = topLeftGrid.Y;
        var bottomGrid = bottomLeftGrid.Y;
        var leftGrid = topLeftGrid.X;
        var rightGrid = topRightGrid.X;

        List<Point> tilesCollided = [];
        for (int i = leftGrid; i <= rightGrid; i++)
        {
            for (int j = topGrid; j <= bottomGrid; j++)
            {
                var tilePos = new Point(i, j);
                if (worldRef.IsPositionSolid(i, j))
                {
                    tilesCollided.Add(tilePos);
                    worldRef.DebugCollidedTiles.Add(tilePos);
                }
                else
                {
                    worldRef.DebugCheckedTiles.Add(tilePos);
                }
            }
        }

        if (tilesCollided.Count == 0)
            return entity.Velocity;
        
        var leftMost  = tilesCollided.OrderBy(p => p.X).First();
        var rightMost = tilesCollided.OrderByDescending(p => p.X).First();
        var topMost   = tilesCollided.OrderBy(p => p.Y).First();
        var bottomMost= tilesCollided.OrderByDescending(p => p.Y).First();
        if (moveOffset.Y > 0) // moving down - should snap to top of topmost block
        {
            var worldOriginOfTopMostTile = worldRef.GridToWorld(topMost);
            var offset = float.Abs(colShapeFinalVertical.Bottom - worldOriginOfTopMostTile.Y);
            moveOffset.Y -= offset;
        }
        else if (moveOffset.Y < 0) // moving up - should snap to bottom of bottommost block
        {
            // TODO:
            // var worldOriginOfBottomMostTile = worldRef.GridToWorld(bottomMost);
            // var bottomOfTile = worldOriginOfBottomMostTile.Y + Settings.TileSize;
            // var offset = float.Abs(colShapeFinalVertical.Top - bottomOfTile);
            // MoveOffset.Y += offset;
        }
        
        return moveOffset.IsNearZero() ? Vector2.Zero : moveOffset;
    }
    
    // public bool IsCollidingWithTile(CollisionShape shape)
    // {
    //     var topLeft = _worldRef.WorldToGrid(shape.TopLeft);
    //     var bottomRight = _worldRef.WorldToGrid(shape.BottomRight);
    //
    //     for (int x = topLeft.X; x <= bottomRight.X; x++)
    //     {
    //         for (int y = topLeft.Y; y <= bottomRight.Y; y++)
    //         {
    //             if (_worldRef.IsPositionSolid(x, y))
    //                 return true;
    //         }
    //     }
    //
    //     return false;
    // }
}