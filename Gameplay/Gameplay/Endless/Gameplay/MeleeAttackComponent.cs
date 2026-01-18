using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000139 RID: 313
	public class MeleeAttackComponent : AttackComponent
	{
		// Token: 0x1700015D RID: 349
		// (get) Token: 0x0600075F RID: 1887 RVA: 0x00022BE8 File Offset: 0x00020DE8
		private MeleeAttackComponent.EnqueuedMeleeAttack CurrentAttack
		{
			get
			{
				if (this.enqueuedMeleeAttacks.Count != 0)
				{
					return this.enqueuedMeleeAttacks[0];
				}
				return default(MeleeAttackComponent.EnqueuedMeleeAttack);
			}
		}

		// Token: 0x06000760 RID: 1888 RVA: 0x00022C18 File Offset: 0x00020E18
		public override void Awake()
		{
			base.Awake();
			this.ComboAttacks.Sort((ComboAttack attack1, ComboAttack attack2) => attack1.Cost.CompareTo(attack2.Cost));
		}

		// Token: 0x06000761 RID: 1889 RVA: 0x00022C4A File Offset: 0x00020E4A
		public void EnqueueMeleeAttack(uint frame, int duration, int meleeAttackIndex)
		{
			this.enqueuedMeleeAttacks.Add(new MeleeAttackComponent.EnqueuedMeleeAttack(frame, duration, meleeAttackIndex));
		}

		// Token: 0x06000762 RID: 1890 RVA: 0x00022C60 File Offset: 0x00020E60
		protected override void HandleOnUpdateAiState(uint frame)
		{
			if (!this.CurrentAttack.IsValid)
			{
				return;
			}
			AttackData attackData = this.GetAttackData(this.CurrentAttack.StartFrame, this.CurrentAttack.MeleeAttackIndex);
			ref NpcState currentState = ref base.NpcEntity.Components.IndividualStateUpdater.GetCurrentState();
			if (this.CurrentAttack.StartFrame >= frame + 10U)
			{
				base.NpcEntity.Components.AttackAlert.ImminentlyAttacking(frame - this.CurrentAttack.StartFrame, false);
				currentState.ImminentlyAttacking = true;
			}
			if (this.CurrentAttack.StartFrame == frame)
			{
				base.NpcEntity.Components.Animator.SetInteger(NpcAnimator.ComboBookmark, this.CurrentAttack.MeleeAttackIndex);
				base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.Attack);
				currentState.attack = this.CurrentAttack.MeleeAttackIndex + 1;
				base.StartCoroutine(this.PlayAttackVfx(attackData.MeleeAttackData, this.CurrentAttack.MeleeAttackIndex));
			}
			if (frame > this.CurrentAttack.StartFrame && frame <= this.CurrentAttack.EndFrame)
			{
				MeleeAttackData runtimeInstance = MeleeAttackDataPool.GetRuntimeInstance(attackData.MeleeAttackData);
				Transform transform = base.transform;
				foreach (HittableComponent hittableComponent in runtimeInstance.CheckCollisions(frame - this.CurrentAttack.StartFrame, transform.position, transform.rotation.eulerAngles.y))
				{
					NpcEntity npcEntity;
					if (base.NpcEntity.Team.Damages(hittableComponent.Team) && !(hittableComponent == base.NpcEntity.Components.HittableComponent) && (!hittableComponent.WorldObject.TryGetUserComponent<NpcEntity>(out npcEntity) || !(npcEntity == base.NpcEntity)) && this.CurrentAttack.MeleeHits.Add(hittableComponent))
					{
						HealthChangeResult healthChangeResult = hittableComponent.ModifyHealth(new HealthModificationArgs(-runtimeInstance.HitDamage, base.NpcEntity.Context, DamageType.Normal, HealthChangeType.Damage));
						base.NpcEntity.RaiseOnModifiedObjectHealthEvent(healthChangeResult, hittableComponent.WorldObject.Context);
					}
				}
			}
			if (frame > this.CurrentAttack.EndFrame)
			{
				this.enqueuedMeleeAttacks.RemoveAt(0);
			}
		}

		// Token: 0x06000763 RID: 1891 RVA: 0x00022EF4 File Offset: 0x000210F4
		public override void ClearAttackQueue()
		{
			this.enqueuedMeleeAttacks.Clear();
		}

		// Token: 0x06000764 RID: 1892 RVA: 0x00022F01 File Offset: 0x00021101
		private IEnumerator PlayAttackVfx(MeleeAttackData data, int comboBookmark)
		{
			if (data.MeleeVFXPrefab == null)
			{
				yield break;
			}
			yield return new WaitForSeconds(data.VFXdelay);
			MeleeVfxPlayer meleeVfxPlayer = MeleeVfxPlayer.GetFromPool(data.MeleeVFXPrefab.gameObject);
			Vector3 position = base.Components.VisualRoot.position;
			Quaternion rotation = base.Components.VisualRoot.rotation;
			if (meleeVfxPlayer == null)
			{
				meleeVfxPlayer = global::UnityEngine.Object.Instantiate<MeleeVfxPlayer>(data.MeleeVFXPrefab, position, rotation);
			}
			Transform transform = meleeVfxPlayer.transform;
			transform.position = position;
			transform.rotation = rotation;
			meleeVfxPlayer.gameObject.SetActive(true);
			meleeVfxPlayer.PlayEffect(base.Components.VisualRoot, base.Components.VisualRoot);
			yield break;
		}

		// Token: 0x06000765 RID: 1893 RVA: 0x00022F18 File Offset: 0x00021118
		private AttackData GetAttackData(uint frame, int attackIndex)
		{
			MeleeAttackData runtimeInstance = MeleeAttackDataPool.GetRuntimeInstance(this.meleeAttacks.GetMeleeAttackDataForIndex(attackIndex));
			return new AttackData(frame, frame + (uint)runtimeInstance.TotalAttackFrameCount, runtimeInstance);
		}

		// Token: 0x040005CC RID: 1484
		[SerializeField]
		private MeleeAttacks meleeAttacks;

		// Token: 0x040005CD RID: 1485
		public List<ComboAttack> ComboAttacks = new List<ComboAttack>();

		// Token: 0x040005CE RID: 1486
		private readonly List<MeleeAttackComponent.EnqueuedMeleeAttack> enqueuedMeleeAttacks = new List<MeleeAttackComponent.EnqueuedMeleeAttack>();

		// Token: 0x0200013A RID: 314
		[Serializable]
		private readonly struct EnqueuedMeleeAttack
		{
			// Token: 0x1700015E RID: 350
			// (get) Token: 0x06000767 RID: 1895 RVA: 0x00022F64 File Offset: 0x00021164
			public uint StartFrame { get; }

			// Token: 0x1700015F RID: 351
			// (get) Token: 0x06000768 RID: 1896 RVA: 0x00022F6C File Offset: 0x0002116C
			public int MeleeAttackIndex { get; }

			// Token: 0x17000160 RID: 352
			// (get) Token: 0x06000769 RID: 1897 RVA: 0x00022F74 File Offset: 0x00021174
			public uint EndFrame { get; }

			// Token: 0x17000161 RID: 353
			// (get) Token: 0x0600076A RID: 1898 RVA: 0x00022F7C File Offset: 0x0002117C
			public HashSet<HittableComponent> MeleeHits { get; }

			// Token: 0x17000162 RID: 354
			// (get) Token: 0x0600076B RID: 1899 RVA: 0x00022F84 File Offset: 0x00021184
			public bool IsValid { get; }

			// Token: 0x0600076C RID: 1900 RVA: 0x00022F8C File Offset: 0x0002118C
			public EnqueuedMeleeAttack(uint startFrame, int duration, int meleeAttackIndex)
			{
				this.StartFrame = startFrame;
				this.EndFrame = startFrame + (uint)duration;
				this.MeleeHits = new HashSet<HittableComponent>();
				this.MeleeAttackIndex = meleeAttackIndex;
				this.IsValid = true;
			}
		}
	}
}
