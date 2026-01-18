using UnityEngine;

namespace Endless.Gameplay;

public class SentryTurretTrackLaserAppearance : MonoBehaviour
{
	public enum State
	{
		Inactive,
		AimLaser,
		ShootLaser
	}

	[Header("References")]
	[SerializeField]
	private LineRenderer trackLineRenderer;

	[SerializeField]
	private LayerMask scanHitLayerMask;

	private float aimLaserDelay = 0.1f;

	private float shootDistance;

	private bool currentlyActive;

	private State currentState;

	private Ray ray;

	public void Init(LineRenderer lr, LayerMask layerMask)
	{
		trackLineRenderer = lr;
		scanHitLayerMask = layerMask;
	}

	private void Start()
	{
		trackLineRenderer.enabled = false;
	}

	public void SetState(bool active, float distance)
	{
		shootDistance = distance;
		if (active && !currentlyActive)
		{
			Invoke("EnableTrackLineRenderer", aimLaserDelay);
		}
		else if (!active)
		{
			trackLineRenderer.enabled = false;
		}
		currentlyActive = active;
	}

	public void SetState(State newState)
	{
		if (currentState != newState)
		{
			switch (newState)
			{
			case State.Inactive:
				trackLineRenderer.enabled = false;
				break;
			case State.AimLaser:
				Invoke("EnableTrackLineRenderer", aimLaserDelay);
				break;
			case State.ShootLaser:
				trackLineRenderer.enabled = false;
				break;
			}
			currentState = newState;
		}
	}

	private void EnableTrackLineRenderer()
	{
		if (currentlyActive)
		{
			trackLineRenderer.enabled = true;
		}
	}

	public void Update()
	{
		ray = new Ray(base.transform.position, base.transform.forward);
		if (Physics.Raycast(base.transform.position, base.transform.forward, out var hitInfo, shootDistance, scanHitLayerMask))
		{
			trackLineRenderer.SetPosition(1, Vector3.forward * hitInfo.distance);
		}
		else
		{
			trackLineRenderer.SetPosition(1, Vector3.forward * shootDistance);
		}
	}
}
