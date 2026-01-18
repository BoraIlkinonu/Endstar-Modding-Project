using Endless.Data;
using Endless.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endless.Gameplay;

public class DevLog : EndlessBehaviour, IStartSubscriber, IGameEndSubscriber
{
	[SerializeField]
	private Renderer[] renderersToManage;

	[SerializeField]
	private Collider[] collidersToManage;

	private bool shown;

	private PlayerInputActions playerInputActions;

	private void Awake()
	{
		playerInputActions = new PlayerInputActions();
	}

	private void OnEnable()
	{
		playerInputActions.Player.ToggleDevLog.performed += ToggleDevLog;
		playerInputActions.Player.ToggleDevLog.Enable();
	}

	private void OnDisable()
	{
		playerInputActions.Player.ToggleDevLog.performed -= ToggleDevLog;
		playerInputActions.Player.ToggleDevLog.Disable();
	}

	private void ToggleDevLog(InputAction.CallbackContext callbackContext)
	{
		if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
		{
			shown = !shown;
			if (shown)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}
	}

	public void EndlessStart()
	{
		shown = false;
		Hide();
	}

	public void EndlessGameEnd()
	{
		Show();
	}

	private void Show()
	{
		Renderer[] array = renderersToManage;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
		Collider[] array2 = collidersToManage;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].enabled = true;
		}
	}

	private void Hide()
	{
		Renderer[] array = renderersToManage;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		Collider[] array2 = collidersToManage;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].enabled = false;
		}
	}
}
