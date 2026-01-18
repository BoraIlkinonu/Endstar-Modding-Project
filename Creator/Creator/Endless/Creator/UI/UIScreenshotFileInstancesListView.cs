using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000170 RID: 368
	public class UIScreenshotFileInstancesListView : UIBaseRoleInteractableListView<ScreenshotFileInstances>
	{
		// Token: 0x06000574 RID: 1396 RVA: 0x0001D184 File Offset: 0x0001B384
		protected override void OnEnable()
		{
			base.OnEnable();
			UIScreenObserver.OnSizeChange = (Action)Delegate.Combine(UIScreenObserver.OnSizeChange, new Action(this.InvalidateSizing));
			UIScreenObserver.OnFullScreenModeChange = (Action)Delegate.Combine(UIScreenObserver.OnFullScreenModeChange, new Action(this.InvalidateSizing));
		}

		// Token: 0x06000575 RID: 1397 RVA: 0x0001D1D8 File Offset: 0x0001B3D8
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			this.forceUpdateCanvases = true;
			UIScreenObserver.OnSizeChange = (Action)Delegate.Remove(UIScreenObserver.OnSizeChange, new Action(this.InvalidateSizing));
			UIScreenObserver.OnFullScreenModeChange = (Action)Delegate.Remove(UIScreenObserver.OnFullScreenModeChange, new Action(this.InvalidateSizing));
		}

		// Token: 0x06000576 RID: 1398 RVA: 0x0001D244 File Offset: 0x0001B444
		public override float GetCellViewSize(int index)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetCellViewSize", new object[] { index });
			}
			if (this.forceUpdateCanvases)
			{
				this.EnsureRectTransformIsSized();
				this.forceUpdateCanvases = false;
			}
			float num = this.GetFirstHeightValue();
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "height", num), this);
				DebugUtility.Log(string.Format("{0}.{1}.{2}: {3}", new object[]
				{
					"RectTransform",
					"rect",
					"height",
					base.RectTransform.rect.height
				}), this);
			}
			UIBaseListView<ScreenshotFileInstances>.Directions scrollDirection = base.ScrollDirection;
			float num2;
			if (scrollDirection != UIBaseListView<ScreenshotFileInstances>.Directions.Vertical)
			{
				if (scrollDirection != UIBaseListView<ScreenshotFileInstances>.Directions.Horizontal)
				{
					throw new ArgumentOutOfRangeException();
				}
				num2 = num * 1.77f;
			}
			else
			{
				UIScreenshotFileInstancesListRowView uiscreenshotFileInstancesListRowView = (UIScreenshotFileInstancesListRowView)this.ActiveCellSource;
				float num3 = (float)uiscreenshotFileInstancesListRowView.Cells.Length / (float)this.screenshotLimit.Value;
				float num4 = num * num3;
				uiscreenshotFileInstancesListRowView.LayoutElement.minHeight = num4;
				num2 = num4;
			}
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "cellViewSize", num2), this);
			}
			return num2;
		}

		// Token: 0x06000577 RID: 1399 RVA: 0x0001D380 File Offset: 0x0001B580
		private float GetFirstHeightValue()
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetFirstHeightValue", Array.Empty<object>());
			}
			if (this.firstHeightValueSet)
			{
				return this.firstHeightValue;
			}
			this.firstHeightValueSet = true;
			this.firstHeightValue = base.RectTransform.rect.height;
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "firstHeightValue", this.firstHeightValue), this);
			}
			return this.firstHeightValue;
		}

		// Token: 0x06000578 RID: 1400 RVA: 0x0001D402 File Offset: 0x0001B602
		private void InvalidateSizing()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvalidateSizing", Array.Empty<object>());
			}
			this.firstHeightValueSet = false;
			this.EnsureRectTransformIsSized();
			base.Resize(true);
		}

		// Token: 0x06000579 RID: 1401 RVA: 0x0001D430 File Offset: 0x0001B630
		private void EnsureRectTransformIsSized()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "EnsureRectTransformIsSized", Array.Empty<object>());
			}
			Canvas.ForceUpdateCanvases();
		}

		// Token: 0x040004DB RID: 1243
		private const float ASPECT_RATIO = 1.77f;

		// Token: 0x040004DC RID: 1244
		[Header("UIScreenshotFileInstancesListView")]
		[SerializeField]
		private IntVariable screenshotLimit;

		// Token: 0x040004DD RID: 1245
		private float firstHeightValue;

		// Token: 0x040004DE RID: 1246
		private bool firstHeightValueSet;

		// Token: 0x040004DF RID: 1247
		private bool forceUpdateCanvases = true;
	}
}
