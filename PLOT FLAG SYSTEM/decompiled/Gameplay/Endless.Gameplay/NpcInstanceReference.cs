using System;
using Endless.Gameplay.Scripting;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

[Serializable]
public class NpcInstanceReference : InstanceReference
{
	internal override ReferenceFilter Filter => ReferenceFilter.Npc;

	internal NpcEntity GetNpcEntity(Context context)
	{
		if (useContext)
		{
			if (!context.IsNpc())
			{
				return null;
			}
			return context.WorldObject.GetUserComponent<NpcEntity>();
		}
		GameObject instanceObject = GetInstanceObject();
		if (!instanceObject)
		{
			return null;
		}
		return instanceObject.GetComponentInChildren<NpcEntity>();
	}

	public Context GetNpc()
	{
		if (useContext)
		{
			if (!Context.StaticLastContext.IsNpc())
			{
				return null;
			}
			return Context.StaticLastContext;
		}
		GameObject instanceObject = GetInstanceObject();
		if (!instanceObject)
		{
			return null;
		}
		return instanceObject.GetComponent<WorldObject>().Context;
	}

	internal NpcInstanceReference()
	{
	}

	internal NpcInstanceReference(SerializableGuid instanceId, bool useContext)
		: base(instanceId, useContext)
	{
	}
}
