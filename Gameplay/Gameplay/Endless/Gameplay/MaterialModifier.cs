using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200026C RID: 620
	public class MaterialModifier : EndlessBehaviour
	{
		// Token: 0x06000CD7 RID: 3287 RVA: 0x00045113 File Offset: 0x00043313
		private void Awake()
		{
			this.propBlock = new MaterialPropertyBlock();
			if (this.renderers.Count == 0)
			{
				this.renderers.AddRange(base.GetComponentsInChildren<Renderer>());
			}
		}

		// Token: 0x06000CD8 RID: 3288 RVA: 0x0004513E File Offset: 0x0004333E
		private void OnValidate()
		{
			if (this.renderers.Count == 0)
			{
				this.renderers.AddRange(base.GetComponentsInChildren<Renderer>());
			}
		}

		// Token: 0x06000CD9 RID: 3289 RVA: 0x00045160 File Offset: 0x00043360
		public void SetZombieCracks(float value)
		{
			foreach (Renderer renderer in this.renderers)
			{
				renderer.material.SetFloat(MaterialModifier.emissiveCracksAmount, value);
			}
		}

		// Token: 0x06000CDA RID: 3290 RVA: 0x000451BC File Offset: 0x000433BC
		private IEnumerator HurtFlash()
		{
			for (float timeElapsed = 0f; timeElapsed < this.hurtFlashDuration; timeElapsed += Time.deltaTime)
			{
				float num = Mathf.Lerp(0f, 1f, timeElapsed / 0.1f);
				foreach (Renderer renderer in this.renderers)
				{
					renderer.material.SetFloat(MaterialModifier.hurtFlash, num);
				}
				yield return null;
			}
			using (List<Renderer>.Enumerator enumerator = this.renderers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Renderer renderer2 = enumerator.Current;
					renderer2.material.SetFloat(MaterialModifier.hurtFlash, 0f);
				}
				yield break;
			}
			yield break;
		}

		// Token: 0x06000CDB RID: 3291 RVA: 0x000451CB File Offset: 0x000433CB
		public void StartHurtFlash()
		{
			base.StartCoroutine(this.HurtFlash());
		}

		// Token: 0x04000BE1 RID: 3041
		[SerializeField]
		private float hurtFlashDuration = 0.1f;

		// Token: 0x04000BE2 RID: 3042
		[SerializeField]
		protected List<Renderer> renderers = new List<Renderer>();

		// Token: 0x04000BE3 RID: 3043
		protected MaterialPropertyBlock propBlock;

		// Token: 0x04000BE4 RID: 3044
		private static readonly int hurtFlash = Shader.PropertyToID("HURT_FLASH");

		// Token: 0x04000BE5 RID: 3045
		private static readonly int emissiveCracksAmount = Shader.PropertyToID("_Emissive_Cracks_Amount");
	}
}
