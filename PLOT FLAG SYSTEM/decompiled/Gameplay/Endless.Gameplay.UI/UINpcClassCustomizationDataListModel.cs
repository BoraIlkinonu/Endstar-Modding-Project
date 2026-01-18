using System;
using Endless.Shared.UI;

namespace Endless.Gameplay.UI;

public class UINpcClassCustomizationDataListModel : UIBaseLocalFilterableListModel<NpcClassCustomizationData>
{
	protected override Comparison<NpcClassCustomizationData> DefaultSort => (NpcClassCustomizationData x, NpcClassCustomizationData y) => string.Compare(x.ClassName, y.ClassName, StringComparison.Ordinal);
}
