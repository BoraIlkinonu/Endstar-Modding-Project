using System;
using System.Collections.Generic;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIPublishController : UIGameObject, IUILoadingSpinnerViewCompatible
{
	[SerializeField]
	private UIPublishModel model;

	[SerializeField]
	private UIPublishView view;

	[SerializeField]
	private UIDropdownVersion betaDropdown;

	[SerializeField]
	private UIDropdownVersion publicDropdown;

	[SerializeField]
	[TextArea]
	private string publishConfirmationBeta = "Publishing will make your game available to all players in the " + UIPublishStates.Beta.ToEndlessCloudServicesCompatibleString() + " channel.";

	[SerializeField]
	[TextArea]
	private string publishConfirmationPublic = "Publishing will make your game available to all players in the " + UIPublishStates.Public.ToEndlessCloudServicesCompatibleString() + " channel.";

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private readonly Dictionary<UIPublishStates, string> confirmationDictionary = new Dictionary<UIPublishStates, string>();

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		betaDropdown.OnValueChanged.AddListener(PublishBeta);
		publicDropdown.OnValueChanged.AddListener(PublishPublic);
		confirmationDictionary.Add(UIPublishStates.Beta, publishConfirmationBeta);
		confirmationDictionary.Add(UIPublishStates.Public, publishConfirmationPublic);
	}

	private void PublishBeta()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "PublishBeta");
		}
		SetVersion(UIPublishStates.Beta, betaDropdown.IndexOfValue);
	}

	private void PublishPublic()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "PublishPublic");
		}
		SetVersion(UIPublishStates.Public, publicDropdown.IndexOfValue);
	}

	private void SetVersion(UIPublishStates publishingState, int dropdownIndex)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetVersion", publishingState, dropdownIndex);
		}
		string text = model.Versions[dropdownIndex];
		string version = model.GetVersion(publishingState);
		bool flag = text == UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString();
		bool flag2 = !flag && version != UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString();
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}", "versionToPublish", text, "activeVersion", version, "isUnpublish", flag, "needsUnpublishingBeforePublish", flag2), this);
		}
		if (text == version)
		{
			return;
		}
		OnLoadingStarted.Invoke();
		if (flag)
		{
			Unpublish(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID, version, delegate
			{
				OnLoadingEnded.Invoke();
				model.Synchronize();
			});
		}
		else
		{
			ConfirmPublish(publishingState, text, flag2);
		}
	}

	private void ConfirmPublish(UIPublishStates publishingState, string version, bool needsUnpublishing)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ConfirmPublish", publishingState, version, needsUnpublishing);
		}
		string assetId = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID;
		string body = "Set v" + version + " to " + publishingState.ToEndlessCloudServicesCompatibleString() + "?\n" + confirmationDictionary[publishingState];
		MonoBehaviourSingleton<UIModalManager>.Instance.Confirm(body, delegate
		{
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack(false);
			if (needsUnpublishing)
			{
				string version2 = model.GetVersion(publishingState);
				Unpublish(assetId, version2, delegate
				{
					Publish(assetId, version, publishingState);
				});
			}
			else
			{
				Publish(assetId, version, publishingState);
			}
		}, delegate
		{
			OnLoadingEnded.Invoke();
			model.Synchronize();
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		});
	}

	private async void Unpublish(string assetId, string version, Action onSuccess)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Unpublish", assetId, version, onSuccess.DebugIsNull());
		}
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.SetPublishStateOnAssetAsync(assetId, version, UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString());
		if (graphQlResult.HasErrors)
		{
			OnLoadingEnded.Invoke();
			ErrorHandler.HandleError(ErrorCodes.UIPublishController_UnpublishAsset, graphQlResult.GetErrorMessage());
			model.Synchronize();
		}
		else
		{
			onSuccess?.Invoke();
		}
	}

	private async void Publish(string assetId, string version, UIPublishStates publishingState)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Publish", assetId, version, publishingState);
		}
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.SetPublishStateOnAssetAsync(assetId, version, publishingState.ToEndlessCloudServicesCompatibleString());
		if (graphQlResult.HasErrors)
		{
			OnLoadingEnded.Invoke();
			ErrorHandler.HandleError(ErrorCodes.UIPublishController_PublishAsset, graphQlResult.GetErrorMessage());
			model.Synchronize();
			return;
		}
		if (verboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "Publish", "Success!", assetId, version, publishingState);
		}
		OnLoadingEnded.Invoke();
		model.Synchronize();
	}
}
