using System.Collections.Generic;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

public class IconList : ScriptableObject
{
	[SerializeField]
	private List<IconDefinition> definitions = new List<IconDefinition>();

	[SerializeField]
	private Texture2D defaultIcon;

	private Dictionary<SerializableGuid, IconDefinition> iconMap;

	private Dictionary<SerializableGuid, IconDefinition> IconMap
	{
		get
		{
			if (iconMap == null)
			{
				iconMap = new Dictionary<SerializableGuid, IconDefinition>();
				foreach (IconDefinition definition in definitions)
				{
					iconMap.Add(definition.IconId, definition);
				}
			}
			return iconMap;
		}
	}

	public IReadOnlyList<IconDefinition> Definitions => definitions;

	public Texture2D this[SerializableGuid guid]
	{
		get
		{
			if (IconMap.TryGetValue(guid, out var value))
			{
				return value.IconTexture;
			}
			return defaultIcon;
		}
	}

	public static bool IconDefinitionIsMissingData(IconDefinition characterCosmeticsDefinition)
	{
		if (!characterCosmeticsDefinition.IconId.IsEmpty)
		{
			return characterCosmeticsDefinition.IconTexture == null;
		}
		return true;
	}
}
