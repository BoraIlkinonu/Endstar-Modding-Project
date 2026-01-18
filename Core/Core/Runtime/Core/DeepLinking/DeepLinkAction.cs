using System;
using System.Threading.Tasks;

namespace Runtime.Core.DeepLinking
{
	// Token: 0x0200000F RID: 15
	public abstract class DeepLinkAction
	{
		// Token: 0x0600003E RID: 62
		public abstract bool Parse(string argString);

		// Token: 0x0600003F RID: 63 RVA: 0x000037C0 File Offset: 0x000019C0
		public virtual async Task<bool> Execute()
		{
			return false;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x000037FB File Offset: 0x000019FB
		protected string[] SplitArguments(string args)
		{
			return args.Split("&", StringSplitOptions.None);
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00003809 File Offset: 0x00001A09
		protected string[] ParseArgument(string arg)
		{
			return arg.Split("=", StringSplitOptions.None);
		}
	}
}
