using System;
using Endless.Gameplay.LuaEnums;
using Endless.Props.Scripting;
using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization
{
	// Token: 0x020004C9 RID: 1225
	public class NpcClassUpgrader : EndlessTypeUpgrader
	{
		// Token: 0x06001E6C RID: 7788 RVA: 0x00084940 File Offset: 0x00082B40
		public override void Upgrade(InspectorScriptValue inspectorScriptValue)
		{
			string text;
			int num;
			string text2;
			this.GetUpgradedData(inspectorScriptValue.Name, inspectorScriptValue.DataType, inspectorScriptValue.DefaultValue, true, out text, out num, out text2);
			inspectorScriptValue.DataType = num;
			inspectorScriptValue.DefaultValue = text2;
		}

		// Token: 0x06001E6D RID: 7789 RVA: 0x0008497C File Offset: 0x00082B7C
		public override void Upgrade(MemberChange memberChange, bool isLua)
		{
			string text;
			int num;
			string text2;
			this.GetUpgradedData(memberChange.MemberName, memberChange.DataType, memberChange.JsonData, isLua, out text, out num, out text2);
			memberChange.MemberName = text;
			memberChange.DataType = num;
			memberChange.JsonData = text2;
		}

		// Token: 0x06001E6E RID: 7790 RVA: 0x000849C0 File Offset: 0x00082BC0
		private void GetUpgradedData(string memberName, int memberDataType, string jsonData, bool isLua, out string newMemberName, out int newDataType, out string newJsonData)
		{
			NpcClass npcClass = JsonConvert.DeserializeObject<NpcClass>(jsonData);
			if (!isLua)
			{
				newMemberName = "NpcClass";
			}
			else
			{
				newMemberName = memberName;
			}
			switch (npcClass)
			{
			case NpcClass.Blank:
				newDataType = EndlessTypeMapping.Instance.GetTypeId(typeof(BlankNpcCustomizationData).AssemblyQualifiedName);
				newJsonData = JsonConvert.SerializeObject(new BlankNpcCustomizationData());
				return;
			case NpcClass.Grunt:
				newDataType = EndlessTypeMapping.Instance.GetTypeId(typeof(GruntNpcCustomizationData).AssemblyQualifiedName);
				newJsonData = JsonConvert.SerializeObject(new GruntNpcCustomizationData());
				return;
			case NpcClass.Rifleman:
				newDataType = EndlessTypeMapping.Instance.GetTypeId(typeof(RiflemanNpcCustomizationData).AssemblyQualifiedName);
				newJsonData = JsonConvert.SerializeObject(new RiflemanNpcCustomizationData());
				return;
			}
			throw new ArgumentOutOfRangeException();
		}
	}
}
