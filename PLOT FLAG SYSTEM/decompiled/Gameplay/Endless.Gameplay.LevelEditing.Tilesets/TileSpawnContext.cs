namespace Endless.Gameplay.LevelEditing.Tilesets;

public class TileSpawnContext
{
	public Tileset Tileset { get; set; }

	public bool AllowTopDecoration { get; set; }

	public bool TopFilled { get; set; }

	public bool BottomFilled { get; set; }

	public bool FrontFilled { get; set; }

	public bool BackFilled { get; set; }

	public bool RightFilled { get; set; }

	public bool LeftFilled { get; set; }

	public Tile Tile { get; set; }
}
