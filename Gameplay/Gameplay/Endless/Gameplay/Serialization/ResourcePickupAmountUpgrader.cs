using System;
using Endless.Props.Scripting;
using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization
{
	// Token: 0x020004CA RID: 1226
	public class ResourcePickupAmountUpgrader : EndlessTypeUpgrader
	{
		// Token: 0x06001E70 RID: 7792 RVA: 0x00084A84 File Offset: 0x00082C84
		private void GetUpgradedData(string memberName, int memberDataType, string jsonData, bool isLua, out string newMemberName, out int newDataType, out string newJsonData)
		{
			newDataType = EndlessTypeMapping.Instance.GetTypeId(typeof(int).AssemblyQualifiedName);
			ResourcePickup.ResourceQuantity resourceQuantity = JsonConvert.DeserializeObject<ResourcePickup.ResourceQuantity>(jsonData);
			int num;
			if (resourceQuantity != ResourcePickup.ResourceQuantity.One)
			{
				if (resourceQuantity != ResourcePickup.ResourceQuantity.Five)
				{
					if (resourceQuantity != ResourcePickup.ResourceQuantity.Ten)
					{
						throw new ArgumentOutOfRangeException();
					}
					num = 10;
				}
				else
				{
					num = 5;
				}
			}
			else
			{
				num = 1;
			}
			newJsonData = JsonConvert.SerializeObject(num);
			if (!isLua)
			{
				newMemberName = "Quantity";
				return;
			}
			newMemberName = memberName;
		}

		// Token: 0x06001E71 RID: 7793 RVA: 0x00084AF8 File Offset: 0x00082CF8
		public override void Upgrade(InspectorScriptValue inspectorScriptValue)
		{
			string text;
			int num;
			string text2;
			this.GetUpgradedData(inspectorScriptValue.Name, inspectorScriptValue.DataType, inspectorScriptValue.DefaultValue, true, out text, out num, out text2);
			inspectorScriptValue.DataType = num;
			inspectorScriptValue.DefaultValue = text2;
		}

		// Token: 0x06001E72 RID: 7794 RVA: 0x00084B34 File Offset: 0x00082D34
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
	}
}
