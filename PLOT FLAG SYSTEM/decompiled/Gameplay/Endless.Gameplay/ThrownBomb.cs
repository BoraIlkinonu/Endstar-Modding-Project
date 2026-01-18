using System;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class ThrownBomb : EndlessNetworkBehaviour, IThrowable, IGameEndSubscriber, NetClock.ISimulateFrameEnvironmentSubscriber
{
	private const uint BOMB_START_DELAY_FRAMES = 5u;

	private const uint SERVER_DESTROY_FRAME_DELAY = 5u;

	[SerializeField]
	private LayerMask layerMask;

	[SerializeField]
	[Min(1f)]
	private uint frameDelay = 1u;

	private NetworkVariable<int> maxDamage = new NetworkVariable<int>(2);

	private NetworkVariable<int> minDamage = new NetworkVariable<int>(1);

	private NetworkVariable<float> maxDistance = new NetworkVariable<float>(4f);

	private NetworkVariable<float> minDistance = new NetworkVariable<float>(2f);

	private NetworkVariable<float> maxForce = new NetworkVariable<float>(12f);

	private NetworkVariable<float> minForce = new NetworkVariable<float>(3f);

	[Header("Dropoff")]
	[Tooltip("Should start at 1, and end at 0 over a period of 1")]
	[SerializeField]
	private AnimationCurve dropoffCurve;

	[Header("References")]
	[SerializeField]
	private NetworkRigidbodyController networkRigidbodyController;

	private NetworkVariable<uint> startFrame = new NetworkVariable<uint>(0u);

	private NetworkVariable<uint> explosionFrame = new NetworkVariable<uint>(0u);

	private NetworkObject ownerEntity;

	private static Collider[] colliderCache = new Collider[60];

	private readonly HashSet<IPhysicsTaker> hitPhysicsTakers = new HashSet<IPhysicsTaker>();

	private readonly HashSet<HittableComponent> hitHittableComponents = new HashSet<HittableComponent>();

	private bool visible;

	private NetworkObject thrower;

	public NetworkObject OwnerEntity => ownerEntity;

	public WorldObject WorldObject { get; private set; }

	public void InitiateThrow(float force, Vector3 directionNormal, uint currentFrame, NetworkObject thrower, Item sourceItem)
	{
		startFrame.Value = currentFrame + 5;
		explosionFrame.Value = startFrame.Value + frameDelay;
		networkRigidbodyController.TakePhysicsForce(force, directionNormal, currentFrame + 1, thrower.NetworkObjectId);
		ownerEntity = sourceItem.NetworkObject;
		ThrownBombItem thrownBombItem = (ThrownBombItem)sourceItem;
		this.thrower = thrower;
		if ((bool)thrownBombItem)
		{
			maxDamage.Value = thrownBombItem.DamageAtCenter;
			minDamage.Value = thrownBombItem.DamageAtEdge;
			maxDistance.Value = thrownBombItem.TotalBlastRadius;
			minDistance.Value = thrownBombItem.CenterRadius;
			maxForce.Value = thrownBombItem.CenterBlastForce;
			minForce.Value = thrownBombItem.EdgeBlastForce;
		}
	}

	public override void OnNetworkSpawn()
	{
		NetClock.Register(this);
		base.OnNetworkSpawn();
	}

	public override void OnNetworkDespawn()
	{
		NetClock.Unregister(this);
		base.OnNetworkDespawn();
	}

	private void Detonate(uint frame)
	{
		visible = false;
		int num = Physics.OverlapSphereNonAlloc(base.transform.position, maxDistance.Value, colliderCache, layerMask);
		hitPhysicsTakers.Clear();
		hitHittableComponents.Clear();
		for (int i = 0; i < num; i++)
		{
			HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(colliderCache[i]);
			IPhysicsTaker physicsTaker = null;
			physicsTaker = ((!(hittableFromMap != null)) ? colliderCache[i].GetComponent<IPhysicsTaker>() : hittableFromMap.WorldObject.GetComponentInChildren<IPhysicsTaker>());
			if (networkRigidbodyController == (UnityEngine.Object)physicsTaker)
			{
				continue;
			}
			bool flag = physicsTaker != null;
			bool flag2 = hittableFromMap != null;
			if (!(flag || flag2))
			{
				continue;
			}
			Vector3 position = colliderCache[i].transform.position;
			if (flag)
			{
				position += physicsTaker.CenterOfMassOffset;
			}
			float value = Vector3.Distance(base.transform.position, position);
			float time = Mathf.Max(0f, Mathf.InverseLerp(minDistance.Value, maxDistance.Value, value));
			float t = dropoffCurve.Evaluate(time);
			if (flag2 && !hitHittableComponents.Contains(hittableFromMap))
			{
				hitHittableComponents.Add(hittableFromMap);
				if (base.IsServer)
				{
					HealthModificationArgs modificationArgs = new HealthModificationArgs(Mathf.RoundToInt(Mathf.Lerp(minDamage.Value, maxDamage.Value, t)) * -1, thrower ? thrower.GetComponent<WorldObject>() : null);
					hittableFromMap.ModifyHealth(modificationArgs);
				}
				else if (NetClock.CurrentFrame == frame)
				{
					NetworkObject componentInParent = colliderCache[i].GetComponentInParent<NetworkObject>();
					if (componentInParent != null && componentInParent.IsOwner)
					{
						HealthModificationArgs modificationArgs2 = new HealthModificationArgs(Mathf.RoundToInt(Mathf.Lerp(minDamage.Value, maxDamage.Value, t)) * -1, thrower ? thrower.GetComponent<WorldObject>() : null);
						hittableFromMap.ModifyHealth(modificationArgs2);
					}
				}
			}
			if (flag && !hitPhysicsTakers.Contains(physicsTaker))
			{
				hitPhysicsTakers.Add(physicsTaker);
				float force = Mathf.Lerp(minForce.Value, maxForce.Value, t);
				physicsTaker.TakePhysicsForce(force, (colliderCache[i].transform.position + physicsTaker.CenterOfMassOffset - base.transform.position).normalized, frame + 1, base.NetworkObject.NetworkObjectId);
			}
		}
		networkRigidbodyController.AppearanceController.SendMessage("SetDetonateFrame", frame);
	}

	private void OnDrawGizmosSelected()
	{
		DebugExtension.DrawCircle(base.transform.position, Vector3.up, Color.red, minDistance.Value);
		float time = Mathf.InverseLerp(minForce.Value, maxForce.Value, 4f);
		float t = dropoffCurve.Evaluate(time);
		DebugExtension.DrawCircle(base.transform.position, Vector3.up, Color.yellow, Mathf.Lerp(minDistance.Value, maxDistance.Value, t));
		DebugExtension.DrawCircle(base.transform.position, Vector3.up, Color.white, maxDistance.Value);
	}

	public void EndlessGameEnd()
	{
		if (base.IsServer)
		{
			base.NetworkObject.Despawn();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void SimulateFrameEnvironment(uint frame)
	{
		if (!visible && frame >= startFrame.Value && frame < explosionFrame.Value)
		{
			visible = true;
		}
		if (frame == explosionFrame.Value)
		{
			Detonate(explosionFrame.Value);
		}
		if (base.IsServer && frame == explosionFrame.Value + 5)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected override void __initializeVariables()
	{
		if (maxDamage == null)
		{
			throw new Exception("ThrownBomb.maxDamage cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		maxDamage.Initialize(this);
		__nameNetworkVariable(maxDamage, "maxDamage");
		NetworkVariableFields.Add(maxDamage);
		if (minDamage == null)
		{
			throw new Exception("ThrownBomb.minDamage cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		minDamage.Initialize(this);
		__nameNetworkVariable(minDamage, "minDamage");
		NetworkVariableFields.Add(minDamage);
		if (maxDistance == null)
		{
			throw new Exception("ThrownBomb.maxDistance cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		maxDistance.Initialize(this);
		__nameNetworkVariable(maxDistance, "maxDistance");
		NetworkVariableFields.Add(maxDistance);
		if (minDistance == null)
		{
			throw new Exception("ThrownBomb.minDistance cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		minDistance.Initialize(this);
		__nameNetworkVariable(minDistance, "minDistance");
		NetworkVariableFields.Add(minDistance);
		if (maxForce == null)
		{
			throw new Exception("ThrownBomb.maxForce cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		maxForce.Initialize(this);
		__nameNetworkVariable(maxForce, "maxForce");
		NetworkVariableFields.Add(maxForce);
		if (minForce == null)
		{
			throw new Exception("ThrownBomb.minForce cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		minForce.Initialize(this);
		__nameNetworkVariable(minForce, "minForce");
		NetworkVariableFields.Add(minForce);
		if (startFrame == null)
		{
			throw new Exception("ThrownBomb.startFrame cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		startFrame.Initialize(this);
		__nameNetworkVariable(startFrame, "startFrame");
		NetworkVariableFields.Add(startFrame);
		if (explosionFrame == null)
		{
			throw new Exception("ThrownBomb.explosionFrame cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		explosionFrame.Initialize(this);
		__nameNetworkVariable(explosionFrame, "explosionFrame");
		NetworkVariableFields.Add(explosionFrame);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "ThrownBomb";
	}
}
