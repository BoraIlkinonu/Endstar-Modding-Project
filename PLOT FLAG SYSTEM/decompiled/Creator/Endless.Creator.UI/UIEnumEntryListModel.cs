using System;
using Endless.Props.Scripting;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIEnumEntryListModel : UIBaseLocalFilterableListModel<EnumEntry>
{
	protected override Comparison<EnumEntry> DefaultSort => (EnumEntry x, EnumEntry y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal);
}
