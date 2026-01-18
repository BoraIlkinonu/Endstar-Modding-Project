using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class PeriodicEffector : EndlessBehaviour, IGameEndSubscriber, IAwakeSubscriber, IComponentBase, IScriptInjector
{
	[SerializeField]
	private float initialInterval;

	[SerializeField]
	private float intervalScalar;

	[SerializeField]
	private float minimumInterval;

	[SerializeField]
	private float maximumInterval;

	[SerializeField]
	private bool startActive;

	private float syncedInterval;

	private float syncedScalar;

	private bool isActive;

	private readonly HashSet<Context> activeContexts = new HashSet<Context>();

	private readonly Dictionary<Context, Coroutine> effectRoutinesByContext = new Dictionary<Context, Coroutine>();

	private EndlessScriptComponent endlessScriptComponent;

	private object luaObject;

	private bool IsActive
	{
		get
		{
			return isActive;
		}
		set
		{
			isActive = value;
		}
	}

	internal float InitialInterval
	{
		get
		{
			return syncedInterval;
		}
		set
		{
			syncedInterval = Mathf.Clamp(value, minimumInterval, maximumInterval);
		}
	}

	internal float IntervalScalar
	{
		get
		{
			return syncedScalar;
		}
		set
		{
			syncedScalar = Mathf.Clamp(value, 0.1f, float.MaxValue);
		}
	}

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public Context Context { get; private set; }

	public object LuaObject => luaObject ?? (luaObject = new Effector(this));

	public Type LuaObjectType => typeof(Effector);

	internal void AddContext(Context target)
	{
		if (activeContexts.Add(target) && IsActive)
		{
			StartEffectRoutine(target);
		}
	}

	private void StartEffectRoutine(Context target)
	{
		effectRoutinesByContext.Add(target, StartCoroutine(EffectRoutine(target, InitialInterval)));
	}

	private IEnumerator EffectRoutine(Context target, float interval)
	{
		yield return new WaitForSeconds(interval);
		AffectTarget(target);
		effectRoutinesByContext[target] = StartCoroutine(EffectRoutine(target, Mathf.Clamp(interval * IntervalScalar, minimumInterval, maximumInterval)));
	}

	private void AffectTarget(Context target)
	{
		endlessScriptComponent.TryExecuteFunction("AffectContext", out var _, target);
	}

	internal void RemoveContext(Context target)
	{
		if (activeContexts.Remove(target))
		{
			StopEffectRoutine(target);
		}
	}

	private void StopEffectRoutine(Context target)
	{
		if (effectRoutinesByContext.TryGetValue(target, out var value))
		{
			StopCoroutine(value);
			effectRoutinesByContext.Remove(target);
		}
	}

	public void DeactivateEffector(Context _)
	{
		IsActive = false;
		foreach (KeyValuePair<Context, Coroutine> item in effectRoutinesByContext)
		{
			StopCoroutine(item.Value);
		}
		effectRoutinesByContext.Clear();
	}

	public void ActivateEffector(Context _)
	{
		if (IsActive)
		{
			return;
		}
		foreach (Context activeContext in activeContexts)
		{
			StartEffectRoutine(activeContext);
		}
	}

	public void EndlessGameEnd()
	{
		DeactivateEffector(null);
	}

	public void EndlessAwake()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			InitialInterval = initialInterval;
			IntervalScalar = intervalScalar;
			IsActive = startActive;
		}
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void InitializeContext()
	{
		Context = new Context(WorldObject);
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		this.endlessScriptComponent = endlessScriptComponent;
	}
}
