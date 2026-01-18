using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIGameAssetPublishModalView : UIEscapableModalView, IUILoadingSpinnerViewCompatible
{
	private static readonly string betaKey = UIPublishStates.Beta.ToEndlessCloudServicesCompatibleString();

	private static readonly string publicKey = UIPublishStates.Public.ToEndlessCloudServicesCompatibleString();

	private static readonly string unpublishedKey = UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString();

	[Header("UIGameAssetPublishModalView")]
	[SerializeField]
	private UIDropdownVersion betaDropdown;

	[SerializeField]
	private UIDropdownVersion publicDropdown;

	private readonly string[] betaValues = new string[1] { unpublishedKey };

	private readonly string[] publicValues = new string[1] { unpublishedKey };

	private bool listenersAdded;

	public UIGameAssetPublishModalModel Model { get; private set; }

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		if (modalData.Length != 1 || !(modalData[0] is UIGameAssetPublishModalModel))
		{
			DebugUtility.LogError("Invalid modalData provided to UIGameAssetPublishModalView.", this);
			return;
		}
		Model = modalData[0] as UIGameAssetPublishModalModel;
		AddListeners();
	}

	public override void Close()
	{
		base.Close();
		RemoveListeners();
	}

	private void ViewVersionsAndPublishedVersion(List<string> versions, List<PublishedVersion> publishedVersions)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewVersionsAndPublishedVersion", versions.Count, publishedVersions.Count);
		}
		betaValues[0] = unpublishedKey;
		publicValues[0] = unpublishedKey;
		foreach (PublishedVersion publishedVersion in publishedVersions)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "publishedVersion", publishedVersion), this);
			}
			if (publishedVersion.State == betaKey)
			{
				betaValues[0] = publishedVersion.AssetVersion;
			}
			else if (publishedVersion.State == publicKey)
			{
				publicValues[0] = publishedVersion.AssetVersion;
			}
		}
		if (!versions.Contains(unpublishedKey))
		{
			versions.Insert(0, unpublishedKey);
		}
		if (base.VerboseLogging)
		{
			DebugUtility.DebugEnumerable("versions", versions, this);
			DebugUtility.DebugEnumerable("betaValues", betaValues[0], this);
			DebugUtility.DebugEnumerable("publicValues", publicValues[0], this);
		}
		betaDropdown.SetOptionsAndValue(versions, betaValues[0], triggerOnValueChanged: false);
		publicDropdown.SetOptionsAndValue(versions, publicValues[0], triggerOnValueChanged: false);
	}

	private void AddListeners()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "AddListeners");
		}
		if (!listenersAdded && Model != null)
		{
			Model.OnLoadingStarted.AddListener(OnLoadingStarted.Invoke);
			Model.OnLoadingEnded.AddListener(OnLoadingEnded.Invoke);
			Model.OnModelChanged.AddListener(ViewVersionsAndPublishedVersion);
			listenersAdded = true;
		}
	}

	private void RemoveListeners()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "RemoveListeners");
		}
		if (listenersAdded && Model != null)
		{
			Model.OnLoadingStarted.RemoveListener(OnLoadingStarted.Invoke);
			Model.OnLoadingEnded.RemoveListener(OnLoadingEnded.Invoke);
			Model.OnModelChanged.RemoveListener(ViewVersionsAndPublishedVersion);
			listenersAdded = false;
		}
	}
}
