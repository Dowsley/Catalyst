using Catalyst.Core.WorldGeneration.Masks;
using Microsoft.Xna.Framework;

// Required for Settings.Layers

namespace Tests // Updated namespace
{
    public class LayerMaskTests
    {
        private static readonly Point TestWorldSize = new(10, 100);

        // Helper to create y-coordinates for specific layer ratios based on TestWorldSize.Y = 100
        // Note: Settings.Layers defines end percentages as exclusive (< end)
        // "space": 0.0f - 0.05f -> y: 0-4
        // "surface": 0.05f - 0.15f -> y: 5-14
        // "underground": 0.15f - 0.5f -> y: 15-49
        // "cavern": 0.5f - 0.9f -> y: 50-89
        // "underworld": 0.9f - 1f -> y: 90-99

        private const int YInSpace = 2;       // Ratio 0.02
        private const int YAtSurfaceStart = 5;  // Ratio 0.05
        private const int YInSurface = 10;      // Ratio 0.10
        private const int YAtSurfaceEnd = 14;   // Ratio 0.14 (IsInLayer should be true)
        private const int YAtUndergroundStart = 15; // Ratio 0.15 (Boundary, IsInLayer for surface should be false)
        private const int YInUnderground = 30;  // Ratio 0.30
        private const int YInCavern = 70;       // Ratio 0.70
        private const int YInUnderworld = 95;   // Ratio 0.95
        private const int YOutOfBoundsLow = -1;
        private const int YOutOfBoundsHigh = 100;

        [Fact]
        public void LayerMask_AllowList_SurfaceOnly_PointInSurface_IsAllowed()
        {
            var layersToAllow = new List<string> { "surface" };
            var mask = new LayerMask(TestWorldSize, layersToAllow, allowList: true);
            Assert.True(mask.IsAllowed(0, YInSurface));
        }

        [Fact]
        public void LayerMask_AllowList_SurfaceOnly_PointAtSurfaceStart_IsAllowed()
        {
            var layersToAllow = new List<string> { "surface" };
            var mask = new LayerMask(TestWorldSize, layersToAllow, allowList: true);
            Assert.True(mask.IsAllowed(0, YAtSurfaceStart));
        }
        
        [Fact]
        public void LayerMask_AllowList_SurfaceOnly_PointAtSurfaceEnd_IsAllowed()
        {
            var layersToAllow = new List<string> { "surface" };
            var mask = new LayerMask(TestWorldSize, layersToAllow, allowList: true);
            Assert.True(mask.IsAllowed(0, YAtSurfaceEnd)); // yRatio 0.14 is < 0.15 (surface end)
        }

        [Fact]
        public void LayerMask_AllowList_SurfaceOnly_PointBelowSurface_IsNotAllowed()
        {
            var layersToAllow = new List<string> { "surface" };
            var mask = new LayerMask(TestWorldSize, layersToAllow, allowList: true);
            Assert.False(mask.IsAllowed(0, YInSpace)); // Point in "space"
        }

        [Fact]
        public void LayerMask_AllowList_SurfaceOnly_PointAboveSurface_IsNotAllowed()
        {
            var layersToAllow = new List<string> { "surface" };
            var mask = new LayerMask(TestWorldSize, layersToAllow, allowList: true);
            Assert.False(mask.IsAllowed(0, YInUnderground)); // Point in "underground"
        }
        
        [Fact]
        public void LayerMask_AllowList_SurfaceOnly_PointAtUndergroundStart_IsNotAllowed()
        {
            // This point is YAtUndergroundStart (y=15, ratio 0.15), which is the start of "underground"
            // and thus outside the "surface" layer (which ends *before* 0.15).
            var layersToAllow = new List<string> { "surface" };
            var mask = new LayerMask(TestWorldSize, layersToAllow, allowList: true);
            Assert.False(mask.IsAllowed(0, YAtUndergroundStart));
        }

        [Fact]
        public void LayerMask_AllowList_MultipleLayers_PointInFirstAllowed_IsAllowed()
        {
            var layersToAllow = new List<string> { "space", "cavern" };
            var mask = new LayerMask(TestWorldSize, layersToAllow, allowList: true);
            Assert.True(mask.IsAllowed(0, YInSpace));
        }

        [Fact]
        public void LayerMask_AllowList_MultipleLayers_PointInSecondAllowed_IsAllowed()
        {
            var layersToAllow = new List<string> { "space", "cavern" };
            var mask = new LayerMask(TestWorldSize, layersToAllow, allowList: true);
            Assert.True(mask.IsAllowed(0, YInCavern));
        }

