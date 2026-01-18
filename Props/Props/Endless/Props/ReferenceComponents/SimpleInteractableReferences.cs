using System;
using System.Collections.Generic;
using Endless.Validation;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x0200002E RID: 46
	public class SimpleInteractableReferences : ComponentReferences
	{
		// Token: 0x1700004A RID: 74
		// (get) Token: 0x060000BF RID: 191 RVA: 0x00003068 File Offset: 0x00001268
		public IReadOnlyList<ColliderInfo> InteractableColliderInfos
		{
			get
			{
				return this.interactableColliderInfos;
			}
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00003070 File Offset: 0x00001270
		public override List<Validator> GetValidators()
		{
			return new List<Validator>
			{
				new SequenceValidator(new List<Validator>
				{
					new CollectionLengthValidator(this, this.interactableColliderInfos, 1, "interactableColliderInfos cannot be empty! You must provide at least 1 collider with an attached collider info, and none of them can be null"),
					new ColliderInfoValidator(this, this.interactableColliderInfos, ColliderInfo.ColliderType.Interactable, "A collider info used by an Interactable must use the interactable collider type")
				})
			};
		}

		// Token: 0x0400007D RID: 125
		[SerializeField]
		private ColliderInfo[] interactableColliderInfos;
	}
}
