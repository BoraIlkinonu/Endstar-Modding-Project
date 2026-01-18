using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CustomProps
{
    /// <summary>
    /// Handles persistence of custom prop injection state across sessions.
    ///
    /// Problem: When a level reloads, the game looks up props by AssetId, but our
    /// custom prop's AssetId doesn't exist in the game's asset system.
    ///
    /// Solution:
    /// 1. Save custom prop AssetIds and their associated prefab paths
    /// 2. On session load, re-inject props BEFORE the game tries to load them
    /// 3. Track which props are "ours" so we know to re-inject them
    /// </summary>
    public static class PropPersistence
    {
        private static string _persistenceFilePath;
        private static PersistenceData _data;
        private static bool _initialized = false;

        [Serializable]
        public class PersistenceData
        {
            public List<InjectedPropRecord> InjectedProps = new List<InjectedPropRecord>();
        }

        [Serializable]
        public class InjectedPropRecord
        {
            public string PropId;           // Our custom ID (e.g., "custom_prop_001")
            public string AssetId;          // The AssetId (GUID) assigned during injection
            public string PrefabBundlePath; // Path to prefab bundle for reference
            public string DisplayName;      // Human-readable name
            public long LastInjectedTimestamp;
        }

        public static void Initialize(string pluginFolder)
        {
            if (_initialized) return;

            _persistenceFilePath = Path.Combine(pluginFolder, "injected_props.json");
            CustomPropsPlugin.Log?.LogInfo($"PropPersistence: Using path {_persistenceFilePath}");

            Load();
            _initialized = true;
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(_persistenceFilePath))
                {
                    var json = File.ReadAllText(_persistenceFilePath);
                    _data = UnityEngine.JsonUtility.FromJson<PersistenceData>(json);
                    CustomPropsPlugin.Log?.LogInfo($"PropPersistence: Loaded {_data.InjectedProps.Count} persisted props");
                }
                else
                {
                    _data = new PersistenceData();
                    CustomPropsPlugin.Log?.LogInfo("PropPersistence: No persistence file found, starting fresh");
                }
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"PropPersistence: Failed to load: {ex.Message}");
                _data = new PersistenceData();
            }
        }

        public static void Save()
        {
            try
            {
                var json = UnityEngine.JsonUtility.ToJson(_data, true);
                File.WriteAllText(_persistenceFilePath, json);
                CustomPropsPlugin.Log?.LogInfo($"PropPersistence: Saved {_data.InjectedProps.Count} props");
            }
            catch (Exception ex)
            {
                CustomPropsPlugin.Log?.LogError($"PropPersistence: Failed to save: {ex.Message}");
            }
        }

        /// <summary>
        /// Record that a prop was injected with a specific AssetId
        /// </summary>
        public static void RecordInjection(string propId, string assetId, string displayName, string prefabPath = null)
        {
            if (_data == null)
            {
                CustomPropsPlugin.Log?.LogWarning("PropPersistence: Not initialized");
                return;
            }

            // Check if already recorded
            var existing = _data.InjectedProps.Find(p => p.PropId == propId);
            if (existing != null)
            {
                // Update existing record
                existing.AssetId = assetId;
                existing.LastInjectedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                CustomPropsPlugin.Log?.LogInfo($"PropPersistence: Updated record for {propId}");
            }
            else
            {
                // Add new record
                _data.InjectedProps.Add(new InjectedPropRecord
                {
                    PropId = propId,
                    AssetId = assetId,
                    DisplayName = displayName,
                    PrefabBundlePath = prefabPath,
                    LastInjectedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
                CustomPropsPlugin.Log?.LogInfo($"PropPersistence: Added new record for {propId} with AssetId={assetId}");
            }

            Save();
        }

        /// <summary>
        /// Check if an AssetId belongs to one of our custom props
        /// </summary>
        public static bool IsCustomPropAssetId(string assetId)
        {
            if (_data == null) return false;
            return _data.InjectedProps.Exists(p => p.AssetId == assetId);
        }

        /// <summary>
        /// Get the PropId for a custom AssetId
        /// </summary>
        public static string GetPropIdForAssetId(string assetId)
        {
            if (_data == null) return null;
            var record = _data.InjectedProps.Find(p => p.AssetId == assetId);
            return record?.PropId;
        }

        /// <summary>
        /// Get all recorded custom prop AssetIds
        /// </summary>
        public static List<string> GetAllCustomAssetIds()
        {
            if (_data == null) return new List<string>();
            var ids = new List<string>();
            foreach (var record in _data.InjectedProps)
            {
                ids.Add(record.AssetId);
            }
            return ids;
        }

        /// <summary>
        /// Get all injection records
        /// </summary>
        public static List<InjectedPropRecord> GetAllRecords()
        {
            if (_data == null) return new List<InjectedPropRecord>();
            return new List<InjectedPropRecord>(_data.InjectedProps);
        }

        /// <summary>
        /// Clear all persistence data (for testing)
        /// </summary>
        public static void Clear()
        {
            _data = new PersistenceData();
            Save();
            CustomPropsPlugin.Log?.LogInfo("PropPersistence: Cleared all data");
        }
    }
}
