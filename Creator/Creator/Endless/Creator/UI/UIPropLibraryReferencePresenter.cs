using System;
using Endless.Gameplay;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000238 RID: 568
	public class UIPropLibraryReferencePresenter : UIBasePropLibraryReferencePresenter<PropLibraryReference>
	{
		// Token: 0x17000128 RID: 296
		// (get) Token: 0x06000930 RID: 2352 RVA: 0x0002B608 File Offset: 0x00029808
		protected override string IEnumerableWindowTitle
		{
			get
			{
				return "Select a Prop";
			}
		}

		// Token: 0x17000129 RID: 297
		// (get) Token: 0x06000931 RID: 2353 RVA: 0x0001BF89 File Offset: 0x0001A189
		protected override SelectionType SelectionType
		{
			get
			{
				return SelectionType.Select0To1;
			}
		}

		// Token: 0x06000932 RID: 2354 RVA: 0x0002B60F File Offset: 0x0002980F
		protected override PropLibraryReference CreateDefaultModel()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateDefaultModel", Array.Empty<object>());
			}
			return ReferenceFactory.CreatePropLibraryReference(SerializableGuid.Empty);
		}
	}
}
