using System;
using System.Collections;
using Endless.Gameplay.UI;
using UnityEngine;

namespace Endless.Gameplay.Test
{
	// Token: 0x02000422 RID: 1058
	public class CrosshairTest : MonoBehaviour
	{
		// Token: 0x06001A48 RID: 6728 RVA: 0x00078D64 File Offset: 0x00076F64
		private void Start()
		{
			this.crosshairUI.CreateCrosshair(this.crosshairPrefab, this.crosshairOverrideSettings, true);
			this.crosshairUI.SetHasAmmo(true);
			this.shotWait = new WaitForSeconds(0.05f);
			base.StartCoroutine(this.DoFakeSpread());
		}

		// Token: 0x06001A49 RID: 6729 RVA: 0x00078DB2 File Offset: 0x00076FB2
		private IEnumerator DoFakeSpread()
		{
			yield return new WaitForSeconds(this.initialDelay);
			int num;
			for (int i = 0; i < this.iterations; i = num + 1)
			{
				yield return this.FireShots();
				yield return new WaitForSeconds(global::UnityEngine.Random.Range(this.minDelay, this.maxDelay));
				num = i;
			}
			yield break;
		}

		// Token: 0x06001A4A RID: 6730 RVA: 0x00078DC1 File Offset: 0x00076FC1
		private IEnumerator FireShots()
		{
			int shotCount = global::UnityEngine.Random.Range(this.minShots, this.maxShots + 1);
			int num;
			for (int i = 0; i < shotCount; i = num + 1)
			{
				this.ApplySpread();
				yield return this.shotWait;
				num = i;
			}
			yield break;
		}

		// Token: 0x06001A4B RID: 6731 RVA: 0x00078DD0 File Offset: 0x00076FD0
		private void ApplySpread()
		{
			this.crosshairUI.ApplySpread(1f, this.crosshairOverrideSettings.weaponStrength, this.crosshairOverrideSettings.maxSpread, this.crosshairOverrideSettings.resetSpeed, 0.05f);
		}

		// Token: 0x040014FF RID: 5375
		public CrosshairUI crosshairUI;

		// Token: 0x04001500 RID: 5376
		public CrosshairBase crosshairPrefab;

		// Token: 0x04001501 RID: 5377
		public CrosshairSettings crosshairOverrideSettings;

		// Token: 0x04001502 RID: 5378
		public int iterations = 10;

		// Token: 0x04001503 RID: 5379
		public int minShots = 1;

		// Token: 0x04001504 RID: 5380
		public int maxShots = 3;

		// Token: 0x04001505 RID: 5381
		public float initialDelay = 1.5f;

		// Token: 0x04001506 RID: 5382
		public float minDelay = 0.75f;

		// Token: 0x04001507 RID: 5383
		public float maxDelay = 2.25f;

		// Token: 0x04001508 RID: 5384
		private YieldInstruction shotWait;
	}
}
