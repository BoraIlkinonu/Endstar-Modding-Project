using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public abstract class UIBaseSocialPresenter<T> : UIBasePresenter<T>
{
	[Header("UIBaseSocialPresenter")]
	[SerializeField]
	private UIUserPresenter userPresenter;

	protected abstract User UserModel { get; }

	public override void SetModel(T model, bool triggerOnModelChanged)
	{
		base.SetModel(model, triggerOnModelChanged);
		userPresenter.SetModel(UserModel, triggerOnModelChanged);
	}
}
