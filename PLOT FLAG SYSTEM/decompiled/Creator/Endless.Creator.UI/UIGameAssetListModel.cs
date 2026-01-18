using System;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIGameAssetListModel : UIBaseLocalFilterableListModel<UIGameAsset>
{
	protected override Comparison<UIGameAsset> DefaultSort => (UIGameAsset x, UIGameAsset y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal);
}
