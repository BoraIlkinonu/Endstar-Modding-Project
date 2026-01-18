using System;
using System.Collections;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200032B RID: 811
	public class MonoBehaviorProxy : MonoBehaviour
	{
		// Token: 0x14000024 RID: 36
		// (add) Token: 0x060012D5 RID: 4821 RVA: 0x0005C6F8 File Offset: 0x0005A8F8
		// (remove) Token: 0x060012D6 RID: 4822 RVA: 0x0005C730 File Offset: 0x0005A930
		public event Action OnUpdate;

		// Token: 0x060012D7 RID: 4823 RVA: 0x0005C765 File Offset: 0x0005A965
		public Coroutine StartMonoBehaviorRoutine(IEnumerator routine)
		{
			return base.StartCoroutine(routine);
		}

		// Token: 0x060012D8 RID: 4824 RVA: 0x0005C76E File Offset: 0x0005A96E
		public void StopMonoBehaviorRoutine(Coroutine coroutine)
		{
			base.StopCoroutine(coroutine);
		}

		// Token: 0x060012D9 RID: 4825 RVA: 0x0005C777 File Offset: 0x0005A977
		private void Update()
		{
			Action onUpdate = this.OnUpdate;
			if (onUpdate == null)
			{
				return;
			}
			onUpdate();
		}
	}
}
