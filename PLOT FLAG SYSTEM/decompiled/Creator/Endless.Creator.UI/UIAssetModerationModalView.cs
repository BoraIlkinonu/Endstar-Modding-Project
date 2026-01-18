using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Endless.Data;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Endless.Creator.UI;

public class UIAssetModerationModalView : UIBaseModalView, IUILoadingSpinnerViewCompatible
{
	[Header("UIAssetModerationModalView")]
	[SerializeField]
	private UIGameAssetSummaryView gameAssetSummary;

	[SerializeField]
	private UIInputField reasonInputField;

	[FormerlySerializedAs("moderationFlagsDropdown")]
	[SerializeField]
	private UIModerationFlagDropdown uiModerationFlagsDropdown;

	private CancellationTokenSource getAppliedAssetModerationFlagCancellationTokenSource;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public UIGameAsset GameAsset { get; private set; }

	public List<Moderation> ExistingAssetModerations { get; } = new List<Moderation>();

	public List<ModerationFlag> ExistingAssetModerationFlags { get; } = new List<ModerationFlag>();

	private void OnDestroy()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		CancellationTokenSourceUtility.CancelAndCleanup(ref getAppliedAssetModerationFlagCancellationTokenSource);
	}

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		GameAsset = (UIGameAsset)modalData[0];
		gameAssetSummary.View(GameAsset);
		reasonInputField.Clear();
		CancellationTokenSourceUtility.RecreateTokenSource(ref getAppliedAssetModerationFlagCancellationTokenSource);
		RequestAndViewExistingAssetModerationFlagsAsync(getAppliedAssetModerationFlagCancellationTokenSource.Token);
	}

	public override void OnBack()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
	}

	public override void Close()
	{
		base.Close();
		CancellationTokenSourceUtility.CancelAndCleanup(ref getAppliedAssetModerationFlagCancellationTokenSource);
	}

	private async Task RequestAndViewExistingAssetModerationFlagsAsync(CancellationToken cancellationToken)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "RequestAndViewExistingAssetModerationFlagsAsync", cancellationToken);
		}
		OnLoadingStarted.Invoke();
		ExistingAssetModerations.Clear();
		ExistingAssetModerationFlags.Clear();
		try
		{
			if (cancellationToken.IsCancellationRequested)
			{
				throw new OperationCanceledException();
			}
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetObjectFlags(GameAsset.AssetID, debugQuery: true);
			if (graphQlResult.HasErrors)
			{
				throw graphQlResult.GetErrorMessage();
			}
			Debug.Log($"GetAssetObjectFlags response {graphQlResult.GetDataMember()}");
			var anonymousTypeObject = new
			{
				identifier = string.Empty,
				moderations = Array.Empty<Moderation>()
			};
			Moderation[] moderations = (JsonConvert.DeserializeAnonymousType(graphQlResult.GetDataMember().ToString(), anonymousTypeObject) ?? throw new NullReferenceException("Unable to parse GetAssetObjectFlags result into valid structure containing moderations.")).moderations;
			for (int i = 0; i < moderations.Length; i++)
			{
				Moderation item = moderations[i];
				ExistingAssetModerations.Add(item);
				ExistingAssetModerationFlags.Add(item.flag);
			}
			uiModerationFlagsDropdown.SetValue(ExistingAssetModerationFlags, triggerValueChanged: false);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.UIAssetModerationModalModel_RequestAndViewExistingAssetModerationFlags, exception);
		}
		finally
		{
			OnLoadingEnded.Invoke();
		}
	}
}
