using System;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002F2 RID: 754
	public abstract class AbstractBlock : EndlessBehaviour, IBaseType, IComponentBase
	{
		// Token: 0x060010EA RID: 4330 RVA: 0x000553B5 File Offset: 0x000535B5
		public virtual void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.iconId = endlessProp.Prop.GetPropMetaData("AbstractIcon");
		}

		// Token: 0x060010EB RID: 4331 RVA: 0x000553D2 File Offset: 0x000535D2
		protected virtual void Awake()
		{
			if (!string.IsNullOrEmpty(this.iconId))
			{
				this.iconRenderer.material.SetTexture("_Icon_Texture", MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultIconList[this.iconId]);
			}
		}

		// Token: 0x17000355 RID: 853
		// (get) Token: 0x060010EC RID: 4332 RVA: 0x00055410 File Offset: 0x00053610
		public Context Context
		{
			get
			{
				Context context;
				if ((context = this.context) == null)
				{
					context = (this.context = new Context(this.WorldObject));
				}
				return context;
			}
		}

		// Token: 0x17000356 RID: 854
		// (get) Token: 0x060010ED RID: 4333 RVA: 0x0005543B File Offset: 0x0005363B
		// (set) Token: 0x060010EE RID: 4334 RVA: 0x00055443 File Offset: 0x00053643
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000357 RID: 855
		// (get) Token: 0x060010EF RID: 4335 RVA: 0x0001BD04 File Offset: 0x00019F04
		public NavType NavValue
		{
			get
			{
				return NavType.Intangible;
			}
		}

		// Token: 0x060010F0 RID: 4336 RVA: 0x0005544C File Offset: 0x0005364C
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x04000EA0 RID: 3744
		private const string ICON_PROPERTY_NAME = "_Icon_Texture";

		// Token: 0x04000EA1 RID: 3745
		public const string ICON_PROP_META_DATA_NAME = "AbstractIcon";

		// Token: 0x04000EA2 RID: 3746
		[SerializeField]
		private SerializableGuid iconId;

		// Token: 0x04000EA3 RID: 3747
		[SerializeField]
		private Renderer iconRenderer;

		// Token: 0x04000EA4 RID: 3748
		private Context context;
	}
}
