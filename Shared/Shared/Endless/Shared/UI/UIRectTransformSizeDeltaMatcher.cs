using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x0200024B RID: 587
	[ExecuteAlways]
	[RequireComponent(typeof(RectTransform))]
	public class UIRectTransformSizeDeltaMatcher : UIBehaviour, ILayoutController
	{
		// Token: 0x170002CF RID: 719
		// (get) Token: 0x06000EE9 RID: 3817 RVA: 0x000401D6 File Offset: 0x0003E3D6
		private RectTransform RectTransform
		{
			get
			{
				if (!this.rectTransform)
				{
					base.TryGetComponent<RectTransform>(out this.rectTransform);
				}
				return this.rectTransform;
			}
		}

		// Token: 0x06000EEA RID: 3818 RVA: 0x000401F8 File Offset: 0x0003E3F8
		public void SetLayoutHorizontal()
		{
			if (this.matchMode == UIRectTransformSizeDeltaMatcher.MatchModes.Vertical)
			{
				return;
			}
			this.Match();
		}

		// Token: 0x06000EEB RID: 3819 RVA: 0x0004020A File Offset: 0x0003E40A
		public void SetLayoutVertical()
		{
			if (this.matchMode == UIRectTransformSizeDeltaMatcher.MatchModes.Horizontal)
			{
				return;
			}
			this.Match();
		}

		// Token: 0x06000EEC RID: 3820 RVA: 0x0004021C File Offset: 0x0003E41C
		public void Match()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Match", Array.Empty<object>());
			}
			Vector2 sizeDelta = this.RectTransform.sizeDelta;
			if (this.matchMode == UIRectTransformSizeDeltaMatcher.MatchModes.Both || this.matchMode == UIRectTransformSizeDeltaMatcher.MatchModes.Horizontal)
			{
				sizeDelta.x = this.target.sizeDelta.x + this.horizontalPadding;
			}
			if (this.matchMode == UIRectTransformSizeDeltaMatcher.MatchModes.Both || this.matchMode == UIRectTransformSizeDeltaMatcher.MatchModes.Vertical)
			{
				sizeDelta.y = this.target.sizeDelta.y + this.verticalPadding;
			}
			this.RectTransform.sizeDelta = sizeDelta;
		}

		// Token: 0x0400095E RID: 2398
		[SerializeField]
		private RectTransform target;

		// Token: 0x0400095F RID: 2399
		[SerializeField]
		private float horizontalPadding;

		// Token: 0x04000960 RID: 2400
		[SerializeField]
		private float verticalPadding;

		// Token: 0x04000961 RID: 2401
		[SerializeField]
		private UIRectTransformSizeDeltaMatcher.MatchModes matchMode;

		// Token: 0x04000962 RID: 2402
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000963 RID: 2403
		private RectTransform rectTransform;

		// Token: 0x0200024C RID: 588
		private enum MatchModes
		{
			// Token: 0x04000965 RID: 2405
			Both,
			// Token: 0x04000966 RID: 2406
			Horizontal,
			// Token: 0x04000967 RID: 2407
			Vertical
		}
	}
}
