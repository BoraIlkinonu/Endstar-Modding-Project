using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200010A RID: 266
	public class UIGameLibraryListModel : UIBaseLocalFilterableListModel<UIGameAsset>, IGameAssetListModel
	{
		// Token: 0x17000063 RID: 99
		// (get) Token: 0x06000440 RID: 1088 RVA: 0x00019A80 File Offset: 0x00017C80
		// (set) Token: 0x06000441 RID: 1089 RVA: 0x00019A88 File Offset: 0x00017C88
		public AssetContexts Context { get; private set; }

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x06000442 RID: 1090 RVA: 0x00019A91 File Offset: 0x00017C91
		// (set) Token: 0x06000443 RID: 1091 RVA: 0x00019A99 File Offset: 0x00017C99
		public UIGameAssetTypes AssetTypeFilter { get; private set; } = (UIGameAssetTypes)(-1);

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x06000444 RID: 1092 RVA: 0x0001974C File Offset: 0x0001794C
		public GameObject GameObject
		{
			get
			{
				return base.gameObject;
			}
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000445 RID: 1093 RVA: 0x00019AA2 File Offset: 0x00017CA2
		protected override Comparison<UIGameAsset> DefaultSort
		{
			get
			{
				return (UIGameAsset x, UIGameAsset y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal);
			}
		}

		// Token: 0x06000446 RID: 1094 RVA: 0x00019AC3 File Offset: 0x00017CC3
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnRepopulate += this.Synchronize;
			this.Synchronize();
		}

		// Token: 0x06000447 RID: 1095 RVA: 0x00019AFA File Offset: 0x00017CFA
		protected void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnRepopulate -= this.Synchronize;
		}

		// Token: 0x06000448 RID: 1096 RVA: 0x00019B2B File Offset: 0x00017D2B
		public void SetAssetIdsToIgnore(HashSet<SerializableGuid> assetIdsToIgnore)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetAssetIdsToIgnore", new object[] { assetIdsToIgnore.Count });
			}
			this.assetIdsToIgnore = assetIdsToIgnore;
		}

		// Token: 0x06000449 RID: 1097 RVA: 0x00019B5B File Offset: 0x00017D5B
		public void SetAssetTypeFilter(UIGameAssetTypes value, bool triggerEvents)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetAssetTypeFilter", new object[] { value, triggerEvents });
			}
			this.AssetTypeFilter = value;
			if (triggerEvents)
			{
				this.Synchronize();
			}
		}

		// Token: 0x0600044A RID: 1098 RVA: 0x00019B98 File Offset: 0x00017D98
		public void Synchronize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Synchronize", Array.Empty<object>());
			}
			List<UIGameAsset> list = new List<UIGameAsset>();
			if (this.FilterAllows(UIGameAssetTypes.Terrain))
			{
				foreach (Tileset tileset in ((IEnumerable<Tileset>)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.RuntimePalette.DistinctTilesets))
				{
					if (base.VerboseLogging)
					{
						DebugUtility.Log("tileset: " + JsonUtility.ToJson(tileset), this);
					}
					if (!(tileset is FallbackTileset) || !(tileset.Asset.AssetID == SerializableGuid.Empty))
					{
						UIGameAsset uigameAsset = new UIGameAsset(tileset);
						list.Add(uigameAsset);
					}
				}
			}
			if (this.FilterAllows(UIGameAssetTypes.Prop))
			{
				foreach (AssetReference assetReference in MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.PropReferences)
				{
					PropLibrary.RuntimePropInfo runtimePropInfo;
					if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetReference.AssetID, out runtimePropInfo))
					{
						UIGameAsset uigameAsset2 = new UIGameAsset(runtimePropInfo);
						list.Add(uigameAsset2);
					}
				}
			}
			bool flag = this.FilterAllows(UIGameAssetTypes.Music);
			bool flag2 = this.FilterAllows(UIGameAssetTypes.SFX);
			bool flag3 = this.FilterAllows(UIGameAssetTypes.Ambient);
			if (flag || flag2 || flag3)
			{
				foreach (AssetReference assetReference2 in MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.AudioReferences)
				{
					RuntimeAudioInfo runtimeAudioInfo;
					if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(assetReference2, out runtimeAudioInfo))
					{
						bool flag4;
						switch (runtimeAudioInfo.AudioAsset.AudioCategory)
						{
						case AudioCategory.Music:
							flag4 = flag;
							break;
						case AudioCategory.SFX:
							flag4 = flag2;
							break;
						case AudioCategory.Ambient:
							flag4 = flag3;
							break;
						default:
							throw new ArgumentOutOfRangeException();
						}
						if (flag4)
						{
							UIGameAsset uigameAsset3 = new UIGameAsset(runtimeAudioInfo);
							list.Add(uigameAsset3);
						}
					}
				}
			}
			this.Set(list, true);
		}

		// Token: 0x0600044B RID: 1099 RVA: 0x00019DCC File Offset: 0x00017FCC
		public override void Set(List<UIGameAsset> list, bool triggerEvents)
		{
			if (this.assetIdsToIgnore.Count > 0)
			{
				for (int i = list.Count - 1; i >= 0; i--)
				{
					if (this.assetIdsToIgnore.Contains(list[i].AssetID))
					{
						list.RemoveAt(i);
					}
				}
			}
			base.Set(list, triggerEvents);
		}

		// Token: 0x0600044C RID: 1100 RVA: 0x00019E27 File Offset: 0x00018027
		private bool FilterAllows(UIGameAssetTypes typeToCheck)
		{
			return this.AssetTypeFilter == UIGameAssetTypes.None || (this.AssetTypeFilter & typeToCheck) == typeToCheck;
		}

		// Token: 0x0400042E RID: 1070
		private HashSet<SerializableGuid> assetIdsToIgnore = new HashSet<SerializableGuid>();
	}
}
