using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class Targeter
{
	private readonly TargeterComponent targeter;

	internal Targeter(TargeterComponent targeterComponent)
	{
		targeter = targeterComponent;
	}

	public void SetMaxLookDistance(Context instigator, float distance)
	{
		targeter.MaxLookDistance = distance;
	}

	public void SetVerticalViewAngle(Context instigator, float angle)
	{
		targeter.VerticalViewAngle = angle;
	}

	public void SetHorizontalViewWidth(Context instigator, float angle)
	{
		targeter.HorizontalViewAngle = angle;
	}

	public void SetTargetSelectionMode(Context instigator, int targetSelectionMode)
	{
		targeter.TargetSelectionMode = (TargetSelectionMode)targetSelectionMode;
	}

	public void SetTargetPrioritizationMode(Context instigator, int targetPrioritizationMode)
	{
		targeter.TargetPrioritizationMode = (TargetPrioritizationMode)targetPrioritizationMode;
	}

	public void SetCurrentTargetHandlingMode(Context instigator, int currentTargetHandlingMode)
	{
		targeter.CurrentTargetHandlingMode = (CurrentTargetHandlingMode)currentTargetHandlingMode;
	}

	public void SetTargetHostilityMode(Context instigator, int targetHostilityMode)
	{
		targeter.TargetHostilityMode = (TargetHostilityMode)targetHostilityMode;
	}

	public void SetZeroHealthTargetMode(Context instigator, int zeroHealthTargetMode)
	{
		targeter.ZeroHealthTargetMode = (ZeroHealthTargetMode)zeroHealthTargetMode;
	}

	public void UseXRayLos(Context instigator, bool useXray)
	{
		targeter.useXRayLos = useXray;
	}

	public void IsNavigationDependent(bool isNavigationDependent)
	{
		targeter.isNavigationDependent = isNavigationDependent;
	}

	public void SetAwarenessLossRate(Context instigator, float newLossRate)
	{
		targeter.awarenessLossRate = newLossRate;
	}

	public float GetAwarenessLossRate()
	{
		return targeter.awarenessLossRate;
	}
}
