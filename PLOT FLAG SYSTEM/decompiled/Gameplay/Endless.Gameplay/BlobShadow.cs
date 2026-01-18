using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Endless.Gameplay;

public class BlobShadow : MonoBehaviour
{
	[SerializeField]
	private DecalProjector blobProjector;

	[SerializeField]
	private float maxDistance = 10f;

	[SerializeField]
	private float bufferDistance = 0.5f;

	[SerializeField]
	private LayerMask raycastLayerMask;

	[SerializeField]
	private AnimationCurve fadeDistanceMapping;

	[SerializeField]
	private AnimationCurve sizeMapping;

	[SerializeField]
	private float fadeSizeScalar = 1f;

	private RaycastHit hitInfo;

	private float currentFade;

	private Vector3 currentNormal;

	private const int GROUND_RAY_COUNT = 6;

	private const float GROUND_CHECK_RADIUS = 0.3f;

	private const float FADE_RATE = 1.6f;

	private void Update()
	{
		CalculateAverageGroundDistance();
		UpdateBlobShadow();
	}

	private void UpdateBlobShadow()
	{
		float time = currentFade / maxDistance;
		blobProjector.fadeFactor = fadeDistanceMapping.Evaluate(time);
		float num = sizeMapping.Evaluate(time) * fadeSizeScalar;
		Vector3 size = new Vector3(num, num, currentFade + bufferDistance);
		blobProjector.size = size;
		blobProjector.pivot = new Vector3(blobProjector.pivot.x, blobProjector.pivot.y, size.z / 2f - 0.1f);
		base.transform.rotation = Quaternion.FromToRotation(base.transform.up, currentNormal) * base.transform.rotation;
	}

	private void CalculateAverageGroundDistance()
	{
		int num = 0;
		float num2 = 0f;
		for (int i = 0; i < 6; i++)
		{
			float f = (float)i * MathF.PI * 2f / 6f;
			if (Physics.Raycast(new Vector3(Mathf.Cos(f) * 0.3f, 0.5f, Mathf.Sin(f) * 0.3f) + base.transform.position, Vector3.down, out var raycastHit, maxDistance, raycastLayerMask, QueryTriggerInteraction.Ignore))
			{
				num++;
				num2 += raycastHit.distance;
				currentNormal += raycastHit.normal;
			}
		}
		if (Physics.Raycast(base.transform.position, Vector3.down, out var raycastHit2, maxDistance, raycastLayerMask, QueryTriggerInteraction.Ignore))
		{
			num++;
			num2 += raycastHit2.distance;
			currentNormal += raycastHit2.normal;
		}
		bool flag = num > 6;
		num2 = ((num > 0) ? (num2 / (float)num) : maxDistance);
		if (num < 1)
		{
			currentNormal = Vector3.up;
		}
		currentFade = Mathf.MoveTowards(currentFade, num2, Time.deltaTime * (flag ? 8f : 1.6f));
	}
}
