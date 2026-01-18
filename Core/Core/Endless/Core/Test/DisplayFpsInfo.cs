using System;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.Test
{
	// Token: 0x020000D3 RID: 211
	public class DisplayFpsInfo : BaseFpsInfo
	{
		// Token: 0x17000095 RID: 149
		// (get) Token: 0x060004D1 RID: 1233 RVA: 0x00017946 File Offset: 0x00015B46
		public override bool IsDone
		{
			get
			{
				return this.isDone;
			}
		}

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x060004D2 RID: 1234 RVA: 0x0001648A File Offset: 0x0001468A
		protected override FpsTestType TestType
		{
			get
			{
				return FpsTestType.Display;
			}
		}

		// Token: 0x060004D3 RID: 1235 RVA: 0x0000229D File Offset: 0x0000049D
		protected override void ProcessFrame_Internal()
		{
		}

		// Token: 0x060004D4 RID: 1236 RVA: 0x0001794E File Offset: 0x00015B4E
		public override void StartTest()
		{
			this.display.Display();
			this.display.OnClosed.AddListener(new UnityAction(this.HandleDisplayClosed));
		}

		// Token: 0x060004D5 RID: 1237 RVA: 0x00017977 File Offset: 0x00015B77
		private void HandleDisplayClosed()
		{
			this.display.OnClosed.RemoveListener(new UnityAction(this.HandleDisplayClosed));
			this.isDone = true;
		}

		// Token: 0x060004D6 RID: 1238 RVA: 0x0001799C File Offset: 0x00015B9C
		public override void StopTest()
		{
			this.display.Clear();
			this.manager.ClearData();
		}

		// Token: 0x04000333 RID: 819
		[SerializeField]
		private FpsTestResultsDisplay display;

		// Token: 0x04000334 RID: 820
		[SerializeField]
		private FpsTestManager manager;

		// Token: 0x04000335 RID: 821
		private bool isDone;
	}
}
