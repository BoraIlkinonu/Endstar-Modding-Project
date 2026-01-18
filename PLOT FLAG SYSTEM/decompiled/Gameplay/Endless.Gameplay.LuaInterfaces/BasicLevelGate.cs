using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class BasicLevelGate
{
	private Endless.Gameplay.BasicLevelGate levelGate;

	internal BasicLevelGate(Endless.Gameplay.BasicLevelGate levelGate)
	{
		this.levelGate = levelGate;
	}

	public void PlayerReady(Context playerContext)
	{
		levelGate.PlayerReady(playerContext);
	}

	public void PlayerUnready(Context playerContext)
	{
		levelGate.PlayerUnready(playerContext);
	}

	public void TriggerGate(Context context)
	{
		levelGate.StartCountdown(context);
	}

	public void ToggleReadyParticles(Context context, bool ready)
	{
		levelGate.ToggleReadyParticles(ready);
	}

	public bool GetIsValidDestination()
	{
		return levelGate.IsValidDestination();
	}
}
