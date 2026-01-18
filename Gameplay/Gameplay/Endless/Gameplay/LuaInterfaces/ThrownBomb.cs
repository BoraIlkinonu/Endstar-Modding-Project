using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000469 RID: 1129
	public class ThrownBomb
	{
		// Token: 0x06001C67 RID: 7271 RVA: 0x0007DAA4 File Offset: 0x0007BCA4
		internal ThrownBomb(ThrownBombItem thrownBombItem)
		{
			this.thrownBombItem = thrownBombItem;
		}

		// Token: 0x06001C68 RID: 7272 RVA: 0x0007DAB3 File Offset: 0x0007BCB3
		public void SetDamageAtCenter(Context instigator, int damage)
		{
			this.thrownBombItem.DamageAtCenter = damage;
		}

		// Token: 0x06001C69 RID: 7273 RVA: 0x0007DAC1 File Offset: 0x0007BCC1
		public void SetDamageAtEdge(Context instigator, int damage)
		{
			this.thrownBombItem.DamageAtEdge = damage;
		}

		// Token: 0x06001C6A RID: 7274 RVA: 0x0007DACF File Offset: 0x0007BCCF
		public void SetCenterRadius(Context instigator, float radius)
		{
			this.thrownBombItem.CenterRadius = radius;
		}

		// Token: 0x06001C6B RID: 7275 RVA: 0x0007DADD File Offset: 0x0007BCDD
		public void SetTotalBlastRadius(Context instigator, float radius)
		{
			this.thrownBombItem.TotalBlastRadius = radius;
		}

		// Token: 0x06001C6C RID: 7276 RVA: 0x0007DAEB File Offset: 0x0007BCEB
		public void SetCenterBlastForce(Context instigator, float force)
		{
			this.thrownBombItem.CenterBlastForce = force;
		}

		// Token: 0x06001C6D RID: 7277 RVA: 0x0007DAF9 File Offset: 0x0007BCF9
		public void SetEdgeBlastForce(Context instigator, float force)
		{
			this.thrownBombItem.EdgeBlastForce = force;
		}

		// Token: 0x06001C6E RID: 7278 RVA: 0x0007DB07 File Offset: 0x0007BD07
		public int GetDamageAtCenter()
		{
			return this.thrownBombItem.DamageAtCenter;
		}

		// Token: 0x06001C6F RID: 7279 RVA: 0x0007DB14 File Offset: 0x0007BD14
		public int GetDamageAtEdge()
		{
			return this.thrownBombItem.DamageAtEdge;
		}

		// Token: 0x06001C70 RID: 7280 RVA: 0x0007DB21 File Offset: 0x0007BD21
		public float GetCenterRadius()
		{
			return this.thrownBombItem.CenterRadius;
		}

		// Token: 0x06001C71 RID: 7281 RVA: 0x0007DB2E File Offset: 0x0007BD2E
		public float GetTotalBlastRadius()
		{
			return this.thrownBombItem.TotalBlastRadius;
		}

		// Token: 0x06001C72 RID: 7282 RVA: 0x0007DB3B File Offset: 0x0007BD3B
		public float GetCenterBlastForce()
		{
			return this.thrownBombItem.CenterBlastForce;
		}

		// Token: 0x06001C73 RID: 7283 RVA: 0x0007DB48 File Offset: 0x0007BD48
		public float GetEdgeBlastForce()
		{
			return this.thrownBombItem.EdgeBlastForce;
		}

		// Token: 0x040015CB RID: 5579
		private ThrownBombItem thrownBombItem;
	}
}
