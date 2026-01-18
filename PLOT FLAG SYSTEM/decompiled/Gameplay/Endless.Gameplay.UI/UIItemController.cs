using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI;

public class UIItemController : UIGameObject
{
	[SerializeField]
	private UIItemView view;

	[SerializeField]
	private UIButton button;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public UnityEvent OnSelectUnityEvent { get; } = new UnityEvent();

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		button.PointerUpUnityEvent.AddListener(OnPointerUp);
	}

	private void OnPointerUp()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnPointerUp");
		}
		if (!view.IsEmpty && !view.HasDragInstance)
		{
			OnSelectUnityEvent.Invoke();
		}
	}
}
