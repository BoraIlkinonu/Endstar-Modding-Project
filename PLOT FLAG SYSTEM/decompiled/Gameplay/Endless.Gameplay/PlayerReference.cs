using System;
using Endless.Gameplay.Scripting;
using Newtonsoft.Json;

namespace Endless.Gameplay;

[Serializable]
public class PlayerReference
{
	[JsonProperty]
	internal bool useContext = true;

	[JsonProperty]
	internal int playerNumber;

	internal bool GetUseContext()
	{
		return useContext;
	}

	internal int GetPlayerNumber()
	{
		return playerNumber;
	}

	public Context GetPlayerContext()
	{
		if (useContext)
		{
			if (!Context.StaticLastContext.IsPlayer())
			{
				return null;
			}
			return Context.StaticLastContext;
		}
		return Game.Instance.GetPlayerBySlot(playerNumber);
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}, {2}: {3}", "useContext", useContext, "playerNumber", playerNumber);
	}
}
