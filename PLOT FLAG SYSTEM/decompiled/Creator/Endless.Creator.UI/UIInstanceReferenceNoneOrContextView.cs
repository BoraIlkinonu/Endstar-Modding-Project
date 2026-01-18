using System;
using Endless.Gameplay;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInstanceReferenceNoneOrContextView : UIBaseInstanceReferenceView<InstanceReference, UIInstanceReferenceView.Styles>
{
	[Header("UIInstanceReferenceNoneOrContextView")]
	[SerializeField]
	private UINoneOrContextRadio noneOrContextRadio;

	public override UIInstanceReferenceView.Styles Style { get; protected set; } = UIInstanceReferenceView.Styles.NoneOrContext;

	protected override ReferenceFilter ReferenceFilter => ReferenceFilter.NonStatic;

	public event Action<InstanceReference> NoneOrContextChanged;

	protected override void Start()
	{
		base.Start();
		noneOrContextRadio.OnValueChanged.AddListener(InvokeNoneOrContextRadioChanged);
	}

	public override void View(InstanceReference model)
	{
		base.View(model);
		Debug.Log(model.useContext, this);
		noneOrContextRadio.SetValue(model.useContext ? NoneOrContext.Context : NoneOrContext.None, triggerOnValueChanged: false);
	}

	private void InvokeNoneOrContextRadioChanged(NoneOrContext noneOrContext)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeNoneOrContextRadioChanged", "noneOrContext", noneOrContext), this);
		}
		InstanceReference obj = ReferenceFactory.CreateInstanceReference(SerializableGuid.Empty, noneOrContext == NoneOrContext.Context);
		this.NoneOrContextChanged?.Invoke(obj);
	}
}
