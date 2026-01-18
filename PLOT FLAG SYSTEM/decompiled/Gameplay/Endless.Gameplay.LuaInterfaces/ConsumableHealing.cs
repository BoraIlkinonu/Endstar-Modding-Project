using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class ConsumableHealing
{
	private ConsumableHealingItem consumableHealingItem;

	public ConsumableHealing(ConsumableHealingItem consumableHealingItem)
	{
		this.consumableHealingItem = consumableHealingItem;
	}

	public void SetHealAmount(Context instigator, int amount)
	{
		consumableHealingItem.HealAmount = amount;
	}

	public int GetHealAmount()
	{
		return consumableHealingItem.HealAmount;
	}
}
