using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class SensorComponent : EndlessBehaviour, IStartSubscriber, IGameEndSubscriber, NetClock.ISimulateFrameEnvironmentSubscriber, IPersistantStateSubscriber, IComponentBase, IScriptInjector, IAwarenessComponent, IUpdateComponent
{
	[Serializable]
	public struct SensorSettings
	{
		public float VerticalAngle;

		public float HorizontalAngle;

		public float ExtentsVertical;

		public float ExtentsHorizontal;

		public float Distance;

		public bool XRay;

		public TeamSense TeamSense;

		public SenseShape Shape;
	}

	private const string SENSOR_TRIGGERED_EVENT = "OnSensorTriggered";

	private const string CONTEXT_ENTER_EVENT = "OnEnterSensor";

	private const string CONTEXT_EXIT_EVENT = "OnExitSensor";

	private bool sensorTriggeredThisFrame;

	[SerializeField]
	private SensorSettings defaultSensorSettings;

	[SerializeField]
	[HideInInspector]
	internal SensorSettings runtimeSensorSettings;

	private readonly List<PerceptionResult> results = new List<PerceptionResult>();

	private bool awaitingResponse;

	private HashSet<Context> previouslyPerceivedContexts = new HashSet<Context>();

	private Context mostRecentContextSensed;

	[SerializeField]
	[HideInInspector]
	private SensorReferences references;

	private Sensor luaInterface;

	private EndlessScriptComponent scriptComponent;

	public bool ShouldSaveAndLoad => true;

	[field: SerializeField]
	public WorldObject WorldObject { get; private set; }

	Type IComponentBase.ComponentReferenceType => typeof(SensorReferences);

	object IScriptInjector.LuaObject => luaInterface ?? (luaInterface = new Sensor(this));

	Type IScriptInjector.LuaObjectType => typeof(Sensor);

	private void HandlePerceptionResult()
	{
		awaitingResponse = false;
		if (sensorTriggeredThisFrame)
		{
			return;
		}
		bool flag = false;
		TeamComponent component;
		Team team = (WorldObject.TryGetUserComponent<TeamComponent>(out component) ? component.Team : Team.Neutral);
		HashSet<Context> hashSet = new HashSet<Context>();
		foreach (PerceptionResult result in results)
		{
			if (result.HittableComponent?.WorldObject?.Context != null)
			{
				hashSet.Add(result.HittableComponent.WorldObject.Context);
			}
		}
		IEnumerable<Context> enumerable = hashSet.Except(previouslyPerceivedContexts);
		IEnumerable<Context> enumerable2 = previouslyPerceivedContexts.Except(hashSet);
		object[] returnValues;
		foreach (Context item in enumerable)
		{
			scriptComponent.TryExecuteFunction("OnEnterSensor", out returnValues, item);
		}
		foreach (Context item2 in enumerable2)
		{
			scriptComponent.TryExecuteFunction("OnExitSensor", out returnValues, item2);
		}
		previouslyPerceivedContexts = hashSet;
		if (results.Count > 0)
		{
			if (runtimeSensorSettings.TeamSense == TeamSense.All || (team == Team.Neutral && runtimeSensorSettings.TeamSense == TeamSense.Friendly))
			{
				foreach (PerceptionResult result2 in results)
				{
					if (result2.Awareness > 0f)
					{
						mostRecentContextSensed = result2.HittableComponent.WorldObject.Context;
						flag = true;
						break;
					}
				}
			}
			else if (runtimeSensorSettings.TeamSense == TeamSense.Enemy)
			{
				foreach (PerceptionResult result3 in results)
				{
					if (result3.Awareness > 0f && result3.HittableComponent.Team.IsHostileTo(team))
					{
						flag = true;
						break;
					}
				}
			}
			else if (runtimeSensorSettings.TeamSense == TeamSense.Friendly)
			{
				foreach (PerceptionResult result4 in results)
				{
					if (result4.Awareness > 0f && result4.HittableComponent.Team == team)
					{
						flag = true;
						break;
					}
				}
			}
		}
		sensorTriggeredThisFrame = flag;
	}

	void IStartSubscriber.EndlessStart()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			UnifiedStateUpdater.RegisterUpdateComponent(this);
			NetClock.Register(this);
		}
	}

	void IGameEndSubscriber.EndlessGameEnd()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			UnifiedStateUpdater.UnregisterUpdateComponent(this);
			NetClock.Unregister(this);
		}
	}

	public void SimulateFrameEnvironment(uint frame)
	{
		if (sensorTriggeredThisFrame)
		{
			scriptComponent.TryExecuteFunction("OnSensorTriggered", out var _, mostRecentContextSensed);
		}
		sensorTriggeredThisFrame = false;
	}

	public object GetSaveState()
	{
		return runtimeSensorSettings;
	}

	public void LoadState(object loadedState)
	{
		if (loadedState != null)
		{
			runtimeSensorSettings = (SensorSettings)loadedState;
		}
	}

	void IComponentBase.ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		references = (SensorReferences)referenceBase;
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void PrefabInitialize(WorldObject prefabWorldObject, bool isServer)
	{
		WorldObject = prefabWorldObject;
		runtimeSensorSettings = defaultSensorSettings;
	}

	void IScriptInjector.ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	public void UpdateAwareness()
	{
		if (!awaitingResponse && (bool)references.sensorPoint)
		{
			PerceptionRequest request = ((runtimeSensorSettings.Shape != SenseShape.Box) ? new PerceptionRequest
			{
				IsBoxcast = false,
				Position = references.sensorPoint.position,
				LookVector = references.sensorPoint.forward,
				MaxDistance = runtimeSensorSettings.Distance,
				VerticalValue = runtimeSensorSettings.VerticalAngle,
				HorizontalValue = runtimeSensorSettings.HorizontalAngle,
				ProximityDistance = 0f,
				UseXray = runtimeSensorSettings.XRay,
				PerceptionResults = results,
				PerceptionUpdatedCallback = HandlePerceptionResult
			} : new PerceptionRequest
			{
				IsBoxcast = true,
				Position = references.sensorPoint.position,
				LookVector = references.sensorPoint.forward,
				MaxDistance = runtimeSensorSettings.Distance,
				VerticalValue = runtimeSensorSettings.ExtentsVertical,
				HorizontalValue = runtimeSensorSettings.ExtentsHorizontal,
				ProximityDistance = 0f,
				UseXray = runtimeSensorSettings.XRay,
				PerceptionResults = results,
				PerceptionUpdatedCallback = HandlePerceptionResult
			});
			MonoBehaviourSingleton<PerceptionManager>.Instance.RequestPerception(request);
			awaitingResponse = true;
		}
	}
}
