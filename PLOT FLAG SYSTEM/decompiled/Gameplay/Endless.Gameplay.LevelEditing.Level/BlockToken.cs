using System;

namespace Endless.Gameplay.LevelEditing.Level;

public class BlockToken
{
	private readonly Action<BlockToken> releaseAction;

	public BlockToken(Action<BlockToken> releaseAction)
	{
		this.releaseAction = releaseAction;
	}

	public void Release()
	{
		releaseAction(this);
	}
}
