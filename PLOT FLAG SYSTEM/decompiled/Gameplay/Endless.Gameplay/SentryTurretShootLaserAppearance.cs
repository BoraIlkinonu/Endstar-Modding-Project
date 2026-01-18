using UnityEngine;

namespace Endless.Gameplay;

public class SentryTurretShootLaserAppearance : MonoBehaviour
{
	[SerializeField]
	private LineRenderer lineRenderer;

	private float stopTime;

	public void Init(LineRenderer lr)
	{
		lineRenderer = lr;
	}

	private void OnDisable()
	{
		Stop();
	}

	public void Play(Vector3 hitPosition, float duration)
	{
		lineRenderer.SetPosition(1, base.transform.InverseTransformPoint(hitPosition));
		lineRenderer.enabled = true;
		stopTime = Time.realtimeSinceStartup + duration;
	}

	public void Stop()
	{
		lineRenderer.enabled = false;
	}

	private void Update()
	{
		if (Time.realtimeSinceStartup > stopTime)
		{
			Stop();
		}
	}
}
