using System;
using UnityEngine;

namespace Endless.Creator
{
	// Token: 0x02000091 RID: 145
	public class CreatorGun : MonoBehaviour
	{
		// Token: 0x06000226 RID: 550 RVA: 0x0001024A File Offset: 0x0000E44A
		private void Start()
		{
			this.ApplyColor(this.lastColor);
		}

		// Token: 0x06000227 RID: 551 RVA: 0x00010258 File Offset: 0x0000E458
		public void SetColor(Color newColor)
		{
			if (this.lastColor != newColor)
			{
				this.ApplyColor(newColor);
			}
		}

		// Token: 0x06000228 RID: 552 RVA: 0x00010270 File Offset: 0x0000E470
		private void ApplyColor(Color newColor)
		{
			Color color = new Color(newColor.r * this.colorAmplificationFactor, newColor.g * this.colorAmplificationFactor, newColor.b * this.colorAmplificationFactor, newColor.a);
			this.lastColor = newColor;
			this.colorableRenderer.material.SetColor(this.colorPropertyName, color);
			this.idleParticleSystem.main.startColor = newColor;
		}

		// Token: 0x06000229 RID: 553 RVA: 0x000102E8 File Offset: 0x0000E4E8
		public void StartFlash()
		{
			this.muzzleFlashParticleSystem.Play();
		}

		// Token: 0x0400028C RID: 652
		[SerializeField]
		private Renderer colorableRenderer;

		// Token: 0x0400028D RID: 653
		[SerializeField]
		private string colorPropertyName = "Color";

		// Token: 0x0400028E RID: 654
		[SerializeField]
		private ParticleSystem idleParticleSystem;

		// Token: 0x0400028F RID: 655
		[SerializeField]
		private ParticleSystem muzzleFlashParticleSystem;

		// Token: 0x04000290 RID: 656
		[SerializeField]
		private float colorAmplificationFactor = 32f;

		// Token: 0x04000291 RID: 657
		private Color lastColor = Color.white;
	}
}
