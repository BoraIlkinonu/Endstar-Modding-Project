using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIPropEntryListModel : UIBaseLocalFilterableListModel<PropEntry>
{
	protected override Comparison<PropEntry> DefaultSort => (PropEntry x, PropEntry y) => string.Compare(x.Label, y.Label, StringComparison.Ordinal);
}
