using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class BouncePad : EndlessNetworkBehaviour, NetClock.ISimulateFrameEnvironmentSubscriber, IStartSubscriber, IGameEndSubscriber, IBaseType, IComponentBase, IScriptInjector
{
	internal struct ToggleInfo : INetworkSerializable
	{
		public uint Frame;

		public bool Active;

		public void NetworkSerialize<ToggleInfo>(BufferSerializer<ToggleInfo> serializer) where ToggleInfo : IReaderWriter
		{
			serializer.SerializeValue(ref Frame, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref Active, default(FastBufferWriter.ForPrimitives));
		}
	}

	internal struct HeightInfo : INetworkSerializable
	{
		public uint Frame;

		public int Height;

		public void NetworkSerialize<HeightInfo>(BufferSerializer<HeightInfo> serializer) where HeightInfo : IReaderWriter
		{
			serializer.SerializeValue(ref Frame, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref Height, default(FastBufferWriter.ForPrimitives));
		}
	}

	private const float BASE_HEIGHT_ADD = 0.5f;

	private const uint TRIGGER_FRAME_DELAY = 5u;

	private const int MIN_BOUNCE_HEIGHT = 1;

	private const int MAX_BOUNCE_HEIGHT = 35;

	[SerializeField]
	private WorldTrigger worldTrigger;

	internal NetworkVariable<ToggleInfo> toggleInfo = new NetworkVariable<ToggleInfo>();

	internal NetworkVariable<HeightInfo> heightInfo = new NetworkVariable<HeightInfo>();

	private List<WorldCollidable> runtimeOverlaps = new List<WorldCollidable>();

	private bool runtimeActive = true;

	private int runtimeHeight;

	private bool playing;

	private Context context;

	[SerializeField]
	[HideInInspector]
	private BouncePadReferences references;

	private Endless.Gameplay.LuaInterfaces.BouncePad luaInterface;

	private EndlessScriptComponent scriptComponent;

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public Type ComponentReferenceType => typeof(BouncePadReferences);

	public Context Context => context ?? (context = new Context(WorldObject));

	public object LuaObject
	{
		get
		{
			if (luaInterface == null)
			{
				luaInterface = new Endless.Gameplay.LuaInterfaces.BouncePad(this);
			}
			return luaInterface;
		}
	}

	public Type LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.BouncePad);

	private void Awake()
	{
		worldTrigger.OnTriggerEnter.AddListener(HandleEntered);
		worldTrigger.OnTriggerExit.AddListener(HandleExited);
		worldTrigger.OnTriggerEnter_Unsimulated.AddListener(HandleEntered_Unsimulated);
	}

	private void OnEnable()
	{
		NetClock.Register(this);
	}

	private void OnDisable()
	{
		NetClock.Unregister(this);
	}

	private void HandleEntered_Unsimulated(WorldCollidable collidedObject)
	{
		if (playing && runtimeActive && NetClock.CurrentSimulationFrame == NetClock.CurrentFrame)
		{
			LocalBounceEffect();
		}
	}

	public void Activate()
	{
		if (base.IsServer)
		{
			toggleInfo.Value = new ToggleInfo
			{
				Frame = NetClock.CurrentFrame + 5,
				Active = true
			};
		}
	}

	public void Deactivate()
	{
		if (base.IsServer)
		{
			toggleInfo.Value = new ToggleInfo
			{
				Frame = NetClock.CurrentFrame + 5,
				Active = false
			};
		}
	}

	private void ActivateLocal()
	{
		bool num = !runtimeActive;
		runtimeActive = true;
		if (!num)
		{
			return;
		}
		foreach (WorldCollidable runtimeOverlap in runtimeOverlaps)
		{
			TriggerForce(runtimeOverlap, triggerVisuals: true);
		}
	}

	private void DeactivateLocal()
	{
		runtimeActive = false;
	}

	private void ToggleActivatedLocal()
	{
		if (runtimeActive)
		{
			DeactivateLocal();
		}
		else
		{
			ActivateLocal();
		}
	}

	public void ToggleActivated()
	{
		if (base.IsServer)
		{
			if (runtimeActive)
			{
				Deactivate();
			}
			else
			{
				Activate();
			}
		}
	}

	private void HandleEntered(WorldCollidable collidedObject, bool isRollbackFrame)
	{
		if (base.IsServer && (bool)collidedObject.WorldObject)
		{
			scriptComponent.ExecuteFunction("OnContextOverlapped", collidedObject.WorldObject.Context);
		}
		runtimeOverlaps.Add(collidedObject);
		TriggerForce(collidedObject, !isRollbackFrame);
	}

	private void TriggerForce(WorldCollidable collidedObject, bool triggerVisuals)
	{
		if (!runtimeActive)
		{
			return;
		}
		if (base.IsServer)
		{
			LocalBounceEffect();
		}
		else if (collidedObject.IsOwner && triggerVisuals)
		{
			Invoke("LocalBounceEffect", NetClock.FixedDeltaTime * 2f);
		}
		if (collidedObject.PhysicsTaker != null)
		{
			float force = CalculateForceToReachCellHeight(runtimeHeight, collidedObject.PhysicsTaker) / collidedObject.PhysicsTaker.BlastForceMultiplier;
			UnityEngine.Vector3 directionNormal = (references.BounceNormal ? references.BounceNormal.up : UnityEngine.Vector3.up);
			collidedObject.PhysicsTaker.TakePhysicsForce(force, directionNormal, NetClock.CurrentSimulationFrame + 1, base.NetworkObject.NetworkObjectId, forceFreeFall: false, friendlyForce: true, applyRandomTorque: true);
			PlayerController component = collidedObject.GetComponent<PlayerController>();
			if ((bool)component)
			{
				component.CurrentState.BouncedThisFrame();
			}
			if (base.IsServer && (bool)collidedObject.WorldObject)
			{
				scriptComponent.ExecuteFunction("OnContextBounced", collidedObject.WorldObject.Context);
			}
		}
	}

	private void LocalBounceEffect()
	{
		references.BounceParticleEffect.Play();
	}

	private void HandleExited(WorldCollidable collidedObject, bool isRollbackFrame)
	{
		runtimeOverlaps.Remove(collidedObject);
	}

	private float CalculateForceToReachCellHeight(float cellHeight, IPhysicsTaker physicsTaker)
	{
		float num = cellHeight + 0.5f;
		return (Mathf.Sqrt(2f * physicsTaker.GravityAccelerationRate * num) - physicsTaker.CurrentVelocity.y) * Mathf.Pow(physicsTaker.Mass, 2f);
	}

	internal void SetBounceHeight(int value)
	{
		if (base.IsServer)
		{
			heightInfo.Value = new HeightInfo
			{
				Frame = NetClock.CurrentFrame + 5,
				Height = Mathf.Clamp(value, 1, 35)
			};
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (base.IsServer)
		{
			Activate();
			heightInfo.Value = new HeightInfo
			{
				Frame = NetClock.CurrentFrame,
				Height = heightInfo.Value.Height
			};
		}
	}

	public void SimulateFrameEnvironment(uint frame)
	{
		if (!playing || (!base.IsServer && frame != NetClock.CurrentFrame))
		{
			return;
		}
		if (frame >= toggleInfo.Value.Frame)
		{
			if (runtimeActive != toggleInfo.Value.Active)
			{
				ToggleActivatedLocal();
			}
		}
		else if (runtimeActive == toggleInfo.Value.Active)
		{
			ToggleActivatedLocal();
		}
		if (frame >= heightInfo.Value.Frame)
		{
			runtimeHeight = heightInfo.Value.Height;
		}
	}

	void IStartSubscriber.EndlessStart()
	{
		playing = true;
	}

	void IGameEndSubscriber.EndlessGameEnd()
	{
		playing = false;
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		references = (BouncePadReferences)referenceBase;
		if ((bool)references.WorldTriggerArea)
		{
			Collider[] cachedColliders = references.WorldTriggerArea.CachedColliders;
			foreach (Collider obj in cachedColliders)
			{
				obj.isTrigger = true;
				obj.gameObject.AddComponent<WorldTriggerCollider>().Initialize(worldTrigger);
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
		if (toggleInfo == null)
		{
			throw new Exception("BouncePad.toggleInfo cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		toggleInfo.Initialize(this);
		__nameNetworkVariable(toggleInfo, "toggleInfo");
		NetworkVariableFields.Add(toggleInfo);
		if (heightInfo == null)
		{
			throw new Exception("BouncePad.heightInfo cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		heightInfo.Initialize(this);
		__nameNetworkVariable(heightInfo, "heightInfo");
		NetworkVariableFields.Add(heightInfo);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "BouncePad";
	}
}
