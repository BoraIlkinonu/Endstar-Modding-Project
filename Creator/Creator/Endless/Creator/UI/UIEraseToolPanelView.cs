using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002A2 RID: 674
	public class UIEraseToolPanelView : UIDockableToolPanelView<EraseTool>, IValidatable
	{
		// Token: 0x06000B3C RID: 2876 RVA: 0x0003497C File Offset: 0x00032B7C
		protected override void Start()
		{
			base.Start();
			this.Tool.OnFunctionChange.AddListener(new UnityAction<EraseToolFunction>(this.OnFunctionChange));
			this.OnFunctionChange(this.Tool.CurrentFunction);
			this.nothingToEraseTextObject.text = string.Empty;
			this.nothingToEraseHideTweens.SetToEnd();
			this.nothingToEraseHideTweens.OnAllTweenCompleted.AddListener(new UnityAction(this.SetNothingToEraseTextToNothing));
		}

		// Token: 0x06000B3D RID: 2877 RVA: 0x000349F3 File Offset: 0x00032BF3
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			this.nothingToEraseHideTweens.ValidateForNumberOfTweens(1);
		}

		// Token: 0x06000B3E RID: 2878 RVA: 0x00034A19 File Offset: 0x00032C19
		protected override void OnToolChange(EndlessTool activeTool)
		{
			base.OnToolChange(activeTool);
			if (activeTool == null)
			{
				return;
			}
			if (activeTool.GetType() == this.Tool.GetType())
			{
				this.OnFunctionChange(this.Tool.CurrentFunction);
			}
		}

		// Token: 0x06000B3F RID: 2879 RVA: 0x00034A58 File Offset: 0x00032C58
		private void OnFunctionChange(EraseToolFunction function)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnFunctionChange", new object[] { function });
			}
			this.terrain.SetIsOn(function.HasFlag(EraseToolFunction.Terrain), true, true);
			this.unwiredProps.SetIsOn(function.HasFlag(EraseToolFunction.UnwiredProps), true, true);
			this.wiredProps.SetIsOn(function.HasFlag(EraseToolFunction.WiredProps), true, true);
			if (!this.terrain.IsOn && !this.unwiredProps.IsOn && !this.wiredProps.IsOn)
			{
				this.nothingToEraseHideTweens.Cancel();
				this.nothingToEraseTextObject.text = this.nothingToEraseText;
				this.nothingToEraseDisplayTweens.Tween();
				return;
			}
			this.nothingToEraseDisplayTweens.Cancel();
			this.nothingToEraseHideTweens.Tween();
		}

		// Token: 0x06000B40 RID: 2880 RVA: 0x00034B46 File Offset: 0x00032D46
		private void SetNothingToEraseTextToNothing()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetNothingToEraseTextToNothing", Array.Empty<object>());
			}
			this.nothingToEraseTextObject.text = string.Empty;
		}

		// Token: 0x0400097B RID: 2427
		[Header("UIEraseToolPanelView")]
		[SerializeField]
		private TextMeshProUGUI nothingToEraseTextObject;

		// Token: 0x0400097C RID: 2428
		[SerializeField]
		private string nothingToEraseText;

		// Token: 0x0400097D RID: 2429
		[SerializeField]
		private UIToggle terrain;

		// Token: 0x0400097E RID: 2430
		[SerializeField]
		private UIToggle unwiredProps;

		// Token: 0x0400097F RID: 2431
		[SerializeField]
		private UIToggle wiredProps;

		// Token: 0x04000980 RID: 2432
		[SerializeField]
		private TweenCollection nothingToEraseDisplayTweens;

		// Token: 0x04000981 RID: 2433
		[SerializeField]
		private TweenCollection nothingToEraseHideTweens;
	}
}
