using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200029B RID: 667
	[DefaultExecutionOrder(2147483647)]
	public abstract class UIBaseToolPanelView<T> : UIGameObject where T : EndlessTool
	{
		// Token: 0x17000164 RID: 356
		// (get) Token: 0x06000B08 RID: 2824 RVA: 0x00033C77 File Offset: 0x00031E77
		// (set) Token: 0x06000B09 RID: 2825 RVA: 0x00033C7F File Offset: 0x00031E7F
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000165 RID: 357
		// (get) Token: 0x06000B0A RID: 2826 RVA: 0x00033C88 File Offset: 0x00031E88
		// (set) Token: 0x06000B0B RID: 2827 RVA: 0x00033C90 File Offset: 0x00031E90
		protected bool SuperVerboseLogging { get; set; }

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x06000B0C RID: 2828 RVA: 0x00033C99 File Offset: 0x00031E99
		protected virtual float ListSize { get; } = -1f;

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x06000B0D RID: 2829 RVA: 0x0002ABC6 File Offset: 0x00028DC6
		protected virtual bool DisplayOnToolChangeMatchToToolType
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x06000B0E RID: 2830 RVA: 0x00033CA1 File Offset: 0x00031EA1
		protected bool IsDisplaying
		{
			get
			{
				return this.displayAndHideHandler.IsDisplaying;
			}
		}

		// Token: 0x06000B0F RID: 2831 RVA: 0x00033CAE File Offset: 0x00031EAE
		private void Awake()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Awake", this);
			}
			this.Tool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<T>();
		}

		// Token: 0x06000B10 RID: 2832 RVA: 0x00033CD4 File Offset: 0x00031ED4
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			MonoBehaviourSingleton<ToolManager>.Instance.OnActiveChange.AddListener(new UnityAction<bool>(this.OnToolModeChanged));
			MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(new UnityAction<EndlessTool>(this.OnToolChange));
			NetworkBehaviourSingleton<CreatorManager>.Instance.LocalClientRoleChanged.AddListener(new UnityAction<Roles>(this.OnLocalClientRoleChanged));
			if (this.ListSize > 0f)
			{
				UIScreenObserver.OnSizeChange = (Action)Delegate.Combine(UIScreenObserver.OnSizeChange, new Action(this.TweenToMaxPanelHeight));
				this.TweenToMaxPanelHeight();
			}
			Canvas.ForceUpdateCanvases();
			this.displayAndHideHandler.SetToHideEnd(true);
		}

		// Token: 0x06000B11 RID: 2833 RVA: 0x00033D8A File Offset: 0x00031F8A
		private void OnDestroy()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnDestroy", this);
			}
			ValueTween.CancelAndNull(ref this.lerpHeightTween);
		}

		// Token: 0x06000B12 RID: 2834 RVA: 0x00033DAC File Offset: 0x00031FAC
		public virtual void Display()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} | {1}: {2}", "Display", "IsDisplaying", this.IsDisplaying), this);
			}
			if (this.displayAndHideHandler.IsDisplaying)
			{
				return;
			}
			this.displayAndHideHandler.Display();
			if (this.ListSize > 0f)
			{
				this.TweenToMaxPanelHeight();
			}
		}

		// Token: 0x06000B13 RID: 2835 RVA: 0x00033E14 File Offset: 0x00032014
		public virtual void Hide()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} | {1}: {2}", "Hide", "IsDisplaying", this.IsDisplaying), this);
			}
			if (!this.IsDisplaying)
			{
				return;
			}
			this.displayAndHideHandler.Hide();
		}

		// Token: 0x06000B14 RID: 2836 RVA: 0x00033E64 File Offset: 0x00032064
		protected virtual void OnToolChange(EndlessTool activeTool)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnToolChange ( activeTool: " + ((activeTool != null) ? activeTool.GetType().Name : "null") + " )", this);
			}
			if (activeTool == null)
			{
				this.Hide();
				return;
			}
			if (this.DisplayOnToolChangeMatchToToolType && activeTool.GetType() == typeof(T))
			{
				this.Display();
				return;
			}
			this.Hide();
		}

		// Token: 0x06000B15 RID: 2837 RVA: 0x00033EE8 File Offset: 0x000320E8
		protected virtual float GetMaxPanelHeight()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("GetMaxPanelHeight", this);
			}
			float num = this.ListSize;
			float num2 = 0f;
			foreach (RectTransform rectTransform in this.rectTransformsToIncludeInSizing)
			{
				num += rectTransform.rect.height;
				num2 += rectTransform.rect.height;
				if (this.VerboseLogging)
				{
					DebugUtility.Log(string.Format("{0}: {1}", rectTransform.name, rectTransform.rect.height), this);
				}
			}
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "debugAdjustedHeight", num2), this);
			}
			return num;
		}

		// Token: 0x06000B16 RID: 2838 RVA: 0x00033FB0 File Offset: 0x000321B0
		protected void TweenToMaxPanelHeight()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("TweenToMaxPanelHeight", this);
			}
			this.oldHeight = base.RectTransform.rect.height;
			this.newHeight = this.GetMaxPanelHeight();
			this.newHeight = Mathf.Clamp(this.newHeight, this.minHeight, this.maximumToolHeight.rect.height);
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "ListSize", this.ListSize), this);
				DebugUtility.Log(string.Format("{0}: {1}", "maximumToolHeight", this.maximumToolHeight.rect.height), this);
				DebugUtility.Log(string.Format("{0}: {1}", "newHeight", this.newHeight), this);
			}
			ValueTween.CancelAndNull(ref this.lerpHeightTween);
			this.lerpHeightTween = MonoBehaviourSingleton<TweenManager>.Instance.TweenValue(this.oldHeight, this.newHeight, 0.25f, new Action<float>(this.LerpHeight), null, TweenTimeMode.Unscaled, this.lerpHeightTweenEase);
		}

		// Token: 0x06000B17 RID: 2839 RVA: 0x000340D4 File Offset: 0x000322D4
		private void LerpHeight(float height)
		{
			if (this.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "LerpHeight", "height", height), this);
			}
			base.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
		}

		// Token: 0x06000B18 RID: 2840 RVA: 0x0003410C File Offset: 0x0003230C
		private void OnLocalClientRoleChanged(Roles localClientLevelRole)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnLocalClientRoleChanged", "localClientLevelRole", localClientLevelRole), this);
			}
			bool flag = false;
			if (MonoBehaviourSingleton<ToolManager>.Instance.IsActive)
			{
				flag = MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool.GetType() == typeof(T) && localClientLevelRole.IsGreaterThanOrEqualTo(Roles.Editor);
			}
			if (flag)
			{
				this.displayAndHideHandler.Display();
				return;
			}
			this.displayAndHideHandler.Hide();
		}

		// Token: 0x06000B19 RID: 2841 RVA: 0x00034195 File Offset: 0x00032395
		private void OnToolModeChanged(bool active)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnToolModeChanged", "active", active), this);
			}
			if (!active)
			{
				this.Hide();
			}
		}

		// Token: 0x0400095A RID: 2394
		[Header("Sizing")]
		[SerializeField]
		protected float minHeight = 200f;

		// Token: 0x0400095B RID: 2395
		[SerializeField]
		private RectTransform[] rectTransformsToIncludeInSizing = Array.Empty<RectTransform>();

		// Token: 0x0400095C RID: 2396
		[SerializeField]
		private RectTransform maximumToolHeight;

		// Token: 0x0400095D RID: 2397
		[SerializeField]
		private AnimationCurve lerpHeightTweenEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		// Token: 0x0400095E RID: 2398
		[Header("Visibility")]
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x04000961 RID: 2401
		protected T Tool;

		// Token: 0x04000962 RID: 2402
		private float oldHeight;

		// Token: 0x04000963 RID: 2403
		private float newHeight;

		// Token: 0x04000964 RID: 2404
		private ValueTween lerpHeightTween;
	}
}
