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

namespace Endless.Creator.UI
{
	// Token: 0x020001ED RID: 493
	public class UILevelDestinationPresenter : UIBasePresenter<LevelDestination>, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x060007A7 RID: 1959 RVA: 0x00025BF8 File Offset: 0x00023DF8
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x060007A8 RID: 1960 RVA: 0x00025C00 File Offset: 0x00023E00
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x060007A9 RID: 1961 RVA: 0x00025C08 File Offset: 0x00023E08
		public IReadOnlyList<UISpawnPoint> AllSpawnPoints
		{
			get
			{
				return this.allSpawnPoints;
			}
		}

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x060007AA RID: 1962 RVA: 0x00025C10 File Offset: 0x00023E10
		public IReadOnlyList<UISpawnPoint> TargetSpawnPoints
		{
			get
			{
				return this.targetSpawnPoints;
			}
		}

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x060007AB RID: 1963 RVA: 0x00025C18 File Offset: 0x00023E18
		private UILevelDestinationView LevelDestinationView
		{
			get
			{
				if (!this.levelDestinationView)
				{
					this.levelDestinationView = base.View.Interface as UILevelDestinationView;
				}
				return this.levelDestinationView;
			}
		}

		// Token: 0x060007AC RID: 1964 RVA: 0x00025C44 File Offset: 0x00023E44
		protected override void Start()
		{
			base.Start();
			this.spawnPointListModel.SelectionChangedUnityEvent.AddListener(new UnityAction<int, bool>(this.SetTargetSpawnPointIds));
			this.spawnPointListModel.ItemRemovedUnityEvent.AddListener(new UnityAction<int, UISpawnPoint>(this.SetTargetSpawnPointIds));
			this.LevelDestinationView.OnOpenLevelSelectionWindow += this.OpenLevelDestinationSelectionModal;
			this.LevelDestinationView.OnOpenSpawnPointSelectionWindow += this.OpenSpawnPointSelectionModal;
		}

		// Token: 0x060007AD RID: 1965 RVA: 0x00025CC0 File Offset: 0x00023EC0
		public override void SetModel(LevelDestination model, bool triggerOnModelChanged)
		{
			base.SetModel(model, triggerOnModelChanged);
			CancellationTokenSource cancellationTokenSource = this.cancellationTokenSource;
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			CancellationTokenSource cancellationTokenSource2 = this.cancellationTokenSource;
			if (cancellationTokenSource2 != null)
			{
				cancellationTokenSource2.Dispose();
			}
			this.cancellationTokenSource = new CancellationTokenSource();
			this.GetAndApplyLevelDestinationData(this.cancellationTokenSource.Token);
		}

		// Token: 0x060007AE RID: 1966 RVA: 0x00025D14 File Offset: 0x00023F14
		public override void Clear()
		{
			base.Clear();
			try
			{
				CancellationTokenSource cancellationTokenSource = this.cancellationTokenSource;
				if (cancellationTokenSource != null)
				{
					cancellationTokenSource.Cancel();
				}
			}
			catch
			{
			}
			finally
			{
				CancellationTokenSource cancellationTokenSource2 = this.cancellationTokenSource;
				if (cancellationTokenSource2 != null)
				{
					cancellationTokenSource2.Dispose();
				}
				this.cancellationTokenSource = null;
			}
			UISpawnPointListModel uispawnPointListModel = this.spawnPointListModel;
			if (uispawnPointListModel != null)
			{
				uispawnPointListModel.Clear(true);
			}
			List<UISpawnPoint> list = this.allSpawnPoints;
			if (list != null)
			{
				list.Clear();
			}
			List<UISpawnPoint> list2 = this.targetSpawnPoints;
			if (list2 == null)
			{
				return;
			}
			list2.Clear();
		}

