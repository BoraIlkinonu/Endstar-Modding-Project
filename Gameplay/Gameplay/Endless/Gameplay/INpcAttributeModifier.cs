using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay
{
	// Token: 0x02000138 RID: 312
	public interface INpcAttributeModifier
	{
		// Token: 0x06000758 RID: 1880
		InstructionNode GetNode();

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x06000759 RID: 1881
		CombatMode CombatMode { get; }

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x0600075A RID: 1882
		DamageMode DamageMode { get; }

		// Token: 0x17000159 RID: 345
		// (get) Token: 0x0600075B RID: 1883
		PhysicsMode PhysicsMode { get; }

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x0600075C RID: 1884
		NpcEnum.FallMode FallMode { get; }

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x0600075D RID: 1885
		MovementMode MovementMode { get; }

		// Token: 0x1700015C RID: 348
		// (get) Token: 0x0600075E RID: 1886
		NpcEnum.AttributeRank AttributeRank { get; }
	}
}
