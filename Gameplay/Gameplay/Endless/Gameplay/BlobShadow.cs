using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Endless.Gameplay
{
	// Token: 0x0200006F RID: 111
	public class BlobShadow : MonoBehaviour
	{
		// Token: 0x060001C8 RID: 456 RVA: 0x0000AA1E File Offset: 0x00008C1E
		private void Update()
		{
			this.CalculateAverageGroundDistance();
			this.UpdateBlobShadow();
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x0000AA2C File Offset: 0x00008C2C
		private void UpdateBlobShadow()
		{
			float num = this.currentFade / this.maxDistance;
			this.blobProjector.fadeFactor = this.fadeDistanceMapping.Evaluate(num);
			float num2 = this.sizeMapping.Evaluate(num) * this.fadeSizeScalar;
			Vector3 vector = new Vector3(num2, num2, this.currentFade + this.bufferDistance);
			this.blobProjector.size = vector;
			this.blobProjector.pivot = new Vector3(this.blobProjector.pivot.x, this.blobProjector.pivot.y, vector.z / 2f - 0.1f);
			base.transform.rotation = Quaternion.FromToRotation(base.transform.up, this.currentNormal) * base.transform.rotation;
		}

		// Token: 0x060001CA RID: 458 RVA: 0x0000AB08 File Offset: 0x00008D08
		private void CalculateAverageGroundDistance()
		{
			int num = 0;
			float num2 = 0f;
			for (int i = 0; i < 6; i++)
			{
				float num3 = (float)i * 3.1415927f * 2f / 6f;
				RaycastHit raycastHit;
				if (Physics.Raycast(new Vector3(Mathf.Cos(num3) * 0.3f, 0.5f, Mathf.Sin(num3) * 0.3f) + base.transform.position, Vector3.down, out raycastHit, this.maxDistance, this.raycastLayerMask, QueryTriggerInteraction.Ignore))
				{
					num++;
					num2 += raycastHit.distance;
					this.currentNormal += raycastHit.normal;
				}
			}
			RaycastHit raycastHit2;
			if (Physics.Raycast(base.transform.position, Vector3.down, out raycastHit2, this.maxDistance, this.raycastLayerMask, QueryTriggerInteraction.Ignore))
			{
				num++;
				num2 += raycastHit2.distance;
				this.currentNormal += raycastHit2.normal;
			}
			bool flag = num > 6;
			num2 = ((num > 0) ? (num2 / (float)num) : this.maxDistance);
			if (num < 1)
			{
				this.currentNormal = Vector3.up;
			}
			this.currentFade = Mathf.MoveTowards(this.currentFade, num2, Time.deltaTime * (flag ? 8f : 1.6f));
		}

		// Token: 0x0400019D RID: 413
		[SerializeField]
		private DecalProjector blobProjector;

		// Token: 0x0400019E RID: 414
		[SerializeField]
		private float maxDistance = 10f;

		// Token: 0x0400019F RID: 415
		[SerializeField]
		private float bufferDistance = 0.5f;

		// Token: 0x040001A0 RID: 416
		[SerializeField]
		private LayerMask raycastLayerMask;

		// Token: 0x040001A1 RID: 417
		[SerializeField]
		private AnimationCurve fadeDistanceMapping;

		// Token: 0x040001A2 RID: 418
		[SerializeField]
		private AnimationCurve sizeMapping;

		// Token: 0x040001A3 RID: 419
		[SerializeField]
		private float fadeSizeScalar = 1f;

		// Token: 0x040001A4 RID: 420
		private RaycastHit hitInfo;

		// Token: 0x040001A5 RID: 421
		private float currentFade;

		// Token: 0x040001A6 RID: 422
		private Vector3 currentNormal;

		// Token: 0x040001A7 RID: 423
		private const int GROUND_RAY_COUNT = 6;

		// Token: 0x040001A8 RID: 424
		private const float GROUND_CHECK_RADIUS = 0.3f;

		// Token: 0x040001A9 RID: 425
		private const float FADE_RATE = 1.6f;
	}
}
