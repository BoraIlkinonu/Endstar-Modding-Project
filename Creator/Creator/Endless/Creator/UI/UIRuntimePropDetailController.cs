using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000244 RID: 580
	public class UIRuntimePropDetailController : UIGameObject, IBackable, IUIDetailControllable
	{
		// Token: 0x17000132 RID: 306
		// (get) Token: 0x06000964 RID: 2404 RVA: 0x0002BDBC File Offset: 0x00029FBC
		public UnityEvent OnHide { get; } = new UnityEvent();

		// Token: 0x06000965 RID: 2405 RVA: 0x0002BDC4 File Offset: 0x00029FC4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.hideButton.onClick.AddListener(new UnityAction(this.Hide));
			this.propTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PropTool>();
			this.editScriptButton.onClick.AddListener(new UnityAction(this.EditScript));
		}

		// Token: 0x06000966 RID: 2406 RVA: 0x0002BE31 File Offset: 0x0002A031
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
		}

		// Token: 0x06000967 RID: 2407 RVA: 0x0002BE56 File Offset: 0x0002A056
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
			}
		}

		// Token: 0x06000968 RID: 2408 RVA: 0x0002BE88 File Offset: 0x0002A088
		public void OnBack()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
			this.Hide();
		}

		// Token: 0x06000969 RID: 2409 RVA: 0x0002BEB4 File Offset: 0x0002A0B4
		private void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			this.displayAndHideHandler.Hide();
			if (!MobileUtility.IsMobile)
			{
				this.propTool.UpdateSelectedAssetId(SerializableGuid.Empty);
			}
			this.OnHide.Invoke();
		}

		// Token: 0x0600096A RID: 2410 RVA: 0x0002BF08 File Offset: 0x0002A108
		private void EditScript()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "EditScript", Array.Empty<object>());
			}
			bool flag = this.view.Mode == UIRuntimePropInfoDetailView.Modes.Read;
			this.propTool.EditScript(flag);
		}

		// Token: 0x040007B7 RID: 1975
		[SerializeField]
		private UIRuntimePropInfoDetailView view;

		// Token: 0x040007B8 RID: 1976
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x040007B9 RID: 1977
		[SerializeField]
		private UIButton hideButton;

		// Token: 0x040007BA RID: 1978
		[SerializeField]
		private UIButton editScriptButton;

		// Token: 0x040007BB RID: 1979
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040007BC RID: 1980
		private PropTool propTool;
	}
}
