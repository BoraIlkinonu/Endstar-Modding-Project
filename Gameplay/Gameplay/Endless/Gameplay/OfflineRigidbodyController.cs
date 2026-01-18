using System;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000119 RID: 281
	public class OfflineRigidbodyController : MonoBehaviour
	{
		// Token: 0x06000652 RID: 1618 RVA: 0x0001F1D0 File Offset: 0x0001D3D0
		private void Awake()
		{
			if (this.addOffline)
			{
				this.AddOffline();
			}
			else
			{
				MonoBehaviourSingleton<RigidbodyManager>.Instance.AddListener(new UnityAction(this.HandleRigidbodySimulationStart), new UnityAction(this.HandleRigidbodySimulationEnd));
				base.gameObject.SetActive(false);
			}
			this.added = true;
		}

		// Token: 0x06000653 RID: 1619 RVA: 0x0001F224 File Offset: 0x0001D424
		private void OnDestroy()
		{
			if (this.added)
			{
				if (this.addOffline)
				{
					this.RemoveOffline();
				}
				else if (MonoBehaviourSingleton<RigidbodyManager>.Instance != null)
				{
					MonoBehaviourSingleton<RigidbodyManager>.Instance.RemoveListener(new UnityAction(this.HandleRigidbodySimulationStart), new UnityAction(this.HandleRigidbodySimulationEnd));
				}
				this.added = false;
			}
		}

		// Token: 0x06000654 RID: 1620 RVA: 0x0001F27F File Offset: 0x0001D47F
		public void AddOffline()
		{
			if (this.addOffline && !this.added)
			{
				this.added = true;
				MonoBehaviourSingleton<RigidbodyManager>.Instance.AddOffline(this);
			}
		}

		// Token: 0x06000655 RID: 1621 RVA: 0x0001F2A3 File Offset: 0x0001D4A3
		public void RemoveOffline()
		{
			if (this.addOffline && this.added)
			{
				this.added = false;
				if (MonoBehaviourSingleton<RigidbodyManager>.Instance != null)
				{
					MonoBehaviourSingleton<RigidbodyManager>.Instance.RemoveOffline(this);
				}
			}
		}

		// Token: 0x06000656 RID: 1622 RVA: 0x0001F2D4 File Offset: 0x0001D4D4
		public void HandleRigidbodySimulationStart()
		{
			this.beforeSimulationEvent.Invoke();
			this.cachedLinearVelocity = this.targetRigidbody.velocity;
			this.cachedAngularVelocity = this.targetRigidbody.angularVelocity;
			this.targetRigidbody.isKinematic = true;
			this.targetCollider.enabled = false;
			this.simStarted = true;
		}

		// Token: 0x06000657 RID: 1623 RVA: 0x0001F330 File Offset: 0x0001D530
		public void HandleRigidbodySimulationEnd()
		{
			if (this.simStarted)
			{
				this.simStarted = false;
				this.targetRigidbody.isKinematic = false;
				this.targetCollider.enabled = true;
				this.targetRigidbody.velocity = this.cachedLinearVelocity;
				this.targetRigidbody.angularVelocity = this.cachedAngularVelocity;
				this.afterSimulationEvent.Invoke();
			}
		}

		// Token: 0x06000658 RID: 1624 RVA: 0x0001F394 File Offset: 0x0001D594
		private void FixedUpdate()
		{
			if (this.clampVelocity && !this.targetRigidbody.IsSleeping() && this.targetRigidbody.velocity.sqrMagnitude > this.maxVelocity * this.maxVelocity)
			{
				this.targetRigidbody.velocity = this.targetRigidbody.velocity.normalized * this.maxVelocity;
			}
		}

		// Token: 0x040004C5 RID: 1221
		[Header("Settings")]
		[SerializeField]
		private bool addOffline = true;

		// Token: 0x040004C6 RID: 1222
		[Header("Reference")]
		[SerializeField]
		private Rigidbody targetRigidbody;

		// Token: 0x040004C7 RID: 1223
		[SerializeField]
		private Collider targetCollider;

		// Token: 0x040004C8 RID: 1224
		[SerializeField]
		private UnityEvent beforeSimulationEvent;

		// Token: 0x040004C9 RID: 1225
		[SerializeField]
		private UnityEvent afterSimulationEvent;

		// Token: 0x040004CA RID: 1226
		[Header("Clamp")]
		[SerializeField]
		private bool clampVelocity = true;

		// Token: 0x040004CB RID: 1227
		[SerializeField]
		private float maxVelocity = 4f;

		// Token: 0x040004CC RID: 1228
		[NonSerialized]
		private Vector3 cachedLinearVelocity;

		// Token: 0x040004CD RID: 1229
		[NonSerialized]
		private Vector3 cachedAngularVelocity;

		// Token: 0x040004CE RID: 1230
		[NonSerialized]
		private bool added;

		// Token: 0x040004CF RID: 1231
		[NonSerialized]
		private bool simStarted;
	}
}
