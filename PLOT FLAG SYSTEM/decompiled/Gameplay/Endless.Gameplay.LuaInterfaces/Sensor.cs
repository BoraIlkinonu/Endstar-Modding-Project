using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class Sensor
{
	private readonly SensorComponent component;

	internal Sensor(SensorComponent sensorComponent)
	{
		component = sensorComponent;
	}

	public void SetSenseTeam(Context instigator, int sense)
	{
		component.runtimeSensorSettings.TeamSense = (TeamSense)sense;
	}

	public void SetSenseShape(Context instigator, int shape)
	{
		component.runtimeSensorSettings.Shape = (SenseShape)shape;
	}

	public void SetVerticalSenseExtents(Context instigator, float height)
	{
		component.runtimeSensorSettings.ExtentsVertical = height;
	}

	public void SetHorizontalSenseExtents(Context instigator, float width)
	{
		component.runtimeSensorSettings.ExtentsHorizontal = width;
	}

	public void SetVerticalSenseAngle(Context instigator, float angle)
	{
		component.runtimeSensorSettings.VerticalAngle = angle;
	}

	public void SetHorizontalSenseAngle(Context instigator, float angle)
	{
		component.runtimeSensorSettings.HorizontalAngle = angle;
	}

	public void SetSenseDistance(Context instigator, float distance)
	{
		component.runtimeSensorSettings.Distance = distance;
	}

	public void SetSenseXRay(Context instigator, bool xray)
	{
		component.runtimeSensorSettings.XRay = xray;
	}
}
