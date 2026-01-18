using System;
using Unity.Netcode;

namespace Endless.Gameplay
{
	// Token: 0x02000378 RID: 888
	public class WorldCollidable : NetworkBehaviour
	{
		// Token: 0x170004C5 RID: 1221
		// (get) Token: 0x060016BC RID: 5820 RVA: 0x0006AA68 File Offset: 0x00068C68
		// (set) Token: 0x060016BD RID: 5821 RVA: 0x0006AA70 File Offset: 0x00068C70
		public Func<bool> isSimulatedCheckOverride { get; set; }

		// Token: 0x170004C6 RID: 1222
		// (get) Token: 0x060016BE RID: 5822 RVA: 0x0006AA79 File Offset: 0x00068C79
		public bool IsSimulated
		{
			get
			{
				if (this.isSimulatedCheckOverride != null)
				{
					return this.isSimulatedCheckOverride();
				}
				return base.IsServer || base.IsOwner;
			}
		}

		// Token: 0x170004C7 RID: 1223
		// (get) Token: 0x060016BF RID: 5823 RVA: 0x0006AA9F File Offset: 0x00068C9F
		public IPhysicsTaker PhysicsTaker
		{
			get
			{
				if (this.physicsTaker == null)
				{
					this.physicsTaker = base.GetComponent<IPhysicsTaker>();
				}
				return this.physicsTaker;
			}
		}

		// Token: 0x170004C8 RID: 1224
		// (get) Token: 0x060016C0 RID: 5824 RVA: 0x0006AABB File Offset: 0x00068CBB
		public WorldObject WorldObject
		{
			get
			{
				if (this.worldObject == null)
				{
					this.worldObject = base.GetComponent<WorldObject>();
					if (this.worldObject == null)
					{
						this.worldObject = base.GetComponentInParent<WorldObject>();
					}
				}
				return this.worldObject;
			}
		}

		// Token: 0x060016C2 RID: 5826 RVA: 0x0006AAF8 File Offset: 0x00068CF8
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060016C3 RID: 5827 RVA: 0x0000E74E File Offset: 0x0000C94E
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060016C4 RID: 5828 RVA: 0x0006AB0E File Offset: 0x00068D0E
		protected internal override string __getTypeName()
		{
			return "WorldCollidable";
		}

		// Token: 0x04001248 RID: 4680
		private IPhysicsTaker physicsTaker;

		// Token: 0x04001249 RID: 4681
		private WorldObject worldObject;
	}
}
