using Endless.Gameplay;
using Endless.Gameplay.Stats;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public abstract class UIStatBasePresenter<TModel> : UIBasePresenter<TModel> where TModel : StatBase
{
	private UIStatBaseView<TModel> statBaseView;

	protected override void Start()
	{
		base.Start();
		statBaseView = base.View.Interface as UIStatBaseView<TModel>;
		statBaseView.IdentifierChanged += SetIdentifier;
		statBaseView.MessageChanged += SetMessage;
		statBaseView.OrderChanged += SetOrder;
		statBaseView.InventoryIconChanged += SetInventoryIcon;
	}

	protected virtual void OnDestroy()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("OnDestroy", this);
		}
		statBaseView.IdentifierChanged -= SetIdentifier;
		statBaseView.MessageChanged -= SetMessage;
		statBaseView.OrderChanged -= SetOrder;
		statBaseView.InventoryIconChanged -= SetInventoryIcon;
	}

	private void SetIdentifier(string identifier)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("SetIdentifier ( identifier: " + identifier + " )", this);
		}
		base.Model.Identifier = identifier;
		InvokeOnModelChanged();
	}

	private void SetMessage(LocalizedString message)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetMessage", "message", message), this);
		}
		base.Model.Message = message;
		InvokeOnModelChanged();
	}

	private void SetOrder(int order)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetOrder", "order", order), this);
		}
		base.Model.Order = order;
		InvokeOnModelChanged();
	}

	private void SetInventoryIcon(InventoryLibraryReference inventoryIcon)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetInventoryIcon", "inventoryIcon", inventoryIcon), this);
		}
		base.Model.InventoryIcon = inventoryIcon;
		InvokeOnModelChanged();
	}
}
