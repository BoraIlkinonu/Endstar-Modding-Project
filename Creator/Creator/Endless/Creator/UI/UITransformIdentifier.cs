using System;
using Endless.Props;

namespace Endless.Creator.UI
{
	// Token: 0x0200018E RID: 398
	public struct UITransformIdentifier
	{
		// Token: 0x060005D3 RID: 1491 RVA: 0x0001E095 File Offset: 0x0001C295
		public UITransformIdentifier(TransformIdentifier transformIdentifier, string displayName)
		{
			this.TransformIdentifier = transformIdentifier;
			this.DisplayName = displayName;
		}

		// Token: 0x04000514 RID: 1300
		public readonly TransformIdentifier TransformIdentifier;

		// Token: 0x04000515 RID: 1301
		public readonly string DisplayName;
	}
}
