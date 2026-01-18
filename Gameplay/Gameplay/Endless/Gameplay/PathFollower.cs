using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared.DataTypes;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000146 RID: 326
	public class PathFollower
	{
		// Token: 0x1400000C RID: 12
		// (add) Token: 0x060007A3 RID: 1955 RVA: 0x000240A0 File Offset: 0x000222A0
		// (remove) Token: 0x060007A4 RID: 1956 RVA: 0x000240D8 File Offset: 0x000222D8
		public event Action<bool> OnPathFinished;

		// Token: 0x060007A5 RID: 1957 RVA: 0x00024110 File Offset: 0x00022310
		public PathFollower(NpcEntity entity, IndividualStateUpdater stateUpdater, NavMeshAgent agent, DynamicAttributes dynamicAttributes, MonoBehaviorProxy proxy)
		{
			this.entity = entity;
			this.agent = agent;
			this.proxy = proxy;
			stateUpdater.OnWriteState += this.HandleOnWriteState;
			stateUpdater.OnUpdateState += this.HandleOnUpdateState;
			dynamicAttributes.OnMovementModeChanged += this.HandleOnMovementModeChanged;
			this.HandleOnMovementModeChanged();
			this.agent.avoidancePriority = global::UnityEngine.Random.Range(0, 100);
		}

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x060007A6 RID: 1958 RVA: 0x000241A2 File Offset: 0x000223A2
		public bool IsJumping
		{
			get
			{
				return this.motion is Jump;
			}
		}

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x060007A7 RID: 1959 RVA: 0x000241B2 File Offset: 0x000223B2
		// (set) Token: 0x060007A8 RID: 1960 RVA: 0x000241BA File Offset: 0x000223BA
		public NavPath Path { get; private set; }

		// Token: 0x060007A9 RID: 1961 RVA: 0x000241C4 File Offset: 0x000223C4
		public void SetPath(NavPath path)
		{
			if (this.motion is Jump)
			{
				bool flag = false;
				foreach (NavPath.Segment segment in path.NavigationSegments)
				{
					ConnectionKind connectionKind = segment.ConnectionKind;
					if (connectionKind == ConnectionKind.Jump || connectionKind == ConnectionKind.Dropdown)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					while (path.NavigationSegments.Count > 0)
					{
						if (path.NavigationSegments.Dequeue().ConnectionKind == ConnectionKind.Jump)
						{
							path.NavigationSegments.Dequeue();
							break;
						}
					}
				}
				this.Path = path;
				return;
			}
			this.StopMotion();
			this.Path = path;
			this.BeginNextSegment();
		}

		// Token: 0x060007AA RID: 1962 RVA: 0x00024280 File Offset: 0x00022480
		public void StopPath(bool forceStop = false)
		{
			if (this.motion != null && (!(this.motion is Jump) || forceStop))
			{
				this.StopMotion();
			}
			this.Path = null;
			this.LookRotationOverride = null;
			if (this.entity.Components.Agent.isOnNavMesh)
			{
				this.entity.Components.Agent.ResetPath();
			}
		}

		// Token: 0x060007AB RID: 1963 RVA: 0x000242EC File Offset: 0x000224EC
		public bool IsRepathNecessary(HashSet<SerializableGuid> updatedProps)
		{
			Threshold threshold = this.motion as Threshold;
			return threshold == null || !updatedProps.Contains(threshold.Door.WorldObject.InstanceId);
		}

		// Token: 0x060007AC RID: 1964 RVA: 0x00024324 File Offset: 0x00022524
		private void HandleOnMovementModeChanged()
		{
			float num;
			switch (this.entity.MovementMode)
			{
			case MovementMode.Walk:
				num = this.entity.Settings.WalkSpeed;
				break;
			case MovementMode.Run:
				num = this.entity.Settings.RunSpeed;
				break;
			case MovementMode.Sprint:
				num = this.entity.Settings.SprintSpeed;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			float num2 = num;
			this.agent.speed = num2;
		}

		// Token: 0x060007AD RID: 1965 RVA: 0x0002439E File Offset: 0x0002259E
		private void HandleOnWriteState(ref NpcState state)
		{
			state.Position = this.entity.transform.position;
			Motion motion = this.motion;
			if (motion == null)
			{
				return;
			}
			motion.WriteState(ref state);
		}

		// Token: 0x060007AE RID: 1966 RVA: 0x000243C8 File Offset: 0x000225C8
		private void HandleOnUpdateState(uint frame)
		{
			if (this.motion == null)
			{
				return;
			}
			if (this.motion.HasFailed())
			{
				Action<bool> onPathFinished = this.OnPathFinished;
				if (onPathFinished != null)
				{
					onPathFinished(false);
				}
				this.StopMotion();
				return;
			}
			if (this.motion.IsComplete())
			{
				this.StopMotion();
				this.BeginNextSegment();
			}
		}

		// Token: 0x060007AF RID: 1967 RVA: 0x00024420 File Offset: 0x00022620
		private void BeginNextSegment()
		{
			if (this.Path != null)
			{
				if (this.Path.NavigationSegments.Count > 0)
				{
					NavPath.Segment segment = this.Path.NavigationSegments.Dequeue();
					this.motion = Motion.Factory.Build(this.entity, segment, this.LookRotationOverride);
					if (this.motion.CanRun())
					{
						this.motionRoutine = this.proxy.StartMonoBehaviorRoutine(this.motion.Execute());
						return;
					}
					Action<bool> onPathFinished = this.OnPathFinished;
					if (onPathFinished != null)
					{
						onPathFinished(false);
					}
					this.Path = null;
					this.LookRotationOverride = null;
					this.motion = null;
					return;
				}
				else
				{
					Action<bool> onPathFinished2 = this.OnPathFinished;
					if (onPathFinished2 != null)
					{
						onPathFinished2(true);
					}
					this.Path = null;
					this.LookRotationOverride = null;
				}
			}
		}

		// Token: 0x060007B0 RID: 1968 RVA: 0x000244E6 File Offset: 0x000226E6
		private void StopMotion()
		{
			Motion motion = this.motion;
			if (motion != null)
			{
				motion.Stop();
			}
			if (this.motionRoutine != null)
			{
				this.proxy.StopMonoBehaviorRoutine(this.motionRoutine);
			}
			this.motionRoutine = null;
			this.motion = null;
		}

		// Token: 0x060007B1 RID: 1969 RVA: 0x00024520 File Offset: 0x00022720
		public void Tick()
		{
			if (this.IsJumping || this.entity.CurrentState.State != NpcEnum.FsmState.Neutral)
			{
				return;
			}
			if (!Physics.CheckSphere(this.entity.FootPosition, this.entity.Components.Agent.radius / 2f, this.mask))
			{
				this.entity.Components.PhysicsTaker.TakePhysicsForce(0f, Vector3.down, NetClock.CurrentFrame, this.entity.NetworkObjectId, true, false, false);
			}
		}

		// Token: 0x060007B2 RID: 1970 RVA: 0x000245B0 File Offset: 0x000227B0
		private void DisplayDebugPath(NavPath path)
		{
			while (path.NavigationSegments.Count > 0)
			{
				NavPath.Segment segment = path.NavigationSegments.Dequeue();
				Color color = (Motion.Factory.Build(this.entity, segment, null).CanRun() ? Color.cyan : Color.red);
				Debug.DrawLine(segment.StartPosition, segment.EndPosition, color, 15f);
			}
		}

		// Token: 0x04000620 RID: 1568
		private Motion motion;

		// Token: 0x04000621 RID: 1569
		private Coroutine motionRoutine;

		// Token: 0x04000622 RID: 1570
		private readonly int mask = LayerMask.GetMask(new string[] { "Default" });

		// Token: 0x04000623 RID: 1571
		private readonly NpcEntity entity;

		// Token: 0x04000624 RID: 1572
		private readonly NavMeshAgent agent;

		// Token: 0x04000625 RID: 1573
		private readonly MonoBehaviorProxy proxy;

		// Token: 0x04000626 RID: 1574
		public Transform LookRotationOverride;
	}
}
