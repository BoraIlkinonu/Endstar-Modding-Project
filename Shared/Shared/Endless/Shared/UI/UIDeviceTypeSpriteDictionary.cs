using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000126 RID: 294
	[CreateAssetMenu(menuName = "ScriptableObject/UI/Shared/Dictionaries/Device Type Sprite Dictionary", fileName = "Device Type Sprite Dictionary")]
	public class UIDeviceTypeSpriteDictionary : BaseEnumKeyScriptableObjectDictionary<DeviceTypes, Sprite>
	{
		// Token: 0x06000739 RID: 1849 RVA: 0x0001E70C File Offset: 0x0001C90C
		public Sprite GetActiveDeviceTypeValue()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetActiveDeviceTypeValue", Array.Empty<object>());
			}
			DeviceTypes deviceTypes = (MobileUtility.IsMobile ? DeviceTypes.Mobile : DeviceTypes.Standalone);
			return base[deviceTypes];
		}
	}
}
