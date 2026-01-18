using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.Serialization;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000270 RID: 624
	public class UIInspectorScriptValueTypeRadio : UIBaseRadio<Type>
	{
		// Token: 0x06000A60 RID: 2656 RVA: 0x000309AB File Offset: 0x0002EBAB
		protected override void Initialize()
		{
			base.SetDefaultValue(EndlessTypeMapping.Instance.LuaInspectorTypes[0]);
			base.Initialize();
		}

		// Token: 0x06000A61 RID: 2657 RVA: 0x000309C8 File Offset: 0x0002EBC8
		protected override Type[] GetValues()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetValues", Array.Empty<object>());
			}
			Type[] luaInspectorTypes = EndlessTypeMapping.Instance.LuaInspectorTypes;
			List<Type> list = new List<Type>(luaInspectorTypes.Length);
			for (int i = 0; i < luaInspectorTypes.Length; i++)
			{
				if (!this.typesToHide.Contains(luaInspectorTypes[i]))
				{
					list.Add(luaInspectorTypes[i]);
				}
			}
			return list.ToArray();
		}

		// Token: 0x040008A0 RID: 2208
		private HashSet<Type> typesToHide = new HashSet<Type> { typeof(TradeInfo.InventoryAndQuantityReference) };
	}
}
