using System;
using Endless.Props.ReferenceComponents;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000359 RID: 857
	public class ShieldComponent : EndlessBehaviour, IComponentBase
	{
		// Token: 0x0600159C RID: 5532 RVA: 0x00066C25 File Offset: 0x00064E25
		public int DamageShields(HealthModificationArgs args)
		{
			return args.Delta;
		}

		// Token: 0x17000485 RID: 1157
		// (get) Token: 0x0600159D RID: 5533 RVA: 0x00066C2D File Offset: 0x00064E2D
		// (set) Token: 0x0600159E RID: 5534 RVA: 0x00066C35 File Offset: 0x00064E35
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000486 RID: 1158
		// (get) Token: 0x0600159F RID: 5535 RVA: 0x00066C3E File Offset: 0x00064E3E
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(ShieldComponentReferences);
			}
		}

		// Token: 0x060015A0 RID: 5536 RVA: 0x00066C4A File Offset: 0x00064E4A
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x040011AC RID: 4524
		public UnityEvent OnShieldReducedReaction = new UnityEvent();

		// Token: 0x040011AD RID: 4525
		public UnityEvent OnShieldLostReaction = new UnityEvent();

		// Token: 0x040011AE RID: 4526
		public UnityEvent OnShieldGainedReaction = new UnityEvent();
	}
}
