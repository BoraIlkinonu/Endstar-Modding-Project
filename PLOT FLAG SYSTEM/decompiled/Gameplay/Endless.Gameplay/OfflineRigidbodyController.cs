using System;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class OfflineRigidbodyController : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private bool addOffline = true;

	[Header("Reference")]
	[SerializeField]
	private Rigidbody targetRigidbody;

	[SerializeField]
	private Collider targetCollider;

	[SerializeField]
	private UnityEvent beforeSimulationEvent;

	[SerializeField]
	private UnityEvent afterSimulationEvent;

	[Header("Clamp")]
	[SerializeField]
	private bool clampVelocity = true;

	[SerializeField]
	private float maxVelocity = 4f;

	[NonSerialized]
	private Vector3 cachedLinearVelocity;

	[NonSerialized]
	private Vector3 cachedAngularVelocity;

	[NonSerialized]
	private bool added;

	[NonSerialized]
	private bool simStarted;

	private void Awake()
	{
		if (addOffline)
		{
			AddOffline();
		}
		else
		{
			MonoBehaviourSingleton<RigidbodyManager>.Instance.AddListener(HandleRigidbodySimulationStart, HandleRigidbodySimulationEnd);
			base.gameObject.SetActive(value: false);
		}
		added = true;
	}

	private void OnDestroy()
	{
		if (added)
		{
			if (addOffline)
			{
				RemoveOffline();
			}
			else if (MonoBehaviourSingleton<RigidbodyManager>.Instance != null)
			{
				MonoBehaviourSingleton<RigidbodyManager>.Instance.RemoveListener(HandleRigidbodySimulationStart, HandleRigidbodySimulationEnd);
			}
			added = false;
		}
	}

	public void AddOffline()
	{
		if (addOffline && !added)
		{
			added = true;
			MonoBehaviourSingleton<RigidbodyManager>.Instance.AddOffline(this);
		}
	}

	public void RemoveOffline()
	{
		if (addOffline && added)
		{
			added = false;
			if (MonoBehaviourSingleton<RigidbodyManager>.Instance != null)
			{
				MonoBehaviourSingleton<RigidbodyManager>.Instance.RemoveOffline(this);
			}
		}
	}

	public void HandleRigidbodySimulationStart()
	{
		beforeSimulationEvent.Invoke();
		cachedLinearVelocity = targetRigidbody.velocity;
		cachedAngularVelocity = targetRigidbody.angularVelocity;
		targetRigidbody.isKinematic = true;
		targetCollider.enabled = false;
		simStarted = true;
	}

	public void HandleRigidbodySimulationEnd()
	{
		if (simStarted)
		{
			simStarted = false;
			targetRigidbody.isKinematic = false;
			targetCollider.enabled = true;
			targetRigidbody.velocity = cachedLinearVelocity;
			targetRigidbody.angularVelocity = cachedAngularVelocity;
			afterSimulationEvent.Invoke();
		}
	}

	private void FixedUpdate()
	{
		if (clampVelocity && !targetRigidbody.IsSleeping() && targetRigidbody.velocity.sqrMagnitude > maxVelocity * maxVelocity)
		{
			targetRigidbody.velocity = targetRigidbody.velocity.normalized * maxVelocity;
		}
	}
}
