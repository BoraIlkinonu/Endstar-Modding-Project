using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIRevisionsView : UIGameObject, IUILoadingSpinnerViewCompatible
{
	[SerializeField]
	private UIVersionListModel versionListModel;

	[SerializeField]
	private TextMeshProUGUI selectedVersionTimestampText;

	[SerializeField]
	private UIRevisionListModel revisionListModel;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private LevelState levelState;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		versionListModel.ModelChangedUnityEvent.AddListener(GetRevisions);
		versionListModel.ItemSelectedUnityEvent.AddListener(OnVersionSelected);
		selectedVersionTimestampText.enabled = false;
	}

	public void Initialize(LevelState levelState)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", levelState.AssetID);
		}
		this.levelState = levelState;
		versionListModel.Initialize(levelState.AssetID);
	}

	private void GetRevisions()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetRevisions");
		}
		if (versionListModel.Count == 0)
		{
			if (verboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "GetRevisions", "There are no versions to get revisions from!");
			}
			return;
		}
		if (versionListModel.SelectedTypedList.Count == 0)
		{
			DebugUtility.LogException(new Exception("versionListModel.SelectedTypedList.Count is 0!"), this);
			return;
		}
		string text = versionListModel.SelectedTypedList[0];
		if (verboseLogging)
		{
			DebugUtility.Log("version: " + text, this);
		}
		revisionListModel.Initialize(levelState, text);
	}

	private void OnVersionSelected(int index)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnVersionSelected", index);
		}
		OnLoadingStarted.Invoke();
		selectedVersionTimestampText.enabled = false;
		string version = versionListModel.SelectedTypedList[0];
		versionListModel.GetTimestampAsync(version, ViewTimestamp);
	}

	private void ViewTimestamp(string version, DateTime timestamp)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewTimestamp", version, timestamp);
		}
		if (versionListModel.ReadOnlySelectedList.Count != 0 && !(versionListModel.SelectedTypedList[0] != version))
		{
			selectedVersionTimestampText.text = timestamp.ToShortDateString() + " at " + timestamp.ToShortTimeString();
			selectedVersionTimestampText.enabled = true;
			OnLoadingEnded.Invoke();
		}
	}
}
