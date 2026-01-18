using UnityEngine;

namespace Endless.Gameplay;

public class SimpleYawSpin : MonoBehaviour
{
	[SerializeField]
	private float spinDegreesPerSecond = 180f;

	[SerializeField]
	private bool randomizeInitialRotation;

	private void Awake()
	{
		if (randomizeInitialRotation)
		{
			base.transform.localRotation *= Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
		}
	}

	private void Update()
	{
		base.transform.localRotation *= Quaternion.AngleAxis(spinDegreesPerSecond * Time.deltaTime, Vector3.up);
	}
}
