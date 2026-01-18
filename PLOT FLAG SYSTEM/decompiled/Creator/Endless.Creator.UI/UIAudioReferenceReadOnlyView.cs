using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIAudioReferenceReadOnlyView : UIBaseView<AudioReference, UIAudioReferenceView.Styles>, IUILoadingSpinnerViewCompatible
{
	protected enum RequestType
	{
		SkeletonLoading
	}

	[Header("Visuals")]
	[SerializeField]
	private GameObject skeletonLoadingVisual;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[Header("Icon")]
	[SerializeField]
	private UIFileInstanceTexture2DView fileInstanceTexture2D;

	private readonly HashSet<RequestType> requestsInProgress = new HashSet<RequestType>();

	private string assetId;

	public override UIAudioReferenceView.Styles Style { get; protected set; } = UIAudioReferenceView.Styles.ReadOnly;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public override void View(AudioReference model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		if (model == null || model.IsReferenceEmpty())
		{
			ShowSkeleton();
			return;
		}
		HideSkeleton();
		ViewAudioInformation(model);
	}

	private void ViewAudioInformation(AudioReference model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewAudioInformation", model);
		}
		displayNameText.text = string.Empty;
		fileInstanceTexture2D.enabled = false;
		SerializableGuid id = InspectorReferenceUtility.GetId(model);
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(id, out var metadata))
		{
			assetId = metadata.AudioAsset.AssetID;
			displayNameText.text = metadata.AudioAsset.Name;
			fileInstanceTexture2D.enabled = true;
			if ((bool)metadata.Icon)
			{
				fileInstanceTexture2D.View(metadata.Icon);
			}
			else
			{
				fileInstanceTexture2D.View(metadata.AudioAsset.IconFileInstanceId);
			}
		}
		else
		{
			displayNameText.text = "Missing";
			DebugUtility.LogWarning($"Could not find audio info for {id}", this);
		}
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		fileInstanceTexture2D.enabled = false;
		fileInstanceTexture2D.Clear();
		UntrackAllRequests();
	}

	protected void TrackRequest(RequestType request)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "TrackRequest", request);
		}
		if (!requestsInProgress.Add(request))
		{
			DebugUtility.LogWarning($"{request} is already in progress.", this);
		}
		else if (requestsInProgress.Count == 1)
		{
			OnLoadingStarted.Invoke();
		}
	}

	protected void UntrackRequest(RequestType request)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "UntrackRequest", request);
		}
		if (requestsInProgress.Remove(request) && requestsInProgress.Count == 0)
		{
			OnLoadingEnded.Invoke();
		}
	}

	private void UntrackAllRequests()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "UntrackAllRequests");
		}
		foreach (RequestType item in requestsInProgress.ToList())
		{
			UntrackRequest(item);
		}
	}

	private void ShowSkeleton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ShowSkeleton");
		}
		TrackRequest(RequestType.SkeletonLoading);
		skeletonLoadingVisual.SetActive(value: true);
	}

	private void HideSkeleton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HideSkeleton");
		}
		if (skeletonLoadingVisual.activeSelf)
		{
			skeletonLoadingVisual.SetActive(value: false);
			UntrackRequest(RequestType.SkeletonLoading);
		}
	}
}
