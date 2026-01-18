using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class TrajectoryDrawer : MonoBehaviourSingleton<TrajectoryDrawer>, NetClock.ISimulateFrameLateSubscriber
{
	private struct TrajectoryDrawerState : IFrameInfo
	{
		public Rigidbody usedRigidbody;

		public Vector3 startPosition;

		public Vector3 totalForce;

		public bool cooldown;

		public uint NetFrame { get; set; }

		public void Clear()
		{
		}

		public void Initialize()
		{
		}
	}

	[SerializeField]
	private LayerMask layerMask;

	[SerializeField]
	private LineRenderer lineRenderer;

	[SerializeField]
	private int maxIterations = 10000;

	[SerializeField]
	private int maxSegmentCount = 300;

	[SerializeField]
	private float segmentStepModulo = 1f;

	[SerializeField]
	private Color AvailableColor = Color.green;

	[SerializeField]
	private Color CooldownColor = Color.gray;

	[SerializeField]
	private bool debugDraw;

	private InterpolationRingBuffer<TrajectoryDrawerState> stateRingBuffer = new InterpolationRingBuffer<TrajectoryDrawerState>(15);

	private Vector3[] segments;

	private int numSegments;

	private RaycastHit rayHitInfo;

	public bool Enabled
	{
		get
		{
			return lineRenderer.enabled;
		}
		set
		{
			lineRenderer.enabled = value;
		}
	}

	public void Start()
	{
		Enabled = false;
		NetClock.Register(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		NetClock.Unregister(this);
	}

	private void Update()
	{
		if (MonoBehaviourSingleton<CameraController>.Instance.AimUsingADS)
		{
			if (NetworkBehaviourSingleton<NetClock>.Instance == null || (!NetworkBehaviourSingleton<NetClock>.Instance.IsServer && !NetworkBehaviourSingleton<NetClock>.Instance.IsClient))
			{
				return;
			}
			stateRingBuffer.ActiveInterpolationTime = (NetworkManager.Singleton.IsServer ? NetClock.ServerAppearanceTime : NetClock.ClientExtrapolatedAppearanceTime);
			if (stateRingBuffer.NextInterpolationState.usedRigidbody != null)
			{
				stateRingBuffer.ActiveInterpolatedState.usedRigidbody = stateRingBuffer.NextInterpolationState.usedRigidbody;
				stateRingBuffer.ActiveInterpolatedState.cooldown = stateRingBuffer.NextInterpolationState.cooldown;
				if (stateRingBuffer.PastInterpolationState.usedRigidbody != null)
				{
					stateRingBuffer.ActiveInterpolatedState.startPosition = Vector3.Lerp(stateRingBuffer.PastInterpolationState.startPosition, stateRingBuffer.NextInterpolationState.startPosition, stateRingBuffer.ActiveStateLerpTime);
					stateRingBuffer.ActiveInterpolatedState.totalForce = Vector3.Lerp(stateRingBuffer.PastInterpolationState.totalForce, stateRingBuffer.NextInterpolationState.totalForce, stateRingBuffer.ActiveStateLerpTime);
				}
				else
				{
					stateRingBuffer.ActiveInterpolatedState.startPosition = stateRingBuffer.NextInterpolationState.startPosition;
					stateRingBuffer.ActiveInterpolatedState.totalForce = stateRingBuffer.NextInterpolationState.totalForce;
				}
			}
			else
			{
				stateRingBuffer.ActiveInterpolatedState.usedRigidbody = null;
			}
			if ((bool)stateRingBuffer.ActiveInterpolatedState.usedRigidbody)
			{
				SimulatePath();
				Enabled = true;
			}
		}
		else
		{
			Enabled = false;
			stateRingBuffer.ActiveInterpolatedState.usedRigidbody = null;
		}
	}

	public void SetTrajectoryDrawerInfo(Rigidbody rb, Vector3 pos, Vector3 force, bool _cooldown, uint frame)
	{
		ref TrajectoryDrawerState atPosition = ref stateRingBuffer.GetAtPosition(frame);
		atPosition.NetFrame = frame;
		atPosition.usedRigidbody = rb;
		atPosition.startPosition = pos;
		atPosition.totalForce = force;
		atPosition.cooldown = _cooldown;
		stateRingBuffer.FrameUpdated(frame);
	}

	private void SimulatePath()
	{
		float fixedDeltaTime = NetClock.FixedDeltaTime;
		float num = 1f - stateRingBuffer.ActiveInterpolatedState.usedRigidbody.drag * fixedDeltaTime;
		Vector3 vector = stateRingBuffer.ActiveInterpolatedState.totalForce / stateRingBuffer.ActiveInterpolatedState.usedRigidbody.mass * fixedDeltaTime;
		Vector3 vector2 = Physics.gravity * fixedDeltaTime * fixedDeltaTime;
		Vector3 vector3 = stateRingBuffer.ActiveInterpolatedState.startPosition + stateRingBuffer.ActiveInterpolatedState.usedRigidbody.centerOfMass;
		if (segments == null || segments.Length != maxSegmentCount)
		{
			segments = new Vector3[maxSegmentCount];
		}
		segments[0] = vector3;
		numSegments = 1;
		for (int i = 0; i < maxIterations; i++)
		{
			if (numSegments >= maxSegmentCount)
			{
				break;
			}
			vector += vector2;
			vector *= num;
			vector3 += vector;
			if (Physics.Linecast(segments[numSegments - 1], vector3, out rayHitInfo, layerMask))
			{
				if (debugDraw)
				{
					Debug.DrawLine(rayHitInfo.point - Vector3.up, rayHitInfo.point + Vector3.up, Color.yellow);
					Debug.DrawLine(rayHitInfo.point - Vector3.forward, rayHitInfo.point + Vector3.forward, Color.yellow);
					Debug.DrawLine(rayHitInfo.point - Vector3.left, rayHitInfo.point + Vector3.left, Color.yellow);
				}
				break;
			}
			if ((float)i % segmentStepModulo == 0f)
			{
				segments[numSegments] = vector3;
				numSegments++;
			}
		}
		Draw();
	}

	private void Draw()
	{
		lineRenderer.transform.position = segments[0];
		if (stateRingBuffer.ActiveInterpolatedState.cooldown)
		{
			lineRenderer.startColor = CooldownColor;
			lineRenderer.endColor = CooldownColor;
		}
		else
		{
			lineRenderer.startColor = AvailableColor;
			lineRenderer.endColor = AvailableColor;
		}
		lineRenderer.positionCount = numSegments;
		for (int i = 0; i < numSegments; i++)
		{
			lineRenderer.SetPosition(i, segments[i]);
		}
		if (!debugDraw)
		{
			return;
		}
		for (int j = 0; j < numSegments; j++)
		{
			if (j + 1 < numSegments)
			{
				Debug.DrawLine(segments[j], segments[j + 1]);
			}
		}
	}

	public void SimulateFrameLate(uint frame)
	{
		ref TrajectoryDrawerState atPosition = ref stateRingBuffer.GetAtPosition(frame);
		if (frame != atPosition.NetFrame)
		{
			atPosition.NetFrame = frame;
			atPosition.usedRigidbody = null;
			stateRingBuffer.FrameUpdated(frame);
		}
	}
}
