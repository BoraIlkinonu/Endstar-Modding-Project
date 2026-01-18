using System;

namespace Endless.Gameplay
{
	// Token: 0x02000266 RID: 614
	[Serializable]
	public class PhysicsObjectLibraryReference : PropLibraryReference
	{
		// Token: 0x17000247 RID: 583
		// (get) Token: 0x06000CA3 RID: 3235 RVA: 0x00044541 File Offset: 0x00042741
		internal override ReferenceFilter Filter
		{
			get
			{
				return ReferenceFilter.PhysicsObject;
			}
		}
	}
}
