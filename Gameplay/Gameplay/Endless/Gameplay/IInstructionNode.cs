using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay
{
	// Token: 0x02000326 RID: 806
	public interface IInstructionNode
	{
		// Token: 0x170003B7 RID: 951
		// (get) Token: 0x060012C3 RID: 4803
		string InstructionName { get; }

		// Token: 0x060012C4 RID: 4804
		void GiveInstruction(Context context);

		// Token: 0x060012C5 RID: 4805
		void RescindInstruction(Context context);

		// Token: 0x060012C6 RID: 4806
		Context GetContext();
	}
}
