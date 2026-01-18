using System;
using System.Collections.Generic;
using Endless.Props;
using Endless.Validation;
using UnityEngine;

namespace Runtime.Props.Validations
{
	// Token: 0x02000003 RID: 3
	public class ReadWriteSetValidator : Validator
	{
		// Token: 0x06000003 RID: 3 RVA: 0x000020C0 File Offset: 0x000002C0
		public ReadWriteSetValidator(GameObject prefab)
		{
			this.target = prefab;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000020D0 File Offset: 0x000002D0
		public override List<ValidationResult> PassesValidation()
		{
			List<ValidationResult> list = new List<ValidationResult>();
			foreach (MeshCollider meshCollider in this.target.GetComponentsInChildren<MeshCollider>())
			{
				bool isReadable = meshCollider.sharedMesh.isReadable;
				ColliderInfo componentInParent = meshCollider.gameObject.GetComponentInParent<ColliderInfo>();
				if (!isReadable)
				{
					if (componentInParent != null)
					{
						if (componentInParent.Type == ColliderInfo.ColliderType.Default)
						{
							list.AddRange(ValidationResult.Fail("Mesh (" + meshCollider.sharedMesh.name + ") requires read/write enabled.", meshCollider.gameObject));
						}
					}
					else if (meshCollider.gameObject.layer == LayerMask.NameToLayer("Default"))
					{
						list.AddRange(ValidationResult.Fail("Mesh (" + meshCollider.sharedMesh.name + ") requires read/write enabled.", meshCollider.gameObject));
					}
				}
			}
			return list;
		}

		// Token: 0x04000001 RID: 1
		private GameObject target;
	}
}
