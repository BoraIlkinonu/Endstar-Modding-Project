using Endless.Shared;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class NetworkStatusIndicator : MonoBehaviourSingleton<NetworkStatusIndicator>
{
	public enum NetworkStatus
	{
		Offline,
		Bad,
		Ok,
		Good
	}

	private const float CLEAR_UP_DELAY = 1.25f;

	private const float WARNING_PONG_THRESHOLD = 0.16f;

	private const float BAD_PONG_THRESHOLD = 0.2f;

	private NetworkStatus _currentNetworkStatus;

	public UnityEvent<NetworkStatus, NetworkStatus> StatusChangedListener = new UnityEvent<NetworkStatus, NetworkStatus>();

	private int timeDesyncCount;

	private int missedInputCount;

	private float serverToClientTime;

	public NetworkStatus CurrentNetworkStatus
	{
		get
		{
			return _currentNetworkStatus;
		}
		protected set
		{
			if (_currentNetworkStatus != value)
			{
				NetworkStatus currentNetworkStatus = _currentNetworkStatus;
				_currentNetworkStatus = value;
				StatusChangedListener.Invoke(currentNetworkStatus, _currentNetworkStatus);
			}
		}
	}

	private void OnEnabled()
	{
		CurrentNetworkStatus = NetworkStatus.Good;
	}

	private void OnDisabled()
	{
		CurrentNetworkStatus = NetworkStatus.Offline;
	}

	public void ServerTimeDesync()
	{
		timeDesyncCount++;
		Invoke("ClearTimeDesync", 1.25f);
	}

	public void MissedInput()
	{
		missedInputCount++;
		Invoke("ClearMissedInput", 1.25f);
	}

	public void UpdateServerToClientTime(float value)
	{
		serverToClientTime = value;
	}

	private void ClearTimeDesync()
	{
		timeDesyncCount--;
	}

	private void ClearMissedInput()
	{
		missedInputCount--;
	}

	private void Update()
	{
		if (timeDesyncCount > 1 || missedInputCount > 0 || serverToClientTime > 0.2f)
		{
			CurrentNetworkStatus = NetworkStatus.Bad;
		}
		else if (timeDesyncCount > 0 || serverToClientTime > 0.16f)
		{
			CurrentNetworkStatus = NetworkStatus.Ok;
		}
		else
		{
			CurrentNetworkStatus = NetworkStatus.Good;
		}
	}
}
