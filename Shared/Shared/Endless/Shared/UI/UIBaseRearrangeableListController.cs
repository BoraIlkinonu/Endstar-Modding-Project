using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000187 RID: 391
	public class UIBaseRearrangeableListController<T> : UIBaseListController<T>
	{
		// Token: 0x060009B4 RID: 2484 RVA: 0x000296F4 File Offset: 0x000278F4
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnEnable", this);
			}
			UIListCellInstanceUpdateHandler.UpdateWithRectTransform = (Action<RectTransform>)Delegate.Combine(UIListCellInstanceUpdateHandler.UpdateWithRectTransform, new Action<RectTransform>(this.TryToScroll));
		}

		// Token: 0x060009B5 RID: 2485 RVA: 0x00029729 File Offset: 0x00027929
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnDisable", this);
			}
			UIListCellInstanceUpdateHandler.UpdateWithRectTransform = (Action<RectTransform>)Delegate.Remove(UIListCellInstanceUpdateHandler.UpdateWithRectTransform, new Action<RectTransform>(this.TryToScroll));
		}

		// Token: 0x060009B6 RID: 2486 RVA: 0x0002975E File Offset: 0x0002795E
		public override void Validate()
		{
			base.Validate();
			if (this.IgnoreValidation)
			{
				return;
			}
			DebugUtility.DebugIsNull("scrollAreaBackward", this.scrollAreaBackward, this);
			DebugUtility.DebugIsNull("scrollAreaForward", this.scrollAreaForward, this);
		}

		// Token: 0x060009B7 RID: 2487 RVA: 0x00029794 File Offset: 0x00027994
		private void TryToScroll(RectTransform listCellInstance)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log("TryToScroll ( listCellInstance: " + listCellInstance.DebugSafeName(true) + " )", this);
			}
			bool flag = listCellInstance.Overlaps(this.scrollAreaBackward);
			bool flag2 = listCellInstance.Overlaps(this.scrollAreaForward);
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}", new object[] { "anyOverlapWithBackward", flag, "anyOverlapWithForward", flag2 }), this);
			}
			if (!flag && !flag2)
			{
				return;
			}
			if (flag && flag2)
			{
				DebugUtility.LogWarning("Can't scroll in a direction when both anyOverlapWithBackward & anyOverlapWithForward are TRUE!", this);
				return;
			}
			if (flag)
			{
				base.View.ScrollPosition = base.View.ScrollPosition - 1f;
				return;
			}
			base.View.ScrollPosition = base.View.ScrollPosition + 1f;
		}

		// Token: 0x0400061B RID: 1563
		[Header("UIBaseRearrangeableListController")]
		[SerializeField]
		private RectTransform scrollAreaBackward;

		// Token: 0x0400061C RID: 1564
		[SerializeField]
		private RectTransform scrollAreaForward;
	}
}
