using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200015C RID: 348
	public static class NpcEnum
	{
		// Token: 0x06000827 RID: 2087 RVA: 0x00026620 File Offset: 0x00024820
		public static int GetAnimationHash(this NpcSpawnAnimation animation)
		{
			int num;
			switch (animation)
			{
			case NpcSpawnAnimation.None:
				num = Animator.StringToHash("");
				break;
			case NpcSpawnAnimation.Teleport:
				num = Animator.StringToHash("Spawn");
				break;
			case NpcSpawnAnimation.CrawlOut:
				num = Animator.StringToHash("SpawnZombie");
				break;
			default:
				throw new ArgumentOutOfRangeException("animation", animation, null);
			}
			return num;
		}

		// Token: 0x0200015D RID: 349
		public enum Animation
		{
			// Token: 0x04000680 RID: 1664
			Taunt,
			// Token: 0x04000681 RID: 1665
			Fidget
		}

		// Token: 0x0200015E RID: 350
		public enum CombatState
		{
			// Token: 0x04000683 RID: 1667
			None,
			// Token: 0x04000684 RID: 1668
			Attacking,
			// Token: 0x04000685 RID: 1669
			Engaged
		}

		// Token: 0x0200015F RID: 351
		[Flags]
		public enum Connection
		{
			// Token: 0x04000687 RID: 1671
			None = 0,
			// Token: 0x04000688 RID: 1672
			Walk = 1,
			// Token: 0x04000689 RID: 1673
			Jump = 2,
			// Token: 0x0400068A RID: 1674
			DropDown = 4,
			// Token: 0x0400068B RID: 1675
			DoubleJump = 8,
			// Token: 0x0400068C RID: 1676
			DoubleJumpDropDown = 16,
			// Token: 0x0400068D RID: 1677
			All = 31
		}

		// Token: 0x02000160 RID: 352
		[Flags]
		public enum Edge : short
		{
			// Token: 0x0400068F RID: 1679
			None = 0,
			// Token: 0x04000690 RID: 1680
			North = 1,
			// Token: 0x04000691 RID: 1681
			East = 2,
			// Token: 0x04000692 RID: 1682
			South = 4,
			// Token: 0x04000693 RID: 1683
			West = 8,
			// Token: 0x04000694 RID: 1684
			All = 15
		}

		// Token: 0x02000161 RID: 353
		public enum Equipment
		{
			// Token: 0x04000696 RID: 1686
			None,
			// Token: 0x04000697 RID: 1687
			DashPack,
			// Token: 0x04000698 RID: 1688
			JetPack,
			// Token: 0x04000699 RID: 1689
			StimPack,
			// Token: 0x0400069A RID: 1690
			Grenade
		}

		// Token: 0x02000162 RID: 354
		public enum FsmState
		{
			// Token: 0x0400069C RID: 1692
			Collapse,
			// Token: 0x0400069D RID: 1693
			Dead,
			// Token: 0x0400069E RID: 1694
			Downed,
			// Token: 0x0400069F RID: 1695
			FreeFall,
			// Token: 0x040006A0 RID: 1696
			Hit,
			// Token: 0x040006A1 RID: 1697
			JumpToNavMesh,
			// Token: 0x040006A2 RID: 1698
			Neutral,
			// Token: 0x040006A3 RID: 1699
			Plunge,
			// Token: 0x040006A4 RID: 1700
			StandUp,
			// Token: 0x040006A5 RID: 1701
			Stumble,
			// Token: 0x040006A6 RID: 1702
			Stunned,
			// Token: 0x040006A7 RID: 1703
			Warp,
			// Token: 0x040006A8 RID: 1704
			Interaction,
			// Token: 0x040006A9 RID: 1705
			Flinch,
			// Token: 0x040006AA RID: 1706
			Landing,
			// Token: 0x040006AB RID: 1707
			Jump,
			// Token: 0x040006AC RID: 1708
			GroundedPhysics,
			// Token: 0x040006AD RID: 1709
			Spawning,
			// Token: 0x040006AE RID: 1710
			Teleport
		}

		// Token: 0x02000163 RID: 355
		public enum Movement
		{
			// Token: 0x040006B0 RID: 1712
			Still,
			// Token: 0x040006B1 RID: 1713
			Moving,
			// Token: 0x040006B2 RID: 1714
			JumpingUp,
			// Token: 0x040006B3 RID: 1715
			JumpingDown
		}

		// Token: 0x02000164 RID: 356
		public enum ZombieOnKillEffect
		{
			// Token: 0x040006B5 RID: 1717
			None,
			// Token: 0x040006B6 RID: 1718
			SpawnZombie,
			// Token: 0x040006B7 RID: 1719
			ZombifyTarget
		}

		// Token: 0x02000165 RID: 357
		public enum PathUpdateResult
		{
			// Token: 0x040006B9 RID: 1721
			SameSegment,
			// Token: 0x040006BA RID: 1722
			NewSegment,
			// Token: 0x040006BB RID: 1723
			PathComplete
		}

		// Token: 0x02000166 RID: 358
		public enum SweepDirection
		{
			// Token: 0x040006BD RID: 1725
			NorthEast = 1,
			// Token: 0x040006BE RID: 1726
			NorthWest,
			// Token: 0x040006BF RID: 1727
			SouthEast,
			// Token: 0x040006C0 RID: 1728
			SouthWest
		}

		// Token: 0x02000167 RID: 359
		public enum Weapon
		{
			// Token: 0x040006C2 RID: 1730
			None,
			// Token: 0x040006C3 RID: 1731
			Sword1H,
			// Token: 0x040006C4 RID: 1732
			Sword2H,
			// Token: 0x040006C5 RID: 1733
			Ranged1H,
			// Token: 0x040006C6 RID: 1734
			Ranged2H,
			// Token: 0x040006C7 RID: 1735
			RocketLauncher
		}

		// Token: 0x02000168 RID: 360
		public enum Interactable
		{
			// Token: 0x040006C9 RID: 1737
			Lever,
			// Token: 0x040006CA RID: 1738
			Door,
			// Token: 0x040006CB RID: 1739
			Chair
		}

		// Token: 0x02000169 RID: 361
		public enum FallMode
		{
			// Token: 0x040006CD RID: 1741
			Death,
			// Token: 0x040006CE RID: 1742
			Damage,
			// Token: 0x040006CF RID: 1743
			Ignore,
			// Token: 0x040006D0 RID: 1744
			UseDefault
		}

		// Token: 0x0200016A RID: 362
		public enum PropFallMode
		{
			// Token: 0x040006D2 RID: 1746
			Death,
			// Token: 0x040006D3 RID: 1747
			Damage,
			// Token: 0x040006D4 RID: 1748
			Ignore
		}

		// Token: 0x0200016B RID: 363
		public enum AttributeRank
		{
			// Token: 0x040006D6 RID: 1750
			Base,
			// Token: 0x040006D7 RID: 1751
			Behavior,
			// Token: 0x040006D8 RID: 1752
			Interaction,
			// Token: 0x040006D9 RID: 1753
			Command
		}

		// Token: 0x0200016C RID: 364
		public enum PathfindingResult
		{
			// Token: 0x040006DB RID: 1755
			Failure,
			// Token: 0x040006DC RID: 1756
			Success,
			// Token: 0x040006DD RID: 1757
			Partial
		}
	}
}
