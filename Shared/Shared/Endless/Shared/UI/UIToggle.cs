using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200011C RID: 284
	public class UIToggle : UIGameObject
	{
		// Token: 0x1700012A RID: 298
		// (get) Token: 0x060006FC RID: 1788 RVA: 0x0001DA93 File Offset: 0x0001BC93
		public UnityEvent<bool> OnChange { get; } = new UnityEvent<bool>();

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x060006FD RID: 1789 RVA: 0x0001DA9B File Offset: 0x0001BC9B
		public bool IsOn
		{
			get
			{
				return this.isOn;
			}
		}

		// Token: 0x060006FE RID: 1790 RVA: 0x0001DAA4 File Offset: 0x0001BCA4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.pointerUpHandler.PointerUpUnityEvent.AddListener(new UnityAction(this.Toggle));
			this.SetInteractable(this.isInteractable, false);
		}

		// Token: 0x060006FF RID: 1791 RVA: 0x0001DAF2 File Offset: 0x0001BCF2
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.pointerUpHandler.PointerUpUnityEvent.RemoveListener(new UnityAction(this.Toggle));
		}

		// Token: 0x06000700 RID: 1792 RVA: 0x0001DB28 File Offset: 0x0001BD28
		public void Toggle()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Toggle", Array.Empty<object>());
			}
			this.SetIsOn(!this.isOn, false, true);
		}

		// Token: 0x06000701 RID: 1793 RVA: 0x0001DB54 File Offset: 0x0001BD54
		public void SetIsOn(bool state, bool suppressOnChange, bool tweenVisuals = true)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetIsOn", new object[] { state, suppressOnChange, tweenVisuals });
			}
			if (this.isOn == state)
			{
				return;
			}
			this.isOn = state;
			this.View(tweenVisuals);
			if (!suppressOnChange)
			{
				this.OnChange.Invoke(state);
			}
		}

		// Token: 0x06000702 RID: 1794 RVA: 0x0001DBBC File Offset: 0x0001BDBC
		private void View(bool tween)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { tween });
			}
			if (!this.isInteractable)
			{
				this.HandleTweenCollection(this.isOn ? this.onTweens : this.offTweens, false);
				this.HandleTweenCollection(this.disabledTweens, true);
				return;
			}
			this.HandleTweenCollection(this.isOn ? this.onTweens : this.offTweens, tween);
		}

		// Token: 0x06000703 RID: 1795 RVA: 0x0001DC3C File Offset: 0x0001BE3C
		public void SetInteractable(bool isInteractable, bool tweenVisuals)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { isInteractable, tweenVisuals });
			}
			this.isInteractable = isInteractable;
			this.pointerUpHandler.enabled = isInteractable;
			this.View(tweenVisuals);
		}

		// Token: 0x06000704 RID: 1796 RVA: 0x0001DC8E File Offset: 0x0001BE8E
		private void HandleTweenCollection(TweenCollection tweenCollection, bool tween)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleTweenCollection", new object[]
				{
					tweenCollection.DebugSafeName(true),
					tween
				});
			}
			if (tween)
			{
				tweenCollection.Tween();
				return;
			}
			tweenCollection.SetToEnd();
		}

		// Token: 0x0400040E RID: 1038
		[Header("UIToggle")]
		[SerializeField]
		private bool isOn;

		// Token: 0x0400040F RID: 1039
		[SerializeField]
		private bool isInteractable = true;

		// Token: 0x04000410 RID: 1040
		[SerializeField]
		private PointerUpHandler pointerUpHandler;

		// Token: 0x04000411 RID: 1041
		[Header("Tweens")]
		[SerializeField]
		private TweenCollection onTweens;

		// Token: 0x04000412 RID: 1042
		[SerializeField]
		private TweenCollection offTweens;

		// Token: 0x04000413 RID: 1043
		[SerializeField]
		private TweenCollection disabledTweens;

		// Token: 0x04000414 RID: 1044
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
