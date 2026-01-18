using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.TerrainCosmetics
{
	// Token: 0x0200000F RID: 15
	[CreateAssetMenu(menuName = "Terrain/Tileset/Cosmetic Profile")]
	public class TilesetCosmeticProfile : ScriptableObject
	{
		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000039 RID: 57 RVA: 0x000025F0 File Offset: 0x000007F0
		public InheritanceState TopDecorationInheritanceState
		{
			get
			{
				return this.topDecorationInheritance;
			}
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x0600003A RID: 58 RVA: 0x000025F8 File Offset: 0x000007F8
		public InheritanceState SideDecorationInheritance
		{
			get
			{
				return this.sideDecorationInheritance;
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x0600003B RID: 59 RVA: 0x00002600 File Offset: 0x00000800
		public InheritanceState BottomDecorationInheritance
		{
			get
			{
				return this.bottomDecorationInheritance;
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600003C RID: 60 RVA: 0x00002608 File Offset: 0x00000808
		public InheritanceState MaterialVariantTablesInheritance
		{
			get
			{
				return this.materialVariantTablesInheritance;
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600003D RID: 61 RVA: 0x00002610 File Offset: 0x00000810
		public InheritanceState TileCosmeticInheritance
		{
			get
			{
				return this.tileCosmeticInheritance;
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x0600003E RID: 62 RVA: 0x00002618 File Offset: 0x00000818
		public InheritanceState BaseFringeSetsInheritance
		{
			get
			{
				return this.baseFringeSetsInheritance;
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x0600003F RID: 63 RVA: 0x00002620 File Offset: 0x00000820
		public InheritanceState SlopeFringeSetsInheritance
		{
			get
			{
				return this.slopeFringeSetsInheritance;
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x06000040 RID: 64 RVA: 0x00002628 File Offset: 0x00000828
		// (set) Token: 0x06000041 RID: 65 RVA: 0x00002630 File Offset: 0x00000830
		public List<Material> Materials { get; private set; } = new List<Material>();

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000042 RID: 66 RVA: 0x00002639 File Offset: 0x00000839
		public TileCosmetic[] TileCosmetics
		{
			get
			{
				return this.tileCosmetics;
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000043 RID: 67 RVA: 0x00002641 File Offset: 0x00000841
		public bool OpenSource
		{
			get
			{
				return this.openSource;
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000044 RID: 68 RVA: 0x00002649 File Offset: 0x00000849
		public SerializableGuid TilesetId
		{
			get
			{
				return this.tilesetId;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000045 RID: 69 RVA: 0x00002651 File Offset: 0x00000851
		// (set) Token: 0x06000046 RID: 70 RVA: 0x00002659 File Offset: 0x00000859
		public SerializableGuid InheritedTilesetId
		{
			get
			{
				return this.inheritedTilesetId;
			}
			set
			{
				this.inheritedTilesetId = value;
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x06000047 RID: 71 RVA: 0x00002662 File Offset: 0x00000862
		public TilesetType TilesetType
		{
			get
			{
				return this.tilesetType;
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x06000048 RID: 72 RVA: 0x0000266A File Offset: 0x0000086A
		public string DisplayName
		{
			get
			{
				return this.displayName;
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000049 RID: 73 RVA: 0x00002672 File Offset: 0x00000872
		public string Description
		{
			get
			{
				return this.description;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x0600004A RID: 74 RVA: 0x0000267A File Offset: 0x0000087A
		public Sprite DisplayIcon
		{
			get
			{
				return this.displayIcon;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x0600004B RID: 75 RVA: 0x00002682 File Offset: 0x00000882
		public SerializableGuid[] ConsideredTilesetIds
		{
			get
			{
				return this.consideredTilesetIds.Append(this.tilesetId).ToArray<SerializableGuid>();
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x0600004C RID: 76 RVA: 0x0000269A File Offset: 0x0000089A
		public MaterialWeightTableAsset[] MaterialVariantWeightTables
		{
			get
			{
				return this.materialVariantWeightTables;
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x0600004D RID: 77 RVA: 0x000026A2 File Offset: 0x000008A2
		public DecorationSet TopDecorationSet
		{
			get
			{
				return this.topDecorationSet;
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x0600004E RID: 78 RVA: 0x000026AA File Offset: 0x000008AA
		public DecorationSet SideDecorationSet
		{
			get
			{
				return this.sideDecorationSet;
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x0600004F RID: 79 RVA: 0x000026B2 File Offset: 0x000008B2
		public DecorationSet BottomDecorationSet
		{
			get
			{
				return this.bottomDecorationSet;
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x06000050 RID: 80 RVA: 0x000026BA File Offset: 0x000008BA
		public BaseFringeCosmeticProfile[] BaseFringeSets
		{
			get
			{
				return this.baseFringeSets;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000051 RID: 81 RVA: 0x000026C2 File Offset: 0x000008C2
		public SlopeFringeCosmeticProfile[] SlopeFringeSets
		{
			get
			{
				return this.slopeFringeSets;
			}
		}

		// Token: 0x06000052 RID: 82 RVA: 0x000026CA File Offset: 0x000008CA
		public TileCosmetic GetTileCosmeticAtIndex(int index)
		{
			if (index < 0 || index >= this.tileCosmetics.Length)
			{
				return this.missingTileReference;
			}
			if (this.tileCosmetics[index] != null)
			{
				return this.tileCosmetics[index];
			}
			return this.missingTileReference;
		}

		// Token: 0x06000053 RID: 83 RVA: 0x000026FB File Offset: 0x000008FB
		public void ApplyTileCosmetics(TileCosmetic[] newCosmetics)
		{
			this.tileCosmetics = newCosmetics;
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00002704 File Offset: 0x00000904
		public void ApplyTopDecorationSet(DecorationSet decorationSet)
		{
			this.topDecorationSet = decorationSet;
		}

		// Token: 0x06000055 RID: 85 RVA: 0x0000270D File Offset: 0x0000090D
		public void ApplySideDecorationSet(DecorationSet decorationSet)
		{
			this.sideDecorationSet = decorationSet;
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00002716 File Offset: 0x00000916
		public void ApplyBottomDecorationSet(DecorationSet decorationSet)
		{
			this.bottomDecorationSet = decorationSet;
		}

		// Token: 0x06000057 RID: 87 RVA: 0x0000271F File Offset: 0x0000091F
		public void ApplyMaterialToIndex(MaterialWeightTableAsset materialTable, int index)
		{
			this.materialVariantWeightTables[index] = materialTable;
		}

		// Token: 0x06000058 RID: 88 RVA: 0x0000272C File Offset: 0x0000092C
		public List<Material> GetMaterialsOnTiles()
		{
			HashSet<Material> hashSet = new HashSet<Material>();
			TileCosmetic[] array = this.tileCosmetics;
			if (array == null || array.Length <= 0)
			{
				return hashSet.ToList<Material>();
			}
			array = this.tileCosmetics;
			for (int i = 0; i < array.Length; i++)
			{
				using (List<Transform>.Enumerator enumerator = array[i].Visuals.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						MeshRenderer meshRenderer;
						if (enumerator.Current.TryGetComponent<MeshRenderer>(out meshRenderer))
						{
							foreach (Material material in meshRenderer.sharedMaterials)
							{
								hashSet.Add(material);
							}
						}
					}
				}
			}
			return hashSet.ToList<Material>();
		}

		// Token: 0x06000059 RID: 89 RVA: 0x000027E8 File Offset: 0x000009E8
		public void ApplyBaseFringeSets(BaseFringeCosmeticProfile[] inheritedBaseFringeSets)
		{
			this.baseFringeSets = inheritedBaseFringeSets;
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000027F1 File Offset: 0x000009F1
		public void ApplySlopeFringeSets(SlopeFringeCosmeticProfile[] inheritedSlopeFringeSets)
		{
			this.slopeFringeSets = inheritedSlopeFringeSets;
		}

		// Token: 0x0600005B RID: 91 RVA: 0x000027FA File Offset: 0x000009FA
		public void CopyTileCosmetics(TileCosmetic[] cosmetics)
		{
			this.tileCosmetics = cosmetics.Select((TileCosmetic cosmetic) => cosmetic.Copy()).ToArray<TileCosmetic>();
		}

		// Token: 0x0400002D RID: 45
		[SerializeField]
		private SerializableGuid inheritedTilesetId = SerializableGuid.Empty;

		// Token: 0x0400002E RID: 46
		[SerializeField]
		private SerializableGuid tilesetId = SerializableGuid.Empty;

		// Token: 0x0400002F RID: 47
		[SerializeField]
		private TilesetType tilesetType;

		// Token: 0x04000030 RID: 48
		[SerializeField]
		private string displayName;

		// Token: 0x04000031 RID: 49
		[SerializeField]
		private string description;

		// Token: 0x04000032 RID: 50
		[SerializeField]
		private Sprite displayIcon;

		// Token: 0x04000033 RID: 51
		[SerializeField]
		private TileCosmetic[] tileCosmetics;

		// Token: 0x04000034 RID: 52
		[SerializeField]
		[Tooltip("Tilesets that will be included when validating rules. Always considers itself.")]
		private List<SerializableGuid> consideredTilesetIds;

		// Token: 0x04000035 RID: 53
		[SerializeField]
		private TileCosmetic missingTileReference;

		// Token: 0x04000036 RID: 54
		[SerializeField]
		private MaterialWeightTableAsset[] materialVariantWeightTables;

		// Token: 0x04000037 RID: 55
		[SerializeField]
		private DecorationSet topDecorationSet;

		// Token: 0x04000038 RID: 56
		[SerializeField]
		private DecorationSet sideDecorationSet;

		// Token: 0x04000039 RID: 57
		[SerializeField]
		private DecorationSet bottomDecorationSet;

		// Token: 0x0400003A RID: 58
		[SerializeField]
		private BaseFringeCosmeticProfile[] baseFringeSets;

		// Token: 0x0400003B RID: 59
		[SerializeField]
		private SlopeFringeCosmeticProfile[] slopeFringeSets;

		// Token: 0x0400003C RID: 60
		[SerializeField]
		private bool openSource;

		// Token: 0x0400003D RID: 61
		[SerializeField]
		[HideInInspector]
		private List<string> materialBaseNames = new List<string>();

		// Token: 0x0400003E RID: 62
		[SerializeField]
		[HideInInspector]
		private InheritanceState topDecorationInheritance = InheritanceState.None;

		// Token: 0x0400003F RID: 63
		[SerializeField]
		[HideInInspector]
		private InheritanceState sideDecorationInheritance = InheritanceState.None;

		// Token: 0x04000040 RID: 64
		[SerializeField]
		[HideInInspector]
		private InheritanceState bottomDecorationInheritance = InheritanceState.None;

		// Token: 0x04000041 RID: 65
		[SerializeField]
		[HideInInspector]
		private InheritanceState materialVariantTablesInheritance = InheritanceState.None;

		// Token: 0x04000042 RID: 66
		[SerializeField]
		[HideInInspector]
		private InheritanceState tileCosmeticInheritance = InheritanceState.New;

		// Token: 0x04000043 RID: 67
		[SerializeField]
		[HideInInspector]
		private InheritanceState baseFringeSetsInheritance = InheritanceState.None;

		// Token: 0x04000044 RID: 68
		[SerializeField]
		[HideInInspector]
		private InheritanceState slopeFringeSetsInheritance = InheritanceState.None;
	}
}
