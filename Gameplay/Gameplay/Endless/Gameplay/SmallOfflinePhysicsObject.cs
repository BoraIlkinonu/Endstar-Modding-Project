using System;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000114 RID: 276
	public class SmallOfflinePhysicsObject : MonoBehaviour, IPoolableT
	{
		// Token: 0x17000118 RID: 280
		// (get) Token: 0x06000631 RID: 1585 RVA: 0x0001EAC6 File Offset: 0x0001CCC6
		// (set) Token: 0x06000632 RID: 1586 RVA: 0x0001EACE File Offset: 0x0001CCCE
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x06000633 RID: 1587 RVA: 0x0001EAD7 File Offset: 0x0001CCD7
		public Rigidbody Rigidbody
		{
			get
			{
				return this.rb;
			}
		}

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x06000634 RID: 1588 RVA: 0x0001EADF File Offset: 0x0001CCDF
		public bool Stopped
		{
			get
			{
				return this.stoppedTime >= this.stopTime;
			}
		}

		// Token: 0x06000635 RID: 1589 RVA: 0x0001EAF2 File Offset: 0x0001CCF2
		private void OnDestroy()
		{
			SmallOfflinePhysicsObjectManager.Remove(this);
		}

		// Token: 0x06000636 RID: 1590 RVA: 0x0001EAFA File Offset: 0x0001CCFA
		public void OnSpawn()
		{
			this.stoppedTime = 0f;
			this.lifetimeRemaining = this.timeToLive;
			this.EnablePhysics();
			base.gameObject.SetActive(true);
			this.onSpawn.Invoke();
		}

		// Token: 0x06000637 RID: 1591 RVA: 0x0001EB30 File Offset: 0x0001CD30
		public void OnDespawn()
		{
			this.onDespawn.Invoke();
			base.gameObject.SetActive(false);
		}

		// Token: 0x06000638 RID: 1592 RVA: 0x0001EB49 File Offset: 0x0001CD49
		public void EnablePhysics()
		{
			this.rb.isKinematic = false;
			if (this.collider)
			{
				this.collider.enabled = true;
			}
		}

		// Token: 0x06000639 RID: 1593 RVA: 0x0001EB70 File Offset: 0x0001CD70
		public void DisablePhysics()
		{
			this.rb.isKinematic = true;
			if (this.collider)
			{
				this.collider.enabled = false;
			}
			this.onDespawn.Invoke();
		}

		// Token: 0x0600063A RID: 1594 RVA: 0x0001EBA4 File Offset: 0x0001CDA4
		public void UpdateStoppedTime(float dt)
		{
			if (this.rb.velocity.sqrMagnitude > this.stopVelocity * this.stopVelocity)
			{
				this.stoppedTime = 0f;
				return;
			}
			this.stoppedTime += dt;
		}

		// Token: 0x040004AF RID: 1199
		[SerializeField]
		private Rigidbody rb;

		// Token: 0x040004B0 RID: 1200
		[SerializeField]
		private Collider collider;

		// Token: 0x040004B1 RID: 1201
		[SerializeField]
		private float timeToLive = 20f;

		// Token: 0x040004B2 RID: 1202
		[Tooltip("If the object is below this velocity for /stopTime/, physics and collision will be disabled on it.")]
		[SerializeField]
		private float stopVelocity = 0.1f;

		// Token: 0x040004B3 RID: 1203
		[Tooltip("If the object is below /stopVelocity/ for this time, physics and collision will be disabled on it. Use a value less than 0 to disable this behavior.")]
		[SerializeField]
		private float stopTime = 1f;

		// Token: 0x040004B4 RID: 1204
		[SerializeField]
		private UnityEvent onSpawn;

		// Token: 0x040004B5 RID: 1205
		[SerializeField]
		private UnityEvent onDespawn;

		// Token: 0x040004B6 RID: 1206
		[NonSerialized]
		public float lifetimeRemaining;

		// Token: 0x040004B7 RID: 1207
		[NonSerialized]
		private float stoppedTime;
	}
}
