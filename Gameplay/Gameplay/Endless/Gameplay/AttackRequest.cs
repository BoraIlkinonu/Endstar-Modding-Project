using System;

namespace Endless.Gameplay
{
	// Token: 0x02000252 RID: 594
	public readonly struct AttackRequest : IEquatable<AttackRequest>
	{
		// Token: 0x06000C51 RID: 3153 RVA: 0x00042BC4 File Offset: 0x00040DC4
		public AttackRequest(WorldObject requester, HittableComponent target, Action<WorldObject> response)
		{
			this.Requester = requester;
			this.Target = target;
			this.Response = response;
		}

		// Token: 0x06000C52 RID: 3154 RVA: 0x00042BDB File Offset: 0x00040DDB
		public bool Equals(AttackRequest other)
		{
			return this.Requester == other.Requester && this.Target == other.Target;
		}

		// Token: 0x06000C53 RID: 3155 RVA: 0x00042C04 File Offset: 0x00040E04
		public override bool Equals(object obj)
		{
			if (obj is AttackRequest)
			{
				AttackRequest attackRequest = (AttackRequest)obj;
				return this.Equals(attackRequest);
			}
			return false;
		}

		// Token: 0x06000C54 RID: 3156 RVA: 0x00042C29 File Offset: 0x00040E29
		public override int GetHashCode()
		{
			return HashCode.Combine<WorldObject, HittableComponent>(this.Requester, this.Target);
		}

		// Token: 0x04000B53 RID: 2899
		public readonly WorldObject Requester;

		// Token: 0x04000B54 RID: 2900
		public readonly HittableComponent Target;

		// Token: 0x04000B55 RID: 2901
		public readonly Action<WorldObject> Response;
	}
}
