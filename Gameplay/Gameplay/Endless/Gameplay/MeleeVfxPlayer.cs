using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Endless.Gameplay
{
	// Token: 0x02000100 RID: 256
	public class MeleeVfxPlayer : MonoBehaviour
	{
		// Token: 0x060005B1 RID: 1457 RVA: 0x0001C680 File Offset: 0x0001A880
		private void OnEnable()
		{
			this.visualEffect.Stop();
		}

		// Token: 0x060005B2 RID: 1458 RVA: 0x0001C68D File Offset: 0x0001A88D
		private void OnValidate()
		{
			if (this.effectPrefab != base.gameObject)
			{
				this.effectPrefab = base.gameObject;
			}
		}

		// Token: 0x060005B3 RID: 1459 RVA: 0x0001C6AE File Offset: 0x0001A8AE
		public void PlayEffect(Transform vfxTrackTargetPosition, Transform vfxTrackTargetRotation)
		{
			this.visualEffect.Play();
			this.killTime = Time.realtimeSinceStartup + 2f;
			this.trackTargetPosition = vfxTrackTargetPosition;
			this.trackTargetRotation = vfxTrackTargetRotation;
		}

		// Token: 0x060005B4 RID: 1460 RVA: 0x0001C6DC File Offset: 0x0001A8DC
		private void Update()
		{
			if (Time.realtimeSinceStartup > this.killTime)
			{
				this.visualEffect.Stop();
				MeleeVfxPlayer.RetireToPool(this);
			}
			if (this.trackTargetPosition != null)
			{
				base.transform.position = this.trackTargetPosition.position;
			}
			if (this.trackTargetRotation != null)
			{
				base.transform.rotation = this.trackTargetRotation.rotation;
			}
		}

		// Token: 0x060005B5 RID: 1461 RVA: 0x0001C750 File Offset: 0x0001A950
		public static MeleeVfxPlayer GetFromPool(GameObject prefab)
		{
			if (MeleeVfxPlayer.pool.ContainsKey(prefab))
			{
				if (MeleeVfxPlayer.pool[prefab] == null)
				{
					MeleeVfxPlayer.pool[prefab] = new List<MeleeVfxPlayer>();
				}
				if (MeleeVfxPlayer.pool[prefab].Count > 0)
				{
					MeleeVfxPlayer meleeVfxPlayer = MeleeVfxPlayer.pool[prefab][0];
					MeleeVfxPlayer.pool[prefab].RemoveAt(0);
					return meleeVfxPlayer;
				}
			}
			return null;
		}

		// Token: 0x060005B6 RID: 1462 RVA: 0x0001C7C0 File Offset: 0x0001A9C0
		public static void RetireToPool(MeleeVfxPlayer item)
		{
			if (!MeleeVfxPlayer.pool.ContainsKey(item.effectPrefab))
			{
				MeleeVfxPlayer.pool.Add(item.effectPrefab, new List<MeleeVfxPlayer>());
			}
			item.gameObject.SetActive(false);
			MeleeVfxPlayer.pool[item.effectPrefab].Add(item);
		}

		// Token: 0x04000445 RID: 1093
		protected static Dictionary<GameObject, List<MeleeVfxPlayer>> pool = new Dictionary<GameObject, List<MeleeVfxPlayer>>();

		// Token: 0x04000446 RID: 1094
		[SerializeField]
		private VisualEffect visualEffect;

		// Token: 0x04000447 RID: 1095
		[SerializeField]
		private GameObject effectPrefab;

		// Token: 0x04000448 RID: 1096
		private float killTime;

		// Token: 0x04000449 RID: 1097
		private Transform trackTargetPosition;

		// Token: 0x0400044A RID: 1098
		private Transform trackTargetRotation;
	}
}
