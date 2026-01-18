using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay;

public class MeleeAttackComponent : AttackComponent
{
	[Serializable]
	private readonly struct EnqueuedMeleeAttack
	{
		public uint StartFrame { get; }

		public int MeleeAttackIndex { get; }

		public uint EndFrame { get; }

		public HashSet<HittableComponent> MeleeHits { get; }

		public bool IsValid { get; }

		public EnqueuedMeleeAttack(uint startFrame, int duration, int meleeAttackIndex)
		{
			StartFrame = startFrame;
			EndFrame = startFrame + (uint)duration;
			MeleeHits = new HashSet<HittableComponent>();
			MeleeAttackIndex = meleeAttackIndex;
			IsValid = true;
		}
	}

	[SerializeField]
	private MeleeAttacks meleeAttacks;

	public List<ComboAttack> ComboAttacks = new List<ComboAttack>();

	private readonly List<EnqueuedMeleeAttack> enqueuedMeleeAttacks = new List<EnqueuedMeleeAttack>();

	private EnqueuedMeleeAttack CurrentAttack
	{
		get
		{
			if (enqueuedMeleeAttacks.Count != 0)
			{
				return enqueuedMeleeAttacks[0];
			}
			return default(EnqueuedMeleeAttack);
		}
	}

	public override void Awake()
	{
		base.Awake();
		ComboAttacks.Sort((ComboAttack attack1, ComboAttack attack2) => attack1.Cost.CompareTo(attack2.Cost));
	}

	public void EnqueueMeleeAttack(uint frame, int duration, int meleeAttackIndex)
	{
		enqueuedMeleeAttacks.Add(new EnqueuedMeleeAttack(frame, duration, meleeAttackIndex));
	}

	protected override void HandleOnUpdateAiState(uint frame)
	{
		if (!CurrentAttack.IsValid)
		{
			return;
		}
		AttackData attackData = GetAttackData(CurrentAttack.StartFrame, CurrentAttack.MeleeAttackIndex);
		ref NpcState currentState = ref base.NpcEntity.Components.IndividualStateUpdater.GetCurrentState();
		if (CurrentAttack.StartFrame >= frame + 10)
		{
			base.NpcEntity.Components.AttackAlert.ImminentlyAttacking(frame - CurrentAttack.StartFrame);
			currentState.ImminentlyAttacking = true;
		}
		if (CurrentAttack.StartFrame == frame)
		{
			base.NpcEntity.Components.Animator.SetInteger(NpcAnimator.ComboBookmark, CurrentAttack.MeleeAttackIndex);
			base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.Attack);
			currentState.attack = CurrentAttack.MeleeAttackIndex + 1;
			StartCoroutine(PlayAttackVfx(attackData.MeleeAttackData, CurrentAttack.MeleeAttackIndex));
		}
		if (frame > CurrentAttack.StartFrame && frame <= CurrentAttack.EndFrame)
		{
			MeleeAttackData runtimeInstance = MeleeAttackDataPool.GetRuntimeInstance(attackData.MeleeAttackData);
			Transform transform = base.transform;
			foreach (HittableComponent item in runtimeInstance.CheckCollisions(frame - CurrentAttack.StartFrame, transform.position, transform.rotation.eulerAngles.y))
			{
				if (base.NpcEntity.Team.Damages(item.Team) && !(item == base.NpcEntity.Components.HittableComponent) && (!item.WorldObject.TryGetUserComponent<NpcEntity>(out var component) || !(component == base.NpcEntity)) && CurrentAttack.MeleeHits.Add(item))
				{
					HealthChangeResult result = item.ModifyHealth(new HealthModificationArgs(-runtimeInstance.HitDamage, base.NpcEntity.Context));
					base.NpcEntity.RaiseOnModifiedObjectHealthEvent(result, item.WorldObject.Context);
				}
			}
		}
		if (frame > CurrentAttack.EndFrame)
		{
			enqueuedMeleeAttacks.RemoveAt(0);
		}
	}

	public override void ClearAttackQueue()
	{
		enqueuedMeleeAttacks.Clear();
	}

	private IEnumerator PlayAttackVfx(MeleeAttackData data, int comboBookmark)
	{
		if ((object)data.MeleeVFXPrefab != null)
		{
			yield return new WaitForSeconds(data.VFXdelay);
			MeleeVfxPlayer meleeVfxPlayer = MeleeVfxPlayer.GetFromPool(data.MeleeVFXPrefab.gameObject);
			Vector3 position = base.Components.VisualRoot.position;
			Quaternion rotation = base.Components.VisualRoot.rotation;
			if ((object)meleeVfxPlayer == null)
			{
				meleeVfxPlayer = UnityEngine.Object.Instantiate(data.MeleeVFXPrefab, position, rotation);
			}
			Transform obj = meleeVfxPlayer.transform;
			obj.position = position;
			obj.rotation = rotation;
			meleeVfxPlayer.gameObject.SetActive(value: true);
			meleeVfxPlayer.PlayEffect(base.Components.VisualRoot, base.Components.VisualRoot);
		}
	}

	private AttackData GetAttackData(uint frame, int attackIndex)
	{
		MeleeAttackData runtimeInstance = MeleeAttackDataPool.GetRuntimeInstance(meleeAttacks.GetMeleeAttackDataForIndex(attackIndex));
		return new AttackData(frame, frame + (uint)runtimeInstance.TotalAttackFrameCount, runtimeInstance);
	}
}
