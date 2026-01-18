using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class SpikeTrap
{
	private Endless.Gameplay.SpikeTrap spikeTrap;

	internal SpikeTrap(Endless.Gameplay.SpikeTrap spikeTrap)
	{
		this.spikeTrap = spikeTrap;
	}

	public void Activate(Context instigator)
	{
		spikeTrap.triggerOnSense.Value = true;
	}

	public void Deactivate(Context instigator)
	{
		spikeTrap.triggerOnSense.Value = false;
	}

	public bool GetActive()
	{
		return spikeTrap.triggerOnSense.Value;
	}

	public void SetSpikeDamage(Context instigator, int damage)
	{
		spikeTrap.spikeDamage = damage;
	}

	public void Trigger(Context instigator)
	{
		spikeTrap.TriggerSpikes();
	}
}
