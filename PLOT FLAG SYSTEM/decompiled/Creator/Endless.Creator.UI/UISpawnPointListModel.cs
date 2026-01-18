using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UISpawnPointListModel : UIBaseListModel<UISpawnPoint>, IUIRoleSubscribable
{
	public bool LocalClientCanSelect { get; private set; } = true;

	public List<SerializableGuid> SpawnPointIds => List.Select((UISpawnPoint entry) => entry.Id).ToList();

	public List<SerializableGuid> SelectedSpawnPointIds
	{
		get
		{
			List<SerializableGuid> list = new List<SerializableGuid>();
			for (int i = 0; i < Count; i++)
			{
				if (IsSelected(i))
				{
					list.Add(List[i].Id);
				}
			}
			return list;
		}
	}

	public void OnLocalClientRoleChanged(Roles localClientRole)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnLocalClientRoleChanged", localClientRole);
		}
		LocalClientCanSelect = localClientRole.IsGreaterThanOrEqualTo(Roles.Editor);
		TriggerModelChanged();
	}
}
