using System;
using Endless.Shared;
using Unity.Netcode;

namespace Endless.Gameplay
{
	// Token: 0x02000097 RID: 151
	public class EndlessNetworkBehaviour : NetworkBehaviour
	{
		// Token: 0x060002A7 RID: 679 RVA: 0x0000DD17 File Offset: 0x0000BF17
		protected virtual void Start()
		{
			MonoBehaviourSingleton<EndlessLoop>.Instance.RegisterBehaviour(this);
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x0000E710 File Offset: 0x0000C910
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (MonoBehaviourSingleton<EndlessLoop>.Instance)
			{
				MonoBehaviourSingleton<EndlessLoop>.Instance.RemoveBehaviour(this);
			}
		}

		// Token: 0x060002AA RID: 682 RVA: 0x0000E738 File Offset: 0x0000C938
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060002AB RID: 683 RVA: 0x0000E74E File Offset: 0x0000C94E
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060002AC RID: 684 RVA: 0x0000E758 File Offset: 0x0000C958
		protected internal override string __getTypeName()
		{
			return "EndlessNetworkBehaviour";
		}
	}
}
