using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level.UpgradeVersions;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Runtime.Gameplay.LevelEditing
{
	// Token: 0x02000019 RID: 25
	[Serializable]
	public class GameLibrary
	{
		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600005B RID: 91 RVA: 0x00003101 File Offset: 0x00001301
		[JsonIgnore]
		public List<TerrainUsage> TerrainEntries
		{
			get
			{
				return this.terrainEntries;
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x0600005C RID: 92 RVA: 0x00003109 File Offset: 0x00001309
		[JsonIgnore]
		public IReadOnlyList<AssetReference> PropReferences
		{
			get
			{
				return this.propReferences;
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x0600005D RID: 93 RVA: 0x00003111 File Offset: 0x00001311
		[JsonIgnore]
		public IReadOnlyList<AssetReference> AudioReferences
		{
			get
			{
				return this.audioReferences;
			}
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00003119 File Offset: 0x00001319
		public GameLibrary.RemoveTerrainEntryResult RemoveTerrainEntry(TerrainUsage terrainUsage, TerrainUsage redirectEntry)
		{
			return this.RemoveTerrainEntry(terrainUsage.TilesetId, redirectEntry.TilesetId);
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00003130 File Offset: 0x00001330
		public GameLibrary.RemoveTerrainEntryResult RemoveTerrainEntry(SerializableGuid id, SerializableGuid redirectId)
		{
			TerrainUsage terrainUsage = this.terrainEntries.FirstOrDefault((TerrainUsage entry) => entry.TilesetId == id);
			if (terrainUsage == null)
			{
				return new GameLibrary.RemoveTerrainEntryResult(false, -1);
			}
			TerrainUsage terrainUsage2 = this.terrainEntries.FirstOrDefault((TerrainUsage replacement) => replacement.TilesetId == redirectId);
			if (terrainUsage2 == null)
			{
				return new GameLibrary.RemoveTerrainEntryResult(false, -1);
			}
			terrainUsage.TerrainAssetReference = null;
			terrainUsage.RedirectIndex = this.terrainEntries.IndexOf(terrainUsage2);
			return new GameLibrary.RemoveTerrainEntryResult(true, this.terrainEntries.IndexOf(terrainUsage));
		}

		// Token: 0x06000060 RID: 96 RVA: 0x000031C4 File Offset: 0x000013C4
		public bool AddTerrainUsage(SerializableGuid tilesetId, string version)
		{
			TerrainUsage terrainUsage = new TerrainUsage(new AssetReference
			{
				AssetID = tilesetId,
				AssetVersion = version,
				AssetType = "terrain-tileset-cosmetic"
			});
			terrainUsage.RedirectIndex = this.terrainEntries.Count;
			if (this.terrainEntries.Any((TerrainUsage entry) => entry.TilesetId == tilesetId))
			{
				return false;
			}
			this.terrainEntries.Add(terrainUsage);
			return true;
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00003248 File Offset: 0x00001448
		public bool AddTerrainUsages(IEnumerable<AssetReference> newUsages)
		{
			int num = 0;
			using (IEnumerator<AssetReference> enumerator = newUsages.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					AssetReference newAssetReference = enumerator.Current;
					TerrainUsage terrainUsage = new TerrainUsage(newAssetReference)
					{
						RedirectIndex = this.terrainEntries.Count
					};
					if (!this.terrainEntries.Any((TerrainUsage entry) => entry.TilesetId == newAssetReference.AssetID))
					{
						this.terrainEntries.Add(terrainUsage);
						num++;
					}
				}
			}
			return num > 0;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x000032E0 File Offset: 0x000014E0
		public int InsertTerrainUsage(SerializableGuid tilesetId, string version, int index)
		{
			TerrainUsage terrainUsage = new TerrainUsage(new AssetReference
			{
				AssetID = tilesetId,
				AssetVersion = version,
				AssetType = "terrain-tileset-cosmetic"
			});
			terrainUsage.RedirectIndex = index;
			int count = this.terrainEntries.Count;
			this.terrainEntries[index] = terrainUsage;
			return this.terrainEntries.Count - 1;
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00003348 File Offset: 0x00001548
		public bool SetTerrainUsageVersion(SerializableGuid id, string newTerrainVersion)
		{
			TerrainUsage terrainUsage = this.terrainEntries.FirstOrDefault((TerrainUsage entry) => entry.TilesetId == id);
			if (terrainUsage == null)
			{
				Debug.LogException(new Exception(string.Format("Could not find {0} in {1} with an {2} of {3}!", new object[] { "TerrainUsage", "terrainEntries", "id", id })));
				return false;
			}
			int num = this.terrainEntries.IndexOf(terrainUsage);
			if (this.terrainEntries[num].TerrainAssetReference.AssetVersion != newTerrainVersion)
			{
				this.terrainEntries[num].TerrainAssetReference.AssetVersion = newTerrainVersion;
				return true;
			}
			return false;
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00003404 File Offset: 0x00001604
		public void OverrideTerrainUsages(List<TerrainUsage> overrides)
		{
			this.terrainEntries = overrides;
		}

		// Token: 0x06000065 RID: 101 RVA: 0x0000340D File Offset: 0x0000160D
		public void ClearProps()
		{
			this.propReferences.Clear();
		}

		// Token: 0x06000066 RID: 102 RVA: 0x0000341C File Offset: 0x0000161C
		public bool AddProp(AssetReference propReference)
		{
			for (int i = 0; i < this.propReferences.Count; i++)
			{
				if (this.propReferences[i].AssetID == propReference.AssetID)
				{
					return false;
				}
			}
			this.propReferences.Add(propReference);
			return true;
		}

		// Token: 0x06000067 RID: 103 RVA: 0x0000346C File Offset: 0x0000166C
		public bool AddProps(IEnumerable<AssetReference> newPropReferences)
		{
			List<AssetReference> list = new List<AssetReference>();
			foreach (AssetReference assetReference in newPropReferences)
			{
				bool flag = false;
				for (int i = 0; i < this.propReferences.Count; i++)
				{
					if (this.propReferences[i].AssetID == assetReference.AssetID)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(assetReference);
				}
			}
			if (list.Count > 0)
			{
				this.propReferences.AddRange(list);
				return true;
			}
			return false;
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00003514 File Offset: 0x00001714
		public bool SetPropVersion(SerializableGuid id, string newAssetVersion)
		{
			AssetReference assetReference = this.propReferences.FirstOrDefault((AssetReference entry) => entry.AssetID == id);
			if (assetReference == null)
			{
				Debug.LogException(new Exception(string.Format("Could not find {0} in {1} with an {2} of {3}!", new object[] { "AssetReference", "propReferences", "id", id })));
				return false;
			}
			int num = this.propReferences.IndexOf(assetReference);
			if (this.propReferences[num].AssetVersion != newAssetVersion)
			{
				this.propReferences[num].AssetVersion = newAssetVersion;
				return true;
			}
			return false;
		}

		// Token: 0x06000069 RID: 105 RVA: 0x000035C8 File Offset: 0x000017C8
		public bool RemoveProp(SerializableGuid assetID)
		{
			AssetReference assetReference = this.propReferences.FirstOrDefault((AssetReference propReference) => propReference.AssetID == assetID);
			if (assetReference == null)
			{
				return false;
			}
			this.propReferences.Remove(assetReference);
			return true;
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00003610 File Offset: 0x00001810
		public static GameLibrary Upgrade(Game_0_0.GameLibrary_0_0 oldLibrary)
		{
			GameLibrary gameLibrary = new GameLibrary();
			gameLibrary.propReferences = new List<AssetReference>();
			gameLibrary.terrainEntries = new List<TerrainUsage>();
			foreach (Game_0_0.TerrainUsage_0_0 terrainUsage_0_ in oldLibrary.terrainEntries)
			{
				gameLibrary.terrainEntries.Add(TerrainUsage.Upgrade(terrainUsage_0_));
			}
			return gameLibrary;
		}

		// Token: 0x0600006B RID: 107 RVA: 0x0000368C File Offset: 0x0000188C
		public IReadOnlyList<AssetReference> GetPropAssetReferencesForLoad(HashSet<SerializableGuid> propIds)
		{
			return this.PropReferences.Where((AssetReference reference) => propIds.Contains(reference.AssetID)).ToArray<AssetReference>();
		}

		// Token: 0x0600006C RID: 108 RVA: 0x000036C4 File Offset: 0x000018C4
		public IReadOnlyList<TerrainUsage> GetTerrainUsagesInLevel(HashSet<int> tilesetIds)
		{
			List<TerrainUsage> list = new List<TerrainUsage>();
			foreach (int num in tilesetIds)
			{
				if (this.TerrainEntries.Count > num)
				{
					TerrainUsage terrainUsage = this.TerrainEntries[num];
					list.Add(terrainUsage);
				}
			}
			return list;
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00003734 File Offset: 0x00001934
		public int GetTerrainIndex(SerializableGuid assetId)
		{
			string text = assetId;
			for (int i = 0; i < this.terrainEntries.Count; i++)
			{
				if (this.terrainEntries[i].IsActive && this.terrainEntries[i].TerrainAssetReference.AssetID == text)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00003794 File Offset: 0x00001994
		public bool AddAudio(AssetReference audioReference)
		{
			for (int i = 0; i < this.audioReferences.Count; i++)
			{
				if (this.audioReferences[i].AssetID == audioReference.AssetID)
				{
					return false;
				}
			}
			this.audioReferences.Add(audioReference);
			return true;
		}

		// Token: 0x0600006F RID: 111 RVA: 0x000037E4 File Offset: 0x000019E4
		public bool SetAudioVersion(SerializableGuid id, string newAssetVersion)
		{
			AssetReference assetReference = this.audioReferences.FirstOrDefault((AssetReference entry) => entry.AssetID == id);
			if (assetReference == null)
			{
				Debug.LogException(new Exception(string.Format("Could not find {0} in {1} with an {2} of {3}!", new object[] { "AssetReference", "audioReferences", "id", id })));
				return false;
			}
			int num = this.audioReferences.IndexOf(assetReference);
			if (this.audioReferences[num].AssetVersion != newAssetVersion)
			{
				this.audioReferences[num].AssetVersion = newAssetVersion;
				return true;
			}
			return false;
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00003898 File Offset: 0x00001A98
		public bool RemoveAudio(SerializableGuid assetID)
		{
			AssetReference assetReference = this.audioReferences.FirstOrDefault((AssetReference propReference) => propReference.AssetID == assetID);
			if (assetReference == null)
			{
				return false;
			}
			this.audioReferences.Remove(assetReference);
			return true;
		}

		// Token: 0x06000071 RID: 113 RVA: 0x000038E0 File Offset: 0x00001AE0
		public TerrainUsage GetActiveTerrainUsageFromIndex(int index)
		{
			HashSet<int> hashSet = new HashSet<int>();
			TerrainUsage terrainUsage = this.TerrainEntries[index];
			while (!terrainUsage.IsActive)
			{
				int redirectIndex = terrainUsage.RedirectIndex;
				if (redirectIndex > this.TerrainEntries.Count)
				{
					Debug.LogException(new Exception("Had improper index in terrain redirects"));
					return null;
				}
				if (hashSet.Contains(redirectIndex))
				{
					Debug.LogException(new Exception(string.Format("Redirect Indexes looped. Can't validate terrain usage at index: {0}", index)));
					return null;
				}
				hashSet.Add(redirectIndex);
				terrainUsage = this.TerrainEntries[redirectIndex];
			}
			return terrainUsage;
		}

		// Token: 0x04000043 RID: 67
		[SerializeField]
		[JsonProperty]
		private List<TerrainUsage> terrainEntries = new List<TerrainUsage>();

		// Token: 0x04000044 RID: 68
		[SerializeField]
		[JsonProperty("props")]
		private List<AssetReference> propReferences = new List<AssetReference>();

		// Token: 0x04000045 RID: 69
		[SerializeField]
		[JsonProperty("audio")]
		private List<AssetReference> audioReferences = new List<AssetReference>();

		// Token: 0x0200001A RID: 26
		public struct RemoveTerrainEntryResult
		{
			// Token: 0x06000073 RID: 115 RVA: 0x00003994 File Offset: 0x00001B94
			public RemoveTerrainEntryResult(bool success, int indexOfTerrainRedirectTo)
			{
				this.Success = success;
				this.IndexOfTerrainRedirectTo = indexOfTerrainRedirectTo;
			}

			// Token: 0x04000046 RID: 70
			public bool Success;

			// Token: 0x04000047 RID: 71
			public int IndexOfTerrainRedirectTo;
		}
	}
}
