using System;
using Endless.Gameplay.LevelEditing.Level;

namespace Endless.Creator.UI
{
	// Token: 0x02000203 RID: 515
	public class UIStoredParameterPackage
	{
		// Token: 0x06000811 RID: 2065 RVA: 0x00027EE2 File Offset: 0x000260E2
		public UIStoredParameterPackage(string name, StoredParameter storedParameter)
		{
			this.Name = name;
			this.StoredParameter = storedParameter;
		}

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x06000812 RID: 2066 RVA: 0x00027EF8 File Offset: 0x000260F8
		// (set) Token: 0x06000813 RID: 2067 RVA: 0x00027F00 File Offset: 0x00026100
		public string Name { get; private set; }

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x06000814 RID: 2068 RVA: 0x00027F09 File Offset: 0x00026109
		// (set) Token: 0x06000815 RID: 2069 RVA: 0x00027F11 File Offset: 0x00026111
		public StoredParameter StoredParameter { get; private set; }
	}
}
