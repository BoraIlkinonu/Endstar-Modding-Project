using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200008B RID: 139
	public class Wobble : MonoBehaviour
	{
		// Token: 0x0600027F RID: 639 RVA: 0x0000DAD5 File Offset: 0x0000BCD5
		private void Reset()
		{
			if (this.targetRenderer == null)
			{
				this.targetRenderer = base.GetComponent<Renderer>();
			}
		}

		// Token: 0x06000280 RID: 640 RVA: 0x0000DAF4 File Offset: 0x0000BCF4
		private void Update()
		{
			this.time += Time.deltaTime;
			this.wobbleAmountToAddX = Mathf.Lerp(this.wobbleAmountToAddX, 0f, Time.deltaTime * this.Recovery);
			this.wobbleAmountToAddZ = Mathf.Lerp(this.wobbleAmountToAddZ, 0f, Time.deltaTime * this.Recovery);
			this.pulse = 6.2831855f * this.WobbleSpeed;
			this.wobbleAmountX = this.wobbleAmountToAddX * Mathf.Sin(this.pulse * this.time);
			this.wobbleAmountZ = this.wobbleAmountToAddZ * Mathf.Sin(this.pulse * this.time);
			this.targetRenderer.material.SetFloat("_WobbleX", this.wobbleAmountX);
			this.targetRenderer.material.SetFloat("_WobbleZ", this.wobbleAmountZ);
			this.velocity = (this.lastPosition - base.transform.position) / Time.deltaTime;
			this.angularVelocity = base.transform.rotation.eulerAngles - this.lastRotation;
			this.wobbleAmountToAddX += Mathf.Clamp((this.velocity.x + this.angularVelocity.z * 0.2f) * this.MaxWobble, -this.MaxWobble, this.MaxWobble);
			this.wobbleAmountToAddZ += Mathf.Clamp((this.velocity.z + this.angularVelocity.x * 0.2f) * this.MaxWobble, -this.MaxWobble, this.MaxWobble);
			this.lastPosition = base.transform.position;
			this.lastRotation = base.transform.rotation.eulerAngles;
		}

		// Token: 0x0400026A RID: 618
		[SerializeField]
		private Renderer targetRenderer;

		// Token: 0x0400026B RID: 619
		[SerializeField]
		private float MaxWobble = 0.03f;

		// Token: 0x0400026C RID: 620
		[SerializeField]
		private float WobbleSpeed = 1f;

		// Token: 0x0400026D RID: 621
		[SerializeField]
		private float Recovery = 1f;

		// Token: 0x0400026E RID: 622
		private Vector3 lastPosition;

		// Token: 0x0400026F RID: 623
		private Vector3 velocity;

		// Token: 0x04000270 RID: 624
		private Vector3 lastRotation;

		// Token: 0x04000271 RID: 625
		private Vector3 angularVelocity;

		// Token: 0x04000272 RID: 626
		private float wobbleAmountX;

		// Token: 0x04000273 RID: 627
		private float wobbleAmountZ;

		// Token: 0x04000274 RID: 628
		private float wobbleAmountToAddX;

		// Token: 0x04000275 RID: 629
		private float wobbleAmountToAddZ;

		// Token: 0x04000276 RID: 630
		private float pulse;

		// Token: 0x04000277 RID: 631
		private float time = 0.5f;
	}
}
