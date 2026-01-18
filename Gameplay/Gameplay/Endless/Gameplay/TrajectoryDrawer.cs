using System;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000081 RID: 129
	public class TrajectoryDrawer : MonoBehaviourSingleton<TrajectoryDrawer>, NetClock.ISimulateFrameLateSubscriber
	{
		// Token: 0x1700006F RID: 111
		// (get) Token: 0x0600024F RID: 591 RVA: 0x0000CFCD File Offset: 0x0000B1CD
		// (set) Token: 0x06000250 RID: 592 RVA: 0x0000CFDA File Offset: 0x0000B1DA
		public bool Enabled
		{
			get
			{
				return this.lineRenderer.enabled;
			}
			set
			{
				this.lineRenderer.enabled = value;
			}
		}

		// Token: 0x06000251 RID: 593 RVA: 0x0000CFE8 File Offset: 0x0000B1E8
		public void Start()
		{
			this.Enabled = false;
			NetClock.Register(this);
		}

		// Token: 0x06000252 RID: 594 RVA: 0x0000CFF7 File Offset: 0x0000B1F7
		protected override void OnDestroy()
		{
			base.OnDestroy();
			NetClock.Unregister(this);
		}

		// Token: 0x06000253 RID: 595 RVA: 0x0000D008 File Offset: 0x0000B208
		private void Update()
		{
			if (MonoBehaviourSingleton<CameraController>.Instance.AimUsingADS)
			{
				if (NetworkBehaviourSingleton<NetClock>.Instance == null || (!NetworkBehaviourSingleton<NetClock>.Instance.IsServer && !NetworkBehaviourSingleton<NetClock>.Instance.IsClient))
				{
					return;
				}
				this.stateRingBuffer.ActiveInterpolationTime = (NetworkManager.Singleton.IsServer ? NetClock.ServerAppearanceTime : NetClock.ClientExtrapolatedAppearanceTime);
				if (this.stateRingBuffer.NextInterpolationState.usedRigidbody != null)
				{
					this.stateRingBuffer.ActiveInterpolatedState.usedRigidbody = this.stateRingBuffer.NextInterpolationState.usedRigidbody;
					this.stateRingBuffer.ActiveInterpolatedState.cooldown = this.stateRingBuffer.NextInterpolationState.cooldown;
					if (this.stateRingBuffer.PastInterpolationState.usedRigidbody != null)
					{
						this.stateRingBuffer.ActiveInterpolatedState.startPosition = Vector3.Lerp(this.stateRingBuffer.PastInterpolationState.startPosition, this.stateRingBuffer.NextInterpolationState.startPosition, this.stateRingBuffer.ActiveStateLerpTime);
						this.stateRingBuffer.ActiveInterpolatedState.totalForce = Vector3.Lerp(this.stateRingBuffer.PastInterpolationState.totalForce, this.stateRingBuffer.NextInterpolationState.totalForce, this.stateRingBuffer.ActiveStateLerpTime);
					}
					else
					{
						this.stateRingBuffer.ActiveInterpolatedState.startPosition = this.stateRingBuffer.NextInterpolationState.startPosition;
						this.stateRingBuffer.ActiveInterpolatedState.totalForce = this.stateRingBuffer.NextInterpolationState.totalForce;
					}
				}
				else
				{
					this.stateRingBuffer.ActiveInterpolatedState.usedRigidbody = null;
				}
				if (this.stateRingBuffer.ActiveInterpolatedState.usedRigidbody)
				{
					this.SimulatePath();
					this.Enabled = true;
					return;
				}
			}
			else
			{
				this.Enabled = false;
				this.stateRingBuffer.ActiveInterpolatedState.usedRigidbody = null;
			}
		}

		// Token: 0x06000254 RID: 596 RVA: 0x0000D1F5 File Offset: 0x0000B3F5
		public void SetTrajectoryDrawerInfo(Rigidbody rb, Vector3 pos, Vector3 force, bool _cooldown, uint frame)
		{
			ref TrajectoryDrawer.TrajectoryDrawerState atPosition = ref this.stateRingBuffer.GetAtPosition(frame);
			atPosition.NetFrame = frame;
			atPosition.usedRigidbody = rb;
			atPosition.startPosition = pos;
			atPosition.totalForce = force;
			atPosition.cooldown = _cooldown;
			this.stateRingBuffer.FrameUpdated(frame);
		}

		// Token: 0x06000255 RID: 597 RVA: 0x0000D238 File Offset: 0x0000B438
		private void SimulatePath()
		{
			float fixedDeltaTime = NetClock.FixedDeltaTime;
			float num = 1f - this.stateRingBuffer.ActiveInterpolatedState.usedRigidbody.drag * fixedDeltaTime;
			Vector3 vector = this.stateRingBuffer.ActiveInterpolatedState.totalForce / this.stateRingBuffer.ActiveInterpolatedState.usedRigidbody.mass * fixedDeltaTime;
			Vector3 vector2 = Physics.gravity * fixedDeltaTime * fixedDeltaTime;
			Vector3 vector3 = this.stateRingBuffer.ActiveInterpolatedState.startPosition + this.stateRingBuffer.ActiveInterpolatedState.usedRigidbody.centerOfMass;
			if (this.segments == null || this.segments.Length != this.maxSegmentCount)
			{
				this.segments = new Vector3[this.maxSegmentCount];
			}
			this.segments[0] = vector3;
			this.numSegments = 1;
			int num2 = 0;
			while (num2 < this.maxIterations && this.numSegments < this.maxSegmentCount)
			{
				vector += vector2;
				vector *= num;
				vector3 += vector;
				if (Physics.Linecast(this.segments[this.numSegments - 1], vector3, out this.rayHitInfo, this.layerMask))
				{
					if (this.debugDraw)
					{
						Debug.DrawLine(this.rayHitInfo.point - Vector3.up, this.rayHitInfo.point + Vector3.up, Color.yellow);
						Debug.DrawLine(this.rayHitInfo.point - Vector3.forward, this.rayHitInfo.point + Vector3.forward, Color.yellow);
						Debug.DrawLine(this.rayHitInfo.point - Vector3.left, this.rayHitInfo.point + Vector3.left, Color.yellow);
						break;
					}
					break;
				}
				else
				{
					if ((float)num2 % this.segmentStepModulo == 0f)
					{
						this.segments[this.numSegments] = vector3;
						this.numSegments++;
					}
					num2++;
				}
			}
			this.Draw();
		}

		// Token: 0x06000256 RID: 598 RVA: 0x0000D46C File Offset: 0x0000B66C
		private void Draw()
		{
			this.lineRenderer.transform.position = this.segments[0];
			if (this.stateRingBuffer.ActiveInterpolatedState.cooldown)
			{
				this.lineRenderer.startColor = this.CooldownColor;
				this.lineRenderer.endColor = this.CooldownColor;
			}
			else
			{
				this.lineRenderer.startColor = this.AvailableColor;
				this.lineRenderer.endColor = this.AvailableColor;
			}
			this.lineRenderer.positionCount = this.numSegments;
			for (int i = 0; i < this.numSegments; i++)
			{
				this.lineRenderer.SetPosition(i, this.segments[i]);
			}
			if (this.debugDraw)
			{
				for (int j = 0; j < this.numSegments; j++)
				{
					if (j + 1 < this.numSegments)
					{
						Debug.DrawLine(this.segments[j], this.segments[j + 1]);
					}
				}
			}
		}

		// Token: 0x06000257 RID: 599 RVA: 0x0000D56C File Offset: 0x0000B76C
		public void SimulateFrameLate(uint frame)
		{
			ref TrajectoryDrawer.TrajectoryDrawerState atPosition = ref this.stateRingBuffer.GetAtPosition(frame);
			if (frame != atPosition.NetFrame)
			{
				atPosition.NetFrame = frame;
				atPosition.usedRigidbody = null;
				this.stateRingBuffer.FrameUpdated(frame);
			}
		}

		// Token: 0x04000242 RID: 578
		[SerializeField]
		private LayerMask layerMask;

		// Token: 0x04000243 RID: 579
		[SerializeField]
		private LineRenderer lineRenderer;

		// Token: 0x04000244 RID: 580
		[SerializeField]
		private int maxIterations = 10000;

		// Token: 0x04000245 RID: 581
		[SerializeField]
		private int maxSegmentCount = 300;

		// Token: 0x04000246 RID: 582
		[SerializeField]
		private float segmentStepModulo = 1f;

		// Token: 0x04000247 RID: 583
		[SerializeField]
		private Color AvailableColor = Color.green;

		// Token: 0x04000248 RID: 584
		[SerializeField]
		private Color CooldownColor = Color.gray;

		// Token: 0x04000249 RID: 585
		[SerializeField]
		private bool debugDraw;

		// Token: 0x0400024A RID: 586
		private InterpolationRingBuffer<TrajectoryDrawer.TrajectoryDrawerState> stateRingBuffer = new InterpolationRingBuffer<TrajectoryDrawer.TrajectoryDrawerState>(15);

		// Token: 0x0400024B RID: 587
		private Vector3[] segments;

		// Token: 0x0400024C RID: 588
		private int numSegments;

		// Token: 0x0400024D RID: 589
		private RaycastHit rayHitInfo;

		// Token: 0x02000082 RID: 130
		private struct TrajectoryDrawerState : IFrameInfo
		{
			// Token: 0x17000070 RID: 112
			// (get) Token: 0x06000259 RID: 601 RVA: 0x0000D603 File Offset: 0x0000B803
			// (set) Token: 0x0600025A RID: 602 RVA: 0x0000D60B File Offset: 0x0000B80B
			public uint NetFrame { readonly get; set; }

			// Token: 0x0600025B RID: 603 RVA: 0x00002DB0 File Offset: 0x00000FB0
			public void Clear()
			{
			}

			// Token: 0x0600025C RID: 604 RVA: 0x00002DB0 File Offset: 0x00000FB0
			public void Initialize()
			{
			}

			// Token: 0x0400024F RID: 591
			public Rigidbody usedRigidbody;

			// Token: 0x04000250 RID: 592
			public Vector3 startPosition;

			// Token: 0x04000251 RID: 593
			public Vector3 totalForce;

			// Token: 0x04000252 RID: 594
			public bool cooldown;
		}
	}
}
