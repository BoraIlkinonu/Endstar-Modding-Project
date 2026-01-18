using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UILevelAssetListModel : UIBaseRearrangeableListModel<LevelAsset>, IUILoadingSpinnerViewCompatible
{
	private readonly HashSet<Vector2Int> orderChanges = new HashSet<Vector2Int>();

	public bool OpenLevelInAdminMode { get; private set; }

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public Asset Game { get; private set; }

	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		base.OnItemMovedUnityEvent.AddListener(OnOrderChanged);
	}

	private void OnEnable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		UIDragInstanceHandler.InstanceEndDragAction = (Action<RectTransform>)Delegate.Combine(UIDragInstanceHandler.InstanceEndDragAction, new Action<RectTransform>(HandleDrop));
	}

	private void OnDisable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		UIDragInstanceHandler.InstanceEndDragAction = (Action<RectTransform>)Delegate.Remove(UIDragInstanceHandler.InstanceEndDragAction, new Action<RectTransform>(HandleDrop));
		orderChanges.Clear();
	}

	public override void Clear(bool triggerEvents)
	{
		base.Clear(triggerEvents);
		OpenLevelInAdminMode = false;
	}

	public void SetGame(Asset game)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetGame", game.Name);
		}
		Game = game;
	}

	public void SetOpenLevelInAdminMode(bool openLevelInAdminMode)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetOpenLevelInAdminMode", openLevelInAdminMode);
		}
		OpenLevelInAdminMode = openLevelInAdminMode;
	}

	private void OnOrderChanged(int oldIndex, int newIndex)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnOrderChanged", oldIndex, newIndex);
		}
		Vector2Int item = new Vector2Int(oldIndex, newIndex);
		Vector2Int item2 = new Vector2Int(newIndex, oldIndex);
		if (orderChanges.Contains(item2))
		{
			orderChanges.Remove(item2);
		}
		else
		{
			orderChanges.Add(item);
		}
	}

	private void HandleDrop(RectTransform rectTransform)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleDrop", rectTransform.name);
		}
		if (rectTransform.TryGetComponent<UILevelAssetListCellView>(out var _))
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}.{1}: {2}", "orderChanges", "Count", orderChanges.Count), this);
			}
			if (orderChanges.Count != 0)
			{
				orderChanges.Clear();
				ApplyLevelOrderChange();
			}
		}
	}

	private async void ApplyLevelOrderChange()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ApplyLevelOrderChange");
		}
		List<LevelReference> list = new List<LevelReference>();
		foreach (LevelAsset readOnly in base.ReadOnlyList)
		{
			LevelReference item = new LevelReference
			{
				AssetID = readOnly.AssetID,
				AssetVersion = readOnly.AssetVersion,
				AssetType = readOnly.AssetType
			};
			list.Add(item);
		}
		OnLoadingStarted.Invoke();
		try
		{
			await MonoBehaviourSingleton<GameEditor>.Instance.ReorderGameLevels(list);
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.UILevelAssetListModel_ApplyLevelOrderChange, exception);
		}
		OnLoadingEnded.Invoke();
	}
}
