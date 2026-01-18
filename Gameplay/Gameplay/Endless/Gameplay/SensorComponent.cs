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

namespace Endless.Gameplay
{
	// Token: 0x02000357 RID: 855
	public class SensorComponent : EndlessBehaviour, IStartSubscriber, IGameEndSubscriber, NetClock.ISimulateFrameEnvironmentSubscriber, IPersistantStateSubscriber, IComponentBase, IScriptInjector, IAwarenessComponent, IUpdateComponent
	{
		// Token: 0x0600158A RID: 5514 RVA: 0x00066634 File Offset: 0x00064834
		private void HandlePerceptionResult()
		{
			this.awaitingResponse = false;
			if (this.sensorTriggeredThisFrame)
			{
				return;
			}
			bool flag = false;
			TeamComponent teamComponent;
			Team team = (this.WorldObject.TryGetUserComponent<TeamComponent>(out teamComponent) ? teamComponent.Team : Team.Neutral);
			HashSet<Context> hashSet = new HashSet<Context>();
			foreach (PerceptionResult perceptionResult in this.results)
			{
				HittableComponent hittableComponent = perceptionResult.HittableComponent;
				bool flag2;
				if (hittableComponent == null)
				{
					flag2 = null != null;
				}
				else
				{
					WorldObject worldObject = hittableComponent.WorldObject;
					flag2 = ((worldObject != null) ? worldObject.Context : null) != null;
				}
				if (flag2)
				{
					hashSet.Add(perceptionResult.HittableComponent.WorldObject.Context);
				}
			}
			IEnumerable<Context> enumerable = hashSet.Except(this.previouslyPerceivedContexts);
			IEnumerable<Context> enumerable2 = this.previouslyPerceivedContexts.Except(hashSet);
			foreach (Context context in enumerable)
			{
				object[] array;
				this.scriptComponent.TryExecuteFunction("OnEnterSensor", out array, new object[] { context });
			}
			foreach (Context context2 in enumerable2)
			{
				object[] array;
				this.scriptComponent.TryExecuteFunction("OnExitSensor", out array, new object[] { context2 });
			}
			this.previouslyPerceivedContexts = hashSet;
			if (this.results.Count > 0)
			{
				if (this.runtimeSensorSettings.TeamSense == TeamSense.All || (team == Team.Neutral && this.runtimeSensorSettings.TeamSense == TeamSense.Friendly))
				{
					using (List<PerceptionResult>.Enumerator enumerator = this.results.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							PerceptionResult perceptionResult2 = enumerator.Current;
							if (perceptionResult2.Awareness > 0f)
							{
								this.mostRecentContextSensed = perceptionResult2.HittableComponent.WorldObject.Context;
								flag = true;
								break;
							}
						}
						goto IL_02A1;
					}
				}
				if (this.runtimeSensorSettings.TeamSense == TeamSense.Enemy)
				{
					using (List<PerceptionResult>.Enumerator enumerator = this.results.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							PerceptionResult perceptionResult3 = enumerator.Current;
							if (perceptionResult3.Awareness > 0f && perceptionResult3.HittableComponent.Team.IsHostileTo(team))
							{
								flag = true;
								break;
							}
						}
						goto IL_02A1;
					}
				}
				if (this.runtimeSensorSettings.TeamSense == TeamSense.Friendly)
				{
					foreach (PerceptionResult perceptionResult4 in this.results)
					{
						if (perceptionResult4.Awareness > 0f && perceptionResult4.HittableComponent.Team == team)
						{
							flag = true;
							break;
						}
					}
				}
			}
			IL_02A1:
			this.sensorTriggeredThisFrame = flag;
		}

		// Token: 0x0600158B RID: 5515 RVA: 0x00066938 File Offset: 0x00064B38
		void IStartSubscriber.EndlessStart()
		{
			if (NetworkManager.Singleton.IsServer)
			{
				UnifiedStateUpdater.RegisterUpdateComponent(this);
				NetClock.Register(this);
			}
		}

		// Token: 0x0600158C RID: 5516 RVA: 0x00066952 File Offset: 0x00064B52
		void IGameEndSubscriber.EndlessGameEnd()
		{
			if (NetworkManager.Singleton.IsServer)
			{
				UnifiedStateUpdater.UnregisterUpdateComponent(this);
				NetClock.Unregister(this);
			}
		}

		// Token: 0x0600158D RID: 5517 RVA: 0x0006696C File Offset: 0x00064B6C
		public void SimulateFrameEnvironment(uint frame)
		{
			if (this.sensorTriggeredThisFrame)
			{
				object[] array;
				this.scriptComponent.TryExecuteFunction("OnSensorTriggered", out array, new object[] { this.mostRecentContextSensed });
			}
			this.sensorTriggeredThisFrame = false;
		}

		// Token: 0x17000480 RID: 1152
		// (get) Token: 0x0600158E RID: 5518 RVA: 0x00017586 File Offset: 0x00015786
		public bool ShouldSaveAndLoad
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600158F RID: 5519 RVA: 0x000669AA File Offset: 0x00064BAA
		public object GetSaveState()
		{
			return this.runtimeSensorSettings;
		}

		// Token: 0x06001590 RID: 5520 RVA: 0x000669B7 File Offset: 0x00064BB7
		public void LoadState(object loadedState)
		{
			if (loadedState != null)
			{
				this.runtimeSensorSettings = (SensorComponent.SensorSettings)loadedState;
			}
		}

		// Token: 0x17000481 RID: 1153
		// (get) Token: 0x06001591 RID: 5521 RVA: 0x000669C8 File Offset: 0x00064BC8
		// (set) Token: 0x06001592 RID: 5522 RVA: 0x000669D0 File Offset: 0x00064BD0
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000482 RID: 1154
		// (get) Token: 0x06001593 RID: 5523 RVA: 0x000669D9 File Offset: 0x00064BD9
		Type IComponentBase.ComponentReferenceType
		{
			get
			{
				return typeof(SensorReferences);
			}
		}

		// Token: 0x06001594 RID: 5524 RVA: 0x000669E5 File Offset: 0x00064BE5
		void IComponentBase.ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.references = (SensorReferences)referenceBase;
		}

		// Token: 0x06001595 RID: 5525 RVA: 0x000669F3 File Offset: 0x00064BF3
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x06001596 RID: 5526 RVA: 0x000669FC File Offset: 0x00064BFC
		public void PrefabInitialize(WorldObject prefabWorldObject, bool isServer)
		{
			this.WorldObject = prefabWorldObject;
			this.runtimeSensorSettings = this.defaultSensorSettings;
		}

		// Token: 0x17000483 RID: 1155
		// (get) Token: 0x06001597 RID: 5527 RVA: 0x00066A14 File Offset: 0x00064C14
		object IScriptInjector.LuaObject
		{
			get
			{
				Sensor sensor;
				if ((sensor = this.luaInterface) == null)
				{
					sensor = (this.luaInterface = new Sensor(this));
				}
				return sensor;
			}
		}

		// Token: 0x17000484 RID: 1156
		// (get) Token: 0x06001598 RID: 5528 RVA: 0x00066A3A File Offset: 0x00064C3A
		Type IScriptInjector.LuaObjectType
		{
			get
			{
				return typeof(Sensor);
			}
		}

		// Token: 0x06001599 RID: 5529 RVA: 0x00066A46 File Offset: 0x00064C46
		void IScriptInjector.ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x0600159A RID: 5530 RVA: 0x00066A50 File Offset: 0x00064C50
		public void UpdateAwareness()
		{
			if (this.awaitingResponse || !this.references.sensorPoint)
			{
				return;
			}
			PerceptionRequest perceptionRequest;
			if (this.runtimeSensorSettings.Shape == SenseShape.Box)
			{
				perceptionRequest = new PerceptionRequest
				{
					IsBoxcast = true,
					Position = this.references.sensorPoint.position,
					LookVector = this.references.sensorPoint.forward,
					MaxDistance = this.runtimeSensorSettings.Distance,
					VerticalValue = this.runtimeSensorSettings.ExtentsVertical,
					HorizontalValue = this.runtimeSensorSettings.ExtentsHorizontal,
					ProximityDistance = 0f,
					UseXray = this.runtimeSensorSettings.XRay,
					PerceptionResults = this.results,
					PerceptionUpdatedCallback = new Action(this.HandlePerceptionResult)
				};
			}
			else
			{
				perceptionRequest = new PerceptionRequest
				{
					IsBoxcast = false,
					Position = this.references.sensorPoint.position,
					LookVector = this.references.sensorPoint.forward,
					MaxDistance = this.runtimeSensorSettings.Distance,
					VerticalValue = this.runtimeSensorSettings.VerticalAngle,
					HorizontalValue = this.runtimeSensorSettings.HorizontalAngle,
					ProximityDistance = 0f,
					UseXray = this.runtimeSensorSettings.XRay,
					PerceptionResults = this.results,
					PerceptionUpdatedCallback = new Action(this.HandlePerceptionResult)
				};
			}
			MonoBehaviourSingleton<PerceptionManager>.Instance.RequestPerception(perceptionRequest);
			this.awaitingResponse = true;
		}

		// Token: 0x04001196 RID: 4502
		private const string SENSOR_TRIGGERED_EVENT = "OnSensorTriggered";

		// Token: 0x04001197 RID: 4503
		private const string CONTEXT_ENTER_EVENT = "OnEnterSensor";

		// Token: 0x04001198 RID: 4504
		private const string CONTEXT_EXIT_EVENT = "OnExitSensor";

		// Token: 0x04001199 RID: 4505
		private bool sensorTriggeredThisFrame;

		// Token: 0x0400119A RID: 4506
		[SerializeField]
		private SensorComponent.SensorSettings defaultSensorSettings;

		// Token: 0x0400119B RID: 4507
		[SerializeField]
		[HideInInspector]
		internal SensorComponent.SensorSettings runtimeSensorSettings;

		// Token: 0x0400119C RID: 4508
		private readonly List<PerceptionResult> results = new List<PerceptionResult>();

		// Token: 0x0400119D RID: 4509
		private bool awaitingResponse;

		// Token: 0x0400119E RID: 4510
		private HashSet<Context> previouslyPerceivedContexts = new HashSet<Context>();

		// Token: 0x0400119F RID: 4511
		private Context mostRecentContextSensed;

		// Token: 0x040011A1 RID: 4513
		[SerializeField]
		[HideInInspector]
		private SensorReferences references;

		// Token: 0x040011A2 RID: 4514
		private Sensor luaInterface;

		// Token: 0x040011A3 RID: 4515
		private EndlessScriptComponent scriptComponent;

		// Token: 0x02000358 RID: 856
		[Serializable]
		public struct SensorSettings
		{
			// Token: 0x040011A4 RID: 4516
			public float VerticalAngle;

			// Token: 0x040011A5 RID: 4517
			public float HorizontalAngle;

			// Token: 0x040011A6 RID: 4518
			public float ExtentsVertical;

			// Token: 0x040011A7 RID: 4519
			public float ExtentsHorizontal;

			// Token: 0x040011A8 RID: 4520
			public float Distance;

			// Token: 0x040011A9 RID: 4521
			public bool XRay;

			// Token: 0x040011AA RID: 4522
			public TeamSense TeamSense;

			// Token: 0x040011AB RID: 4523
			public SenseShape Shape;
		}
	}
}
