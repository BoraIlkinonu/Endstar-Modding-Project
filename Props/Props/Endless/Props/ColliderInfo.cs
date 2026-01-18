using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Endless.Props
{
	// Token: 0x02000004 RID: 4
	public class ColliderInfo : MonoBehaviour
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000005 RID: 5 RVA: 0x000021A7 File Offset: 0x000003A7
		// (set) Token: 0x06000006 RID: 6 RVA: 0x000021AF File Offset: 0x000003AF
		public ColliderInfo.ColliderType Type
		{
			get
			{
				return this.colliderType;
			}
			set
			{
				this.colliderType = value;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000007 RID: 7 RVA: 0x000021B8 File Offset: 0x000003B8
		public Collider[] CachedColliders
		{
			get
			{
				Collider[] array;
				if ((array = this.cachedColliders) == null)
				{
					array = (this.cachedColliders = this.GetColliderOnObjectAndChildren(base.gameObject).ToArray());
				}
				return array;
			}
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000021EC File Offset: 0x000003EC
		private List<Collider> GetColliderOnObjectAndChildren(GameObject targetObject)
		{
			List<Collider> list = targetObject.GetComponents<Collider>().ToList<Collider>();
			for (int i = 0; i < targetObject.transform.childCount; i++)
			{
				list.AddRange(this.GetColliderOnObjectAndChildren(targetObject.transform.GetChild(i).gameObject));
			}
			return list;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x0000223C File Offset: 0x0000043C
		private void Awake()
		{
			ColliderInfo.ColliderTypeInfoAttribute infoAttribute = ColliderInfo.GetInfoAttribute(this.colliderType);
			this.SetLayerOnObjectAndChildren(base.gameObject, infoAttribute.Layer);
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002268 File Offset: 0x00000468
		private void SetLayerOnObjectAndChildren(GameObject targetGameObject, int layer)
		{
			targetGameObject.layer = layer;
			for (int i = 0; i < targetGameObject.transform.childCount; i++)
			{
				this.SetLayerOnObjectAndChildren(targetGameObject.transform.GetChild(i).gameObject, layer);
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000022AC File Offset: 0x000004AC
		public void SetCollidersEnabled(bool enabled)
		{
			Collider[] array = this.CachedColliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = enabled;
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000022D7 File Offset: 0x000004D7
		public static ColliderInfo.ColliderTypeInfoAttribute GetInfoAttribute(ColliderInfo.ColliderType colliderType)
		{
			return typeof(ColliderInfo.ColliderType).GetField(colliderType.ToString()).GetCustomAttributes(false).OfType<ColliderInfo.ColliderTypeInfoAttribute>()
				.SingleOrDefault<ColliderInfo.ColliderTypeInfoAttribute>();
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002308 File Offset: 0x00000508
		public float GetDistanceFromPoint(Vector3 position)
		{
			float num = float.MaxValue;
			foreach (Collider collider in this.CachedColliders)
			{
				float num2 = Vector3.Distance(position, collider.ClosestPoint(position));
				if (num2 < num)
				{
					num = num2;
				}
			}
			return num;
		}

		// Token: 0x04000002 RID: 2
		[SerializeField]
		private ColliderInfo.ColliderType colliderType;

		// Token: 0x04000003 RID: 3
		private Collider[] cachedColliders;

		// Token: 0x02000045 RID: 69
		[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
		public sealed class ColliderTypeInfoAttribute : Attribute
		{
			// Token: 0x17000076 RID: 118
			// (get) Token: 0x06000130 RID: 304 RVA: 0x00004151 File Offset: 0x00002351
			// (set) Token: 0x06000131 RID: 305 RVA: 0x00004159 File Offset: 0x00002359
			public string Description { get; private set; }

			// Token: 0x17000077 RID: 119
			// (get) Token: 0x06000132 RID: 306 RVA: 0x00004162 File Offset: 0x00002362
			// (set) Token: 0x06000133 RID: 307 RVA: 0x0000416A File Offset: 0x0000236A
			public int Layer { get; private set; }

			// Token: 0x06000134 RID: 308 RVA: 0x00004173 File Offset: 0x00002373
			public ColliderTypeInfoAttribute(string description, string layer)
			{
				this.Description = description;
				this.Layer = LayerMask.NameToLayer(layer);
			}
		}

		// Token: 0x02000046 RID: 70
		public enum ColliderType
		{
			// Token: 0x040000DB RID: 219
			[ColliderInfo.ColliderTypeInfoAttribute("Normal everyday collisions. Can be hit by attacks", "Default")]
			Default,
			// Token: 0x040000DC RID: 220
			[ColliderInfo.ColliderTypeInfoAttribute("Characters can use this collider for interacting. This dos NOT cause physical collisions!", "PlayerInteractable")]
			Interactable,
			// Token: 0x040000DD RID: 221
			[ColliderInfo.ColliderTypeInfoAttribute("Used for hit boxes. Can be hit by attacks, but does not block movement.", "HittableColliders")]
			Hittable,
			// Token: 0x040000DE RID: 222
			[ColliderInfo.ColliderTypeInfoAttribute("Used for collision that only affects the player. Other objects will pass through (ie, projectiles) but stop the player.", "PlayerOnly")]
			PlayerOnly,
			// Token: 0x040000DF RID: 223
			[ColliderInfo.ColliderTypeInfoAttribute("Used for World Trigger overlaps.", "WorldTriggers")]
			WorldTrigger,
			// Token: 0x040000E0 RID: 224
			[ColliderInfo.ColliderTypeInfoAttribute("Normal everyday collisions, but does not affect navigation. Used for dynamic objects.", "DefaultNoBake")]
			DefaultNoNavBake
		}
	}
}
