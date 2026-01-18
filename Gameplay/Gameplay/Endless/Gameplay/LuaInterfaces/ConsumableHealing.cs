using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000447 RID: 1095
	public class ConsumableHealing
	{
		// Token: 0x06001B73 RID: 7027 RVA: 0x0007C4EC File Offset: 0x0007A6EC
		public ConsumableHealing(ConsumableHealingItem consumableHealingItem)
		{
			this.consumableHealingItem = consumableHealingItem;
		}

		// Token: 0x06001B74 RID: 7028 RVA: 0x0007C4FB File Offset: 0x0007A6FB
		public void SetHealAmount(Context instigator, int amount)
		{
			this.consumableHealingItem.HealAmount = amount;
		}

		// Token: 0x06001B75 RID: 7029 RVA: 0x0007C509 File Offset: 0x0007A709
		public int GetHealAmount()
		{
			return this.consumableHealingItem.HealAmount;
		}

		// Token: 0x040015AB RID: 5547
		private ConsumableHealingItem consumableHealingItem;
	}
}
