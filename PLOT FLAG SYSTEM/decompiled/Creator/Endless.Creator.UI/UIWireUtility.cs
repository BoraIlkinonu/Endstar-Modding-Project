using System.Collections.Generic;
using Endless.Props.Scripting;

namespace Endless.Creator.UI;

public static class UIWireUtility
{
	private const bool VERBOSE_LOGGING = false;

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
}
