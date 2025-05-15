using System;
using System.Collections.Generic;
using System.Linq;
using Catalyst.Core;
using Catalyst.Core.Extensions;
using Catalyst.Globals;
using Microsoft.Xna.Framework;

namespace Catalyst.Entities.Actions;

public class WalkAction(Entity entityRef, Vector2 moveOffset) : Action(entityRef)
{
    protected Vector2 MoveOffset = moveOffset;
    
    public override bool CanPerform(World worldRef)
    {
        var colShapeFinalVertical = EntityRef.CollisionShape;
        colShapeFinalVertical.Position.Y += MoveOffset.Y;

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
            return true;
        
        var leftMost  = tilesCollided.OrderBy(p => p.X).First();
        var rightMost = tilesCollided.OrderByDescending(p => p.X).First();
        var topMost   = tilesCollided.OrderBy(p => p.Y).First();
        var bottomMost= tilesCollided.OrderByDescending(p => p.Y).First();
        if (MoveOffset.Y > 0) // moving down - should snap to top of topmost block
        {
            var worldOriginOfTopMostTile = worldRef.GridToWorld(topMost);
            var offset = float.Abs(colShapeFinalVertical.Bottom - worldOriginOfTopMostTile.Y);
            MoveOffset.Y -= offset;
        }
        else if (MoveOffset.Y < 0) // moving up - should snap to bottom of bottommost block
        {
            // var worldOriginOfBottomMostTile = worldRef.GridToWorld(bottomMost);
            // var bottomOfTile = worldOriginOfBottomMostTile.Y + Settings.TileSize;
            // var offset = float.Abs(colShapeFinalVertical.Top - bottomOfTile);
            // MoveOffset.Y += offset;
        }
        
        Console.WriteLine(EntityRef.Velocity);
        return !MoveOffset.IsNearZero();
    }

    public override void Perform(World worldRef)
    {
        EntityRef.Position += MoveOffset;
        
        var feetPos = worldRef.WorldToGrid(EntityRef.Position + new Vector2(0, EntityRef.CollisionShape.Size.Y));
        bool isGrounded = worldRef.IsPositionSolid(feetPos.X, feetPos.Y);
        if (isGrounded)
        {
            EntityRef.Velocity.X *= Settings.GroundFriction;
        }
        else
        {
            EntityRef.Velocity.X *= Settings.AirResistance;
        }
        
        // Clamp horizontal velocity
        if (MathF.Abs(EntityRef.Velocity.X) < Settings.MinimumVelocity)
        {
            EntityRef.Velocity.X = 0;
        }
    }
}
