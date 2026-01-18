using System;
using Endless.Gameplay.Serialization;
using Endless.Props.Scripting;
using Endless.Shared.Debugging;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIStoredParameterPackageView : UIBasePropertyView<UIStoredParameterPackage, UIStoredParameterPackageView.Styles>
{
	public enum Styles
	{
		Default
	}

	[field: Header("UIStoredParameterPackageView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public override void View(UIStoredParameterPackage model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(model.StoredParameter.DataType);
		object model2 = JsonConvert.DeserializeObject(model.StoredParameter.JsonData, typeFromId);
		ViewProperty(model.Name, model2, model.StoredParameter.DataType, "", Array.Empty<ClampValue>());
	}

	protected override object ExtractModel(object model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "ExtractModel", (model == null) ? "null" : model.GetType().Name, model);
		}
		if (!(model is UIStoredParameterPackage uIStoredParameterPackage))
		{
			return model;
		}
		Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(uIStoredParameterPackage.StoredParameter.DataType);
		return JsonConvert.DeserializeObject(uIStoredParameterPackage.StoredParameter.JsonData, typeFromId);
	}
}
