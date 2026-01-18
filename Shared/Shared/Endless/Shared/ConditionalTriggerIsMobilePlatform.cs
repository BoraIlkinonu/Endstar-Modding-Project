using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200004D RID: 77
	public class ConditionalTriggerIsMobilePlatform : BaseConditionalTrigger
	{
		// Token: 0x06000293 RID: 659 RVA: 0x0000D3C4 File Offset: 0x0000B5C4
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}.{1}: {2}", "MobileUtility", "IsMobile", MobileUtility.IsMobile), this);
			}
			if (this.inverse ? (!MobileUtility.IsMobile) : MobileUtility.IsMobile)
			{
				this.Trigger.Invoke();
			}
		}

		// Token: 0x0400014F RID: 335
		[Header("ConditionalTriggerIsMobilePlatform")]
		[SerializeField]
		private bool inverse;
	}
}
