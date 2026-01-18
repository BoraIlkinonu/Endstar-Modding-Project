using System;
using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization
{
	// Token: 0x020004C2 RID: 1218
	public class CellReferenceArrayEndlessUpdater : EndlessTypeJsonSerializer
	{
		// Token: 0x170005ED RID: 1517
		// (get) Token: 0x06001E57 RID: 7767 RVA: 0x000846A0 File Offset: 0x000828A0
		protected override JsonConverter Converter
		{
			get
			{
				return new CellReferenceArrayJsonConverter();
			}
		}
	}
}
