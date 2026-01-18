using System;
using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay
{
	// Token: 0x0200025B RID: 603
	public class GenericWorldState : IEquatable<GenericWorldState>
	{
		// Token: 0x06000C7F RID: 3199 RVA: 0x000435B4 File Offset: 0x000417B4
		public GenericWorldState(WorldState worldState, Func<NpcEntity, bool> stateFunc)
		{
			this.WorldState = worldState;
			this.stateFunc = stateFunc;
		}

		// Token: 0x1700023E RID: 574
		// (get) Token: 0x06000C80 RID: 3200 RVA: 0x000435CA File Offset: 0x000417CA
		public WorldState WorldState { get; }

		// Token: 0x06000C81 RID: 3201 RVA: 0x000435D2 File Offset: 0x000417D2
		public bool Evaluate(NpcEntity entity)
		{
			return this.stateFunc(entity);
		}

		// Token: 0x06000C82 RID: 3202 RVA: 0x000435E0 File Offset: 0x000417E0
		public bool Equals(GenericWorldState other)
		{
			return other != null && (this == other || (this.WorldState == other.WorldState && object.Equals(this.stateFunc, other.stateFunc)));
		}

		// Token: 0x06000C83 RID: 3203 RVA: 0x0004360E File Offset: 0x0004180E
		public override bool Equals(object obj)
		{
			return obj != null && (this == obj || (obj.GetType() == base.GetType() && this.Equals((GenericWorldState)obj)));
		}

		// Token: 0x06000C84 RID: 3204 RVA: 0x0004363C File Offset: 0x0004183C
		public override int GetHashCode()
		{
			return HashCode.Combine<WorldState, Func<NpcEntity, bool>>(this.WorldState, this.stateFunc);
		}

		// Token: 0x04000B7A RID: 2938
		private readonly Func<NpcEntity, bool> stateFunc;
	}
}
