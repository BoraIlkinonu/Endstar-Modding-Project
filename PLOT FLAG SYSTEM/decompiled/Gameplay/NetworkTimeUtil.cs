using Endless.Gameplay;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine.Events;

public class NetworkTimeUtil : MonoBehaviourSingleton<NetworkTimeUtil>
{
	public UnityAction<double> ServerTime;

	public UnityAction<double> LocalTime;

	public UnityAction<uint> RollbackFrame;

	public UnityAction<uint> ExtrapolationFrame;

	private bool registered;

	private void Update()
	{
		if (!registered && NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkTickSystem != null)
		{
			NetworkManager.Singleton.NetworkTickSystem.Tick += NetworkUpdate;
			registered = true;
		}
	}

	private void OnDisable()
	{
		if (registered && NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkTickSystem != null)
		{
			NetworkManager.Singleton.NetworkTickSystem.Tick -= NetworkUpdate;
		}
		registered = false;
	}

	private void NetworkUpdate()
	{
		ServerTime?.Invoke(NetworkManager.Singleton.ServerTime.FixedTime);
		LocalTime?.Invoke(NetworkManager.Singleton.LocalTime.FixedTime);
		RollbackFrame?.Invoke(NetClock.GetFrameFromTime(NetworkManager.Singleton.ServerTime.FixedTime) - 2);
		ExtrapolationFrame?.Invoke(NetClock.GetFrameFromTime(NetworkManager.Singleton.ServerTime.FixedTime) - 2);
	}
}
