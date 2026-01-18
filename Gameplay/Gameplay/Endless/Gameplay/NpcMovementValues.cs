using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000208 RID: 520
	public static class NpcMovementValues
	{
		// Token: 0x17000204 RID: 516
		// (get) Token: 0x06000AC6 RID: 2758 RVA: 0x0003AA7C File Offset: 0x00038C7C
		public static float MaxVerticalVelocity
		{
			get
			{
				return 5.7f;
			}
		}

		// Token: 0x17000205 RID: 517
		// (get) Token: 0x06000AC7 RID: 2759 RVA: 0x0003AA83 File Offset: 0x00038C83
		public static float MaxHorizontalVelocity
		{
			get
			{
				return 3.3f;
			}
		}

		// Token: 0x17000206 RID: 518
		// (get) Token: 0x06000AC8 RID: 2760 RVA: 0x0003AA8A File Offset: 0x00038C8A
		public static float Gravity
		{
			get
			{
				return 9.81f;
			}
		}

		// Token: 0x17000207 RID: 519
		// (get) Token: 0x06000AC9 RID: 2761 RVA: 0x0003AA91 File Offset: 0x00038C91
		public static LayerMask JumpSweepMask
		{
			get
			{
				return LayerMask.GetMask(new string[] { "Default" });
			}
		}

		// Token: 0x17000208 RID: 520
		// (get) Token: 0x06000ACA RID: 2762 RVA: 0x0003AAAB File Offset: 0x00038CAB
		public static float NpcHeight
		{
			get
			{
				return 1.6f;
			}
		}

		// Token: 0x17000209 RID: 521
		// (get) Token: 0x06000ACB RID: 2763 RVA: 0x0003AAB2 File Offset: 0x00038CB2
		public static float NpcRadius
		{
			get
			{
				return 0.18f;
			}
		}

		// Token: 0x1700020A RID: 522
		// (get) Token: 0x06000ACC RID: 2764 RVA: 0x0003AAB9 File Offset: 0x00038CB9
		public static float JumpCostScalar
		{
			get
			{
				return 2f;
			}
		}

		// Token: 0x1700020B RID: 523
		// (get) Token: 0x06000ACD RID: 2765 RVA: 0x0003AAC0 File Offset: 0x00038CC0
		public static float DropCostScalar
		{
			get
			{
				return 2.5f;
			}
		}
	}
}
