using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIHeartHalfView : UIPoolableGameObject
{
	[Header("UIHeartHalfView")]
	[SerializeField]
	private Sprite leftSprite;

	[SerializeField]
	private Sprite rightSprite;

	[SerializeField]
	private Image image;

	[SerializeField]
	private TextMeshProUGUI surplusText;

	[SerializeField]
	private UIDisplayAndHideHandler displayAndHideHandler;

	public override void OnSpawn()
	{
		base.OnSpawn();
		surplusText.enabled = false;
		displayAndHideHandler.SetToDisplayStart(triggerUnityEvent: false);
	}

	public void View(int index, int displayIndex)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", index, displayIndex);
		}
		bool flag = index % 2 == 0;
		image.sprite = (flag ? leftSprite : rightSprite);
		float x = (flag ? 0f : (base.RectTransform.rect.width / -2f));
		surplusText.rectTransform.localPosition = new Vector3(x, 0f, 0f);
		displayAndHideHandler.SetDisplayDelay((float)displayIndex * 0.125f);
		displayAndHideHandler.Display();
	}

	public void ViewSurplus(int surplus)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewSurplus", surplus);
		}
		surplusText.text = $"+{surplus:N0}";
		surplusText.enabled = true;
	}

	public void DisableSurplus()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "DisableSurplus");
		}
		surplusText.enabled = false;
	}

	public void HideAndDespawnOnComplete()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HideAndDespawnOnComplete");
		}
		displayAndHideHandler.Hide(Despawn);
	}

	private void Despawn()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Despawn");
		}
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(this);
	}
}
