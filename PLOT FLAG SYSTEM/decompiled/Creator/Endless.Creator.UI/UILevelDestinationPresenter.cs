using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Endless.Data;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UILevelDestinationPresenter : UIBasePresenter<LevelDestination>, IUILoadingSpinnerViewCompatible
{
	[Header("UILevelDestinationPresenter")]
	[SerializeField]
	private UISpawnPointListModel spawnPointListModel;

	[SerializeField]
	private UILevelDestinationSelectionModalView levelDestinationSelectionModalSource;

	[SerializeField]
	private UISpawnPointSelectionModalView spawnPointSelectionModalSource;

	private List<UISpawnPoint> allSpawnPoints = new List<UISpawnPoint>();

	private List<UISpawnPoint> targetSpawnPoints = new List<UISpawnPoint>();

	private UILevelDestinationView levelDestinationView;

	private CancellationTokenSource cancellationTokenSource;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public IReadOnlyList<UISpawnPoint> AllSpawnPoints => allSpawnPoints;

	public IReadOnlyList<UISpawnPoint> TargetSpawnPoints => targetSpawnPoints;

	private UILevelDestinationView LevelDestinationView
	{
		get
		{
			if (!levelDestinationView)
			{
				levelDestinationView = base.View.Interface as UILevelDestinationView;
			}
			return levelDestinationView;
		}
	}

	protected override void Start()
	{
		base.Start();
		spawnPointListModel.SelectionChangedUnityEvent.AddListener(SetTargetSpawnPointIds);
		spawnPointListModel.ItemRemovedUnityEvent.AddListener(SetTargetSpawnPointIds);
		LevelDestinationView.OnOpenLevelSelectionWindow += OpenLevelDestinationSelectionModal;
		LevelDestinationView.OnOpenSpawnPointSelectionWindow += OpenSpawnPointSelectionModal;
	}

	public override void SetModel(LevelDestination model, bool triggerOnModelChanged)
	{
		base.SetModel(model, triggerOnModelChanged);
		cancellationTokenSource?.Cancel();
		cancellationTokenSource?.Dispose();
		cancellationTokenSource = new CancellationTokenSource();
		GetAndApplyLevelDestinationData(cancellationTokenSource.Token);
	}

	public override void Clear()
	{
		base.Clear();
		try
		{
			cancellationTokenSource?.Cancel();
		}
		catch
		{
		}
		finally
		{
			cancellationTokenSource?.Dispose();
			cancellationTokenSource = null;
		}
		spawnPointListModel?.Clear(triggerEvents: true);
		allSpawnPoints?.Clear();
		targetSpawnPoints?.Clear();
	}

	public void SetSpawnPoints(IReadOnlyList<UISpawnPoint> selectedSpawnPoints)
	{
		if (selectedSpawnPoints == null)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogWarning("SetSpawnPoints called with null selectedSpawnPoints", this);
			}
			return;
		}
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetSpawnPoints", selectedSpawnPoints.Count);
		}
		if (base.Model == null)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogWarning("SetSpawnPoints called with null Model", this);
			}
			return;
		}
		base.Model.TargetSpawnPointIds = selectedSpawnPoints.Select((UISpawnPoint item) => item.Id).ToList();
		SetModel(base.Model, triggerOnModelChanged: true);
		spawnPointListModel.Set(selectedSpawnPoints.ToList(), triggerEvents: true);
	}

	public void SetLevelDestination(LevelDestination newLevelDestination)
	{
		if (newLevelDestination == null)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogWarning("SetLevelDestination called with null newLevelDestination", this);
			}
			return;
		}
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetLevelDestination", newLevelDestination.TargetLevelId);
		}
		SetModel(newLevelDestination, triggerOnModelChanged: true);
	}

	private async void GetAndApplyLevelDestinationData(CancellationToken cancelToken)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetAndApplyLevelDestinationData");
		}
		if (base.Model == null || !base.Model.IsValidLevel())
		{
			LevelDestinationView.SetLevelNameText("None");
			spawnPointListModel.Clear(triggerEvents: false);
			return;
		}
		string text = base.Model.TargetLevelId;
		OnLoadingStarted.Invoke();
		try
		{
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(text, "", new AssetParams("x { Name propEntries spawnPointIds }"));
			cancelToken.ThrowIfCancellationRequested();
			if (base.Model == null)
			{
				return;
			}
			string empty = string.Empty;
			List<SerializableGuid> spawnPointIds = new List<SerializableGuid>();
			List<PropEntry> propEntries = new List<PropEntry>();
			if (graphQlResult.HasErrors)
			{
				ErrorHandler.HandleError(ErrorCodes.UILevelDestinationPropertyView_RetrievingLevelAsset, graphQlResult.GetErrorMessage());
				LevelDestinationView.SetLevelNameText("Error loading level");
				spawnPointListModel.Clear(triggerEvents: false);
				return;
			}
			var anonymousTypeObject = new
			{
				Name = string.Empty,
				propEntries = new List<PropEntry>(),
				spawnPointIds = new List<SerializableGuid>()
			};
			var anon = JsonConvert.DeserializeAnonymousType(graphQlResult.GetDataMember().ToString(), anonymousTypeObject);
			if (anon != null)
			{
				empty = anon.Name;
				spawnPointIds = anon.spawnPointIds;
				propEntries = anon.propEntries;
			}
			cancelToken.ThrowIfCancellationRequested();
			if (base.Model != null)
			{
				LevelDestinationView.SetLevelNameText(empty);
				allSpawnPoints = UISpawnPointUtility.GetSpawnPointsFrom(spawnPointIds, propEntries);
				List<SerializableGuid> spawnPointIds2 = base.Model.TargetSpawnPointIds ?? new List<SerializableGuid>();
				targetSpawnPoints = UISpawnPointUtility.GetSpawnPointsFrom(spawnPointIds2, propEntries);
				spawnPointListModel.Set(targetSpawnPoints, triggerEvents: true);
			}
		}
		catch (OperationCanceledException)
		{
		}
		finally
		{
			OnLoadingEnded.Invoke();
		}
	}

	private void OpenLevelDestinationSelectionModal()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OpenLevelDestinationSelectionModal");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(levelDestinationSelectionModalSource, UIModalManagerStackActions.ClearStack, this);
	}

	private void OpenSpawnPointSelectionModal()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OpenSpawnPointSelectionModal");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(spawnPointSelectionModalSource, UIModalManagerStackActions.ClearStack, this);
	}

	private void SetTargetSpawnPointIds(int index, UISpawnPoint removedItem)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", "SetTargetSpawnPointIds", "index", index, "removedItem", removedItem), this);
		}
		SetTargetSpawnPointIds();
	}

	private void SetTargetSpawnPointIds(int index, bool isSelected)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", "SetTargetSpawnPointIds", "index", index, "isSelected", isSelected), this);
		}
		SetTargetSpawnPointIds();
	}

	private void SetTargetSpawnPointIds()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("SetTargetSpawnPointIds", this);
		}
		if (base.Model == null)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogWarning("SetTargetSpawnPointIds called with null Model", this);
			}
		}
		else
		{
			LevelDestination model = new LevelDestination
			{
				TargetLevelId = base.Model.TargetLevelId,
				TargetSpawnPointIds = (spawnPointListModel?.SpawnPointIds ?? new List<SerializableGuid>())
			};
			SetModel(model, triggerOnModelChanged: true);
		}
	}
}
