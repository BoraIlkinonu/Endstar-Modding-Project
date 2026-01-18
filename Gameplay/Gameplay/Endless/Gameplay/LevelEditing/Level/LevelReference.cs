using System;
using Endless.Assets;
using Newtonsoft.Json;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000546 RID: 1350
	[Serializable]
	public class LevelReference : AssetReference
	{
		// Token: 0x0600208C RID: 8332 RVA: 0x000927F4 File Offset: 0x000909F4
		public LevelReference()
		{
			this.UpdateParentVersion = true;
		}

		// Token: 0x0600208D RID: 8333 RVA: 0x00092803 File Offset: 0x00090A03
		public virtual object GetAnonymousObject()
		{
			return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(this));
		}
	}
}
