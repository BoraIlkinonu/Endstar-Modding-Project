using System;
using System.Collections.Generic;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.Serialization;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInspectorPropertyPresenter : UIBasePresenter<UIInspectorPropertyModel>
{
	[Header("UIInspectorPropertyPresenter")]
	[SerializeField]
	private InterfaceReference<IUIBasePropertyViewable> propertyViewable;

	private InspectorTool inspectorTool;

	protected override void Start()
	{
		base.Start();
		inspectorTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<InspectorTool>();
		propertyViewable.Interface.OnUserChangedModel += SaveChanges;
	}

	private void SaveChanges(object model)
	{
		if (base.VerboseLogging || Application.isEditor)
		{
			DebugUtility.LogMethod(this, "SaveChanges", model);
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
		inspectorTool.HandleMemberChange(base.Model.PropInstanceId, base.Model.MemberChange, base.Model.ComponentTypeName);
	}
}
