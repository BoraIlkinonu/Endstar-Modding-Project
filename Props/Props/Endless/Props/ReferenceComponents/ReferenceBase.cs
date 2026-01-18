using System;
using System.Collections.Generic;
using Endless.Validation;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x02000027 RID: 39
	public abstract class ReferenceBase : MonoBehaviour
	{
		// Token: 0x060000A1 RID: 161 RVA: 0x00002E04 File Offset: 0x00001004
		public virtual List<Validator> GetValidators()
		{
			return new List<Validator>();
		}
	}
}
