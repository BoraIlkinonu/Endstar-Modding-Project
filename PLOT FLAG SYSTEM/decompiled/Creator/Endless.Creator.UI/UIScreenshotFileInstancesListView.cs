using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScreenshotFileInstancesListView : UIBaseRoleInteractableListView<ScreenshotFileInstances>
{
	private const float ASPECT_RATIO = 1.77f;

	[Header("UIScreenshotFileInstancesListView")]
	[SerializeField]
	private IntVariable screenshotLimit;

	private float firstHeightValue;

	private bool firstHeightValueSet;

	private bool forceUpdateCanvases = true;

	protected override void OnEnable()
	{
		base.OnEnable();
		UIScreenObserver.OnSizeChange = (Action)Delegate.Combine(UIScreenObserver.OnSizeChange, new Action(InvalidateSizing));
		UIScreenObserver.OnFullScreenModeChange = (Action)Delegate.Combine(UIScreenObserver.OnFullScreenModeChange, new Action(InvalidateSizing));
	}

	private void OnDisable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		forceUpdateCanvases = true;
		UIScreenObserver.OnSizeChange = (Action)Delegate.Remove(UIScreenObserver.OnSizeChange, new Action(InvalidateSizing));
		UIScreenObserver.OnFullScreenModeChange = (Action)Delegate.Remove(UIScreenObserver.OnFullScreenModeChange, new Action(InvalidateSizing));
	}

	public override float GetCellViewSize(int index)
	{
		if (base.SuperVerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetCellViewSize", index);
		}
		if (forceUpdateCanvases)
		{
			EnsureRectTransformIsSized();
			forceUpdateCanvases = false;
		}
		float num = 0f;
		float num2 = GetFirstHeightValue();
		if (base.SuperVerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "height", num2), this);
			DebugUtility.Log(string.Format("{0}.{1}.{2}: {3}", "RectTransform", "rect", "height", base.RectTransform.rect.height), this);
		}
		switch (base.ScrollDirection)
		{
		case Directions.Vertical:
		{
			UIScreenshotFileInstancesListRowView obj = (UIScreenshotFileInstancesListRowView)ActiveCellSource;
			float num3 = (float)obj.Cells.Length / (float)screenshotLimit.Value;
			float num4 = num2 * num3;
			obj.LayoutElement.minHeight = num4;
			num = num4;
			break;
		}
		case Directions.Horizontal:
			num = num2 * 1.77f;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (base.SuperVerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "cellViewSize", num), this);
		}
		return num;
	}

	private float GetFirstHeightValue()
	{
		if (base.SuperVerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetFirstHeightValue");
		}
		if (firstHeightValueSet)
		{
			return firstHeightValue;
		}
		firstHeightValueSet = true;
		firstHeightValue = base.RectTransform.rect.height;
		if (base.SuperVerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "firstHeightValue", firstHeightValue), this);
		}
		return firstHeightValue;
	}

	private void InvalidateSizing()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvalidateSizing");
		}
		firstHeightValueSet = false;
		EnsureRectTransformIsSized();
		Resize(keepPosition: true);
	}

	private void EnsureRectTransformIsSized()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "EnsureRectTransformIsSized");
		}
		Canvas.ForceUpdateCanvases();
	}
}
