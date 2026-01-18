using Endless.Gameplay;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPropLibraryReferenceView : UIBasePropLibraryReferenceView<PropLibraryReference, UIPropLibraryReferenceView.Styles>
{
	public enum Styles
	{
		Default
	}

	[field: Header("UIPropLibraryReferenceView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	protected override ReferenceFilter ReferenceFilter => ReferenceFilter.None;

	protected override string GetReferenceName(PropLibraryReference model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetReferenceName", "model", model), this);
		}
		return GetPropLibraryReferenceName(model);
	}
}
