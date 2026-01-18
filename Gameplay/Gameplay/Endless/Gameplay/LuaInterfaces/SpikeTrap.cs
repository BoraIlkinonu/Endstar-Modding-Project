using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000464 RID: 1124
	public class SpikeTrap
	{
		// Token: 0x06001C28 RID: 7208 RVA: 0x0007D5B2 File Offset: 0x0007B7B2
		internal SpikeTrap(SpikeTrap spikeTrap)
		{
			this.spikeTrap = spikeTrap;
		}

		// Token: 0x06001C29 RID: 7209 RVA: 0x0007D5C1 File Offset: 0x0007B7C1
		public void Activate(Context instigator)
		{
			this.spikeTrap.triggerOnSense.Value = true;
		}

		// Token: 0x06001C2A RID: 7210 RVA: 0x0007D5D4 File Offset: 0x0007B7D4
		public void Deactivate(Context instigator)
		{
			this.spikeTrap.triggerOnSense.Value = false;
		}

		// Token: 0x06001C2B RID: 7211 RVA: 0x0007D5E7 File Offset: 0x0007B7E7
		public bool GetActive()
		{
			return this.spikeTrap.triggerOnSense.Value;
		}

		// Token: 0x06001C2C RID: 7212 RVA: 0x0007D5F9 File Offset: 0x0007B7F9
		public void SetSpikeDamage(Context instigator, int damage)
		{
			this.spikeTrap.spikeDamage = damage;
		}

		// Token: 0x06001C2D RID: 7213 RVA: 0x0007D607 File Offset: 0x0007B807
		public void Trigger(Context instigator)
		{
			this.spikeTrap.TriggerSpikes();
		}

		// Token: 0x040015C6 RID: 5574
		private SpikeTrap spikeTrap;
	}
}
