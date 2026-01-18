using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Endless.Core.Test
{
	// Token: 0x020000D4 RID: 212
	public class DynamicFpsInfo : LoadFpsInfo
	{
		// Token: 0x17000097 RID: 151
		// (get) Token: 0x060004D8 RID: 1240 RVA: 0x00016DC5 File Offset: 0x00014FC5
		protected override FpsTestType TestType
		{
			get
			{
				return FpsTestType.Dynamic;
			}
		}

		// Token: 0x060004D9 RID: 1241 RVA: 0x000179B4 File Offset: 0x00015BB4
		public override void StartTest()
		{
			base.StartTest();
			if (this.cart)
			{
				this.cart.m_Position = 0f;
				this.cart.m_Speed = 1f / this.ShotTime;
				this.cart.enabled = true;
			}
		}

		// Token: 0x060004DA RID: 1242 RVA: 0x00017A07 File Offset: 0x00015C07
		public override void StopTest()
		{
			base.StopTest();
			if (this.cart)
			{
				this.cart.enabled = false;
			}
		}

		// Token: 0x04000336 RID: 822
		[SerializeField]
		private CinemachineDollyCart cart;
	}
}
