using UnityEngine;

namespace Endless.Gameplay;

public abstract class AttackComponent : NpcComponent
{
	public const uint WARNING_FRAMES = 10u;

	[field: SerializeField]
	public int EquipmentNumber { get; private set; }

	public virtual void Awake()
	{
		if (base.IsServer)
		{
			base.NpcEntity.Components.IndividualStateUpdater.OnWriteState += HandleOnWriteState;
		}
		base.NpcEntity.Components.IndividualStateUpdater.OnReadState += HandleOnReadAiState;
		base.NpcEntity.Components.IndividualStateUpdater.OnUpdateState += HandleOnUpdateAiState;
	}

	protected virtual void HandleOnReadAiState(ref NpcState state)
	{
	}

	protected virtual void HandleOnUpdateAiState(uint frame)
	{
	}

	protected virtual void HandleOnWriteState(ref NpcState currentState)
	{
	}

	public abstract void ClearAttackQueue();
}
