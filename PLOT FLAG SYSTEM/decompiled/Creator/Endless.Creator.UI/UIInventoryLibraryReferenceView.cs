using Endless.Gameplay;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInventoryLibraryReferenceView : UIBaseInventoryLibraryReferenceView<InventoryLibraryReference, UIInventoryLibraryReferenceView.Styles>
{
	public enum Styles
	{
		Default
	}

	[field: Header("UIInventoryLibraryReferenceView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	protected override ReferenceFilter ReferenceFilter => ReferenceFilter.InventoryItem;

	protected override string GetReferenceName(InventoryLibraryReference model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetReferenceName", model);
		}
		return GetPropLibraryReferenceName(model);
	}
}
