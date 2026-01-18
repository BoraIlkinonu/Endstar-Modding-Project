using System;
using System.Collections.Generic;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.Serialization;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000200 RID: 512
	public class UIInspectorPropertyPresenter : UIBasePresenter<UIInspectorPropertyModel>
	{
		// Token: 0x06000808 RID: 2056 RVA: 0x00027CC5 File Offset: 0x00025EC5
		protected override void Start()
		{
			base.Start();
			this.inspectorTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<InspectorTool>();
			this.propertyViewable.Interface.OnUserChangedModel += this.SaveChanges;
		}

		// Token: 0x06000809 RID: 2057 RVA: 0x00027CFC File Offset: 0x00025EFC
		private void SaveChanges(object model)
		{
			if (base.VerboseLogging || Application.isEditor)
			{
				DebugUtility.LogMethod(this, "SaveChanges", new object[] { model });
			}
			base.Model.SetModel(model);
			base.Model.MemberChange.JsonData = JsonConvert.SerializeObject(model);
			Type type = model.GetType();
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
			{
				model = type.GetMethod("ToArray").Invoke(model, null);
			}
			else
			{
				base.Model.MemberChange.DataType = EndlessTypeMapping.Instance.GetTypeId(type.AssemblyQualifiedName);
			}
			this.inspectorTool.HandleMemberChange(base.Model.PropInstanceId, base.Model.MemberChange, base.Model.ComponentTypeName);
		}

		// Token: 0x04000720 RID: 1824
		[Header("UIInspectorPropertyPresenter")]
		[SerializeField]
		private InterfaceReference<IUIBasePropertyViewable> propertyViewable;

		// Token: 0x04000721 RID: 1825
		private InspectorTool inspectorTool;
	}
}
