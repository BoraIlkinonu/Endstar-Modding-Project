using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI.Test
{
	// Token: 0x0200029D RID: 669
	public class UITestListModelHandler : UIGameObject
	{
		// Token: 0x06001099 RID: 4249 RVA: 0x00046CA8 File Offset: 0x00044EA8
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (this.logModelChanged)
			{
				this.listModel.ModelChangedUnityEvent.AddListener(new UnityAction(this.LogModelChanged));
			}
			this.Synchronize();
		}

		// Token: 0x0600109A RID: 4250 RVA: 0x00046CF8 File Offset: 0x00044EF8
		public void Synchronize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Synchronize", Array.Empty<object>());
			}
			List<int> list = new List<int>();
			for (int i = this.count; i > 0; i--)
			{
				int num = list.Count + 1;
				list.Add(num);
			}
			if (this.shuffle)
			{
				list = this.Shuffle<int>(list);
			}
			this.listModel.Set(list, true);
		}

		// Token: 0x0600109B RID: 4251 RVA: 0x00046D61 File Offset: 0x00044F61
		private void LogModelChanged()
		{
			DebugUtility.Log("Model changed", this);
		}

		// Token: 0x0600109C RID: 4252 RVA: 0x00046D70 File Offset: 0x00044F70
		private List<T> Shuffle<T>(List<T> list)
		{
			int i = list.Count;
			while (i > 1)
			{
				i--;
				int num = this.rng.Next(i + 1);
				T t = list[num];
				list[num] = list[i];
				list[i] = t;
			}
			return list;
		}

		// Token: 0x04000A7B RID: 2683
		[SerializeField]
		private UIBaseListModel<int> listModel;

		// Token: 0x04000A7C RID: 2684
		[Min(0f)]
		[SerializeField]
		private int count = 25;

		// Token: 0x04000A7D RID: 2685
		[SerializeField]
		private bool logModelChanged;

		// Token: 0x04000A7E RID: 2686
		[SerializeField]
		private bool shuffle;

		// Token: 0x04000A7F RID: 2687
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000A80 RID: 2688
		private readonly global::System.Random rng = new global::System.Random();
	}
}
