using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UISpawnPointListView : UIBaseListView<UISpawnPoint>
{
	[field: SerializeField]
	public bool CanSelect { get; private set; } = true;
}
