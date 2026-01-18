using System;
using System.Collections.Generic;
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

namespace Runtime.Creator.UI;

public abstract class UIAssetList<TAssetList, TUIModel> : MonoBehaviour, IUILoadingSpinnerViewCompatible where TAssetList : AssetList
{
	[SerializeField]
	private NetworkEnvironmentAssetListDictionary<TAssetList> assetListScriptableObject;

	[SerializeField]
	private UIText nameText;

	[SerializeField]
	private UITooltip descriptionTooltip;

	[SerializeField]
	private UIBaseListModel<TUIModel> listModel;

	private AssetListScriptableObject<TAssetList> model;

	protected bool VerboseLogging { get; set; }

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	private void Start()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("Start", this);
		}
		model = assetListScriptableObject[MatchmakingClientController.Instance.NetworkEnv];
		bool flag = !model.Description.IsNullOrEmptyOrWhiteSpace();
		nameText.TextMeshPro.color = (flag ? UIColors.AzureRadiance : Color.white);
		nameText.TextMeshPro.fontStyle = (flag ? FontStyles.Underline : FontStyles.Normal);
		nameText.Value = (flag ? ("<color=white>" + model.Name + "</color>") : model.Name);
		descriptionTooltip.SetTooltip(model.Description);
		descriptionTooltip.ShouldShow = flag;
		LoadAssetListContent();
	}

	private async Task LoadAssetListContent()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("LoadAssetListContent", this);
		}
		OnLoadingStarted.Invoke();
		List<(AssetCore, List<PublishedVersion>)> list = await model.GetData();
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "result", list.Count), this);
		}
		try
		{
			List<TUIModel> list2 = await ConvertToUiModelsAsync(list);
			if (VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "models", list2.Count), this);
			}
			listModel.Set(list2, triggerEvents: true);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
		finally
		{
			OnLoadingEnded.Invoke();
		}
	}

	protected abstract Task<List<TUIModel>> ConvertToUiModelsAsync(List<(AssetCore asset, List<PublishedVersion> versions)> result);
}
