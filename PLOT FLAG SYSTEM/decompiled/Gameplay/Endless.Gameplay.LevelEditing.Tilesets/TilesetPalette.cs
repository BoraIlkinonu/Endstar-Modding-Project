using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

[CreateAssetMenu(menuName = "Level Editing/Tileset Palette")]
public class TilesetPalette : ScriptableObject
{
	[field: SerializeField]
	public Tileset[] Tilesets { get; private set; }
}
