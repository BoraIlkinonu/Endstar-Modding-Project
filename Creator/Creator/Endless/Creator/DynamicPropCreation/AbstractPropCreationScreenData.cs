using System;
using System.Threading.Tasks;
using Endless.Gameplay;
using Endless.Props.Assets;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.DynamicPropCreation
{
	// Token: 0x020003AD RID: 941
	[CreateAssetMenu(menuName = "ScriptableObject/Dynamic Prop Creation/AbstractPropCreationScreenData")]
	public class AbstractPropCreationScreenData : PropCreationScreenData
	{
		// Token: 0x170002A2 RID: 674
		// (get) Token: 0x0600126D RID: 4717 RVA: 0x0005F0AE File Offset: 0x0005D2AE
		public string DefaultIconGuid
		{
			get
			{
				return this.defaultAbstractIcon.IconId;
			}
		}

		// Token: 0x0600126E RID: 4718 RVA: 0x0005F0C0 File Offset: 0x0005D2C0
		public async Task<Prop> UploadProp(string name, string description, bool shareWithGameOwners, SerializableGuid iconId, Texture2D capturedIconTexture)
		{
			Script script;
			Prop prop;
			base.SetupComponents(name, description, out script, out prop);
			prop.SetPropMetaData("AbstractIcon", iconId);
			return await base.UploadProp(prop, script, capturedIconTexture, shareWithGameOwners);
		}

		// Token: 0x04000F31 RID: 3889
		[SerializeField]
		private IconDefinition defaultAbstractIcon;
	}
}
