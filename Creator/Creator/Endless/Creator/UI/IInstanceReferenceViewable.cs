using System;
using Endless.Shared.DataTypes;

namespace Endless.Creator.UI
{
	// Token: 0x02000220 RID: 544
	public interface IInstanceReferenceViewable
	{
		// Token: 0x14000017 RID: 23
		// (add) Token: 0x060008C4 RID: 2244
		// (remove) Token: 0x060008C5 RID: 2245
		event Action<SerializableGuid> OnInstanceEyeDropped;
	}
}
