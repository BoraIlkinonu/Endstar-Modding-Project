using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x0200045C RID: 1116
	public class Npc
	{
		// Token: 0x17000589 RID: 1417
		// (get) Token: 0x06001BDD RID: 7133 RVA: 0x0007CC17 File Offset: 0x0007AE17
		private TextBubble TextBubble
		{
			get
			{
				return this.entity.Components.TextBubble;
			}
		}

		// Token: 0x1700058A RID: 1418
		// (get) Token: 0x06001BDE RID: 7134 RVA: 0x0007CC29 File Offset: 0x0007AE29
		private NpcInteractable Interactable
		{
			get
			{
				return this.entity.Components.Interactable;
			}
		}

		// Token: 0x06001BDF RID: 7135 RVA: 0x0007CC3B File Offset: 0x0007AE3B
		internal Npc(NpcEntity npcEntity)
		{
			this.entity = npcEntity;
		}

		// Token: 0x06001BE0 RID: 7136 RVA: 0x0007CC4A File Offset: 0x0007AE4A
		public void Kill(Context instigator)
		{
			this.entity.Components.HittableComponent.ModifyHealth(new HealthModificationArgs(-10000, instigator, DamageType.Normal, HealthChangeType.Unavoidable));
		}

		// Token: 0x06001BE1 RID: 7137 RVA: 0x0007CC6F File Offset: 0x0007AE6F
		public void SetCombatMode(Context instigator, int combatMode)
		{
			this.entity.BaseCombatMode = (CombatMode)combatMode;
		}

		// Token: 0x06001BE2 RID: 7138 RVA: 0x0007CC7D File Offset: 0x0007AE7D
		public void SetDamageMode(Context instigator, int damageMode)
		{
			this.entity.BaseDamageMode = (DamageMode)damageMode;
		}

		// Token: 0x06001BE3 RID: 7139 RVA: 0x0007CC8B File Offset: 0x0007AE8B
		public void SetPhysicsMode(Context instigator, int physicsMode)
		{
			this.entity.BasePhysicsMode = (PhysicsMode)physicsMode;
		}

		// Token: 0x06001BE4 RID: 7140 RVA: 0x0007CC99 File Offset: 0x0007AE99
		public void SetCanFidget(Context instigator, bool canFidget)
		{
			this.entity.NpcBlackboard.Set<bool>(NpcBlackboard.Key.CanFidget, canFidget);
		}

		// Token: 0x06001BE5 RID: 7141 RVA: 0x0007CCAD File Offset: 0x0007AEAD
		public void SetDestinationTolerance(Context instigator, float newTolerance)
		{
			this.entity.NpcBlackboard.Set<float>(NpcBlackboard.Key.DestinationTolerance, newTolerance);
		}

		// Token: 0x06001BE6 RID: 7142 RVA: 0x0007CCC4 File Offset: 0x0007AEC4
		public void SetDestination(Context instigator)
		{
			AbstractBlock abstractBlock = instigator.WorldObject.BaseType as AbstractBlock;
			if (abstractBlock != null)
			{
				this.entity.NpcBlackboard.Set<global::UnityEngine.Vector3>(NpcBlackboard.Key.CommandDestination, abstractBlock.transform.position + global::UnityEngine.Vector3.down * 0.5f);
			}
		}

		// Token: 0x06001BE7 RID: 7143 RVA: 0x0007CD18 File Offset: 0x0007AF18
		public void SetDestinationToCell(Context instigator, CellReference cellReference, Context fallbackContextForPosition)
		{
			if (cellReference.HasValue)
			{
				global::UnityEngine.Vector3 vector;
				if (Npc.TryGetPositionOnNavMesh(cellReference.GetCellPositionAsVector3Int(), out vector))
				{
					if (cellReference.Rotation != null)
					{
						this.SetDestination(vector, NpcBlackboard.Key.CommandDestination, new Quaternion?(Quaternion.Euler(0f, cellReference.Rotation.Value, 0f)));
						return;
					}
					this.SetDestination(vector, NpcBlackboard.Key.CommandDestination, null);
					return;
				}
			}
			else
			{
				if (fallbackContextForPosition == null)
				{
					throw new Exception("No cell reference or fallback position source");
				}
				global::UnityEngine.Vector3 vector2;
				if (Npc.TryGetPositionOnNavMesh(fallbackContextForPosition.GetPosition(), out vector2))
				{
					this.SetDestination(vector2, NpcBlackboard.Key.CommandDestination, null);
					return;
				}
			}
		}

		// Token: 0x06001BE8 RID: 7144 RVA: 0x0007CDBC File Offset: 0x0007AFBC
		public void SetDestinationToPosition(Context instigator, global::UnityEngine.Vector3 position)
		{
			global::UnityEngine.Vector3 vector;
			if (Npc.TryGetPositionOnNavMesh(position, out vector))
			{
				this.SetDestination(vector, NpcBlackboard.Key.CommandDestination, null);
			}
		}

		// Token: 0x06001BE9 RID: 7145 RVA: 0x0007CDE8 File Offset: 0x0007AFE8
		public void SetDestinationToPosition(Context instigator, Vector3Int position)
		{
			global::UnityEngine.Vector3 vector;
			if (Npc.TryGetPositionOnNavMesh(position, out vector))
			{
				this.SetDestination(vector, NpcBlackboard.Key.CommandDestination, null);
			}
		}

		// Token: 0x06001BEA RID: 7146 RVA: 0x0007CE18 File Offset: 0x0007B018
		private static bool TryGetPositionOnNavMesh(global::UnityEngine.Vector3 point, out global::UnityEngine.Vector3 groundPosition)
		{
			groundPosition = global::UnityEngine.Vector3.zero;
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(point, out navMeshHit, 0.5f, -1))
			{
				groundPosition = navMeshHit.position;
				return true;
			}
			return false;
		}

		// Token: 0x06001BEB RID: 7147 RVA: 0x0007CE50 File Offset: 0x0007B050
		private void SetDestination(global::UnityEngine.Vector3 position, NpcBlackboard.Key destinationKey, Quaternion? rotation = null)
		{
			this.entity.NpcBlackboard.Set<global::UnityEngine.Vector3>(destinationKey, position);
			if (rotation != null)
			{
				this.entity.NpcBlackboard.Set<Quaternion>(NpcBlackboard.Key.Rotation, rotation.Value);
			}
		}

		// Token: 0x06001BEC RID: 7148 RVA: 0x0007CE86 File Offset: 0x0007B086
		public void ClearDestination(Context instigator)
		{
			this.entity.NpcBlackboard.Clear<global::UnityEngine.Vector3>(NpcBlackboard.Key.CommandDestination);
			this.entity.NpcBlackboard.Clear<Quaternion>(NpcBlackboard.Key.Rotation);
		}

		// Token: 0x06001BED RID: 7149 RVA: 0x0007CEAC File Offset: 0x0007B0AC
		public global::UnityEngine.Vector3 GetNpcPosition(Context instigator)
		{
			return this.entity.transform.position;
		}

		// Token: 0x06001BEE RID: 7150 RVA: 0x0007CEC0 File Offset: 0x0007B0C0
		public void SetNewWanderPosition(Context instigator, float distance)
		{
			global::UnityEngine.Vector3? wanderPosition = Pathfinding.GetWanderPosition(this.entity.FootPosition, distance);
			if (wanderPosition != null)
			{
				this.SetDestination(wanderPosition.Value, NpcBlackboard.Key.BehaviorDestination, null);
			}
		}

		// Token: 0x06001BEF RID: 7151 RVA: 0x0007CF00 File Offset: 0x0007B100
		private void SetNewRovePosition(Context instigator, global::UnityEngine.Vector3 position, float roveDistance, float distancePerMove)
		{
			global::UnityEngine.Vector3? rovePosition = Pathfinding.GetRovePosition(position + global::UnityEngine.Vector3.down * 0.5f, this.entity.FootPosition, roveDistance, distancePerMove);
			if (rovePosition != null)
			{
				this.SetDestination(rovePosition.Value, NpcBlackboard.Key.BehaviorDestination, null);
			}
		}

		// Token: 0x06001BF0 RID: 7152 RVA: 0x0007CF58 File Offset: 0x0007B158
		public void SetNewRovePosition(Context instigator, float roveDistance, float distancePerMove)
		{
			AbstractBlock abstractBlock = instigator.WorldObject.BaseType as AbstractBlock;
			if (abstractBlock != null)
			{
				this.SetNewRovePosition(instigator, abstractBlock.transform.position, roveDistance, distancePerMove);
			}
		}

		// Token: 0x06001BF1 RID: 7153 RVA: 0x00017586 File Offset: 0x00015786
		public int GetNumberOfInteractables()
		{
			return 1;
		}

		// Token: 0x040015BE RID: 5566
		private readonly NpcEntity entity;
	}
}
