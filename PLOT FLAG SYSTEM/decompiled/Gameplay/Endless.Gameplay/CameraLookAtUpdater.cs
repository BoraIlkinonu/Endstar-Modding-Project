using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay;

public class CameraLookAtUpdater : MonoBehaviour
{
	[SerializeField]
	private List<Transform> targets = new List<Transform>();

	private void LateUpdate()
	{
		float num = 0f;
		float num2 = 0f;
		base.transform.position = base.transform.position + Vector3.left * num + Vector3.back * num2;
		foreach (Transform target in targets)
		{
			if ((bool)target)
			{
				target.SetPositionAndRotation(base.transform.position, Quaternion.identity);
			}
		}
	}
}
