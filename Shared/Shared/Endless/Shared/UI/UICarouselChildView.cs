using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000113 RID: 275
	[RequireComponent(typeof(UILayoutElement))]
	public class UICarouselChildView : UIGameObject
	{
		// Token: 0x0600069F RID: 1695 RVA: 0x0001C352 File Offset: 0x0001A552
		public void OnTweenToCenter()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnTweenToCenter", Array.Empty<object>());
			}
			this.OnTweenToCenterEvent.Invoke();
		}

		// Token: 0x060006A0 RID: 1696 RVA: 0x0001C377 File Offset: 0x0001A577
		public void OnTweenAwayFromCenter()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnTweenAwayFromCenter", Array.Empty<object>());
			}
			this.OnTweenAwayFromCenterEvent.Invoke();
		}

		// Token: 0x060006A1 RID: 1697 RVA: 0x0001C39C File Offset: 0x0001A59C
		public void OnCentered()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnCentered", Array.Empty<object>());
			}
			this.OnCenteredEvent.Invoke();
		}

		// Token: 0x060006A2 RID: 1698 RVA: 0x0001C3C1 File Offset: 0x0001A5C1
		public void OnLostCenterStatus()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLostCenterStatus", Array.Empty<object>());
			}
			this.OnLostCenterStatusEvent.Invoke();
		}

		// Token: 0x040003D6 RID: 982
		public UnityEvent OnTweenToCenterEvent = new UnityEvent();

		// Token: 0x040003D7 RID: 983
		public UnityEvent OnTweenAwayFromCenterEvent = new UnityEvent();

		// Token: 0x040003D8 RID: 984
		public UnityEvent OnCenteredEvent = new UnityEvent();

		// Token: 0x040003D9 RID: 985
		public UnityEvent OnLostCenterStatusEvent = new UnityEvent();

		// Token: 0x040003DA RID: 986
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
