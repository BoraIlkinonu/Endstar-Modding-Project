using UnityEngine;

namespace SwimmingSystem
{
    /// <summary>
    /// Configuration constants for the swimming system
    /// </summary>
    public static class SwimConfig
    {
        // === Movement Speeds ===
        public const float SwimSpeed = 4.0f;           // XZ plane movement speed
        public const float DiveSpeed = 2.5f;           // Downward diving speed
        public const float SurfaceSpeed = 3.0f;        // Upward surfacing speed
        public const float RotationSpeed = 120f;       // Degrees per second for turning

        // === Input Keys ===
        public const KeyCode DiveKey = KeyCode.Alpha1;      // Dive down (1 key)
        public const KeyCode SurfaceKey = KeyCode.Alpha2;   // Surface up (2 key)
        public const KeyCode DebugToggleKey = KeyCode.Alpha9; // DEBUG ONLY: Toggle swimming (9 key)

        // === Animation Timings ===
        public const float DiveEntryDuration = 1.2f;   // Run To Dive animation length
        public const float SwimmingLoopLength = 3.23f; // Swimming_Generic length
        public const float TreadingLoopLength = 2.13f; // Treading Water_Generic length
        public const float AnimationBlendTime = 0.15f; // Crossfade time between anims

        // === Water Detection ===
        public const string WaterLayerName = "Water";
        public const string PlayerTag = "Player";
        public const float WaterSurfaceOffset = 0.5f;  // How far below surface to start swimming

        // === Gameplay Mode ===
        public const string GameplaySceneName = "MainScene";
        public const string CreatorSceneName = "Creator";

        // === Bundle Info ===
        public const string BundleFileName = "generic_animations.bundle";
        public const string SwimmingClipName = "Swimming_Generic";
        public const string TreadingClipName = "Treading Water_Generic";
        public const string DiveEntryClipName = "Run To Dive_Generic";

        // === Physics ===
        public const float WaterDrag = 2.0f;           // Drag when in water
        public const float WaterBuoyancy = 0.5f;       // Upward force in water
        public const float GravityInWater = 0.1f;      // Reduced gravity in water
    }
}
