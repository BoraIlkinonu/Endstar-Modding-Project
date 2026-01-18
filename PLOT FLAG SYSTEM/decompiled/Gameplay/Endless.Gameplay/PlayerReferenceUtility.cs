namespace Endless.Gameplay;

public static class PlayerReferenceUtility
{
	public static void SetUseContext(PlayerReference reference, bool useContext)
	{
		reference.useContext = useContext;
	}

	public static bool GetUseContext(PlayerReference reference)
	{
		return reference.GetUseContext();
	}

	public static void SetPlayerNumber(PlayerReference reference, int playerNumber)
	{
		reference.playerNumber = playerNumber;
	}

	public static int GetPlayerNumber(PlayerReference reference)
	{
		return reference.GetPlayerNumber();
	}
}
