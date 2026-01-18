using System;
using System.Collections.Generic;

namespace Endless.Gameplay
{
	// Token: 0x02000135 RID: 309
	public interface IAttributeSourceController
	{
		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000728 RID: 1832
		// (remove) Token: 0x06000729 RID: 1833
		event Action OnAttributeSourceChanged;

		// Token: 0x0600072A RID: 1834
		List<INpcAttributeModifier> GetAttributeModifiers();
	}
}
