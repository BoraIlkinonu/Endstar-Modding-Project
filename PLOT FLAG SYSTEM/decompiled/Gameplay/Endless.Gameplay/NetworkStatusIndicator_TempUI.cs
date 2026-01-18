using Endless.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay;

public class NetworkStatusIndicator_TempUI : MonoBehaviour
{
	[SerializeField]
	private Color goodColor;

	[SerializeField]
	private Color warningColor;

	[SerializeField]
	private Color badColor;

	[SerializeField]
	private Image image;

	[SerializeField]
	private bool RegisterOnAwake;

	private void OnEnabled()
	{
		image.color = goodColor;
	}

	private void Start()
	{
		if (RegisterOnAwake)
		{
			MonoBehaviourSingleton<NetworkStatusIndicator>.Instance.StatusChangedListener.AddListener(HandleNetworkStatusChanged);
		}
	}

	public void HandleNetworkStatusChanged(NetworkStatusIndicator.NetworkStatus oldStatus, NetworkStatusIndicator.NetworkStatus newNetworkStatus)
	{
		switch (newNetworkStatus)
		{
		case NetworkStatusIndicator.NetworkStatus.Good:
			image.color = goodColor;
			break;
		case NetworkStatusIndicator.NetworkStatus.Ok:
			image.color = warningColor;
			break;
		default:
			image.color = badColor;
			break;
		}
	}
}
