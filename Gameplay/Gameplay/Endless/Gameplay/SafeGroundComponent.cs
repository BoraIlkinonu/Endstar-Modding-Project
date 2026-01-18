using System;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000298 RID: 664
	public class SafeGroundComponent : NetworkBehaviour
	{
		// Token: 0x170002C4 RID: 708
		// (get) Token: 0x06000EBC RID: 3772 RVA: 0x0004E73A File Offset: 0x0004C93A
		public Vector3 LastSafePosition
		{
			get
			{
				return this.position.Value;
			}
		}

		// Token: 0x06000EBD RID: 3773 RVA: 0x0004E747 File Offset: 0x0004C947
		public void RegisterSafeGround(Vector3 pos)
		{
			if (base.IsServer)
			{
				this.position.Value = pos;
			}
		}

		// Token: 0x06000EBF RID: 3775 RVA: 0x0004E78C File Offset: 0x0004C98C
		protected override void __initializeVariables()
		{
			bool flag = this.position == null;
			if (flag)
			{
				throw new Exception("SafeGroundComponent.position cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.position.Initialize(this);
			base.__nameNetworkVariable(this.position, "position");
			this.NetworkVariableFields.Add(this.position);
			base.__initializeVariables();
		}

		// Token: 0x06000EC0 RID: 3776 RVA: 0x0000E74E File Offset: 0x0000C94E
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000EC1 RID: 3777 RVA: 0x0004E7EF File Offset: 0x0004C9EF
		protected internal override string __getTypeName()
		{
			return "SafeGroundComponent";
		}

		// Token: 0x04000D37 RID: 3383
		private NetworkVariable<Vector3> position = new NetworkVariable<Vector3>(default(Vector3), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	}
}
