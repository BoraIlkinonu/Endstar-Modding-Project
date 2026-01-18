using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Core.DeepLinking
{
	// Token: 0x02000011 RID: 17
	public static class DeepLinkActionMap
	{
		// Token: 0x06000045 RID: 69 RVA: 0x00003874 File Offset: 0x00001A74
		public static DeepLinkAction GetActionInstanceFromMap(string actionName)
		{
			Type type;
			if (!DeepLinkActionMap.Map.TryGetValue(actionName, out type))
			{
				Debug.LogException(new ArgumentException("Expected actionName to be valid action type, but was " + actionName + " instead."));
				return new NullDeepLinkAction();
			}
			DeepLinkAction deepLinkAction = Activator.CreateInstance(type) as DeepLinkAction;
			if (deepLinkAction == null)
			{
				Debug.LogException(new ArgumentException("Type returned for map for actionName (" + actionName + ") did not produce a valid DeepLinkAction."));
				return new NullDeepLinkAction();
			}
			return deepLinkAction;
		}

		// Token: 0x0400002F RID: 47
		private static readonly Dictionary<string, Type> Map = new Dictionary<string, Type> { 
		{
			InspectPublishedAssetDeepLinkAction.ActionName,
			typeof(InspectPublishedAssetDeepLinkAction)
		} };
	}
}
