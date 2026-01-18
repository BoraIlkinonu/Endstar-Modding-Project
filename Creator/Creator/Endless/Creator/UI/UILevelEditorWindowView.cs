using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002D9 RID: 729
	public class UILevelEditorWindowView : UIBaseWindowView, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000196 RID: 406
		// (get) Token: 0x06000C62 RID: 3170 RVA: 0x0003B3EC File Offset: 0x000395EC
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000197 RID: 407
		// (get) Token: 0x06000C63 RID: 3171 RVA: 0x0003B3F4 File Offset: 0x000395F4
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x17000198 RID: 408
		// (get) Token: 0x06000C64 RID: 3172 RVA: 0x0003B3FC File Offset: 0x000395FC
		// (set) Token: 0x06000C65 RID: 3173 RVA: 0x0003B404 File Offset: 0x00039604
		public Roles LocalClientRole { get; private set; } = Roles.None;

		// Token: 0x06000C66 RID: 3174 RVA: 0x0003B40D File Offset: 0x0003960D
		protected override void Start()
		{
			base.Start();
			this.screenshotTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<ScreenshotTool>();
			this.screenshotTool.OnScreenshotRemoved.AddListener(new UnityAction(this.OnScreenshotRemoved));
			this.layoutables.RequestLayout();
		}

		// Token: 0x06000C67 RID: 3175 RVA: 0x0003B44C File Offset: 0x0003964C
		public static UILevelEditorWindowView Display(Transform parent = null)
		{
			return (UILevelEditorWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UILevelEditorWindowView>(parent, null);
		}

		// Token: 0x06000C68 RID: 3176 RVA: 0x0003B460 File Offset: 0x00039660
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.LocalClientRole = Roles.None;
			this.spawnPointListModel.Clear(true);
			MonoBehaviourSingleton<UIUserRoleWizard>.Instance.OnComplete.RemoveListener(new UnityAction(this.InitializeUserRoles));
			UIScreenObserver.OnSizeChange = (Action)Delegate.Remove(UIScreenObserver.OnSizeChange, new Action(this.layoutables.RequestLayout));
		}

		// Token: 0x06000C69 RID: 3177 RVA: 0x0003B4C8 File Offset: 0x000396C8
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			LevelState levelState = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState;
			MonoBehaviourSingleton<UIUserRoleWizard>.Instance.OnComplete.AddListener(new UnityAction(this.InitializeUserRoles));
			this.InitializeUserRoles();
			this.levelModelHandler.Set(levelState);
			this.ViewSpawnPoints();
			this.revisions.Initialize(levelState);
			UIScreenObserver.OnSizeChange = (Action)Delegate.Combine(UIScreenObserver.OnSizeChange, new Action(this.layoutables.RequestLayout));
		}

		// Token: 0x06000C6A RID: 3178 RVA: 0x0003B550 File Offset: 0x00039750
		private void ViewSpawnPoints()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewSpawnPoints", Array.Empty<object>());
			}
			LevelState levelState = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState;
			using (IEnumerator<SerializableGuid> enumerator = levelState.SpawnPointIds.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					SerializableGuid spawnPointId = enumerator.Current;
					PropEntry propEntry = levelState.PropEntries.FirstOrDefault((PropEntry item) => item.InstanceId == spawnPointId);
					if (propEntry == null)
					{
						DebugUtility.LogError(string.Format("Could not find the {0} that was a Spawn Point with an {1} of {2}!", "PropEntry", "InstanceId", spawnPointId), this);
					}
					else
					{
						bool flag = levelState.SelectedSpawnPointIds.Contains(spawnPointId);
						UISpawnPoint uispawnPoint = new UISpawnPoint(spawnPointId, propEntry.Label);
						this.spawnPointListModel.Add(uispawnPoint, false);
						if (flag)
						{
							this.spawnPointListModel.Select(this.spawnPointListModel.Count - 1, false);
						}
					}
				}
			}
			this.spawnPointListModel.TriggerModelChanged();
		}

		// Token: 0x06000C6B RID: 3179 RVA: 0x0003B66C File Offset: 0x0003986C
		private void InitializeUserRoles()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InitializeUserRoles", Array.Empty<object>());
			}
			AssetContexts assetContexts = AssetContexts.GameOrLevelEditor;
			MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID;
			SerializableGuid activeLevelGuid = MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid;
			string name = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.Name;
			this.gameUserRolesModel.Initialize(activeLevelGuid, name, SerializableGuid.Empty, assetContexts);
			string name2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Name;
			this.levelUserRolesModel.Initialize(activeLevelGuid, name2, SerializableGuid.Empty, assetContexts);
		}

		// Token: 0x06000C6C RID: 3180 RVA: 0x0003B700 File Offset: 0x00039900
		private void OnScreenshotRemoved()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScreenshotRemoved", Array.Empty<object>());
			}
			LevelState levelState = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState;
			this.levelModelHandler.Set(levelState);
		}

		// Token: 0x04000AA9 RID: 2729
		public UnityEvent OnDisplayLevel = new UnityEvent();

		// Token: 0x04000AAA RID: 2730
		[Header("UILevelEditorWindowView")]
		[SerializeField]
		private UILevelModelHandler levelModelHandler;

		// Token: 0x04000AAB RID: 2731
		[SerializeField]
		private UISpawnPointListModel spawnPointListModel;

		// Token: 0x04000AAC RID: 2732
		[Header("User Roles")]
		[SerializeField]
		private UIUserRolesModel levelUserRolesModel;

		// Token: 0x04000AAD RID: 2733
		[SerializeField]
		private UIUserRolesModel gameUserRolesModel;

		// Token: 0x04000AAE RID: 2734
		[SerializeField]
		private UIRevisionsView revisions;

		// Token: 0x04000AAF RID: 2735
		[SerializeField]
		private InterfaceReference<IUILayoutable>[] layoutables = Array.Empty<InterfaceReference<IUILayoutable>>();

		// Token: 0x04000AB0 RID: 2736
		private ScreenshotTool screenshotTool;
	}
}
