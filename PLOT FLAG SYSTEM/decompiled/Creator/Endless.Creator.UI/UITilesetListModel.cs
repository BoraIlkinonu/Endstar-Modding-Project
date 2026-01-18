using System;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UITilesetListModel : UIBaseLocalFilterableListModel<Tileset>
{
	[field: SerializeField]
	public bool IsPaintTool { get; private set; }

	protected override Comparison<Tileset> DefaultSort => (Tileset x, Tileset y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal);
}
