using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

[DefaultExecutionOrder(int.MaxValue)]
public abstract class UIBaseToolPanelView<T> : UIGameObject where T : EndlessTool
{
	[Header("Sizing")]
	[SerializeField]
	protected float minHeight = 200f;

	[SerializeField]
	private RectTransform[] rectTransformsToIncludeInSizing = Array.Empty<RectTransform>();

	[SerializeField]
	private RectTransform maximumToolHeight;

	[SerializeField]
	private AnimationCurve lerpHeightTweenEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[Header("Visibility")]
	[SerializeField]
	private UIDisplayAndHideHandler displayAndHideHandler;

	protected T Tool;

	private float oldHeight;

	private float newHeight;

	private ValueTween lerpHeightTween;

	protected bool VerboseLogging { get; set; }

	protected bool SuperVerboseLogging { get; set; }

	protected virtual float ListSize { get; } = -1f;

	protected virtual bool DisplayOnToolChangeMatchToToolType => true;

	protected bool IsDisplaying => displayAndHideHandler.IsDisplaying;

	private void Awake()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("Awake", this);
		}
		Tool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<T>();
	}

	protected virtual void Start()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("Start", this);
		}
		MonoBehaviourSingleton<ToolManager>.Instance.OnActiveChange.AddListener(OnToolModeChanged);
		MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(OnToolChange);
		NetworkBehaviourSingleton<CreatorManager>.Instance.LocalClientRoleChanged.AddListener(OnLocalClientRoleChanged);
		if (ListSize > 0f)
		{
			UIScreenObserver.OnSizeChange = (Action)Delegate.Combine(UIScreenObserver.OnSizeChange, new Action(TweenToMaxPanelHeight));
			TweenToMaxPanelHeight();
		}
		Canvas.ForceUpdateCanvases();
		displayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
	}

	private void OnDestroy()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("OnDestroy", this);
		}
		ValueTween.CancelAndNull(ref lerpHeightTween);
	}

	public virtual void Display()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} | {1}: {2}", "Display", "IsDisplaying", IsDisplaying), this);
		}
		if (!displayAndHideHandler.IsDisplaying)
		{
			displayAndHideHandler.Display();
			if (ListSize > 0f)
			{
				TweenToMaxPanelHeight();
			}
		}
	}

	public virtual void Hide()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} | {1}: {2}", "Hide", "IsDisplaying", IsDisplaying), this);
		}
		if (IsDisplaying)
		{
			displayAndHideHandler.Hide();
		}
	}

	protected virtual void OnToolChange(EndlessTool activeTool)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("OnToolChange ( activeTool: " + ((activeTool != null) ? activeTool.GetType().Name : "null") + " )", this);
		}
		if (activeTool == null)
		{
			Hide();
		}
		else if (DisplayOnToolChangeMatchToToolType && activeTool.GetType() == typeof(T))
		{
			Display();
		}
		else
		{
			Hide();
		}
	}

	protected virtual float GetMaxPanelHeight()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("GetMaxPanelHeight", this);
		}
		float num = ListSize;
		float num2 = 0f;
		RectTransform[] array = rectTransformsToIncludeInSizing;
		foreach (RectTransform rectTransform in array)
		{
			num += rectTransform.rect.height;
			num2 += rectTransform.rect.height;
			if (VerboseLogging)
			{
				DebugUtility.Log($"{rectTransform.name}: {rectTransform.rect.height}", this);
			}
		}
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "debugAdjustedHeight", num2), this);
		}
		return num;
	}

	protected void TweenToMaxPanelHeight()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("TweenToMaxPanelHeight", this);
		}
		oldHeight = base.RectTransform.rect.height;
		newHeight = GetMaxPanelHeight();
		newHeight = Mathf.Clamp(newHeight, minHeight, maximumToolHeight.rect.height);
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "ListSize", ListSize), this);
			DebugUtility.Log(string.Format("{0}: {1}", "maximumToolHeight", maximumToolHeight.rect.height), this);
			DebugUtility.Log(string.Format("{0}: {1}", "newHeight", newHeight), this);
		}
		ValueTween.CancelAndNull(ref lerpHeightTween);
		lerpHeightTween = MonoBehaviourSingleton<TweenManager>.Instance.TweenValue(oldHeight, newHeight, 0.25f, LerpHeight, null, TweenTimeMode.Unscaled, lerpHeightTweenEase);
	}

	private void LerpHeight(float height)
	{
		if (SuperVerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "LerpHeight", "height", height), this);
		}
		base.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
	}

	private void OnLocalClientRoleChanged(Roles localClientLevelRole)
	{
		if (VerboseLogging)
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
			displayAndHideHandler.Display();
		}
		else
		{
			displayAndHideHandler.Hide();
		}
	}

	private void OnToolModeChanged(bool active)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnToolModeChanged", "active", active), this);
		}
		if (!active)
		{
			Hide();
		}
	}
}
