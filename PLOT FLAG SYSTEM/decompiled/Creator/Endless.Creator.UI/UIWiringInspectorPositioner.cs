using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIWiringInspectorPositioner : UIGameObject
{
	[SerializeField]
	private RectTransform leftWiringInspectorViewContainer;

	[SerializeField]
	private RectTransform rightWiringInspectorViewContainer;

	public RectTransform LeftWiringInspectorViewContainer => leftWiringInspectorViewContainer;

	public RectTransform ReftWiringInspectorViewContainer => rightWiringInspectorViewContainer;
}
