using System;
using System.Collections;
using UnityEngine;

namespace Unity.Cloud.UserReporting.Plugin
{
	// Token: 0x02000025 RID: 37
	public class UnityUserReportingUpdater : IEnumerator
	{
		// Token: 0x0600010A RID: 266 RVA: 0x00005811 File Offset: 0x00003A11
		public UnityUserReportingUpdater()
		{
			this.waitForEndOfFrame = new WaitForEndOfFrame();
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x0600010B RID: 267 RVA: 0x00005824 File Offset: 0x00003A24
		// (set) Token: 0x0600010C RID: 268 RVA: 0x0000582C File Offset: 0x00003A2C
		public object Current { get; private set; }

		// Token: 0x0600010D RID: 269 RVA: 0x00005838 File Offset: 0x00003A38
		public bool MoveNext()
		{
			if (this.step == 0)
			{
				UnityUserReporting.CurrentClient.Update();
				this.Current = null;
				this.step = 1;
				return true;
			}
			if (this.step == 1)
			{
				this.Current = this.waitForEndOfFrame;
				this.step = 2;
				return true;
			}
			if (this.step == 2)
			{
				UnityUserReporting.CurrentClient.UpdateOnEndOfFrame();
				this.Current = null;
				this.step = 3;
				return false;
			}
			return false;
		}

		// Token: 0x0600010E RID: 270 RVA: 0x000058A9 File Offset: 0x00003AA9
		public void Reset()
		{
			this.step = 0;
		}

		// Token: 0x04000089 RID: 137
		private int step;

		// Token: 0x0400008A RID: 138
		private WaitForEndOfFrame waitForEndOfFrame;
	}
}
