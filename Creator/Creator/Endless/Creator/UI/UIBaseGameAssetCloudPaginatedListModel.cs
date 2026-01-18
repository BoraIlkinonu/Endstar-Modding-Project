using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000104 RID: 260
	public abstract class UIBaseGameAssetCloudPaginatedListModel : UIBaseAssetCloudPaginatedListModel<UIGameAsset>, IGameAssetListModel
	{
		// Token: 0x1700005D RID: 93
		// (get) Token: 0x06000423 RID: 1059 RVA: 0x00019685 File Offset: 0x00017885
		protected override UIGameAsset SkeletonData { get; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x06000424 RID: 1060 RVA: 0x0001968D File Offset: 0x0001788D
		protected override string AssetType
		{
			get
			{
				return UIBaseGameAssetCloudPaginatedListModel.AssetTypeDictionary[this.assetTypeKey];
			}
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x06000425 RID: 1061 RVA: 0x000196A0 File Offset: 0x000178A0
		protected override string AssetFilter
		{
			get
			{
				if (!this.assetTypeKey.IsAudio())
				{
					return base.AssetFilter;
				}
				return string.Concat(new string[]
				{
					"(Name: \"*",
					this.StringFilter,
					"*\", audio_category: ",
					this.assetTypeKey.ToString().ToLower(),
					", ",
					base.SortQuery,
					")"
				});
			}
		}

		// Token: 0x06000426 RID: 1062 RVA: 0x00019717 File Offset: 0x00017917
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnEnable", this);
			}
			if (this.requestOnEnableIfCountIsZero)
			{
				base.Request(null);
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x06000427 RID: 1063 RVA: 0x0001973B File Offset: 0x0001793B
		// (set) Token: 0x06000428 RID: 1064 RVA: 0x00019743 File Offset: 0x00017943
		public AssetContexts Context { get; private set; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x06000429 RID: 1065 RVA: 0x0001974C File Offset: 0x0001794C
		public GameObject GameObject
		{
			get
			{
				return base.gameObject;
			}
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x00019754 File Offset: 0x00017954
		public void Synchronize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Synchronize", this);
			}
			this.Clear(true);
			base.Request(null);
		}

		// Token: 0x0600042B RID: 1067 RVA: 0x00019777 File Offset: 0x00017977
		public void SetAssetIdsToIgnore(HashSet<SerializableGuid> assetIdsToIgnore)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetAssetIdsToIgnore", "assetIdsToIgnore", assetIdsToIgnore.Count), this);
			}
			this.assetIdsToIgnore = assetIdsToIgnore;
		}

		// Token: 0x0600042C RID: 1068 RVA: 0x000197B0 File Offset: 0x000179B0
		public void SetAssetTypeFilter(UIGameAssetTypes value, bool triggerRequest)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetAssetTypeFilter", "value", value, "triggerRequest", triggerRequest }), this);
			}
			if (this.assetTypeKey == value)
			{
				return;
			}
			this.assetTypeKey = value;
			if (triggerRequest)
			{
				this.Clear(true);
				base.Request(null);
			}
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x00019827 File Offset: 0x00017A27
		public override void Set(List<UIGameAsset> list, bool triggerEvents)
		{
			if (this.assetIdsToIgnore.Count > 0)
			{
				list.RemoveAll((UIGameAsset item) => this.assetIdsToIgnore.Contains(item.AssetID));
			}
			base.Set(list, triggerEvents);
		}

		// Token: 0x0600042E RID: 1070 RVA: 0x00019852 File Offset: 0x00017A52
		protected override UIPageRequestResult<UIGameAsset> ParseJson(string json)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("ParseJson ( json: " + json + " )", this);
			}
			return UIGameAssetDeserializer.Deserialize(json, this.assetTypeKey);
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x0001987E File Offset: 0x00017A7E
		protected override void HandleError(Exception exception)
		{
			DebugUtility.LogException(exception, this);
		}

		// Token: 0x04000421 RID: 1057
		private static readonly Dictionary<UIGameAssetTypes, string> AssetTypeDictionary = new Dictionary<UIGameAssetTypes, string>
		{
			{
				UIGameAssetTypes.Terrain,
				"terrain-tileset-cosmetic"
			},
			{
				UIGameAssetTypes.Prop,
				"prop"
			},
			{
				UIGameAssetTypes.SFX,
				"audio"
			},
			{
				UIGameAssetTypes.Ambient,
				"audio"
			},
			{
				UIGameAssetTypes.Music,
				"audio"
			}
		};

		// Token: 0x04000422 RID: 1058
		[SerializeField]
		private bool requestOnEnableIfCountIsZero;

		// Token: 0x04000423 RID: 1059
		private UIGameAssetTypes assetTypeKey = UIGameAssetTypes.Terrain;

		// Token: 0x04000424 RID: 1060
		private HashSet<SerializableGuid> assetIdsToIgnore = new HashSet<SerializableGuid>();
	}
}
