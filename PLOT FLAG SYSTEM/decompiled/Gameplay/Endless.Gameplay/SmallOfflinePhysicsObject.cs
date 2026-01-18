using System;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class SmallOfflinePhysicsObject : MonoBehaviour, IPoolableT
{
	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private Collider collider;

	[SerializeField]
	private float timeToLive = 20f;

	[Tooltip("If the object is below this velocity for /stopTime/, physics and collision will be disabled on it.")]
	[SerializeField]
	private float stopVelocity = 0.1f;

	[Tooltip("If the object is below /stopVelocity/ for this time, physics and collision will be disabled on it. Use a value less than 0 to disable this behavior.")]
	[SerializeField]
	private float stopTime = 1f;

	[SerializeField]
	private UnityEvent onSpawn;

	[SerializeField]
	private UnityEvent onDespawn;

	[NonSerialized]
	public float lifetimeRemaining;

	[NonSerialized]
	private float stoppedTime;

	public MonoBehaviour Prefab { get; set; }

	public Rigidbody Rigidbody => rb;

	public bool Stopped => stoppedTime >= stopTime;

	private void OnDestroy()
	{
		SmallOfflinePhysicsObjectManager.Remove(this);
	}

	public void OnSpawn()
	{
		stoppedTime = 0f;
		lifetimeRemaining = timeToLive;
		EnablePhysics();
		base.gameObject.SetActive(value: true);
		onSpawn.Invoke();
	}

	public void OnDespawn()
	{
		onDespawn.Invoke();
		base.gameObject.SetActive(value: false);
	}

	public void EnablePhysics()
	{
		rb.isKinematic = false;
		if ((bool)collider)
		{
			collider.enabled = true;
		}
	}

	public void DisablePhysics()
	{
		rb.isKinematic = true;
		if ((bool)collider)
		{
			collider.enabled = false;
		}
		onDespawn.Invoke();
	}

	public void UpdateStoppedTime(float dt)
	{
		if (rb.velocity.sqrMagnitude > stopVelocity * stopVelocity)
		{
			stoppedTime = 0f;
		}
		else
		{
			stoppedTime += dt;
		}
	}
}
