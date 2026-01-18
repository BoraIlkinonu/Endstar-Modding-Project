using System;
using UnityEngine;

namespace Endless.Gameplay;

public class Wobble : MonoBehaviour
{
	[SerializeField]
	private Renderer targetRenderer;

	[SerializeField]
	private float MaxWobble = 0.03f;

	[SerializeField]
	private float WobbleSpeed = 1f;

	[SerializeField]
	private float Recovery = 1f;

	private Vector3 lastPosition;

	private Vector3 velocity;

	private Vector3 lastRotation;

	private Vector3 angularVelocity;

	private float wobbleAmountX;

	private float wobbleAmountZ;

	private float wobbleAmountToAddX;

	private float wobbleAmountToAddZ;

	private float pulse;

	private float time = 0.5f;

	private void Reset()
	{
		if (targetRenderer == null)
		{
			targetRenderer = GetComponent<Renderer>();
		}
	}

	private void Update()
	{
		time += Time.deltaTime;
		wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0f, Time.deltaTime * Recovery);
		wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0f, Time.deltaTime * Recovery);
		pulse = MathF.PI * 2f * WobbleSpeed;
		wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(pulse * time);
		wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(pulse * time);
		targetRenderer.material.SetFloat("_WobbleX", wobbleAmountX);
		targetRenderer.material.SetFloat("_WobbleZ", wobbleAmountZ);
		velocity = (lastPosition - base.transform.position) / Time.deltaTime;
		angularVelocity = base.transform.rotation.eulerAngles - lastRotation;
		wobbleAmountToAddX += Mathf.Clamp((velocity.x + angularVelocity.z * 0.2f) * MaxWobble, 0f - MaxWobble, MaxWobble);
		wobbleAmountToAddZ += Mathf.Clamp((velocity.z + angularVelocity.x * 0.2f) * MaxWobble, 0f - MaxWobble, MaxWobble);
		lastPosition = base.transform.position;
		lastRotation = base.transform.rotation.eulerAngles;
	}
}
