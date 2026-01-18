using System;
using Endless.Gameplay;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000235 RID: 565
	public class UIPhysicsObjectLibraryReferencePresenter : UIInspectorPropReferencePresenter<PhysicsObjectLibraryReference>
	{
		// Token: 0x17000124 RID: 292
		// (get) Token: 0x06000927 RID: 2343 RVA: 0x0002B58D File Offset: 0x0002978D
		protected override string IEnumerableWindowTitle
		{
			get
			{
				return "Select a Physics Object";
			}
		}

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x06000928 RID: 2344 RVA: 0x0001BF89 File Offset: 0x0001A189
		protected override SelectionType SelectionType
		{
			get
			{
				return SelectionType.Select0To1;
			}
		}

		// Token: 0x06000929 RID: 2345 RVA: 0x0002B594 File Offset: 0x00029794
		protected override PhysicsObjectLibraryReference CreateDefaultModel()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateDefaultModel", Array.Empty<object>());
			}
			return ReferenceFactory.CreatePhysicsObjectLibraryReference(SerializableGuid.Empty);
		}
	}
}
