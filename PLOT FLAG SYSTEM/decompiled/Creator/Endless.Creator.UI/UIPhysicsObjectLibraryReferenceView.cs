using Endless.Gameplay;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPhysicsObjectLibraryReferenceView : UIBasePropLibraryReferenceView<PhysicsObjectLibraryReference, UIPhysicsObjectLibraryReferenceView.Styles>
{
	public enum Styles
	{
		Default
	}

	[field: Header("PhysicsObjectLibraryReference")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	protected override ReferenceFilter ReferenceFilter => ReferenceFilter.PhysicsObject;

	protected override string GetReferenceName(PhysicsObjectLibraryReference model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetReferenceName", "model", model), this);
		}
		return GetPropLibraryReferenceName(model);
	}
}
