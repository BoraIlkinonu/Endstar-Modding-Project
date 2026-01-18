using System;
using Endless.Shared;

namespace Runtime.Gameplay.LuaClasses
{
	// Token: 0x02000018 RID: 24
	public class LocalizedStringFactory
	{
		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000057 RID: 87 RVA: 0x000030DA File Offset: 0x000012DA
		internal static LocalizedStringFactory Instance
		{
			get
			{
				LocalizedStringFactory localizedStringFactory;
				if ((localizedStringFactory = LocalizedStringFactory.instance) == null)
				{
					localizedStringFactory = (LocalizedStringFactory.instance = new LocalizedStringFactory());
				}
				return localizedStringFactory;
			}
		}

		// Token: 0x06000058 RID: 88 RVA: 0x000030F0 File Offset: 0x000012F0
		public LocalizedString Create(string text)
		{
			return new LocalizedString(text);
		}

		// Token: 0x06000059 RID: 89 RVA: 0x000030F8 File Offset: 0x000012F8
		public LocalizedString Create(int language, string text)
		{
			return new LocalizedString((Language)language, text);
		}

		// Token: 0x04000042 RID: 66
		private static LocalizedStringFactory instance;
	}
}
