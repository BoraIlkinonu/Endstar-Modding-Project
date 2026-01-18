using System.Collections.Generic;

namespace Endless.Gameplay.LevelEditing.Level;

public class BlockTokenCollection
{
	private readonly List<BlockToken> issuedTokens = new List<BlockToken>();

	public bool IsPoolEmpty => issuedTokens.Count == 0;

	public BlockToken RequestBlockToken()
	{
		BlockToken blockToken = new BlockToken(ReleaseBlockToken);
		issuedTokens.Add(blockToken);
		return blockToken;
	}

	private void ReleaseBlockToken(BlockToken blockToken)
	{
		issuedTokens.Remove(blockToken);
	}
}
