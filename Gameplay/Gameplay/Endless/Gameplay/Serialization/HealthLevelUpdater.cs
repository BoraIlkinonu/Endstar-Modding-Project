using System;
using Endless.Gameplay.LuaEnums;
using Endless.Props.Scripting;
using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization
{
	// Token: 0x020004C8 RID: 1224
	public class HealthLevelUpdater : EndlessTypeUpgrader
	{
		// Token: 0x06001E68 RID: 7784 RVA: 0x00084844 File Offset: 0x00082A44
		public override void Upgrade(InspectorScriptValue inspectorScriptValue)
		{
			string text;
			int num;
			string text2;
			this.GetUpgradedData(inspectorScriptValue.Name, inspectorScriptValue.DataType, inspectorScriptValue.DefaultValue, true, out text, out num, out text2);
			inspectorScriptValue.DataType = num;
			inspectorScriptValue.DefaultValue = text2;
		}

		// Token: 0x06001E69 RID: 7785 RVA: 0x00084880 File Offset: 0x00082A80
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

		// Token: 0x06001E6A RID: 7786 RVA: 0x000848C4 File Offset: 0x00082AC4
		private void GetUpgradedData(string memberName, int memberDataType, string jsonData, bool isLua, out string newMemberName, out int newDataType, out string newJsonData)
		{
			newDataType = EndlessTypeMapping.Instance.GetTypeId(typeof(int).AssemblyQualifiedName);
			int num;
			switch (JsonConvert.DeserializeObject<HealthLevel>(jsonData))
			{
			case HealthLevel.Fragile:
				num = 5;
				break;
			case HealthLevel.Regular:
				num = 10;
				break;
			case HealthLevel.Tough:
				num = 15;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			newJsonData = JsonConvert.SerializeObject(num);
			if (isLua)
			{
				newMemberName = memberName;
				return;
			}
			newMemberName = "StartingHealth";
		}
	}
}
