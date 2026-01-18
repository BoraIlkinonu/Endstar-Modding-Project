using Endless.Creator.DynamicPropCreation;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIGenericPropCreationModalView : UIBasePropCreationModalView
{
	[Header("UIGenericPropCreationModalView")]
	[SerializeField]
	private RawImage iconDisplay;

	public GenericPropCreationScreenData GenericPropCreationScreenData { get; private set; }

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		GenericPropCreationScreenData = (GenericPropCreationScreenData)modalData[0];
		iconDisplay.texture = GenericPropCreationScreenData.PropIcon;
	}
}
