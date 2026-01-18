using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

public abstract class AbstractBlock : EndlessBehaviour, IBaseType, IComponentBase
{
	private const string ICON_PROPERTY_NAME = "_Icon_Texture";

	public const string ICON_PROP_META_DATA_NAME = "AbstractIcon";

	[SerializeField]
	private SerializableGuid iconId;

	[SerializeField]
	private Renderer iconRenderer;

	private Context context;

	public Context Context => context ?? (context = new Context(WorldObject));

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public NavType NavValue => NavType.Intangible;

	public virtual void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		iconId = endlessProp.Prop.GetPropMetaData("AbstractIcon");
	}

	protected virtual void Awake()
	{
		if (!string.IsNullOrEmpty(iconId))
		{
			iconRenderer.material.SetTexture("_Icon_Texture", MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultIconList[iconId]);
		}
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}
}
