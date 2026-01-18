using System;
using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization
{
	// Token: 0x020004C5 RID: 1221
	public class CharacterVisualsEndlessUpdater : EndlessTypeJsonSerializer
	{
		// Token: 0x170005EE RID: 1518
		// (get) Token: 0x06001E5F RID: 7775 RVA: 0x0008477D File Offset: 0x0008297D
		protected override JsonConverter Converter
		{
			get
			{
				return new CharacterVisualsReferenceJsonConverter();
			}
		}
	}
}
