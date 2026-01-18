using Endless.Assets;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public abstract class UIAssetView<T> : UIGameObject where T : Asset
{
	protected UIAssetModelHandler<T> ModelHandler { get; private set; }

	protected bool VerboseLogging { get; set; }

	protected void Start()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("Start", this);
		}
		ModelHandler.OnSet.AddListener(View);
		if (ModelHandler.Model != null)
		{
			View(ModelHandler.Model);
		}
	}

	private void OnDisable()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("OnDisable", this);
		}
		Clear();
	}

	public abstract void View(T model);

	public abstract void Clear();
}
