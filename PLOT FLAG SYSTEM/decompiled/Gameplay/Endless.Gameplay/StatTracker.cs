using System.Collections.Generic;
using System.Linq;
using Endless.Shared;

namespace Endless.Gameplay;

public class StatTracker : MonoBehaviourSingleton<StatTracker>
{
	private Dictionary<int, int> playerDowns = new Dictionary<int, int>();

	private Dictionary<int, int> playerRevives = new Dictionary<int, int>();

	private void Start()
	{
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(HandleGameplayCleanup);
	}

	private void HandleGameplayCleanup()
	{
		playerDowns = new Dictionary<int, int>();
		playerRevives = new Dictionary<int, int>();
	}

	public void TrackPlayerDown(int userId)
	{
		if (!playerDowns.TryAdd(userId, 1))
		{
			playerDowns[userId]++;
		}
	}

	public int GetGlobalDowns()
	{
		return playerDowns.Values.Sum();
	}

	public Dictionary<int, float> GetDownsAsFloat()
	{
		Dictionary<int, float> dictionary = new Dictionary<int, float>();
		foreach (KeyValuePair<int, int> playerDown in playerDowns)
		{
			dictionary.Add(playerDown.Key, playerDown.Value);
		}
		return dictionary;
	}

	public void TrackRevive(int userId)
	{
		if (!playerDowns.TryAdd(userId, 1))
		{
			playerDowns[userId]++;
		}
	}

	public int GetGlobalRevives()
	{
		return playerRevives.Values.Sum();
	}

	public Dictionary<int, float> GetRevivesAsFloat()
	{
		Dictionary<int, float> dictionary = new Dictionary<int, float>();
		foreach (KeyValuePair<int, int> playerRevife in playerRevives)
		{
			dictionary.Add(playerRevife.Key, playerRevife.Value);
		}
		return dictionary;
	}
}
