using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002AE RID: 686
	public class UIPropToolPanelView : UIItemSelectionToolPanelView<PropTool, PropLibrary.RuntimePropInfo>
	{
		// Token: 0x17000178 RID: 376
		// (get) Token: 0x06000B86 RID: 2950 RVA: 0x00036250 File Offset: 0x00034450
		protected override bool HasSelectedItem
		{
			get
			{
				return this.Tool.SelectedAssetId != SerializableGuid.Empty;
			}
		}

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x06000B87 RID: 2951 RVA: 0x0002ABC6 File Offset: 0x00028DC6
		protected override bool CanViewDetail
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000B88 RID: 2952 RVA: 0x00036268 File Offset: 0x00034468
		protected override void Start()
		{
			base.Start();
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(new UnityAction(this.OnCreatorStarted));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(new UnityAction(this.OnCreatorEnded));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnPropsRepopulated += this.OnLibraryRepopulated;
			this.Tool.OnSelectedAssetChanged.AddListener(new UnityAction<SerializableGuid>(this.OnSelectedAssetChanged));
			MonoBehaviourSingleton<UIWindowManager>.Instance.DisplayUnityEvent.AddListener(new UnityAction<UIBaseWindowView>(this.OnWindowDisplayed));
		}

		// Token: 0x06000B89 RID: 2953 RVA: 0x00036300 File Offset: 0x00034500
		protected override void OnToolChange(EndlessTool activeTool)
		{
			base.OnToolChange(activeTool);
			if (activeTool.GetType() != typeof(PropTool))
			{
				return;
			}
			this.runtimePropInfoListModel.Synchronize(ReferenceFilter.None, null);
			if (this.Tool.PreviousSelectedAssetId.IsEmpty)
			{
				return;
			}
			this.Tool.UpdateSelectedAssetId(this.Tool.PreviousSelectedAssetId);
		}

		// Token: 0x06000B8A RID: 2954 RVA: 0x00036368 File Offset: 0x00034568
		private void OnLibraryRepopulated()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLibraryRepopulated", Array.Empty<object>());
			}
			this.runtimePropInfoListModel.Synchronize(ReferenceFilter.None, null);
			if (this.Tool.PreviousSelectedAssetId.IsEmpty)
			{
				return;
			}
			this.Tool.UpdateSelectedAssetId(this.Tool.PreviousSelectedAssetId);
		}

		// Token: 0x06000B8B RID: 2955 RVA: 0x000363C8 File Offset: 0x000345C8
		private void OnSelectedAssetChanged(SerializableGuid selectedAssetId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSelectedAssetChanged", new object[] { selectedAssetId });
			}
			if (this.selectedAssetId == selectedAssetId)
			{
				bool flag = !selectedAssetId.IsEmpty;
			}
			this.selectedAssetId = selectedAssetId;
			if (selectedAssetId.IsEmpty)
			{
				base.OnItemSelectionEmpty();
				return;
			}
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(selectedAssetId);
			if (runtimePropInfo.IsMissingObject)
			{
				this.Tool.UpdateSelectedAssetId(SerializableGuid.Empty);
				return;
			}
			base.ViewSelectedItem(runtimePropInfo);
		}

		// Token: 0x06000B8C RID: 2956 RVA: 0x0003645B File Offset: 0x0003465B
		private void OnCreatorStarted()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnCreatorStarted", Array.Empty<object>());
			}
			this.inCreatorMode = true;
		}

		// Token: 0x06000B8D RID: 2957 RVA: 0x0003647C File Offset: 0x0003467C
		private void OnCreatorEnded()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnCreatorEnded", Array.Empty<object>());
			}
			this.inCreatorMode = false;
		}

		// Token: 0x06000B8E RID: 2958 RVA: 0x000364A0 File Offset: 0x000346A0
		private void OnWindowDisplayed(UIBaseWindowView displayedWindow)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnWindowDisplayed", new object[] { displayedWindow.GetType() });
				DebugUtility.Log(string.Format("{0}: {1}", "inCreatorMode", this.inCreatorMode), this);
				DebugUtility.Log(string.Format("{0}.{1}: {2}", "Tool", "IsActive", this.Tool.IsActive), this);
				DebugUtility.Log(string.Format("is{0}: {1}", "UIScriptWindowView", displayedWindow is UIScriptWindowView), this);
			}
			if (!this.inCreatorMode || !this.Tool.IsActive || !(displayedWindow is UIScriptWindowView))
			{
				return;
			}
			this.scriptWindow = (UIScriptWindowView)displayedWindow;
			this.scriptWindow.CloseUnityEvent.AddListener(new UnityAction(this.OnScriptWindowClosed));
			base.Dock();
		}

		// Token: 0x06000B8F RID: 2959 RVA: 0x0003658C File Offset: 0x0003478C
		private void OnScriptWindowClosed()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScriptWindowClosed", Array.Empty<object>());
			}
			base.Undock();
			if (this.scriptWindow)
			{
				this.scriptWindow.CloseUnityEvent.RemoveListener(new UnityAction(this.OnScriptWindowClosed));
				this.scriptWindow = null;
				return;
			}
			DebugUtility.LogException(new NullReferenceException("scriptWindow is null on OnScriptWindowClosed!"), this);
		}

		// Token: 0x040009BA RID: 2490
		[Header("UIPropToolPanelView")]
		[SerializeField]
		private UIRuntimePropInfoListModel runtimePropInfoListModel;

		// Token: 0x040009BB RID: 2491
		private SerializableGuid selectedAssetId;

		// Token: 0x040009BC RID: 2492
		private bool inCreatorMode;

		// Token: 0x040009BD RID: 2493
		private UIScriptWindowView scriptWindow;
	}
}
