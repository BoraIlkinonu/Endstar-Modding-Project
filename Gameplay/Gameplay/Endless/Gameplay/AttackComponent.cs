using System;

namespace Endless.Gameplay
{
	// Token: 0x02000130 RID: 304
	public abstract class AttackComponent : NpcComponent
	{
		// Token: 0x17000137 RID: 311
		// (get) Token: 0x060006DB RID: 1755 RVA: 0x000217C0 File Offset: 0x0001F9C0
		// (set) Token: 0x060006DC RID: 1756 RVA: 0x000217C8 File Offset: 0x0001F9C8
		public int EquipmentNumber { get; private set; }

		// Token: 0x060006DD RID: 1757 RVA: 0x000217D4 File Offset: 0x0001F9D4
		public virtual void Awake()
		{
			if (base.IsServer)
			{
				base.NpcEntity.Components.IndividualStateUpdater.OnWriteState += this.HandleOnWriteState;
			}
			base.NpcEntity.Components.IndividualStateUpdater.OnReadState += this.HandleOnReadAiState;
			base.NpcEntity.Components.IndividualStateUpdater.OnUpdateState += this.HandleOnUpdateAiState;
		}

		// Token: 0x060006DE RID: 1758 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleOnReadAiState(ref NpcState state)
		{
		}

		// Token: 0x060006DF RID: 1759 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleOnUpdateAiState(uint frame)
		{
		}

		// Token: 0x060006E0 RID: 1760 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleOnWriteState(ref NpcState currentState)
		{
		}

		// Token: 0x060006E1 RID: 1761
		public abstract void ClearAttackQueue();

		// Token: 0x04000596 RID: 1430
		public const uint WARNING_FRAMES = 10U;
	}
}