		// Token: 0x060007AF RID: 1967 RVA: 0x00025DA8 File Offset: 0x00023FA8
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
				DebugUtility.LogMethod(this, "SetSpawnPoints", new object[] { selectedSpawnPoints.Count });
			}
			if (base.Model == null)
			{
				if (base.VerboseLogging)
				{
					DebugUtility.LogWarning("SetSpawnPoints called with null Model", this);
				}
				return;
			}
			base.Model.TargetSpawnPointIds = selectedSpawnPoints.Select((UISpawnPoint item) => item.Id).ToList<SerializableGuid>();
			this.SetModel(base.Model, true);
			this.spawnPointListModel.Set(selectedSpawnPoints.ToList<UISpawnPoint>(), true);
		}

		// Token: 0x060007B0 RID: 1968 RVA: 0x00025E64 File Offset: 0x00024064
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
				DebugUtility.LogMethod(this, "SetLevelDestination", new object[] { newLevelDestination.TargetLevelId });
			}
			this.SetModel(newLevelDestination, true);
		}

		// Token: 0x060007B1 RID: 1969 RVA: 0x00025EB8 File Offset: 0x000240B8
		private async void GetAndApplyLevelDestinationData(CancellationToken cancelToken)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetAndApplyLevelDestinationData", Array.Empty<object>());
			}
			if (base.Model == null || !base.Model.IsValidLevel())
			{
				this.LevelDestinationView.SetLevelNameText("None");
				this.spawnPointListModel.Clear(false);
			}
			else
			{
				string text = base.Model.TargetLevelId;
				this.OnLoadingStarted.Invoke();
				try
				{
					GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(text, "", new AssetParams("x { Name propEntries spawnPointIds }", false, null), false, 10);
					cancelToken.ThrowIfCancellationRequested();
					if (base.Model != null)
					{
						string text2 = string.Empty;
						List<SerializableGuid> list = new List<SerializableGuid>();
						List<PropEntry> list2 = new List<PropEntry>();
						if (graphQlResult.HasErrors)
						{
							ErrorHandler.HandleError(ErrorCodes.UILevelDestinationPropertyView_RetrievingLevelAsset, graphQlResult.GetErrorMessage(0), true, false);
							this.LevelDestinationView.SetLevelNameText("Error loading level");
							this.spawnPointListModel.Clear(false);
						}
						else
						{
							var <>f__AnonymousType = new
							{
								Name = string.Empty,
								propEntries = new List<PropEntry>(),
								spawnPointIds = new List<SerializableGuid>()
							};
							var <>f__AnonymousType2 = JsonConvert.DeserializeAnonymousType(graphQlResult.GetDataMember().ToString(), <>f__AnonymousType);
							if (<>f__AnonymousType2 != null)
							{
								text2 = <>f__AnonymousType2.Name;
								list = <>f__AnonymousType2.spawnPointIds;
								list2 = <>f__AnonymousType2.propEntries;
							}
							cancelToken.ThrowIfCancellationRequested();
							if (base.Model != null)
							{
								this.LevelDestinationView.SetLevelNameText(text2);
								this.allSpawnPoints = UISpawnPointUtility.GetSpawnPointsFrom(list, list2);
								List<SerializableGuid> list3 = base.Model.TargetSpawnPointIds ?? new List<SerializableGuid>();
								this.targetSpawnPoints = UISpawnPointUtility.GetSpawnPointsFrom(list3, list2);
								this.spawnPointListModel.Set(this.targetSpawnPoints, true);
							}
						}
					}
				}
				catch (OperationCanceledException)
				{
				}
				finally
				{
					this.OnLoadingEnded.Invoke();
				}
			}
		}

		// Token: 0x060007B2 RID: 1970 RVA: 0x00025EF7 File Offset: 0x000240F7
		private void OpenLevelDestinationSelectionModal()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OpenLevelDestinationSelectionModal", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.levelDestinationSelectionModalSource, UIModalManagerStackActions.ClearStack, new object[] { this });
		}

		// Token: 0x060007B3 RID: 1971 RVA: 0x00025F2C File Offset: 0x0002412C
		private void OpenSpawnPointSelectionModal()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OpenSpawnPointSelectionModal", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.spawnPointSelectionModalSource, UIModalManagerStackActions.ClearStack, new object[] { this });
		}

		// Token: 0x060007B4 RID: 1972 RVA: 0x00025F64 File Offset: 0x00024164
		private void SetTargetSpawnPointIds(int index, UISpawnPoint removedItem)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetTargetSpawnPointIds", "index", index, "removedItem", removedItem }), this);
			}
			this.SetTargetSpawnPointIds();
		}

		// Token: 0x060007B5 RID: 1973 RVA: 0x00025FC0 File Offset: 0x000241C0
		private void SetTargetSpawnPointIds(int index, bool isSelected)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetTargetSpawnPointIds", "index", index, "isSelected", isSelected }), this);
			}
			this.SetTargetSpawnPointIds();
		}

		// Token: 0x060007B6 RID: 1974 RVA: 0x0002601C File Offset: 0x0002421C
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
				return;
			}
			LevelDestination levelDestination = new LevelDestination();
			levelDestination.TargetLevelId = base.Model.TargetLevelId;
			UISpawnPointListModel uispawnPointListModel = this.spawnPointListModel;
			levelDestination.TargetSpawnPointIds = ((uispawnPointListModel != null) ? uispawnPointListModel.SpawnPointIds : null) ?? new List<SerializableGuid>();
			LevelDestination levelDestination2 = levelDestination;
			this.SetModel(levelDestination2, true);
		}

		// Token: 0x040006D5 RID: 1749
		[Header("UILevelDestinationPresenter")]
		[SerializeField]
		private UISpawnPointListModel spawnPointListModel;

		// Token: 0x040006D6 RID: 1750
		[SerializeField]
		private UILevelDestinationSelectionModalView levelDestinationSelectionModalSource;

		// Token: 0x040006D7 RID: 1751
		[SerializeField]
		private UISpawnPointSelectionModalView spawnPointSelectionModalSource;

		// Token: 0x040006D8 RID: 1752
		private List<UISpawnPoint> allSpawnPoints = new List<UISpawnPoint>();

		// Token: 0x040006D9 RID: 1753
		private List<UISpawnPoint> targetSpawnPoints = new List<UISpawnPoint>();

		// Token: 0x040006DA RID: 1754
		private UILevelDestinationView levelDestinationView;

		// Token: 0x040006DB RID: 1755
		private CancellationTokenSource cancellationTokenSource;
	}
}
