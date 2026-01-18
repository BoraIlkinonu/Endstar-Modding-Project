using System;
using Endless.Data;

namespace Endless.Core.UI
{
	// Token: 0x02000086 RID: 134
	public struct UIInstallationErrorScreenModel
	{
		// Token: 0x060002AD RID: 685 RVA: 0x0000ECCB File Offset: 0x0000CECB
		public UIInstallationErrorScreenModel(ErrorCodes errorCode)
		{
			this.ErrorCode = errorCode;
		}

		// Token: 0x04000202 RID: 514
		public ErrorCodes ErrorCode;
	}
}
