using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x02000075 RID: 117
	[RequireComponent(typeof(NetworkObject))]
	public class NetworkPrefabSpawner : NetworkBehaviour
	{
		// Token: 0x06000392 RID: 914 RVA: 0x000104F3 File Offset: 0x0000E6F3
		public override void OnNetworkSpawn()
		{
			if (this.verboseLogging)
			{
				Debug.Log("OnNetworkSpawn", this);
			}
			base.OnNetworkSpawn();
			if (base.IsServer || base.IsHost)
			{
				this.hasSpawned = true;
				this.SpawnObject();
			}
		}

		// Token: 0x06000393 RID: 915 RVA: 0x0001052C File Offset: 0x0000E72C
		protected virtual void SpawnObject()
		{
			this.spawnedPrefab = global::UnityEngine.Object.Instantiate<GameObject>(this.networkPrefab, base.transform.position, base.transform.rotation);
			NetworkObject networkObject;
			this.spawnedPrefab.TryGetComponent<NetworkObject>(out networkObject);
			networkObject.Spawn(false);
		}

		// Token: 0x06000394 RID: 916 RVA: 0x00010578 File Offset: 0x0000E778
		private void OnDrawGizmos()
		{
			if (!this.networkPrefab)
			{
				return;
			}
			Gizmos.color = Color.blue;
			MeshFilter[] componentsInChildren = this.networkPrefab.GetComponentsInChildren<MeshFilter>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Vector3 vector = base.transform.position + componentsInChildren[i].transform.position + Vector3.up * 0.5f;
				Gizmos.DrawMesh(componentsInChildren[i].sharedMesh, vector, componentsInChildren[i].transform.rotation, componentsInChildren[i].transform.localScale);
			}
		}

		// Token: 0x06000395 RID: 917 RVA: 0x00010611 File Offset: 0x0000E811
		private void Update()
		{
			if (this.respawnTime > 0f && this.hasSpawned && this.spawnedPrefab == null && this.spawnCoroutine == null)
			{
				this.spawnCoroutine = base.StartCoroutine(this.SpawnWithDelay());
			}
		}

		// Token: 0x06000396 RID: 918 RVA: 0x00010650 File Offset: 0x0000E850
		private IEnumerator SpawnWithDelay()
		{
			yield return new WaitForSecondsRealtime(this.respawnTime);
			this.SpawnObject();
			this.spawnCoroutine = null;
			yield break;
		}

		// Token: 0x06000398 RID: 920 RVA: 0x00010674 File Offset: 0x0000E874
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000399 RID: 921 RVA: 0x000104E2 File Offset: 0x0000E6E2
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x0600039A RID: 922 RVA: 0x0001068A File Offset: 0x0000E88A
		protected internal override string __getTypeName()
		{
			return "NetworkPrefabSpawner";
		}

		// Token: 0x040001B6 RID: 438
		[SerializeField]
		private float respawnTime = 10f;

		// Token: 0x040001B7 RID: 439
		[SerializeField]
		private GameObject networkPrefab;

		// Token: 0x040001B8 RID: 440
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040001B9 RID: 441
		private GameObject spawnedPrefab;

		// Token: 0x040001BA RID: 442
		private bool hasSpawned;

		// Token: 0x040001BB RID: 443
		private Coroutine spawnCoroutine;
	}
}
