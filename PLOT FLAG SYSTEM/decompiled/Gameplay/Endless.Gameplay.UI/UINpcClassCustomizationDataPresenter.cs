using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Gameplay.UI;

public abstract class UINpcClassCustomizationDataPresenter<TModel> : UIBasePresenter<TModel>, IUINpcClassCustomizationDataPresentable, IUIPresentable, IPoolableT, IClearable where TModel : NpcClassCustomizationData
{
	public abstract NpcClass NpcClass { get; }

	public event Action<NpcClass> OnNpcClassChanged;

	protected override void Start()
	{
		base.Start();
		(base.View.Interface as IUINpcClassCustomizationDataViewable).OnNpcClassChanged += SetNpcClass;
	}

	private void SetNpcClass(NpcClass npcClass)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} ) | {3}: {4}", "SetNpcClass", "npcClass", npcClass, "NpcClass", NpcClass), this);
		}
		if (NpcClass != npcClass)
		{
			this.OnNpcClassChanged?.Invoke(npcClass);
		}
	}
}
