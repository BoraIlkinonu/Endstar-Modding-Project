using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay;

public static class NpcEnum
{
	public enum Animation
	{
		Taunt,
		Fidget
	}

	public enum CombatState
	{
		None,
		Attacking,
		Engaged
	}

	[Flags]
	public enum Connection
	{
		None = 0,
		Walk = 1,
		Jump = 2,
		DropDown = 4,
		DoubleJump = 8,
		DoubleJumpDropDown = 0x10,
		All = 0x1F
	}

	[Flags]
	public enum Edge : short
	{
		None = 0,
		North = 1,
		East = 2,
		South = 4,
		West = 8,
		All = 0xF
	}

	public enum Equipment
	{
		None,
		DashPack,
		JetPack,
		StimPack,
		Grenade
	}

	public enum FsmState
	{
		Collapse,
		Dead,
		Downed,
		FreeFall,
		Hit,
		JumpToNavMesh,
		Neutral,
		Plunge,
		StandUp,
		Stumble,
		Stunned,
		Warp,
		Interaction,
		Flinch,
		Landing,
		Jump,
		GroundedPhysics,
		Spawning,
		Teleport
	}

	public enum Movement
	{
		Still,
		Moving,
		JumpingUp,
		JumpingDown
	}

	public enum ZombieOnKillEffect
	{
		None,
		SpawnZombie,
		ZombifyTarget
	}

	public enum PathUpdateResult
	{
		SameSegment,
		NewSegment,
		PathComplete
	}

	public enum SweepDirection
	{
		NorthEast = 1,
		NorthWest,
		SouthEast,
		SouthWest
	}

	public enum Weapon
	{
		None,
		Sword1H,
		Sword2H,
		Ranged1H,
		Ranged2H,
		RocketLauncher
	}

	public enum Interactable
	{
		Lever,
		Door,
		Chair
	}

	public enum FallMode
	{
		Death,
		Damage,
		Ignore,
		UseDefault
	}

	public enum PropFallMode
	{
		Death,
		Damage,
		Ignore
	}

	public enum AttributeRank
	{
		Base,
		Behavior,
		Interaction,
		Command
	}

	public enum PathfindingResult
	{
		Failure,
		Success,
		Partial
	}

	public static int GetAnimationHash(this NpcSpawnAnimation animation)
	{
		return animation switch
		{
			NpcSpawnAnimation.None => Animator.StringToHash(""), 
			NpcSpawnAnimation.Teleport => Animator.StringToHash("Spawn"), 
			NpcSpawnAnimation.CrawlOut => Animator.StringToHash("SpawnZombie"), 
			_ => throw new ArgumentOutOfRangeException("animation", animation, null), 
		};
	}
}
