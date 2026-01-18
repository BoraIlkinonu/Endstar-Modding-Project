using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class PositionPrediction : EndlessBehaviour, NetClock.ISimulateFrameLateSubscriber
{
	private const int BUFFER_SIZE = 6;

	private readonly Vector3CircularBuffer buffer = new Vector3CircularBuffer(6);

	public bool TryGetPredictedNavigationPosition(float predictionTime, out Vector3 position)
	{
		position = default(Vector3);
		Vector3 predictedPosition = GetPredictedPosition(predictionTime);
		predictedPosition.y = buffer[0].y + 0.1f;
		if (Physics.Raycast(predictedPosition, Vector3.down, out var hitInfo, 15f, LayerMask.GetMask("Default")) && NavMesh.SamplePosition(hitInfo.point, out var hit, 1f, NpcEntity.NavFilter))
		{
			position = hit.position;
			return true;
		}
		Vector3CircularBuffer vector3CircularBuffer = buffer;
		if (Physics.Raycast(vector3CircularBuffer[vector3CircularBuffer.Count - 1], Vector3.down, out hitInfo, 15f, LayerMask.GetMask("Default")) && NavMesh.SamplePosition(hitInfo.point, out var hit2, 1f, NpcEntity.NavFilter))
		{
			position = hit2.position;
			return true;
		}
		return false;
	}

	public Vector3 GetPredictedPosition(float predictionTime)
	{
		Vector3 vector = buffer[0];
		Vector3CircularBuffer vector3CircularBuffer = buffer;
		Vector3 vector2 = vector3CircularBuffer[vector3CircularBuffer.Count - 1];
		float num = (float)buffer.Count * NetClock.FixedDeltaTime;
		Vector3 vector3 = (vector2 - vector) / num;
		return base.transform.position + vector3 * predictionTime;
	}

	protected override void Start()
	{
		base.Start();
		NetClock.Register(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		NetClock.Unregister(this);
	}

	public void SimulateFrameLate(uint frame)
	{
		buffer.Add(base.transform.position);
	}
}
