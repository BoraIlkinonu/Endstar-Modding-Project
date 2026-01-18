using System;
using UnityEngine;

namespace Endless.Shared.SoVariables
{
	// Token: 0x020000D5 RID: 213
	public class SoVariable<T> : ScriptableObject
	{
		// Token: 0x040002E0 RID: 736
		[TextArea]
		public string Description;

		// Token: 0x040002E1 RID: 737
		public T Value;
	}
}
