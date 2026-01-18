using System;
using Endless.Shared;
using Unity.Netcode;

namespace Endless.Gameplay
{
	// Token: 0x02000098 RID: 152
	public class EndlessNetworkBehaviourSingleton<T> : NetworkBehaviourSingleton<T> where T : NetworkBehaviour
	{
		// Token: 0x060002AD RID: 685 RVA: 0x0000DD17 File Offset: 0x0000BF17
		protected virtual void Start()
		{
			MonoBehaviourSingleton<EndlessLoop>.Instance.RegisterBehaviour(this);
		}

		// Token: 0x060002AE RID: 686 RVA: 0x0000E75F File Offset: 0x0000C95F
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (MonoBehaviourSingleton<EndlessLoop>.Instance)
			{
				MonoBehaviourSingleton<EndlessLoop>.Instance.RemoveBehaviour(this);
			}
		}

		// Token: 0x060002B0 RID: 688 RVA: 0x0000E788 File Offset: 0x0000C988
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060002B1 RID: 689 RVA: 0x0000E79E File Offset: 0x0000C99E
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060002B2 RID: 690 RVA: 0x0000E7A8 File Offset: 0x0000C9A8
		protected internal override string __getTypeName()
		{
			return "EndlessNetworkBehaviourSingleton`1";
		}
	}
}
