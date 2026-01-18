using Endless.Gameplay.Scripting;
using Endless.Gameplay.Stats;

namespace Endless.Gameplay.LuaInterfaces;

public class GameEndBlock
{
	private readonly Endless.Gameplay.GameEndBlock gameEndBlock;

	public GameEndBlock(Endless.Gameplay.GameEndBlock gameEndBlock)
	{
		this.gameEndBlock = gameEndBlock;
	}

	public void RecordBasicStat(Context instigator, BasicStat basicStat)
	{
		gameEndBlock.RecordBasicStat(instigator, basicStat);
	}

	public void RecordPerPlayerStat(Context instigator, PerPlayerStat perPlayerStat)
	{
		gameEndBlock.RecordPerPlayerStat(instigator, perPlayerStat);
	}

	public void RecordComparativeStat(Context instigator, ComparativeStat comparativeStat)
	{
		gameEndBlock.RecordComparativeStat(instigator, comparativeStat);
	}

	public void TriggerEndGame(Context instigator)
	{
		gameEndBlock.EndGame(instigator);
	}
}
