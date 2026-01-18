using Endless.Gameplay;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIResourceLibraryReferenceView : UIBasePropLibraryReferenceView<ResourceLibraryReference, UIResourceLibraryReferenceView.Styles>
{
	public enum Styles
	{
		Default
	}

	[field: Header("UIInventoryLibraryReferenceView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	protected override ReferenceFilter ReferenceFilter => ReferenceFilter.Resource;

	protected override string GetReferenceName(ResourceLibraryReference model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetReferenceName", "model", model), this);
		}
		return GetPropLibraryReferenceName(model);
	}
}
