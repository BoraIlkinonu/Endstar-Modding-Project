using System;
using UnityEngine;

namespace Endless
{
	// Token: 0x02000036 RID: 54
	internal interface INetFrameSynced
	{
		// Token: 0x060000F2 RID: 242
		void RollbackListener(uint frame);

		// Token: 0x060000F3 RID: 243
		void SimulateFrameListener(uint frame);

		// Token: 0x060000F4 RID: 244
		Vector3 GetPositionAtFrame(uint frame);
	}
}
