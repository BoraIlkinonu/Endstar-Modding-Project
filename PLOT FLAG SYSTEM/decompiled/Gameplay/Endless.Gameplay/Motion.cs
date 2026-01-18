using System;
using System.Collections;
using UnityEngine;

namespace Endless.Gameplay;

public abstract class Motion
{
	public static class Factory
	{
		private class WalkProxy : Walk
		{
			public WalkProxy(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride)
				: base(entity, segment, lookRotationOverride)
			{
			}
		}

		private class JumpProxy : Jump
		{
			public JumpProxy(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride)
				: base(entity, segment, lookRotationOverride)
			{
			}
		}

		private class ThresholdProxy : Threshold
		{
			public ThresholdProxy(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride)
				: base(entity, segment, lookRotationOverride)
			{
			}
		}

		public static Motion Build(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride = null)
		{
			return segment.ConnectionKind switch
			{
				ConnectionKind.Walk => new WalkProxy(entity, segment, lookRotationOverride), 
				ConnectionKind.Threshold => new ThresholdProxy(entity, segment, lookRotationOverride), 
				ConnectionKind.Jump => new JumpProxy(entity, segment, lookRotationOverride), 
				ConnectionKind.Dropdown => new JumpProxy(entity, segment, lookRotationOverride), 
				ConnectionKind.Swim => throw new NotImplementedException(), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}

	protected readonly NpcEntity entity;

	protected readonly NavPath.Segment segment;

	protected readonly Transform lookRotationOverride;

	protected Motion(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride)
	{
		this.entity = entity;
		this.segment = segment;
		this.lookRotationOverride = lookRotationOverride;
	}

	public abstract bool CanRun();

	public abstract bool IsComplete();

	public abstract bool HasFailed();

	public abstract void WriteState(ref NpcState state);

	public abstract IEnumerator Execute();

	public abstract void Stop();
}
