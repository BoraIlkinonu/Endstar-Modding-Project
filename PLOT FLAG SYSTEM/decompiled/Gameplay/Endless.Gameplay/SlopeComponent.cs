using UnityEngine;

namespace Endless.Gameplay;

public class SlopeComponent
{
	private readonly LayerMask mask;

	private readonly Transform transform;

	private Vector3 groundNormal;

	private Vector3 normalVelocity;

	public float SlopeAngle
	{
		get
		{
			Vector3 forward = transform.forward;
			float num = Vector3.Angle(groundNormal, forward);
			Vector2 vector = new Vector2(forward.x, forward.z);
			Vector2 vector2 = new Vector2(groundNormal.x, groundNormal.z);
			return Mathf.Abs(Vector2.Dot(vector.normalized, vector2.normalized)) * (num - 90f) / 45f;
		}
	}

	public SlopeComponent(Transform transform, LayerMask mask, IndividualStateUpdater stateUpdater)
	{
		this.transform = transform;
		this.mask = mask;
		stateUpdater.OnCheckGroundState += HandleCheckGroundState;
	}

	private void HandleCheckGroundState()
	{
		RaycastHit hitInfo;
		Vector3 target = (Physics.Raycast(transform.position, Vector3.down, out hitInfo, 1f, mask) ? hitInfo.normal : Vector3.up);
		groundNormal = Vector3.SmoothDamp(groundNormal, target, ref normalVelocity, 0.2f, 90f * NetClock.FixedDeltaTime, NetClock.FixedDeltaTime);
	}
}
