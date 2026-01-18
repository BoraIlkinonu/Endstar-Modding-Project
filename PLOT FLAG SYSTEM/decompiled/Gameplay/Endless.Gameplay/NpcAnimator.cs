using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay;

public class NpcAnimator
{
	private readonly NpcEntity entity;

	private readonly Animator animator;

	private readonly AnimationEvents animationEvents;

	public static readonly int Flinch = Animator.StringToHash("Flinch");

	public static readonly int EquippedItem = Animator.StringToHash("EquippedItem");

	public static readonly int LargePush = Animator.StringToHash("LargePush");

	public static readonly int Landed = Animator.StringToHash("Landed");

	public static readonly int Downed = Animator.StringToHash("Downed");

	public static readonly int PhysicsForceExit = Animator.StringToHash("PhysicsForceExit");

	public static readonly int Attack = Animator.StringToHash("Attack");

	public static readonly int Grounded = Animator.StringToHash("Grounded");

	public static readonly int FallTime = Animator.StringToHash("FallTime");

	public static readonly int VelX = Animator.StringToHash("VelX");

	public static readonly int VelY = Animator.StringToHash("VelY");

	public static readonly int VelZ = Animator.StringToHash("VelZ");

	public static readonly int AngularVelocity = Animator.StringToHash("AngularVelocity");

	public static readonly int HorizVelMagnitude = Animator.StringToHash("HorizVelMagnitude");

	public static readonly int Jump = Animator.StringToHash("Jump");

	public static readonly int SlopeAngle = Animator.StringToHash("SlopeAngle");

	public static readonly int Moving = Animator.StringToHash("Moving");

	public static readonly int SmallPush = Animator.StringToHash("SmallPush");

	public static readonly int EndSmallPush = Animator.StringToHash("EndSmallPush");

	public static readonly int LoopSmallPush = Animator.StringToHash("LoopSmallPush");

	public static readonly int ComboBookmark = Animator.StringToHash("ComboBookmark");

	public static readonly int Dbno = Animator.StringToHash("DBNO");

	public static readonly int Roar = Animator.StringToHash("Roar");

	public static readonly int Revived = Animator.StringToHash("Revived");

	public static readonly int Walking = Animator.StringToHash("Walking");

	public static readonly int Reviving = Animator.StringToHash("Reviving");

	public static readonly int Fidget = Animator.StringToHash("Fidget");

	public static readonly int FidgetInt = Animator.StringToHash("FidgetInt");

	public static readonly int Taunt = Animator.StringToHash("Taunt");

	public static readonly int TauntInt = Animator.StringToHash("TauntInt");

	public static readonly int ZLock = Animator.StringToHash("ZLock");

	public static readonly int Interact = Animator.StringToHash("Interact");

	public NpcAnimator(NpcEntity entity, Animator animator, AnimationEvents animationEvents)
	{
		this.entity = entity;
		this.animator = animator;
		this.animationEvents = animationEvents;
	}

	public void PlaySpawnAnimation()
	{
		switch (entity.SpawnAnimation)
		{
		case NpcSpawnAnimation.Teleport:
		case NpcSpawnAnimation.CrawlOut:
		{
			animator.SetTrigger(entity.SpawnAnimation.GetAnimationHash());
			AnimationEvents obj2 = animationEvents;
			obj2.OnAnimationComplete = (Action<string>)Delegate.Combine(obj2.OnAnimationComplete, new Action<string>(OnSpawnAnimationComplete));
			break;
		}
		case NpcSpawnAnimation.Rise:
		{
			AnimationEvents obj = animationEvents;
			obj.OnAnimationComplete = (Action<string>)Delegate.Combine(obj.OnAnimationComplete, new Action<string>(OnRiseAnimationComplete));
			animator.SetBool(Dbno, value: true);
			animator.SetBool(Reviving, value: true);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case NpcSpawnAnimation.None:
			break;
		}
	}

	private void OnSpawnAnimationComplete(string animationName)
	{
		entity.FinishedSpawnAnimation = true;
	}

	private void OnRiseAnimationComplete(string animationName)
	{
		AnimationEvents obj = animationEvents;
		obj.OnAnimationComplete = (Action<string>)Delegate.Remove(obj.OnAnimationComplete, new Action<string>(OnRiseAnimationComplete));
		animator.SetBool(Reviving, value: false);
		animator.SetBool(Dbno, value: false);
		animator.SetTrigger(Revived);
		animator.SetTrigger(Roar);
		AnimationEvents obj2 = animationEvents;
		obj2.OnAnimationComplete = (Action<string>)Delegate.Combine(obj2.OnAnimationComplete, new Action<string>(OnRoarComplete));
	}

	private void OnRoarComplete(string animationName)
	{
		entity.FinishedSpawnAnimation = true;
	}
}
