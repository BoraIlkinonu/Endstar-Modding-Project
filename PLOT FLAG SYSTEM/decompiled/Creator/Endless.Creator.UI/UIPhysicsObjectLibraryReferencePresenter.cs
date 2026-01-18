using Endless.Gameplay;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIPhysicsObjectLibraryReferencePresenter : UIInspectorPropReferencePresenter<PhysicsObjectLibraryReference>
{
	protected override string IEnumerableWindowTitle => "Select a Physics Object";

	protected override SelectionType SelectionType => SelectionType.Select0To1;

	protected override PhysicsObjectLibraryReference CreateDefaultModel()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "CreateDefaultModel");
		}
		return ReferenceFactory.CreatePhysicsObjectLibraryReference(SerializableGuid.Empty);
	}
}
