using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200013F RID: 319
	public class NpcAnimator
	{
		// Token: 0x0600077B RID: 1915 RVA: 0x000232A6 File Offset: 0x000214A6
		public NpcAnimator(NpcEntity entity, Animator animator, AnimationEvents animationEvents)
		{
			this.entity = entity;
			this.animator = animator;
			this.animationEvents = animationEvents;
		}

		// Token: 0x0600077C RID: 1916 RVA: 0x000232C4 File Offset: 0x000214C4
		public void PlaySpawnAnimation()
		{
			switch (this.entity.SpawnAnimation)
			{
			case NpcSpawnAnimation.None:
				return;
			case NpcSpawnAnimation.Teleport:
			case NpcSpawnAnimation.CrawlOut:
			{
				this.animator.SetTrigger(this.entity.SpawnAnimation.GetAnimationHash());
				AnimationEvents animationEvents = this.animationEvents;
				animationEvents.OnAnimationComplete = (Action<string>)Delegate.Combine(animationEvents.OnAnimationComplete, new Action<string>(this.OnSpawnAnimationComplete));
				return;
			}
			case NpcSpawnAnimation.Rise:
			{
				AnimationEvents animationEvents2 = this.animationEvents;
				animationEvents2.OnAnimationComplete = (Action<string>)Delegate.Combine(animationEvents2.OnAnimationComplete, new Action<string>(this.OnRiseAnimationComplete));
				this.animator.SetBool(NpcAnimator.Dbno, true);
				this.animator.SetBool(NpcAnimator.Reviving, true);
				return;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x0600077D RID: 1917 RVA: 0x0002338B File Offset: 0x0002158B
		private void OnSpawnAnimationComplete(string animationName)
		{
			this.entity.FinishedSpawnAnimation = true;
		}

		// Token: 0x0600077E RID: 1918 RVA: 0x0002339C File Offset: 0x0002159C
		private void OnRiseAnimationComplete(string animationName)
		{
			AnimationEvents animationEvents = this.animationEvents;
			animationEvents.OnAnimationComplete = (Action<string>)Delegate.Remove(animationEvents.OnAnimationComplete, new Action<string>(this.OnRiseAnimationComplete));
			this.animator.SetBool(NpcAnimator.Reviving, false);
			this.animator.SetBool(NpcAnimator.Dbno, false);
			this.animator.SetTrigger(NpcAnimator.Revived);
			this.animator.SetTrigger(NpcAnimator.Roar);
			AnimationEvents animationEvents2 = this.animationEvents;
			animationEvents2.OnAnimationComplete = (Action<string>)Delegate.Combine(animationEvents2.OnAnimationComplete, new Action<string>(this.OnRoarComplete));
		}

		// Token: 0x0600077F RID: 1919 RVA: 0x0002338B File Offset: 0x0002158B
		private void OnRoarComplete(string animationName)
		{
			this.entity.FinishedSpawnAnimation = true;
		}

		// Token: 0x040005DC RID: 1500
		private readonly NpcEntity entity;

		// Token: 0x040005DD RID: 1501
		private readonly Animator animator;

		// Token: 0x040005DE RID: 1502
		private readonly AnimationEvents animationEvents;

		// Token: 0x040005DF RID: 1503
		public static readonly int Flinch = Animator.StringToHash("Flinch");

		// Token: 0x040005E0 RID: 1504
		public static readonly int EquippedItem = Animator.StringToHash("EquippedItem");

		// Token: 0x040005E1 RID: 1505
		public static readonly int LargePush = Animator.StringToHash("LargePush");

		// Token: 0x040005E2 RID: 1506
		public static readonly int Landed = Animator.StringToHash("Landed");

		// Token: 0x040005E3 RID: 1507
		public static readonly int Downed = Animator.StringToHash("Downed");

		// Token: 0x040005E4 RID: 1508
		public static readonly int PhysicsForceExit = Animator.StringToHash("PhysicsForceExit");

		// Token: 0x040005E5 RID: 1509
		public static readonly int Attack = Animator.StringToHash("Attack");

		// Token: 0x040005E6 RID: 1510
		public static readonly int Grounded = Animator.StringToHash("Grounded");

		// Token: 0x040005E7 RID: 1511
		public static readonly int FallTime = Animator.StringToHash("FallTime");

		// Token: 0x040005E8 RID: 1512
		public static readonly int VelX = Animator.StringToHash("VelX");

		// Token: 0x040005E9 RID: 1513
		public static readonly int VelY = Animator.StringToHash("VelY");

		// Token: 0x040005EA RID: 1514
		public static readonly int VelZ = Animator.StringToHash("VelZ");

		// Token: 0x040005EB RID: 1515
		public static readonly int AngularVelocity = Animator.StringToHash("AngularVelocity");

		// Token: 0x040005EC RID: 1516
		public static readonly int HorizVelMagnitude = Animator.StringToHash("HorizVelMagnitude");

		// Token: 0x040005ED RID: 1517
		public static readonly int Jump = Animator.StringToHash("Jump");

		// Token: 0x040005EE RID: 1518
		public static readonly int SlopeAngle = Animator.StringToHash("SlopeAngle");

		// Token: 0x040005EF RID: 1519
		public static readonly int Moving = Animator.StringToHash("Moving");

		// Token: 0x040005F0 RID: 1520
		public static readonly int SmallPush = Animator.StringToHash("SmallPush");

		// Token: 0x040005F1 RID: 1521
		public static readonly int EndSmallPush = Animator.StringToHash("EndSmallPush");

		// Token: 0x040005F2 RID: 1522
		public static readonly int LoopSmallPush = Animator.StringToHash("LoopSmallPush");

		// Token: 0x040005F3 RID: 1523
		public static readonly int ComboBookmark = Animator.StringToHash("ComboBookmark");

		// Token: 0x040005F4 RID: 1524
		public static readonly int Dbno = Animator.StringToHash("DBNO");

		// Token: 0x040005F5 RID: 1525
		public static readonly int Roar = Animator.StringToHash("Roar");

		// Token: 0x040005F6 RID: 1526
		public static readonly int Revived = Animator.StringToHash("Revived");

		// Token: 0x040005F7 RID: 1527
		public static readonly int Walking = Animator.StringToHash("Walking");

		// Token: 0x040005F8 RID: 1528
		public static readonly int Reviving = Animator.StringToHash("Reviving");

		// Token: 0x040005F9 RID: 1529
		public static readonly int Fidget = Animator.StringToHash("Fidget");

		// Token: 0x040005FA RID: 1530
		public static readonly int FidgetInt = Animator.StringToHash("FidgetInt");

		// Token: 0x040005FB RID: 1531
		public static readonly int Taunt = Animator.StringToHash("Taunt");

		// Token: 0x040005FC RID: 1532
		public static readonly int TauntInt = Animator.StringToHash("TauntInt");

		// Token: 0x040005FD RID: 1533
		public static readonly int ZLock = Animator.StringToHash("ZLock");

		// Token: 0x040005FE RID: 1534
		public static readonly int Interact = Animator.StringToHash("Interact");
	}
}
