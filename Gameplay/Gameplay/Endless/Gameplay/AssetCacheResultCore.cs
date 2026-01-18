using System;
using Endless.Assets;
using Endless.GraphQl;

namespace Endless.Gameplay
{
	// Token: 0x0200009B RID: 155
	public abstract class AssetCacheResultCore<T> where T : Asset
	{
		// Token: 0x17000082 RID: 130
		// (get) Token: 0x060002C1 RID: 705 RVA: 0x0000E940 File Offset: 0x0000CB40
		public bool HasErrors
		{
			get
			{
				GraphQlResult result = this.Result;
				return result != null && result.HasErrors;
			}
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x0000E95F File Offset: 0x0000CB5F
		public Exception GetErrorMessage()
		{
			return this.Result.GetErrorMessage(0);
		}

		// Token: 0x0400028A RID: 650
		public GraphQlResult Result;
	}
}
