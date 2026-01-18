using System;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002A6 RID: 678
	public class IconDefinition : ScriptableObject
	{
		// Token: 0x170002D6 RID: 726
		// (get) Token: 0x06000EEB RID: 3819 RVA: 0x0004EBDF File Offset: 0x0004CDDF
		public SerializableGuid IconId
		{
			get
			{
				return this.iconId;
			}
		}

		// Token: 0x170002D7 RID: 727
		// (get) Token: 0x06000EEC RID: 3820 RVA: 0x0004EBE7 File Offset: 0x0004CDE7
		public Texture2D IconTexture
		{
			get
			{
				return this.iconTexture;
			}
		}

		// Token: 0x04000D4B RID: 3403
		[SerializeField]
		private SerializableGuid iconId;

		// Token: 0x04000D4C RID: 3404
		[SerializeField]
		private Texture2D iconTexture;
	}
}
