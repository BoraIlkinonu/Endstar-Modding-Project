using System;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIEndlessParameterInfoView : UIBasePropertyView<EndlessParameterInfo, UIEndlessParameterInfoView.Styles>
{
	public enum Styles
	{
		Default
	}

	[field: Header("UIEndlessParameterInfoView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public override void View(EndlessParameterInfo model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		Type referencedType = model.GetReferencedType();
		object model2 = MonoBehaviourSingleton<UIDynamicTypeFactory>.Instance.Create(referencedType);
		ViewProperty(model.DisplayName, model2, model.DataType, "", Array.Empty<ClampValue>());
	}

	protected override object ExtractModel(object model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "ExtractModel", (model == null) ? "null" : model.GetType().Name, model);
		}
		if (!(model is EndlessParameterInfo info))
		{
			return model;
		}
		Type referencedType = info.GetReferencedType();
		return MonoBehaviourSingleton<UIDynamicTypeFactory>.Instance.Create(referencedType);
	}
}
