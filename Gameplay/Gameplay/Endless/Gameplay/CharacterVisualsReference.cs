using System;
using Endless.Gameplay.LevelEditing;
using Endless.Shared;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay
{
	// Token: 0x020000F3 RID: 243
	[Serializable]
	public class CharacterVisualsReference : AssetLibraryReferenceClass
	{
		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x06000560 RID: 1376 RVA: 0x00016B42 File Offset: 0x00014D42
		internal SerializableGuid AssetId
		{
			get
			{
				return this.Id;
			}
		}

		// Token: 0x06000561 RID: 1377 RVA: 0x0001B75F File Offset: 0x0001995F
		internal CharacterCosmeticsDefinition GetDefinition()
		{
			if (this.Id.IsEmpty)
			{
				return null;
			}
			return MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultCharacterCosmetics[this.Id];
		}

		// Token: 0x06000562 RID: 1378 RVA: 0x0001B785 File Offset: 0x00019985
		public override string ToString()
		{
			return base.ToString() + " | " + ((this.GetDefinition() == null) ? "null" : this.GetDefinition().DisplayName);
		}
	}
}
