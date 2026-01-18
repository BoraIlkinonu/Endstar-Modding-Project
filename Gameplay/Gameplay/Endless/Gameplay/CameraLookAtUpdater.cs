using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000084 RID: 132
	public class CameraLookAtUpdater : MonoBehaviour
	{
		// Token: 0x0600025E RID: 606 RVA: 0x0000D614 File Offset: 0x0000B814
		private void LateUpdate()
		{
			float num = 0f;
			float num2 = 0f;
			base.transform.position = base.transform.position + Vector3.left * num + Vector3.back * num2;
			foreach (Transform transform in this.targets)
			{
				if (transform)
				{
					transform.SetPositionAndRotation(base.transform.position, Quaternion.identity);
				}
			}
		}

		// Token: 0x04000253 RID: 595
		[SerializeField]
		private List<Transform> targets = new List<Transform>();
	}
}
