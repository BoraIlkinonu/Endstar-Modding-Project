using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class SpikeTrap : EndlessNetworkBehaviour, IGameEndSubscriber, IAwakeSubscriber, NetClock.ISimulateFrameEnvironmentSubscriber, IPersistantStateSubscriber, IBaseType, IComponentBase, IScriptInjector
{
	[Tooltip("Should start at 0, end at 1, and take 1 second")]
	[SerializeField]
	private AnimationCurve stabSpeedCurve;

	[SerializeField]
	private float stabDuration = 1f;

	[Tooltip("Should start at 1, end at 0, and take 1 second")]
	[SerializeField]
	private AnimationCurve retractSpeedCurve;

	[SerializeField]
	private float retractDuration = 1f;

	[SerializeField]
	private WorldTrigger damageTrigger;

	[Header("Frames")]
	[SerializeField]
	private uint triggerDelayFrames = 12u;

	internal NetworkVariable<bool> triggerOnSense = new NetworkVariable<bool>(value: true);

	private Coroutine stabCoroutine;

	private NetworkVariable<uint> triggerFrame = new NetworkVariable<uint>(0u);

	private bool triggeredLocally = true;

	private Context scriptingContext;

	private bool isStabbing;

	internal int spikeDamage = 1;

	public EndlessEvent OnEntityHit = new EndlessEvent();

	public EndlessEvent OnTriggered = new EndlessEvent();

	private WorldCollidable mostRecentOverlap;

	private List<WorldCollidable> hitEntities = new List<WorldCollidable>();

	private Context context;

	[SerializeField]
	[HideInInspector]
	private SpikeTrapReferences references;

	private Endless.Gameplay.LuaInterfaces.SpikeTrap luaInterface;

	private EndlessScriptComponent scriptComponent;

	private uint painFrames => (uint)Mathf.Max(1, Mathf.RoundToInt(stabDuration / NetClock.FixedDeltaTime));

	bool IPersistantStateSubscriber.ShouldSaveAndLoad => true;

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public Type ComponentReferenceType => typeof(SpikeTrapReferences);

	public Context Context => context ?? (context = new Context(WorldObject));

	public object LuaObject
	{
		get
		{
			if (luaInterface == null)
			{
				luaInterface = new Endless.Gameplay.LuaInterfaces.SpikeTrap(this);
			}
			return luaInterface;
		}
	}

	public Type LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.SpikeTrap);

	private void Awake()
	{
		NetworkVariable<uint> networkVariable = triggerFrame;
		networkVariable.OnValueChanged = (NetworkVariable<uint>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<uint>.OnValueChangedDelegate(TriggerFrameUpdated));
	}

	protected override void Start()
	{
		base.Start();
		NetClock.Register(this);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		NetClock.Unregister(this);
	}

	public virtual void EndlessAwake()
	{
		damageTrigger.OnTriggerEnter.AddListener(HandleTriggerEntered);
		damageTrigger.OnTriggerStay.AddListener(HandleTriggerStay);
	}

	private void TriggerFrameUpdated(uint oldValue, uint newValue)
	{
		triggeredLocally = false;
	}

	public void EndlessGameEnd()
	{
		if (stabCoroutine != null)
		{
			StopCoroutine(stabCoroutine);
			stabCoroutine = null;
			MoveSpikes(0f);
		}
		hitEntities.Clear();
		if (base.IsServer)
		{
			triggerOnSense.Value = true;
		}
	}

	private IEnumerator StabCoroutine()
	{
		double frameTime = NetClock.GetFrameTime(triggerFrame.Value);
		OnTriggered.Invoke(mostRecentOverlap ? mostRecentOverlap.WorldObject.Context : WorldObject.Context);
		double num = ((!base.IsServer) ? NetClock.ClientExtrapolatedAppearanceTime : NetClock.LocalNetworkTime);
		float elapsedTime = (float)(num - frameTime);
		for (double totalAnimationTime = stabDuration + retractDuration; (double)elapsedTime < totalAnimationTime; elapsedTime += Time.deltaTime)
		{
			isStabbing = elapsedTime < stabDuration;
			if (isStabbing)
			{
				MoveSpikes(stabSpeedCurve.Evaluate(elapsedTime / stabDuration));
			}
			else
			{
				MoveSpikes(retractSpeedCurve.Evaluate((elapsedTime - stabDuration) / retractDuration));
			}
			yield return null;
		}
		MoveSpikes(0f);
		isStabbing = false;
		stabCoroutine = null;
		hitEntities.Clear();
	}

	private void MoveSpikes(float percentage)
	{
		references.SpikeTransform.position = UnityEngine.Vector3.Lerp(references.RetractedPosition.position, references.StabPosition.position, percentage);
	}

	private void HandleTriggerEntered(WorldCollidable worldCollidable, bool isRollbackFrame)
	{
		HandleOverlap(worldCollidable, isRollbackFrame);
	}

	private void HandleTriggerStay(WorldCollidable worldCollidable, bool isRollbackFrame)
	{
		HandleOverlap(worldCollidable, isRollbackFrame);
	}

	private void HandleOverlap(WorldCollidable worldCollidable, bool isRollbackFrame)
	{
		if (base.IsServer && triggerOnSense.Value)
		{
			TriggerSpikes();
		}
		if (NetClock.CurrentSimulationFrame >= triggerFrame.Value && NetClock.CurrentSimulationFrame < painFrames + triggerFrame.Value)
		{
			TryApplyDamage(worldCollidable);
		}
	}

	private void TryApplyDamage(WorldCollidable hitReference)
	{
		if (!(hitReference.WorldObject == null) && !hitEntities.Contains(hitReference))
		{
			hitEntities.Add(hitReference);
			if (hitReference.WorldObject.TryGetUserComponent<HittableComponent>(out var component) && (base.IsServer || component.WorldObject.NetworkObject.IsOwner))
			{
				OnEntityHit.Invoke(hitReference.WorldObject.Context);
				component.ModifyHealth(new HealthModificationArgs(-spikeDamage, WorldObject));
			}
		}
	}

	internal void TriggerSpikes()
	{
		if (base.IsServer && NetClock.CurrentFrame > triggerFrame.Value && stabCoroutine == null)
		{
			triggerFrame.Value = NetClock.CurrentFrame + triggerDelayFrames;
		}
	}

	public void SimulateFrameEnvironment(uint frame)
	{
		if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying && !triggeredLocally && frame >= triggerFrame.Value)
		{
			if (stabCoroutine != null)
			{
				StopCoroutine(stabCoroutine);
				stabCoroutine = null;
			}
			stabCoroutine = StartCoroutine(StabCoroutine());
			triggeredLocally = true;
		}
	}

	object IPersistantStateSubscriber.GetSaveState()
	{
		return (spikeDamage, triggerOnSense.Value);
	}

	void IPersistantStateSubscriber.LoadState(object loadedState)
	{
		if (loadedState != null)
		{
			(int, bool) tuple = ((int, bool))loadedState;
			(spikeDamage, _) = tuple;
			if (base.IsServer)
			{
				triggerOnSense.Value = tuple.Item2;
			}
		}
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		references = (SpikeTrapReferences)referenceBase;
		if ((bool)references.WorldTriggerArea)
		{
			Collider[] cachedColliders = references.WorldTriggerArea.CachedColliders;
			foreach (Collider obj in cachedColliders)
			{
				obj.isTrigger = true;
				obj.gameObject.AddComponent<WorldTriggerCollider>().Initialize(damageTrigger);
			}
		}
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	protected override void __initializeVariables()
	{
		if (triggerOnSense == null)
		{
			throw new Exception("SpikeTrap.triggerOnSense cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		triggerOnSense.Initialize(this);
		__nameNetworkVariable(triggerOnSense, "triggerOnSense");
		NetworkVariableFields.Add(triggerOnSense);
		if (triggerFrame == null)
		{
			throw new Exception("SpikeTrap.triggerFrame cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		triggerFrame.Initialize(this);
		__nameNetworkVariable(triggerFrame, "triggerFrame");
		NetworkVariableFields.Add(triggerFrame);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "SpikeTrap";
	}
}
