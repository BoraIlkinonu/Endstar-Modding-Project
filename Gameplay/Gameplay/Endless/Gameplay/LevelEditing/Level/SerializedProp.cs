using System;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000562 RID: 1378
	[Serializable]
	public class SerializedProp
	{
		// Token: 0x04001A5B RID: 6747
		public Vector3 Position;

		// Token: 0x04001A5C RID: 6748
		public float Rotation;

		// Token: 0x04001A5D RID: 6749
		public SerializableGuid MasterReference;
	}
}
