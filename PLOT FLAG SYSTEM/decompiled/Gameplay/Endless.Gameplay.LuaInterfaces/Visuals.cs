using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces;

public class Visuals
{
	private DynamicVisuals visuals;

	internal Visuals(DynamicVisuals visuals)
	{
		this.visuals = visuals;
	}

	public void SetLocalPosition(Context instigator, string transformId, UnityEngine.Vector3 position)
	{
		visuals.SetPositionData(instigator, transformId, position);
	}

	public void SetLocalPosition(Context instigator, string transformId, UnityEngine.Vector3 position, string callbackName)
	{
		visuals.SetPositionData(instigator, transformId, position, callbackName);
	}

	public void SetLocalPosition(Context instigator, string transformId, UnityEngine.Vector3 position, float duration)
	{
		visuals.SetPositionData(instigator, transformId, position, duration);
	}

	public void SetLocalPosition(Context instigator, string transformId, UnityEngine.Vector3 position, float duration, string callbackName)
	{
		visuals.SetPositionData(instigator, transformId, position, duration, callbackName);
	}

	public void SetLocalPositionFromTo(Context instigator, string transformId, UnityEngine.Vector3 positionOne, UnityEngine.Vector3 positionTwo, float duration)
	{
		visuals.SetPositionData(instigator, transformId, positionOne, positionTwo, duration);
	}

	public void SetLocalPositionFromTo(Context instigator, string transformId, UnityEngine.Vector3 positionOne, UnityEngine.Vector3 positionTwo, float duration, string callbackName)
	{
		visuals.SetPositionData(instigator, transformId, positionOne, positionTwo, duration, callbackName);
	}

	public void SetLocalRotation(Context instigator, string transformId, UnityEngine.Vector3 rotation)
	{
		visuals.SetRotationData(transformId, rotation);
	}

	public void SetLocalRotation(Context instigator, string transformId, UnityEngine.Vector3 rotation, string callbackName)
	{
		visuals.SetRotationData(transformId, rotation, instigator, callbackName);
	}

	public void SetLocalRotation(Context instigator, string transformId, UnityEngine.Vector3 rotation, float duration)
	{
		visuals.SetRotationData(transformId, rotation, duration);
	}

	public void SetLocalRotation(Context instigator, string transformId, UnityEngine.Vector3 rotation, float duration, string callbackName)
	{
		visuals.SetRotationData(transformId, rotation, duration, instigator, callbackName);
	}

	public void SetLocalRotationFromTo(Context instigator, string transformId, UnityEngine.Vector3 positionOne, UnityEngine.Vector3 positionTwo, float duration)
	{
		visuals.SetRotationData(transformId, positionOne, positionTwo, duration);
	}

	public void SetLocalRotationFromTo(Context instigator, string transformId, UnityEngine.Vector3 positionOne, UnityEngine.Vector3 positionTwo, float duration, string callbackName)
	{
		visuals.SetRotationData(transformId, positionOne, positionTwo, duration, instigator, callbackName);
	}

	public void SetContinousRotation(Context instigator, string transformId, UnityEngine.Vector3 rotationRate)
	{
		visuals.SetContinuousRotationData(transformId, rotationRate, instigator);
	}

	public void StopContinousRotation(Context instigator, string transformId)
	{
		visuals.SetContinuousRotationData(transformId, UnityEngine.Vector3.zero, instigator);
	}

	public void SetEmissiveColor(Context instigator, string transformId, UnityEngine.Color emissiveColor)
	{
		visuals.SetEmissionColor(transformId, emissiveColor);
	}

	public void SetAlbedoColor(Context instigator, string transformId, UnityEngine.Color albedoColor)
	{
		visuals.SetAlbedoColor(transformId, albedoColor);
	}

	public void SetEnabled(Context instigator, string transformId, bool enabled)
	{
		visuals.SetEnabled(transformId, enabled);
	}
}
