using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInspectorPropertyView : UIBasePropertyView<UIInspectorPropertyModel, UIInspectorPropertyView.Styles>
{
	public enum Styles
	{
		Default
	}

	[SerializeField]
	private UITooltip descriptionTooltip;

	[field: Header("UIInspectorPropertyView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public override void View(UIInspectorPropertyModel model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		ViewProperty(model.Name, model.GetModel(), model.MemberChange.DataType, model.Description, model.ClampValues);
	}

	protected override object ExtractModel(object model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "ExtractModel", (model == null) ? "null" : model.GetType().Name, model);
		}
		if (!(model is UIInspectorPropertyModel uIInspectorPropertyModel))
		{
			return model;
		}
		return uIInspectorPropertyModel.GetModel();
	}

	protected override void ViewProperty(string name, object model, int dataTypeId, string description, ClampValue[] clampValues)
	{
		base.ViewProperty(name, model, dataTypeId, description, clampValues);
		bool flag = !description.IsNullOrEmptyOrWhiteSpace();
		descriptionTooltip.enabled = flag;
		descriptionTooltip.SetTooltip(description);
	}
}
