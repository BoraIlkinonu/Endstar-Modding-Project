using Endless.Gameplay;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIPropLibraryReferencePresenter : UIBasePropLibraryReferencePresenter<PropLibraryReference>
{
	protected override string IEnumerableWindowTitle => "Select a Prop";

	protected override SelectionType SelectionType => SelectionType.Select0To1;

	protected override PropLibraryReference CreateDefaultModel()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "CreateDefaultModel");
		}
		return ReferenceFactory.CreatePropLibraryReference(SerializableGuid.Empty);
	}
}
