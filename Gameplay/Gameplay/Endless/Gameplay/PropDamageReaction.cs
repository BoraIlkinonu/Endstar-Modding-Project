using System;
using System.Linq;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000D0 RID: 208
	public class PropDamageReaction : MaterialModifier, IStartSubscriber
	{
		// Token: 0x06000427 RID: 1063 RVA: 0x00016A72 File Offset: 0x00014C72
		private void Awake()
		{
			this.propBlock = new MaterialPropertyBlock();
		}

		// Token: 0x06000428 RID: 1064 RVA: 0x00016A7F File Offset: 0x00014C7F
		public void Initialize(HittableComponent hittable, HittableReferences references)
		{
			this.hittableComponent = hittable;
			this.renderers = references.HitFlashRenderers.ToList<Renderer>();
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x00016A99 File Offset: 0x00014C99
		private void HandleDamaged(HittableComponent hittable, HealthModificationArgs args)
		{
			base.StartHurtFlash();
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x00016AA1 File Offset: 0x00014CA1
		public void EndlessStart()
		{
			this.hittableComponent.OnDamaged += this.HandleDamaged;
		}

		// Token: 0x040003AA RID: 938
		[SerializeField]
		[HideInInspector]
		private HittableComponent hittableComponent;
	}
}
