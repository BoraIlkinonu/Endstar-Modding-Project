using System;
using System.Threading.Tasks;
using Endless.Props.Assets;
using UnityEngine;

namespace Endless.Creator.DynamicPropCreation
{
	// Token: 0x020003B1 RID: 945
	[CreateAssetMenu(menuName = "ScriptableObject/Dynamic Prop Creation/GenericPropCreationScreenData")]
	public class GenericPropCreationScreenData : PropCreationScreenData
	{
		// Token: 0x170002A3 RID: 675
		// (get) Token: 0x06001279 RID: 4729 RVA: 0x0005F465 File Offset: 0x0005D665
		public Texture2D PropIcon
		{
			get
			{
				return this.propIcon;
			}
		}

		// Token: 0x0600127A RID: 4730 RVA: 0x0005F470 File Offset: 0x0005D670
		public async Task<Prop> UploadProp(string name, string description, bool shareWithGameOwners)
		{
			Script script;
			Prop prop;
			base.SetupComponents(name, description, out script, out prop);
			return await base.UploadProp(prop, script, this.propIcon, shareWithGameOwners);
		}

		// Token: 0x04000F3D RID: 3901
		[SerializeField]
		private Texture2D propIcon;
	}
}
