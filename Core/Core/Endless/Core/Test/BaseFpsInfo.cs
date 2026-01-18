using System;
using UnityEngine;

namespace Endless.Core.Test
{
	// Token: 0x020000DE RID: 222
	public abstract class BaseFpsInfo : MonoBehaviour
	{
		// Token: 0x1700009D RID: 157
		// (get) Token: 0x06000500 RID: 1280
		public abstract bool IsDone { get; }

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x06000501 RID: 1281 RVA: 0x00003CF2 File Offset: 0x00001EF2
		protected virtual FpsTestType TestType
		{
			get
			{
				return FpsTestType.Load;
			}
		}

		// Token: 0x06000502 RID: 1282 RVA: 0x000184D9 File Offset: 0x000166D9
		public void ProcessFrame()
		{
			this.FpsInfo.Frames.Add(Time.deltaTime);
			this.ProcessFrame_Internal();
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x000184F6 File Offset: 0x000166F6
		protected virtual void Awake()
		{
			this.FpsInfo.SectionName = base.gameObject.name;
			this.FpsInfo.TestType = this.TestType;
		}

		// Token: 0x06000504 RID: 1284
		protected abstract void ProcessFrame_Internal();

		// Token: 0x06000505 RID: 1285
		public abstract void StartTest();

		// Token: 0x06000506 RID: 1286
		public abstract void StopTest();

		// Token: 0x06000507 RID: 1287 RVA: 0x0001851F File Offset: 0x0001671F
		[ContextMenu("Restart Test")]
		public void ForceTestRestart()
		{
			this.StartTest();
		}

		// Token: 0x0400036A RID: 874
		[HideInInspector]
		public FpsInfo FpsInfo;
	}
}
