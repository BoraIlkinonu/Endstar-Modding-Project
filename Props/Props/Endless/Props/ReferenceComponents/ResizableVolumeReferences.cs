using System;
using System.Collections.Generic;
using Endless.Validation;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x02000028 RID: 40
	public class ResizableVolumeReferences : ReferenceBase
	{
		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060000A3 RID: 163 RVA: 0x00002E13 File Offset: 0x00001013
		// (set) Token: 0x060000A4 RID: 164 RVA: 0x00002E1B File Offset: 0x0000101B
		public BoxCollider[] CollidersToScale { get; private set; }

		// Token: 0x060000A5 RID: 165 RVA: 0x00002E24 File Offset: 0x00001024
		public override List<Validator> GetValidators()
		{
			return new List<Validator>
			{
				new SequenceValidator(new List<Validator>
				{
					new CollectionLengthValidator(this, this.CollidersToScale, 1, "CollidersToScale cannot be empty! You must provide at least 1 collider, and none of them can be null"),
					new BoxColliderValidator(this, this.CollidersToScale, Vector3.one, "A collider used by the resizable volume must be 1x1x1.")
				})
			};
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00002E7A File Offset: 0x0000107A
		private void OnDrawGizmos()
		{
			Color color = Gizmos.color;
			Gizmos.color = new Color(0f, 1f, 0f, 0.6f);
			Gizmos.DrawCube(base.transform.position, Vector3.one);
			Gizmos.color = color;
		}
	}
}
