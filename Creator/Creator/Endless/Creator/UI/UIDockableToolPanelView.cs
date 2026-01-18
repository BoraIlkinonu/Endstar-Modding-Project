using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020002A0 RID: 672
	public abstract class UIDockableToolPanelView<T> : UIBaseToolPanelView<T>, IDockableToolPanelView where T : EndlessTool
	{
		// Token: 0x06000B30 RID: 2864 RVA: 0x000346AC File Offset: 0x000328AC
		protected override void Start()
		{
			base.Start();
			this.undockButtonDisplayAndHideHandler.SetToHideEnd(true);
			MonoBehaviourSingleton<ToolManager>.Instance.OnSetActiveToolToSameTool.AddListener(new UnityAction<EndlessTool>(this.ToggleDockIfActivatedSameTool));
			this.toolIconImage.sprite = this.Tool.Icon;
			this.toolIconImage.color = this.toolTypeColorDictionary[this.Tool.ToolType];
		}

		// Token: 0x06000B31 RID: 2865 RVA: 0x00034727 File Offset: 0x00032927
		public override void Display()
		{
			base.Display();
			this.Undock();
		}

		// Token: 0x06000B32 RID: 2866 RVA: 0x00034735 File Offset: 0x00032935
		public override void Hide()
		{
			base.Hide();
			this.dockingDisplayAndHideHandler.Hide();
			this.undockButtonDisplayAndHideHandler.Hide();
			this.isDocked = false;
		}

		// Token: 0x06000B33 RID: 2867 RVA: 0x0003475A File Offset: 0x0003295A
		public void Dock()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Dock", this);
			}
			this.dockingDisplayAndHideHandler.Hide();
			this.undockButtonDisplayAndHideHandler.Display();
			this.isDocked = true;
		}

		// Token: 0x06000B34 RID: 2868 RVA: 0x0003478C File Offset: 0x0003298C
		public void Undock()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Undock", this);
			}
			this.dockingDisplayAndHideHandler.Display();
			this.undockButtonDisplayAndHideHandler.Hide();
			this.isDocked = false;
		}

		// Token: 0x06000B35 RID: 2869 RVA: 0x000347C0 File Offset: 0x000329C0
		private void ToggleDockIfActivatedSameTool(EndlessTool activeTool)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("ToggleDockIfActivatedSameTool ( activeTool: " + ((activeTool != null) ? activeTool.GetType().Name : "null") + " )", this);
			}
			if (activeTool == null)
			{
				return;
			}
			if (activeTool.GetType() != typeof(T))
			{
				return;
			}
			if (this.isDocked)
			{
				this.Undock();
				return;
			}
			this.Dock();
		}

		// Token: 0x04000971 RID: 2417
		[Header("UIDockableToolPanelView")]
		[SerializeField]
		private Image toolIconImage;

		// Token: 0x04000972 RID: 2418
		[SerializeField]
		private UIToolTypeColorDictionary toolTypeColorDictionary;

		// Token: 0x04000973 RID: 2419
		[SerializeField]
		private UIDisplayAndHideHandler dockingDisplayAndHideHandler;

		// Token: 0x04000974 RID: 2420
		[SerializeField]
		private UIDisplayAndHideHandler undockButtonDisplayAndHideHandler;

		// Token: 0x04000975 RID: 2421
		private bool isDocked;
	}
}
