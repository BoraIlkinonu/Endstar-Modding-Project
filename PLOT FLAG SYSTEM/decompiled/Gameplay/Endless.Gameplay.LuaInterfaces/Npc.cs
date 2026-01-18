using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay.LuaInterfaces;

public class Npc
{
	private readonly NpcEntity entity;

	private Endless.Gameplay.TextBubble TextBubble => entity.Components.TextBubble;

	private NpcInteractable Interactable => entity.Components.Interactable;

	internal Npc(NpcEntity npcEntity)
	{
		entity = npcEntity;
	}

	public void Kill(Context instigator)
	{
		entity.Components.HittableComponent.ModifyHealth(new HealthModificationArgs(-10000, instigator, DamageType.Normal, HealthChangeType.Unavoidable));
	}

	public void SetCombatMode(Context instigator, int combatMode)
	{
		entity.BaseCombatMode = (CombatMode)combatMode;
	}

	public void SetDamageMode(Context instigator, int damageMode)
	{
		entity.BaseDamageMode = (DamageMode)damageMode;
	}

	public void SetPhysicsMode(Context instigator, int physicsMode)
	{
		entity.BasePhysicsMode = (PhysicsMode)physicsMode;
	}

	public void SetCanFidget(Context instigator, bool canFidget)
	{
		entity.NpcBlackboard.Set(NpcBlackboard.Key.CanFidget, canFidget);
	}

	public void SetDestinationTolerance(Context instigator, float newTolerance)
	{
		entity.NpcBlackboard.Set(NpcBlackboard.Key.DestinationTolerance, newTolerance);
	}

	public void SetDestination(Context instigator)
	{
		if (instigator.WorldObject.BaseType is AbstractBlock abstractBlock)
		{
			entity.NpcBlackboard.Set(NpcBlackboard.Key.CommandDestination, abstractBlock.transform.position + UnityEngine.Vector3.down * 0.5f);
		}
	}

	public void SetDestinationToCell(Context instigator, CellReference cellReference, Context fallbackContextForPosition)
	{
		if (cellReference.HasValue)
		{
			if (TryGetPositionOnNavMesh(cellReference.GetCellPositionAsVector3Int(), out var groundPosition))
			{
				if (cellReference.Rotation.HasValue)
				{
					SetDestination(groundPosition, NpcBlackboard.Key.CommandDestination, Quaternion.Euler(0f, cellReference.Rotation.Value, 0f));
				}
				else
				{
					SetDestination(groundPosition, NpcBlackboard.Key.CommandDestination);
				}
			}
			return;
		}
		if (fallbackContextForPosition != null)
		{
			if (TryGetPositionOnNavMesh(fallbackContextForPosition.GetPosition(), out var groundPosition2))
			{
				SetDestination(groundPosition2, NpcBlackboard.Key.CommandDestination);
			}
			return;
		}
		throw new Exception("No cell reference or fallback position source");
	}

	public void SetDestinationToPosition(Context instigator, UnityEngine.Vector3 position)
	{
		if (TryGetPositionOnNavMesh(position, out var groundPosition))
		{
			SetDestination(groundPosition, NpcBlackboard.Key.CommandDestination);
		}
	}

	public void SetDestinationToPosition(Context instigator, Vector3Int position)
	{
		if (TryGetPositionOnNavMesh(position, out var groundPosition))
		{
			SetDestination(groundPosition, NpcBlackboard.Key.CommandDestination);
		}
	}

	private static bool TryGetPositionOnNavMesh(UnityEngine.Vector3 point, out UnityEngine.Vector3 groundPosition)
	{
		groundPosition = UnityEngine.Vector3.zero;
		if (NavMesh.SamplePosition(point, out var hit, 0.5f, -1))
		{
			groundPosition = hit.position;
			return true;
		}
		return false;
	}

	private void SetDestination(UnityEngine.Vector3 position, NpcBlackboard.Key destinationKey, Quaternion? rotation = null)
	{
		entity.NpcBlackboard.Set(destinationKey, position);
		if (rotation.HasValue)
		{
			entity.NpcBlackboard.Set(NpcBlackboard.Key.Rotation, rotation.Value);
		}
	}

	public void ClearDestination(Context instigator)
	{
		entity.NpcBlackboard.Clear<UnityEngine.Vector3>(NpcBlackboard.Key.CommandDestination);
		entity.NpcBlackboard.Clear<Quaternion>(NpcBlackboard.Key.Rotation);
	}

	public UnityEngine.Vector3 GetNpcPosition(Context instigator)
	{
		return entity.transform.position;
	}

	public void SetNewWanderPosition(Context instigator, float distance)
	{
		UnityEngine.Vector3? wanderPosition = Pathfinding.GetWanderPosition(entity.FootPosition, distance);
		if (wanderPosition.HasValue)
		{
			SetDestination(wanderPosition.Value, NpcBlackboard.Key.BehaviorDestination);
		}
	}

	private void SetNewRovePosition(Context instigator, UnityEngine.Vector3 position, float roveDistance, float distancePerMove)
	{
		UnityEngine.Vector3? rovePosition = Pathfinding.GetRovePosition(position + UnityEngine.Vector3.down * 0.5f, entity.FootPosition, roveDistance, distancePerMove);
		if (rovePosition.HasValue)
		{
			SetDestination(rovePosition.Value, NpcBlackboard.Key.BehaviorDestination);
		}
	}

	public void SetNewRovePosition(Context instigator, float roveDistance, float distancePerMove)
	{
		if (instigator.WorldObject.BaseType is AbstractBlock abstractBlock)
		{
			SetNewRovePosition(instigator, abstractBlock.transform.position, roveDistance, distancePerMove);
		}
	}

	public int GetNumberOfInteractables()
	{
		return 1;
	}
}
