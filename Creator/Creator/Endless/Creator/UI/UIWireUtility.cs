using System;
using System.Collections.Generic;
using Endless.Props.Scripting;

namespace Endless.Creator.UI
{
	// Token: 0x020002FD RID: 765
	public static class UIWireUtility
	{
		// Token: 0x06000D49 RID: 3401 RVA: 0x0003FDF0 File Offset: 0x0003DFF0
		public static bool CanOverrideEmitterContextualValue(List<EndlessParameterInfo> emitterParamList, List<EndlessParameterInfo> receiverNodeParamList)
		{
			if (emitterParamList.Count != receiverNodeParamList.Count)
			{
				return false;
			}
			if (emitterParamList.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < emitterParamList.Count; i++)
			{
				if (emitterParamList[i].DataType != receiverNodeParamList[i].DataType)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x04000B6D RID: 2925
		private const bool VERBOSE_LOGGING = false;
	}
}
