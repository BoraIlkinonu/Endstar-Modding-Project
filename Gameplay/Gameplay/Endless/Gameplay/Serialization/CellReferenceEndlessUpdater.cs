using System;
using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization
{
	// Token: 0x020004C1 RID: 1217
	public class CellReferenceEndlessUpdater : EndlessTypeJsonSerializer
	{
		// Token: 0x170005EC RID: 1516
		// (get) Token: 0x06001E55 RID: 7765 RVA: 0x00084691 File Offset: 0x00082891
		protected override JsonConverter Converter
		{
			get
			{
				return new CellReferenceJsonConverter();
			}
		}
	}
}
