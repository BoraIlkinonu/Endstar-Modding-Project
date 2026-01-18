using Endless.Gameplay.LevelEditing;
using Endless.Shared.UI;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIRuntimePropInfoListView : UIBaseListView<PropLibrary.RuntimePropInfo>
{
	public UnityEvent<PropLibrary.RuntimePropInfo> OnCellSelected { get; private set; } = new UnityEvent<PropLibrary.RuntimePropInfo>();
}
