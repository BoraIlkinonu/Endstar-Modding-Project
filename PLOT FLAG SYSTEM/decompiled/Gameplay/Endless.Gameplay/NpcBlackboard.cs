using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay;

public class NpcBlackboard
{
	public enum Key
	{
		LastPatrol,
		RunSpeed,
		LastWander,
		CanFidget,
		IsRangedAttacker,
		CloseDistance,
		OptimalAttackDistance,
		MeleeDistance,
		NearDistance,
		AroundDistance,
		MaxRangedAttackDistance,
		MaxPerceptionDistance,
		MaxViewAngle,
		ProximityDistance,
		AwarenessGainRate,
		AwarenessLossRate,
		DestinationTolerance,
		JumpPosition,
		BehaviorDestination,
		CommandDestination,
		Rotation,
		TeleportType,
		TeleportPosition,
		TeleportRotation,
		LastIdleFrame,
		OverrideSpeed,
		BoredomTime
	}

	private readonly Dictionary<Type, IDictionary> typeDictionary = new Dictionary<Type, IDictionary>();

	public NpcBlackboard()
	{
		typeDictionary.Add(typeof(int), new Dictionary<Key, int>());
		typeDictionary.Add(typeof(float), new Dictionary<Key, float>());
		typeDictionary.Add(typeof(bool), new Dictionary<Key, bool>());
		typeDictionary.Add(typeof(Vector3), new Dictionary<Key, Vector3>());
		typeDictionary.Add(typeof(Quaternion), new Dictionary<Key, Quaternion>());
		typeDictionary.Add(typeof(uint), new Dictionary<Key, uint>());
		typeDictionary.Add(typeof(object), new Dictionary<Key, object>());
	}

	internal void Set<T>(Key key, T value)
	{
		if (!typeDictionary.TryGetValue(typeof(T), out var value2))
		{
			throw new ArgumentException("Unsupported type");
		}
		((Dictionary<Key, T>)value2)[key] = value;
	}

	public T Get<T>(Key key)
	{
		if (!typeDictionary.TryGetValue(typeof(T), out var value))
		{
			throw new ArgumentException("Unsupported type");
		}
		return ((Dictionary<Key, T>)value)[key];
	}

	public bool TryGet<T>(Key key, out T value)
	{
		if (!typeDictionary.TryGetValue(typeof(T), out var value2))
		{
			throw new ArgumentException("Unsupported type");
		}
		return ((Dictionary<Key, T>)value2).TryGetValue(key, out value);
	}

	public T GetValueOrDefault<T>(Key key, T defaultValue)
	{
		if (!typeDictionary.TryGetValue(typeof(T), out var value))
		{
			throw new ArgumentException("Unsupported type");
		}
		return ((Dictionary<Key, T>)value).GetValueOrDefault(key, defaultValue);
	}

	public void Clear<T>(Key key)
	{
		if (!typeDictionary.TryGetValue(typeof(T), out var value))
		{
			throw new ArgumentException("Unsupported type");
		}
		((Dictionary<Key, T>)value).Remove(key);
	}
}
