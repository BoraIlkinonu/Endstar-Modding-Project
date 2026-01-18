using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Creator.UI;
using Endless.Gameplay;
using Endless.Gameplay.Serialization;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x0200009E RID: 158
	public class UIIEnumerableTester : UIGameObject
	{
		// Token: 0x06000356 RID: 854 RVA: 0x00011AF8 File Offset: 0x0000FCF8
		private void Start()
		{
			Debug.Log("Click me", this);
		}

		// Token: 0x06000357 RID: 855 RVA: 0x00011B05 File Offset: 0x0000FD05
		public void Clear()
		{
			this.iEnumerablePresenter.Clear();
		}

		// Token: 0x06000358 RID: 856 RVA: 0x00011B14 File Offset: 0x0000FD14
		public void AddStringList()
		{
			List<object> list = ((this.iEnumerablePresenter.Model != null) ? new List<object>(this.iEnumerablePresenter.Model.Cast<object>()) : new List<object>());
			for (int i = 0; i < this.testItemCount; i++)
			{
				list.Add(list.Count);
			}
			this.iEnumerablePresenter.SetModel(list, true);
		}

		// Token: 0x06000359 RID: 857 RVA: 0x00011B7C File Offset: 0x0000FD7C
		public void AddNumbers()
		{
			List<object> list = ((this.iEnumerablePresenter.Model != null) ? new List<object>(this.iEnumerablePresenter.Model.Cast<object>()) : new List<object>());
			list.OfType<int>().DefaultIfEmpty(-1).Max();
			for (int i = 0; i < this.testItemCount; i++)
			{
				list.Add(list.Count);
			}
			this.iEnumerablePresenter.SetModel(list, true);
		}

		// Token: 0x0600035A RID: 858 RVA: 0x00011BF4 File Offset: 0x0000FDF4
		public void AddLuaInspectorTypes()
		{
			List<object> list = ((this.iEnumerablePresenter.Model != null) ? new List<object>(this.iEnumerablePresenter.Model.Cast<object>()) : new List<object>());
			foreach (Type type in EndlessTypeMapping.Instance.LuaInspectorTypes)
			{
				if (!type.Name.Contains("Reference") && !(type == typeof(LevelDestination)))
				{
					object obj = MonoBehaviourSingleton<UIDynamicTypeFactory>.Instance.Create(type);
					list.Add(obj);
					Type type2 = type.MakeArrayType();
					object obj2 = MonoBehaviourSingleton<UIDynamicTypeFactory>.Instance.Create(type2);
					list.Add(obj2);
				}
			}
			this.iEnumerablePresenter.SetModel(list, true);
		}

		// Token: 0x0600035B RID: 859 RVA: 0x00011CAC File Offset: 0x0000FEAC
		public void AddPrimitiveLists()
		{
			List<object> list = ((this.iEnumerablePresenter.Model != null) ? new List<object>(this.iEnumerablePresenter.Model.Cast<object>()) : new List<object>());
			List<int> list2 = new List<int>();
			for (int i = 0; i < this.testItemCount; i++)
			{
				list2.Add(i);
			}
			list.Add(list2);
			List<string> list3 = new List<string> { "One", "Two", "Three" };
			list.Add(list3);
			List<float> list4 = new List<float>();
			for (int j = 0; j < this.testItemCount; j++)
			{
				list4.Add((float)j);
			}
			list.Add(list4);
			this.iEnumerablePresenter.SetModel(list, true);
		}

		// Token: 0x0600035C RID: 860 RVA: 0x00011D74 File Offset: 0x0000FF74
		public void AddTypedLists()
		{
			List<object> list = ((this.iEnumerablePresenter.Model != null) ? new List<object>(this.iEnumerablePresenter.Model.Cast<object>()) : new List<object>());
			foreach (Type type in EndlessTypeMapping.Instance.LuaInspectorTypes)
			{
				if (!type.Name.Contains("Reference") && !(type == typeof(LevelDestination)))
				{
					IList list2 = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[] { type }));
					object obj = MonoBehaviourSingleton<UIDynamicTypeFactory>.Instance.Create(type);
					list2.Add(obj);
					list.Add(list2);
				}
			}
			this.iEnumerablePresenter.SetModel(list, true);
		}

		// Token: 0x04000273 RID: 627
		[SerializeField]
		private UIIEnumerablePresenter iEnumerablePresenter;

		// Token: 0x04000274 RID: 628
		[Min(0f)]
		[SerializeField]
		private int testItemCount = 1000;
	}
}
