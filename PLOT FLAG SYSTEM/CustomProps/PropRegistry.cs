using System.Collections.Generic;
using UnityEngine;

namespace CustomProps
{
    /// <summary>
    /// Data structure for a custom prop
    /// </summary>
    public class PropData
    {
        public string PropId;
        public string DisplayName;
        public string Description;
        public GameObject Prefab;
        public Sprite Icon;
        public int Value;
        public PropBehaviorType Behavior;
        public AudioClip PickupSound;
        public GameObject PickupVFX;
        public float SpawnWeight = 1f;
    }

    public enum PropBehaviorType
    {
        Treasure,
        Key,
        Resource,
        Consumable,
        Quest
    }

    /// <summary>
    /// Registry for all custom props
    /// </summary>
    public static class PropRegistry
    {
        private static Dictionary<string, PropData> _props = new Dictionary<string, PropData>();
        private static List<GameObject> _spawnedInstances = new List<GameObject>();

        public static int Count => _props.Count;

        /// <summary>
        /// Register a new custom prop
        /// </summary>
        public static void Register(PropData prop)
        {
            if (prop == null || string.IsNullOrEmpty(prop.PropId))
            {
                CustomPropsPlugin.Log?.LogWarning("Cannot register prop with null/empty PropId");
                return;
            }

            if (_props.ContainsKey(prop.PropId))
            {
                CustomPropsPlugin.Log?.LogWarning($"Prop already registered: {prop.PropId}");
                return;
            }

            _props[prop.PropId] = prop;
            CustomPropsPlugin.Log?.LogInfo($"Registered prop: {prop.PropId}");
        }

        /// <summary>
        /// Get a prop by ID
        /// </summary>
        public static PropData Get(string propId)
        {
            if (_props.TryGetValue(propId, out var prop))
            {
                return prop;
            }
            return null;
        }

        /// <summary>
        /// Get all registered props
        /// </summary>
        public static ICollection<PropData> GetAll()
        {
            return _props.Values;
        }

        /// <summary>
        /// Check if a prop is registered
        /// </summary>
        public static bool IsRegistered(string propId)
        {
            return _props.ContainsKey(propId);
        }

        /// <summary>
        /// Spawn a custom prop at a position
        /// </summary>
        public static GameObject Spawn(string propId, Vector3 position, Quaternion rotation)
        {
            var prop = Get(propId);
            if (prop == null)
            {
                CustomPropsPlugin.Log?.LogWarning($"Cannot spawn unknown prop: {propId}");
                return null;
            }

            if (prop.Prefab == null)
            {
                CustomPropsPlugin.Log?.LogWarning($"Prop {propId} has no prefab");
                return null;
            }

            // Instantiate the prop
            var instance = Object.Instantiate(prop.Prefab, position, rotation);
            instance.name = $"CustomProp_{prop.PropId}_{_spawnedInstances.Count}";

            // Add pickup handler component
            var pickup = instance.AddComponent<CustomPropPickup>();
            pickup.Initialize(prop);

            // Ensure it has a collider for pickup
            var collider = instance.GetComponent<Collider>();
            if (collider == null)
            {
                // Add a sphere collider as fallback
                var sphereCollider = instance.AddComponent<SphereCollider>();
                sphereCollider.isTrigger = true;
                sphereCollider.radius = 0.5f;
            }
            else
            {
                // Make existing collider a trigger
                collider.isTrigger = true;
            }

            // Track spawned instances
            _spawnedInstances.Add(instance);

            CustomPropsPlugin.Log?.LogInfo($"Spawned {propId} at {position}");
            return instance;
        }

        /// <summary>
        /// Despawn all custom props
        /// </summary>
        public static void DespawnAll()
        {
            foreach (var instance in _spawnedInstances)
            {
                if (instance != null)
                {
                    Object.Destroy(instance);
                }
            }
            _spawnedInstances.Clear();
        }

        /// <summary>
        /// Clear all registered props
        /// </summary>
        public static void Clear()
        {
            _props.Clear();
            DespawnAll();
        }
    }
}
