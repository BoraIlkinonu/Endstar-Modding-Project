using System;

namespace Endless.Gameplay
{
	// Token: 0x020002F0 RID: 752
	public abstract class WorldUsableController : EndlessNetworkBehaviour
	{
		// Token: 0x17000354 RID: 852
		// (get) Token: 0x060010E1 RID: 4321
		public abstract WorldUsableDefinition WorldUsableDefinition { get; }

		// Token: 0x060010E2 RID: 4322
		public abstract bool AttemptInteract(InteractorBase interactorBase, int colliderIndex);

		// Token: 0x060010E3 RID: 4323
		public abstract void CancelInteraction(InteractorBase interactor);

		// Token: 0x060010E4 RID: 4324
		public abstract bool IsInteractingWith(InteractorBase interactor);

		// Token: 0x060010E6 RID: 4326 RVA: 0x00055390 File Offset: 0x00053590
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060010E7 RID: 4327 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060010E8 RID: 4328 RVA: 0x000553A6 File Offset: 0x000535A6
		protected internal override string __getTypeName()
		{
			return "WorldUsableController";
		}
	}
}
