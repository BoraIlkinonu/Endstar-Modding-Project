using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200012C RID: 300
	[Serializable]
	public struct DisplayNameAndDescription
	{
		// Token: 0x0400044C RID: 1100
		public string DisplayName;

		// Token: 0x0400044D RID: 1101
		[TextArea]
		public string Description;
	}
}
