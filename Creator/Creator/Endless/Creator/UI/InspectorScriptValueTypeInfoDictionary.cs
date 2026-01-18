using System;
using Endless.Gameplay.Serialization;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000E0 RID: 224
	[CreateAssetMenu(menuName = "ScriptableObject/UI/Creator/Dictionaries/Inspector Script Value Type Info Dictionary", fileName = "Inspector Script Value Type Info Dictionary")]
	public class InspectorScriptValueTypeInfoDictionary : BaseScriptableObjectDictionary<string, DisplayNameAndDescription>
	{
		// Token: 0x060003BF RID: 959 RVA: 0x000181B4 File Offset: 0x000163B4
		[ContextMenu("Validate")]
		public override void Validate()
		{
			base.Validate();
			foreach (Type type in EndlessTypeMapping.Instance.LuaInspectorTypes)
			{
				if (!base.Contains(type.Name))
				{
					DebugUtility.LogError(type.Name + " is missing!", this);
				}
			}
		}
	}
}
