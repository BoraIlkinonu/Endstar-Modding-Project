using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UITilesetDetailController : UIGameObject, IBackable, IUIDetailControllable
{
	[SerializeField]
	private UITilesetDetailView view;

	[SerializeField]
	private UIDisplayAndHideHandler displayAndHideHandler;

	[SerializeField]
	private UIButton hideButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private PaintingTool paintingTool;

	public UnityEvent OnHide { get; } = new UnityEvent();

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		hideButton.onClick.AddListener(Hide);
		paintingTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PaintingTool>();
	}

	private void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
		{
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		}
	}

	public void OnBack()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		Hide();
	}

	private void Hide()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Hide");
		}
		displayAndHideHandler.Hide();
		if (!MobileUtility.IsMobile)
		{
			paintingTool.SetActiveTilesetIndex(PaintingTool.NoSelection);
		}
		OnHide.Invoke();
	}
}
