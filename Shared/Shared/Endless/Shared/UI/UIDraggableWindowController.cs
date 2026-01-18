using System;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x02000284 RID: 644
	public class UIDraggableWindowController : UIWindowController
	{
		// Token: 0x06001034 RID: 4148 RVA: 0x000450BA File Offset: 0x000432BA
		public override void Validate()
		{
			base.Validate();
			if (!base.GetComponentInChildren<UIDragPositionController>())
			{
				DebugUtility.LogError("A MonoBehaviour with a type of UIDragPositionController is required here!", base.gameObject);
			}
		}
	}
}
