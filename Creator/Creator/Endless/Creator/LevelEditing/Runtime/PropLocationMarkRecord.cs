using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x0200037D RID: 893
	public class PropLocationMarkRecord
	{
		// Token: 0x04000E15 RID: 3605
		public SerializableGuid Id;

		// Token: 0x04000E16 RID: 3606
		public PropLocationType Type;

		// Token: 0x04000E17 RID: 3607
		public List<Vector3Int> Locations = new List<Vector3Int>();
	}
}
