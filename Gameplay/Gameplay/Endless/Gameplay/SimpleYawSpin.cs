using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002C1 RID: 705
	public class SimpleYawSpin : MonoBehaviour
	{
		// Token: 0x06001015 RID: 4117 RVA: 0x0005206F File Offset: 0x0005026F
		private void Awake()
		{
			if (this.randomizeInitialRotation)
			{
				base.transform.localRotation *= Quaternion.AngleAxis(global::UnityEngine.Random.Range(0f, 360f), Vector3.up);
			}
		}

		// Token: 0x06001016 RID: 4118 RVA: 0x000520A8 File Offset: 0x000502A8
		private void Update()
		{
			base.transform.localRotation *= Quaternion.AngleAxis(this.spinDegreesPerSecond * Time.deltaTime, Vector3.up);
		}

		// Token: 0x04000DD1 RID: 3537
		[SerializeField]
		private float spinDegreesPerSecond = 180f;

		// Token: 0x04000DD2 RID: 3538
		[SerializeField]
		private bool randomizeInitialRotation;
	}
}
