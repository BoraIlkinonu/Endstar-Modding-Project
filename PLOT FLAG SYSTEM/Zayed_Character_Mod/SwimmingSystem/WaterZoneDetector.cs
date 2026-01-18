using UnityEngine;
using System.Collections.Generic;

namespace SwimmingSystem
{
    /// <summary>
    /// Detects when the player enters or exits a water zone.
    /// Handles blur removal and damage prevention.
    /// </summary>
    public class WaterZoneDetector : MonoBehaviour
    {
        private SwimmingManager _manager;
        private Collider _waterCollider;

        // Blur/effect references (found at runtime)
        private List<MonoBehaviour> _disabledEffects = new List<MonoBehaviour>();
        private bool _blurDisabled = false;

        // Damage component (found at runtime)
        private MonoBehaviour _damageComponent;
        private bool _damageDisabled = false;

        public void Initialize(SwimmingManager manager)
        {
            _manager = manager;
            _waterCollider = GetComponent<Collider>();

            // Make sure collider is a trigger
            if (_waterCollider != null)
            {
                _waterCollider.isTrigger = true;
                Plugin.Log.LogWarning($"[WATERZONE] Initialized on: {gameObject.name}");
            }

            // Look for and disable blur/damage components
            FindAndPrepareWaterEffects();
        }

        private void FindAndPrepareWaterEffects()
        {
            // Search for water-related components on this object and children
            var components = GetComponentsInChildren<MonoBehaviour>();

            foreach (var comp in components)
            {
                if (comp == null || comp == this) continue;

                string typeName = comp.GetType().Name.ToLower();

                // Look for blur/post-processing effects
                if (typeName.Contains("blur") || typeName.Contains("underwater") ||
                    typeName.Contains("postprocess") || typeName.Contains("fog") ||
                    typeName.Contains("distort"))
                {
                    Plugin.Log.LogWarning($"[WATERZONE] Found potential blur effect: {comp.GetType().Name}");
                    // Don't disable yet, just track it
                }

                // Look for damage/hazard components
                if (typeName.Contains("damage") || typeName.Contains("hazard") ||
                    typeName.Contains("hurt") || typeName.Contains("kill") ||
                    typeName.Contains("health"))
                {
                    _damageComponent = comp;
                    Plugin.Log.LogWarning($"[WATERZONE] Found potential damage component: {comp.GetType().Name}");
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // Check if this is the player
            if (!IsPlayer(other)) return;

            Plugin.Log.LogWarning($"[WATERZONE] Player ENTERED water zone: {gameObject.name}");

            // Disable blur effects
            DisableBlurEffects();

            // Disable damage
            DisableDamage();

            // Notify manager
            _manager?.EnterWater();
        }

        void OnTriggerExit(Collider other)
        {
            // Check if this is the player
            if (!IsPlayer(other)) return;

            Plugin.Log.LogWarning($"[WATERZONE] Player EXITED water zone: {gameObject.name}");

            // Re-enable effects (optional - might want to keep blur disabled)
            // EnableBlurEffects();

            // Re-enable damage (keeping disabled for our modified water)
            // EnableDamage();

            // Notify manager
            _manager?.ExitWater();
        }

        private bool IsPlayer(Collider other)
        {
            // Check by tag
            if (other.CompareTag("Player")) return true;

            // Check by name
            string name = other.gameObject.name.ToLower();
            if (name.Contains("player") || name.Contains("character")) return true;

            // Check parent names
            Transform parent = other.transform.parent;
            while (parent != null)
            {
                string parentName = parent.name.ToLower();
                if (parentName.Contains("player") || parentName.Contains("character"))
                    return true;
                parent = parent.parent;
            }

            return false;
        }

        private void DisableBlurEffects()
        {
            if (_blurDisabled) return;

            // Find blur effects globally (they might be on camera or post-processing volume)
            var camera = Camera.main;
            if (camera != null)
            {
                // Check for Image Effects on camera
                var cameraEffects = camera.GetComponents<MonoBehaviour>();
                foreach (var effect in cameraEffects)
                {
                    if (effect == null) continue;

                    string typeName = effect.GetType().Name.ToLower();
                    if (typeName.Contains("blur") || typeName.Contains("underwater") ||
                        typeName.Contains("depth") || typeName.Contains("fog"))
                    {
                        if (effect.enabled)
                        {
                            effect.enabled = false;
                            _disabledEffects.Add(effect);
                            Plugin.Log.LogWarning($"[WATERZONE] Disabled camera effect: {effect.GetType().Name}");
                        }
                    }
                }
            }

            // Also search for global post-processing
            var allObjects = FindObjectsOfType<MonoBehaviour>();
            foreach (var obj in allObjects)
            {
                if (obj == null) continue;

                string typeName = obj.GetType().Name.ToLower();
                if ((typeName.Contains("underwater") && typeName.Contains("effect")) ||
                    (typeName.Contains("water") && typeName.Contains("blur")))
                {
                    if (obj.enabled)
                    {
                        obj.enabled = false;
                        _disabledEffects.Add(obj);
                        Plugin.Log.LogWarning($"[WATERZONE] Disabled global effect: {obj.GetType().Name}");
                    }
                }
            }

            _blurDisabled = true;
            Plugin.Log.LogWarning($"[WATERZONE] Disabled {_disabledEffects.Count} blur/underwater effects");
        }

        private void EnableBlurEffects()
        {
            foreach (var effect in _disabledEffects)
            {
                if (effect != null)
                {
                    effect.enabled = true;
                }
            }
            _disabledEffects.Clear();
            _blurDisabled = false;
            Plugin.Log.LogWarning("[WATERZONE] Re-enabled blur effects");
        }

        private void DisableDamage()
        {
            if (_damageDisabled) return;

            if (_damageComponent != null)
            {
                _damageComponent.enabled = false;
                _damageDisabled = true;
                Plugin.Log.LogWarning($"[WATERZONE] Disabled damage component: {_damageComponent.GetType().Name}");
            }

            // Also search for damage components on this object's children
            var damageComps = GetComponentsInChildren<MonoBehaviour>();
            foreach (var comp in damageComps)
            {
                if (comp == null || comp == this) continue;

                string typeName = comp.GetType().Name.ToLower();
                if (typeName.Contains("damage") || typeName.Contains("hazard"))
                {
                    comp.enabled = false;
                    Plugin.Log.LogWarning($"[WATERZONE] Disabled damage: {comp.GetType().Name}");
                }
            }

            _damageDisabled = true;
        }

        private void EnableDamage()
        {
            if (_damageComponent != null)
            {
                _damageComponent.enabled = true;
            }
            _damageDisabled = false;
        }
    }
}
