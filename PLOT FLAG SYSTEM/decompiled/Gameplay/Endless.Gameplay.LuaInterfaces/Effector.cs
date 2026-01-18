using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class Effector
{
	private readonly PeriodicEffector periodicEffector;

	internal Effector(PeriodicEffector periodicEffector)
	{
		this.periodicEffector = periodicEffector;
	}

	public void AddContext(Context instigator, Context target)
	{
		periodicEffector.AddContext(target);
	}

	public void RemoveContext(Context instigator, Context target)
	{
		periodicEffector.RemoveContext(target);
	}

	public void SetInitialInterval(Context instigator, float newInterval)
	{
		periodicEffector.InitialInterval = newInterval;
	}

	public void SetIntervalScalar(Context instigator, float newScalar)
	{
		periodicEffector.IntervalScalar = newScalar;
	}

	public void Deactivate(Context instigator)
	{
		periodicEffector.DeactivateEffector(instigator);
	}

	public void Activate(Context instigator)
	{
		periodicEffector.ActivateEffector(instigator);
	}
}