        [Fact]
        public void LayerMask_AllowList_MultipleLayers_PointInBetweenAllowed_IsNotAllowed()
        {
            var layersToAllow = new List<string> { "space", "cavern" };
            var mask = new LayerMask(TestWorldSize, layersToAllow, allowList: true);
            Assert.False(mask.IsAllowed(0, YInSurface)); // Surface is not in the allow list
        }
        
        [Fact]
        public void LayerMask_AllowList_OutOfBounds_IsNotAllowed()
        {
            var layersToAllow = new List<string> { "surface" };
            var mask = new LayerMask(TestWorldSize, layersToAllow, allowList: true);
            Assert.False(mask.IsAllowed(0, YOutOfBoundsLow));
            Assert.False(mask.IsAllowed(0, YOutOfBoundsHigh));
            Assert.False(mask.IsAllowed(TestWorldSize.X, YInSurface)); // x out of bounds
        }

        // --- Tests for allowList = false (BlockList behavior) ---

        [Fact]
        public void LayerMask_BlockList_SurfaceOnly_PointInSurface_IsNotAllowed()
        {
            var layersToBlock = new List<string> { "surface" };
            var mask = new LayerMask(TestWorldSize, layersToBlock, allowList: false);
            Assert.False(mask.IsAllowed(0, YInSurface));
        }

        [Fact]
        public void LayerMask_BlockList_SurfaceOnly_PointOutsideSurface_IsAllowed()
        {
            var layersToBlock = new List<string> { "surface" };
            var mask = new LayerMask(TestWorldSize, layersToBlock, allowList: false);
            Assert.True(mask.IsAllowed(0, YInSpace));       // Allowed (in "space")
            Assert.True(mask.IsAllowed(0, YInUnderground)); // Allowed (in "underground")
        }

        [Fact]
        public void LayerMask_BlockList_MultipleLayers_PointInBlocked_IsNotAllowed()
        {
            var layersToBlock = new List<string> { "space", "cavern" };
            var mask = new LayerMask(TestWorldSize, layersToBlock, allowList: false);
            Assert.False(mask.IsAllowed(0, YInSpace));
            Assert.False(mask.IsAllowed(0, YInCavern));
        }

        [Fact]
        public void LayerMask_BlockList_MultipleLayers_PointNotInBlocked_IsAllowed()
        {
            var layersToBlock = new List<string> { "space", "cavern" };
            var mask = new LayerMask(TestWorldSize, layersToBlock, allowList: false);
            Assert.True(mask.IsAllowed(0, YInSurface)); // Surface is not blocked
        }
        
        [Fact]
        public void LayerMask_BlockList_OutOfBounds_IsNotAllowed()
        {
            var layersToBlock = new List<string> { "surface" };
            var mask = new LayerMask(TestWorldSize, layersToBlock, allowList: false);
            Assert.False(mask.IsAllowed(0, YOutOfBoundsLow));
            Assert.False(mask.IsAllowed(0, YOutOfBoundsHigh));
        }

        // --- Edge cases for layer lists ---

        [Fact]
        public void LayerMask_AllowList_EmptyLayerList_NoPointsAllowed()
        {
            var layersToAllow = new List<string>();
            var mask = new LayerMask(TestWorldSize, layersToAllow, allowList: true);
            Assert.False(mask.IsAllowed(0, YInSurface));
            Assert.False(mask.IsAllowed(0, YInSpace));
        }

        [Fact]
        public void LayerMask_BlockList_EmptyLayerList_AllPointsAllowed()
        {
            var layersToBlock = new List<string>();
            var mask = new LayerMask(TestWorldSize, layersToBlock, allowList: false);
            Assert.True(mask.IsAllowed(0, YInSurface));
            Assert.True(mask.IsAllowed(0, YInSpace));
        }

        [Fact]
        public void LayerMask_AllowList_NonExistentLayer_NoPointsAllowed()
        {
            var layersToAllow = new List<string> { "nonexistent_layer" };
            var mask = new LayerMask(TestWorldSize, layersToAllow, allowList: true);
            Assert.False(mask.IsAllowed(0, YInSurface));
        }

        [Fact]
        public void LayerMask_BlockList_NonExistentLayer_AllPointsAllowed()
        {
            // If a non-existent layer is "blocked", it doesn't actually block anything.
            var layersToBlock = new List<string> { "nonexistent_layer" };
            var mask = new LayerMask(TestWorldSize, layersToBlock, allowList: false);
            Assert.True(mask.IsAllowed(0, YInSurface));
        }
    }
} 