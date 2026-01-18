using System;
using Endless.Gameplay;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI;

public class UINpcInstanceReferenceNoneOrContextView : UIBaseInstanceReferenceView<NpcInstanceReference, UINpcInstanceReferenceView.Styles>
{
	[Header("UIInstanceReferenceNoneOrContextView")]
	[SerializeField]
	private UINoneOrContextRadio noneOrContextRadio;

	public override UINpcInstanceReferenceView.Styles Style { get; protected set; } = UINpcInstanceReferenceView.Styles.NoneOrContext;

	protected override ReferenceFilter ReferenceFilter => ReferenceFilter.Npc;

	public event Action<NpcInstanceReference> NoneOrContextChanged;

	protected override void Start()
	{
		base.Start();
		noneOrContextRadio.OnValueChanged.AddListener(InvokeNoneOrContextRadioChanged);
	}

	public override void View(NpcInstanceReference model)
	{
		base.View(model);
		noneOrContextRadio.SetValue(model.useContext ? NoneOrContext.Context : NoneOrContext.None, triggerOnValueChanged: false);
	}

	private void InvokeNoneOrContextRadioChanged(NoneOrContext noneOrContext)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeNoneOrContextRadioChanged", "noneOrContext", noneOrContext), this);
		}
		NpcInstanceReference obj = ReferenceFactory.CreateNpcInstanceReference(SerializableGuid.Empty, noneOrContext == NoneOrContext.Context);
		this.NoneOrContextChanged?.Invoke(obj);
	}
}
