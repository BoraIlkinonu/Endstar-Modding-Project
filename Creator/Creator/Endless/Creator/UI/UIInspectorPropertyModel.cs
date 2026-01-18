using System;
using System.Linq;
using System.Reflection;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Serialization;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using HackAnythingAnywhere.Core;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000207 RID: 519
	public class UIInspectorPropertyModel
	{
		// Token: 0x0600081C RID: 2076 RVA: 0x00028024 File Offset: 0x00026224
		public UIInspectorPropertyModel(PropEntry propEntry, InspectorScriptValue inspectorScriptValue)
		{
			this.Name = inspectorScriptValue.Name;
			this.Description = inspectorScriptValue.Description;
			this.ClampValues = inspectorScriptValue.ClampValues;
			this.MemberChange = propEntry.LuaMemberChanges.FirstOrDefault((MemberChange item) => item.MemberName == inspectorScriptValue.Name) ?? new MemberChange(inspectorScriptValue.Name, inspectorScriptValue.DataType, inspectorScriptValue.DefaultValue);
			try
			{
				this.model = this.MemberChange.ToObject();
			}
			catch (Exception ex)
			{
				DebugUtility.Log(this.MemberChange.ToString(), null);
				DebugUtility.LogException(ex, null);
			}
			this.PropInstanceId = propEntry.InstanceId;
			this.ComponentTypeName = string.Empty;
			this.ShouldSerialize = true;
			this.MemberType = MemberTypes.Field;
		}

		// Token: 0x0600081D RID: 2077 RVA: 0x00028120 File Offset: 0x00026320
		public UIInspectorPropertyModel(PropEntry propEntry, InspectorExposedVariable inspectorExposedVariable, ComponentDefinition componentDefinition)
		{
			this.Name = inspectorExposedVariable.MemberName;
			this.Description = inspectorExposedVariable.Description;
			this.ClampValues = inspectorExposedVariable.ClampValues;
			string assemblyQualifiedTypeName = componentDefinition.ComponentBase.GetType().AssemblyQualifiedName;
			ComponentEntry componentEntry = propEntry.ComponentEntries.FirstOrDefault((ComponentEntry item) => item.AssemblyQualifiedName == assemblyQualifiedTypeName);
			int typeId = EndlessTypeMapping.Instance.GetTypeId(inspectorExposedVariable.DataType);
			if (componentEntry == null)
			{
				this.MemberChange = new MemberChange(inspectorExposedVariable.MemberName, typeId, inspectorExposedVariable.DefaultValue);
			}
			else
			{
				this.MemberChange = componentEntry.Changes.FirstOrDefault((MemberChange item) => item.MemberName == inspectorExposedVariable.MemberName) ?? new MemberChange(inspectorExposedVariable.MemberName, typeId, inspectorExposedVariable.DefaultValue);
			}
			this.model = this.MemberChange.ToObject();
			this.PropInstanceId = propEntry.InstanceId;
			this.ComponentTypeName = assemblyQualifiedTypeName;
			this.ShouldSerialize = true;
			MemberInfo[] member = Type.GetType(assemblyQualifiedTypeName).GetMember(inspectorExposedVariable.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (member.Length != 0)
			{
				this.ShouldSerialize = member[0].GetCustomAttribute<EndlessNonSerializedAttribute>() == null;
			}
			this.MemberType = inspectorExposedVariable.MemberType;
		}

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x0600081E RID: 2078 RVA: 0x00028298 File Offset: 0x00026498
		// (set) Token: 0x0600081F RID: 2079 RVA: 0x000282A0 File Offset: 0x000264A0
		public string Name { get; private set; }

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x06000820 RID: 2080 RVA: 0x000282A9 File Offset: 0x000264A9
		// (set) Token: 0x06000821 RID: 2081 RVA: 0x000282B1 File Offset: 0x000264B1
		public string Description { get; private set; }

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x06000822 RID: 2082 RVA: 0x000282BA File Offset: 0x000264BA
		// (set) Token: 0x06000823 RID: 2083 RVA: 0x000282C2 File Offset: 0x000264C2
		public ClampValue[] ClampValues { get; private set; }

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x06000824 RID: 2084 RVA: 0x000282CB File Offset: 0x000264CB
		// (set) Token: 0x06000825 RID: 2085 RVA: 0x000282D3 File Offset: 0x000264D3
		public MemberChange MemberChange { get; private set; }

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x06000826 RID: 2086 RVA: 0x000282DC File Offset: 0x000264DC
		// (set) Token: 0x06000827 RID: 2087 RVA: 0x000282E4 File Offset: 0x000264E4
		public SerializableGuid PropInstanceId { get; private set; }

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x06000828 RID: 2088 RVA: 0x000282ED File Offset: 0x000264ED
		// (set) Token: 0x06000829 RID: 2089 RVA: 0x000282F5 File Offset: 0x000264F5
		public string ComponentTypeName { get; private set; }

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x0600082A RID: 2090 RVA: 0x000282FE File Offset: 0x000264FE
		// (set) Token: 0x0600082B RID: 2091 RVA: 0x00028306 File Offset: 0x00026506
		public bool ShouldSerialize { get; private set; }

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x0600082C RID: 2092 RVA: 0x0002830F File Offset: 0x0002650F
		// (set) Token: 0x0600082D RID: 2093 RVA: 0x00028317 File Offset: 0x00026517
		public MemberTypes MemberType { get; private set; }

		// Token: 0x0600082E RID: 2094 RVA: 0x00028320 File Offset: 0x00026520
		public object GetModel()
		{
			if (this.ShouldSerialize)
			{
				return this.model;
			}
			GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(this.PropInstanceId);
			Type type = Type.GetType(this.ComponentTypeName);
			Component componentInChildren = gameObjectFromInstanceId.GetComponentInChildren(type);
			return this.RetrieveMemberInfo(type).GetValue(componentInChildren);
		}

		// Token: 0x0600082F RID: 2095 RVA: 0x00028374 File Offset: 0x00026574
		private MemberInfo RetrieveMemberInfo(Type getType)
		{
			MemberTypes memberType = this.MemberType;
			MemberInfo memberInfo;
			if (memberType != MemberTypes.Field)
			{
				if (memberType != MemberTypes.Property)
				{
					throw new ArgumentOutOfRangeException("MemberType", this.MemberType, "No support for model.MemberType!");
				}
				memberInfo = getType.GetProperty(this.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
			else
			{
				memberInfo = getType.GetField(this.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
			return memberInfo;
		}

		// Token: 0x06000830 RID: 2096 RVA: 0x000283D4 File Offset: 0x000265D4
		public override string ToString()
		{
			return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}, {8}: {9}, {10}: {11}, {12}: {13}, {14}: {15}, {16}: {17} }}", new object[]
			{
				"Name",
				this.Name,
				"Description",
				this.Description,
				"ClampValues",
				this.ClampValues.Length,
				"model",
				this.model,
				"PropInstanceId",
				this.PropInstanceId,
				"MemberChange",
				this.MemberChange,
				"ComponentTypeName",
				this.ComponentTypeName,
				"ShouldSerialize",
				this.ShouldSerialize,
				"MemberType",
				this.MemberType
			});
		}

		// Token: 0x06000831 RID: 2097 RVA: 0x000284AA File Offset: 0x000266AA
		public void SetModel(object newModel)
		{
			this.model = newModel;
		}

		// Token: 0x0400072B RID: 1835
		private object model;
	}
}
