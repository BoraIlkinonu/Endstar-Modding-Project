using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Endless.Assets;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Props.Assets
{
	// Token: 0x0200003C RID: 60
	[Serializable]
	public class Prop : Asset
	{
		// Token: 0x1700005A RID: 90
		// (get) Token: 0x060000EB RID: 235 RVA: 0x00003471 File Offset: 0x00001671
		[JsonIgnore]
		public string BaseTypeId
		{
			get
			{
				return this.baseTypeId;
			}
		}

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x060000EC RID: 236 RVA: 0x00003479 File Offset: 0x00001679
		[JsonIgnore]
		public IReadOnlyList<string> ComponentIds
		{
			get
			{
				return this.componentIds;
			}
		}

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x060000ED RID: 237 RVA: 0x00003481 File Offset: 0x00001681
		[JsonIgnore]
		public List<AssetReference> VisualAssets
		{
			get
			{
				return this.visualAssets;
			}
		}

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x060000EE RID: 238 RVA: 0x00003489 File Offset: 0x00001689
		// (set) Token: 0x060000EF RID: 239 RVA: 0x00003491 File Offset: 0x00001691
		[JsonIgnore]
		public AssetReference ScriptAsset
		{
			get
			{
				return this.scriptAsset;
			}
			set
			{
				this.scriptAsset = value;
			}
		}

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x060000F0 RID: 240 RVA: 0x0000349A File Offset: 0x0000169A
		[JsonIgnore]
		public AssetReference PrefabAsset
		{
			get
			{
				return this.prefabBundle;
			}
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x060000F1 RID: 241 RVA: 0x000034A2 File Offset: 0x000016A2
		// (set) Token: 0x060000F2 RID: 242 RVA: 0x000034AA File Offset: 0x000016AA
		[JsonIgnore]
		public int IconFileInstanceId
		{
			get
			{
				return this.iconFileInstanceId;
			}
			set
			{
				this.iconFileInstanceId = value;
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x060000F3 RID: 243 RVA: 0x000034B3 File Offset: 0x000016B3
		[JsonIgnore]
		public IReadOnlyCollection<PropLocationOffset> PropLocationOffsets
		{
			get
			{
				return this.propLocationOffsets;
			}
		}

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x060000F4 RID: 244 RVA: 0x000034BC File Offset: 0x000016BC
		// (set) Token: 0x060000F5 RID: 245 RVA: 0x000034E4 File Offset: 0x000016E4
		[JsonIgnore]
		public bool ApplyXOffset
		{
			get
			{
				return this.applyXOffset && this.Bounds.x > 1;
			}
			set
			{
				this.applyXOffset = value;
			}
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x060000F6 RID: 246 RVA: 0x000034F0 File Offset: 0x000016F0
		// (set) Token: 0x060000F7 RID: 247 RVA: 0x00003518 File Offset: 0x00001718
		[JsonIgnore]
		public bool ApplyZOffset
		{
			get
			{
				return this.applyZOffset && this.Bounds.z > 1;
			}
			set
			{
				this.applyZOffset = value;
			}
		}

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x060000F8 RID: 248 RVA: 0x00003521 File Offset: 0x00001721
		[JsonIgnore]
		public Vector3Int Bounds
		{
			get
			{
				if (this.bounds.x == 0 && this.bounds.y == 0 && this.bounds.z == 0)
				{
					this.bounds = this.GetBoundingSize();
				}
				return this.bounds;
			}
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x060000F9 RID: 249 RVA: 0x0000355C File Offset: 0x0000175C
		[JsonIgnore]
		public bool OpenSource
		{
			get
			{
				return this.openSource;
			}
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x060000FA RID: 250 RVA: 0x00003564 File Offset: 0x00001764
		[JsonIgnore]
		public bool HasScript
		{
			get
			{
				return this.scriptAsset != null && this.scriptAsset.AssetID != SerializableGuid.Empty;
			}
		}

		// Token: 0x060000FB RID: 251 RVA: 0x00003590 File Offset: 0x00001790
		public string GetPropMetaData(string key)
		{
			return this.propMetaData.GetEntry(key);
		}

		// Token: 0x060000FC RID: 252 RVA: 0x0000359E File Offset: 0x0000179E
		public void SetPropMetaData(string key, string value)
		{
			this.propMetaData.SetEntry(key, value);
		}

		// Token: 0x060000FD RID: 253 RVA: 0x000035B0 File Offset: 0x000017B0
		public List<string> RemapTransforms(List<string> transforms)
		{
			if (this.transformRemaps.Count > 0)
			{
				if (this.remapDictionary == null)
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					foreach (ValueTuple<string, string> valueTuple in this.transformRemaps)
					{
						dictionary.Add(valueTuple.Item1, valueTuple.Item2);
					}
				}
				for (int i = 0; i < transforms.Count; i++)
				{
					string text;
					if (this.remapDictionary.TryGetValue(transforms[i], out text))
					{
						transforms[i] = text;
					}
				}
			}
			return transforms;
		}

		// Token: 0x060000FE RID: 254 RVA: 0x0000365C File Offset: 0x0000185C
		public void SetComponentIds(SerializableGuid baseType, List<string> components)
		{
			this.baseTypeId = baseType;
			this.componentIds = components;
		}

		// Token: 0x060000FF RID: 255 RVA: 0x00003671 File Offset: 0x00001871
		public Prop Clone()
		{
			Prop prop = JsonConvert.DeserializeObject<Prop>(JsonConvert.SerializeObject(this));
			prop.CleanupProp();
			return prop;
		}

		// Token: 0x06000100 RID: 256 RVA: 0x00003684 File Offset: 0x00001884
		public void AddLocationOffset(PropLocationOffset offset)
		{
			if (this.propLocationOffsets == null)
			{
				this.propLocationOffsets = Array.Empty<PropLocationOffset>();
			}
			List<PropLocationOffset> list = this.propLocationOffsets.ToList<PropLocationOffset>();
			list.Add(offset);
			this.propLocationOffsets = list.ToArray();
		}

		// Token: 0x06000101 RID: 257 RVA: 0x000036C4 File Offset: 0x000018C4
		private void CleanupProp()
		{
			List<AssetReference> list = this.visualAssets;
			if (list != null && list.Count > 0)
			{
				for (int i = this.visualAssets.Count - 1; i >= 0; i--)
				{
					if (this.visualAssets[i] != null && string.IsNullOrEmpty(this.visualAssets[i].AssetID))
					{
						this.visualAssets.Remove(this.visualAssets[i]);
					}
				}
			}
			if (this.scriptAsset != null && string.IsNullOrEmpty(this.scriptAsset.AssetID))
			{
				this.scriptAsset = null;
			}
			if (this.prefabBundle != null && string.IsNullOrEmpty(this.prefabBundle.AssetID))
			{
				this.prefabBundle = null;
			}
		}

		// Token: 0x06000102 RID: 258 RVA: 0x0000378E File Offset: 0x0000198E
		public override object GetAnonymousObjectForUpload()
		{
			Prop prop = JsonConvert.DeserializeObject<Prop>(JsonConvert.SerializeObject(this));
			prop.CleanupProp();
			prop.AssetVersion = string.Empty;
			return prop;
		}

		// Token: 0x06000103 RID: 259 RVA: 0x000037AC File Offset: 0x000019AC
		public Vector3Int GetBoundingSize()
		{
			Vector3[] allOffsets = this.GetAllOffsets();
			Vector3Int vector3Int = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
			Vector3Int vector3Int2 = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
			for (int i = 0; i < allOffsets.Length; i++)
			{
				vector3Int2.x = ((allOffsets[i].x < (float)vector3Int2.x) ? Mathf.FloorToInt(allOffsets[i].x) : vector3Int2.x);
				vector3Int2.y = ((allOffsets[i].y < (float)vector3Int2.y) ? Mathf.FloorToInt(allOffsets[i].y) : vector3Int2.y);
				vector3Int2.z = ((allOffsets[i].z < (float)vector3Int2.z) ? Mathf.FloorToInt(allOffsets[i].z) : vector3Int2.z);
				vector3Int.x = ((allOffsets[i].x > (float)vector3Int.x) ? Mathf.FloorToInt(allOffsets[i].x) : vector3Int.x);
				vector3Int.y = ((allOffsets[i].y > (float)vector3Int.y) ? Mathf.FloorToInt(allOffsets[i].y) : vector3Int.y);
				vector3Int.z = ((allOffsets[i].z > (float)vector3Int.z) ? Mathf.FloorToInt(allOffsets[i].z) : vector3Int.z);
			}
			vector3Int.x = Mathf.Max(vector3Int.x - vector3Int2.x, 0) + 1;
			vector3Int.y = Mathf.Max(vector3Int.y - vector3Int2.y, 0) + 1;
			vector3Int.z = Mathf.Max(vector3Int.z - vector3Int2.z, 0) + 1;
			return vector3Int;
		}

		// Token: 0x06000104 RID: 260 RVA: 0x000039A8 File Offset: 0x00001BA8
		public Vector3[] GetLocalCellPositions()
		{
			if (this.PropLocationOffsets == null)
			{
				return new Vector3[] { Vector3.zero };
			}
			Vector3[] allOffsets = this.GetAllOffsets();
			Vector3 vector = new Vector3(this.applyXOffset ? (-0.5f) : 0f, 0f, this.applyZOffset ? (-0.5f) : 0f);
			for (int i = 0; i < this.PropLocationOffsets.Count; i++)
			{
				Vector3[] array = allOffsets;
				int num = i;
				array[num].x = array[num].x + vector.x;
				Vector3[] array2 = allOffsets;
				int num2 = i;
				array2[num2].y = array2[num2].y + vector.y;
				Vector3[] array3 = allOffsets;
				int num3 = i;
				array3[num3].z = array3[num3].z + vector.z;
			}
			return allOffsets;
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00003A64 File Offset: 0x00001C64
		private Vector3[] GetAllOffsets()
		{
			if (this.PropLocationOffsets != null)
			{
				Vector3[] array = new Vector3[this.PropLocationOffsets.Count];
				for (int i = 0; i < this.PropLocationOffsets.Count; i++)
				{
					array[i] = this.PropLocationOffsets.ElementAt(i).Offset;
				}
				return array;
			}
			return new Vector3[] { Vector3.zero };
		}

		// Token: 0x06000106 RID: 262 RVA: 0x00003AD1 File Offset: 0x00001CD1
		public void SetPropLocationOffsets(PropLocationOffset[] newPropLocationOffsets)
		{
			this.propLocationOffsets = newPropLocationOffsets;
		}

		// Token: 0x0400009F RID: 159
		public static readonly SemanticVersion INTERNAL_VERSION = new SemanticVersion(1, 0, 0);

		// Token: 0x040000A0 RID: 160
		[SerializeField]
		[JsonProperty]
		private string baseTypeId = SerializableGuid.Empty;

		// Token: 0x040000A1 RID: 161
		[SerializeField]
		[JsonProperty]
		private List<string> componentIds = new List<string>();

		// Token: 0x040000A2 RID: 162
		[SerializeField]
		[JsonProperty]
		private AssetReference scriptAsset;

		// Token: 0x040000A3 RID: 163
		[SerializeField]
		[JsonProperty]
		private AssetReference prefabBundle;

		// Token: 0x040000A4 RID: 164
		[SerializeField]
		[JsonProperty]
		private List<AssetReference> visualAssets;

		// Token: 0x040000A5 RID: 165
		[SerializeField]
		[JsonProperty]
		private int iconFileInstanceId;

		// Token: 0x040000A6 RID: 166
		[SerializeField]
		[JsonProperty]
		private PropLocationOffset[] propLocationOffsets;

		// Token: 0x040000A7 RID: 167
		[SerializeField]
		[JsonProperty]
		private bool applyXOffset = true;

		// Token: 0x040000A8 RID: 168
		[SerializeField]
		[JsonProperty]
		private bool applyZOffset = true;

		// Token: 0x040000A9 RID: 169
		[SerializeField]
		[JsonProperty]
		private StringPairDictionary propMetaData;

		// Token: 0x040000AA RID: 170
		[SerializeField]
		[JsonProperty]
		private bool openSource;

		// Token: 0x040000AB RID: 171
		[JsonIgnore]
		private Vector3Int bounds = Vector3Int.zero;

		// Token: 0x040000AC RID: 172
		[TupleElementNames(new string[] { "original", "newValue" })]
		private List<ValueTuple<string, string>> transformRemaps = new List<ValueTuple<string, string>>();

		// Token: 0x040000AD RID: 173
		private Dictionary<string, string> remapDictionary;
	}
}
