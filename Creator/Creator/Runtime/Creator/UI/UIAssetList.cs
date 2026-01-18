using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Data.UI;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.Creator.UI
{
	// Token: 0x02000009 RID: 9
	public abstract class UIAssetList<TAssetList, TUIModel> : MonoBehaviour, IUILoadingSpinnerViewCompatible where TAssetList : AssetList
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600001F RID: 31 RVA: 0x00002586 File Offset: 0x00000786
		// (set) Token: 0x06000020 RID: 32 RVA: 0x0000258E File Offset: 0x0000078E
		protected bool VerboseLogging { get; set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000021 RID: 33 RVA: 0x00002597 File Offset: 0x00000797
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000022 RID: 34 RVA: 0x0000259F File Offset: 0x0000079F
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000023 RID: 35 RVA: 0x000025A8 File Offset: 0x000007A8
		private void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.model = this.assetListScriptableObject[MatchmakingClientController.Instance.NetworkEnv];
			bool flag = !this.model.Description.IsNullOrEmptyOrWhiteSpace();
			this.nameText.TextMeshPro.color = (flag ? UIColors.AzureRadiance : Color.white);
			this.nameText.TextMeshPro.fontStyle = (flag ? FontStyles.Underline : FontStyles.Normal);
			this.nameText.Value = (flag ? ("<color=white>" + this.model.Name + "</color>") : this.model.Name);
			this.descriptionTooltip.SetTooltip(this.model.Description);
			this.descriptionTooltip.ShouldShow = flag;
			this.LoadAssetListContent();
		}

		// Token: 0x06000024 RID: 36 RVA: 0x0000268C File Offset: 0x0000088C
		private async Task LoadAssetListContent()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("LoadAssetListContent", this);
			}
			this.OnLoadingStarted.Invoke();
			List<ValueTuple<AssetCore, List<PublishedVersion>>> list = await this.model.GetData();
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "result", list.Count), this);
			}
			try
			{
				List<TUIModel> list2 = await this.ConvertToUiModelsAsync(list);
				if (this.VerboseLogging)
				{
					DebugUtility.Log(string.Format("{0}: {1}", "models", list2.Count), this);
				}
				this.listModel.Set(list2, true);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex, this);
			}
			finally
			{
				this.OnLoadingEnded.Invoke();
			}
		}

		// Token: 0x06000025 RID: 37
		protected abstract Task<List<TUIModel>> ConvertToUiModelsAsync([TupleElementNames(new string[] { "asset", "versions" })] List<ValueTuple<AssetCore, List<PublishedVersion>>> result);

		// Token: 0x0400000E RID: 14
		[SerializeField]
		private NetworkEnvironmentAssetListDictionary<TAssetList> assetListScriptableObject;

		// Token: 0x0400000F RID: 15
		[SerializeField]
		private UIText nameText;

		// Token: 0x04000010 RID: 16
		[SerializeField]
		private UITooltip descriptionTooltip;

		// Token: 0x04000011 RID: 17
		[SerializeField]
		private UIBaseListModel<TUIModel> listModel;

		// Token: 0x04000012 RID: 18
		private AssetListScriptableObject<TAssetList> model;
	}
}
