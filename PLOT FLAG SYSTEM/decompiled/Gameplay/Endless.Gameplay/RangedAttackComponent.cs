using Endless.Matchmaking;
using UnityEngine;

namespace Endless.Gameplay;

public class RangedAttackComponent : AttackComponent
{
	private struct EnqueuedRangedAttack
	{
		public uint StartFrame { get; }

		public uint EndFrame { get; }

		public bool IsValid { get; }

		public EnqueuedRangedAttack(uint startFrame, uint duration)
		{
			StartFrame = startFrame;
			EndFrame = startFrame + duration;
			IsValid = true;
		}
	}

	[SerializeField]
	private uint attackLength;

	[SerializeField]
	private ProjectileEmitter emitter;

	[SerializeField]
	[Range(0f, 10f)]
	private float minDeflectionScalar;

	[SerializeField]
	[Range(0f, 10f)]
	private float maxDeflectionScalar;

	private EnqueuedRangedAttack nextAttackTiming;

	protected override void HandleOnUpdateAiState(uint frame)
	{
		if (!nextAttackTiming.IsValid)
		{
			return;
		}
		ref NpcState currentState = ref base.NpcEntity.Components.IndividualStateUpdater.GetCurrentState();
		if (nextAttackTiming.StartFrame == frame + 10)
		{
			base.NpcEntity.Components.AttackAlert.ImminentlyAttacking();
			currentState.ImminentlyAttacking = true;
		}
		if (frame >= nextAttackTiming.StartFrame && frame <= nextAttackTiming.EndFrame && (nextAttackTiming.StartFrame - frame) % 2 == 0)
		{
			base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.Attack);
			currentState.attack = 1;
			if (MatchSession.Instance.IsHost || MatchSession.Instance.IsServer)
			{
				emitter.SpawnProjectile(randomizeDeflection: true, minDeflectionScalar, maxDeflectionScalar);
			}
		}
	}

	public override void ClearAttackQueue()
	{
		nextAttackTiming = default(EnqueuedRangedAttack);
	}

	public void EnqueueRangedAttack(uint nextAttackFrame, uint duration)
	{
		nextAttackTiming = new EnqueuedRangedAttack(nextAttackFrame, duration);
	}
}
