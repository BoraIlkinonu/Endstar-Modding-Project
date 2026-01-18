using System;
using System.Collections;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Shared
{
	// Token: 0x02000087 RID: 135
	public abstract class ScriptableObjectArray<T> : ScriptableObject, IEnumerable
	{
		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x060003DF RID: 991 RVA: 0x00011155 File Offset: 0x0000F355
		public int Length
		{
			get
			{
				return this.Value.Length;
			}
		}

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x060003E0 RID: 992 RVA: 0x0001115F File Offset: 0x0000F35F
		public T[] Value
		{
			get
			{
				if (!this.arraysCombined)
				{
					this.CombineArrays();
				}
				return this.combinedArray;
			}
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x00011175 File Offset: 0x0000F375
		public IEnumerator GetEnumerator()
		{
			return new ScriptableObjectArray<T>.ScriptableObjectArrayEnumerator(this);
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x00011180 File Offset: 0x0000F380
		private void CombineArrays()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CombineArrays", Array.Empty<object>());
			}
			if (this.arraysCombined)
			{
				return;
			}
			int num = this.array.Length;
			foreach (ScriptableObjectArray<T> scriptableObjectArray in this.arraysToInclude)
			{
				if (scriptableObjectArray == null)
				{
					DebugUtility.LogException(new NullReferenceException("There is a null reference in arraysToInclude"), this);
				}
				else
				{
					num += scriptableObjectArray.Length;
				}
			}
			this.combinedArray = new T[num];
			int num2 = 0;
			Array.Copy(this.array, 0, this.combinedArray, num2, this.array.Length);
			num2 += this.array.Length;
			foreach (ScriptableObjectArray<T> scriptableObjectArray2 in this.arraysToInclude)
			{
				if (scriptableObjectArray2 != null)
				{
					Array.Copy(scriptableObjectArray2.Value, 0, this.combinedArray, num2, scriptableObjectArray2.Length);
					num2 += scriptableObjectArray2.Length;
				}
			}
			this.arraysCombined = true;
		}

		// Token: 0x040001DC RID: 476
		[SerializeField]
		protected T[] array = Array.Empty<T>();

		// Token: 0x040001DD RID: 477
		[FormerlySerializedAs("arraysToAdd")]
		[SerializeField]
		protected ScriptableObjectArray<T>[] arraysToInclude = Array.Empty<ScriptableObjectArray<T>>();

		// Token: 0x040001DE RID: 478
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040001DF RID: 479
		[SerializeField]
		private bool validateThatEachEntryIsUnique = true;

		// Token: 0x040001E0 RID: 480
		[SerializeField]
		private bool validateThatEachEntryHasValue = true;

		// Token: 0x040001E1 RID: 481
		[NonSerialized]
		private bool arraysCombined;

		// Token: 0x040001E2 RID: 482
		[NonSerialized]
		private T[] combinedArray = Array.Empty<T>();

		// Token: 0x02000088 RID: 136
		private class ScriptableObjectArrayEnumerator : IEnumerator
		{
			// Token: 0x060003E4 RID: 996 RVA: 0x000112A6 File Offset: 0x0000F4A6
			public ScriptableObjectArrayEnumerator(ScriptableObjectArray<T> scriptableObjectArray)
			{
				this.scriptableObjectArray = scriptableObjectArray;
			}

			// Token: 0x170000A5 RID: 165
			// (get) Token: 0x060003E5 RID: 997 RVA: 0x000112BC File Offset: 0x0000F4BC
			public object Current
			{
				get
				{
					if (this.index < 0 || this.index >= this.scriptableObjectArray.Length)
					{
						throw new InvalidOperationException();
					}
					return this.scriptableObjectArray.Value[this.index];
				}
			}

			// Token: 0x060003E6 RID: 998 RVA: 0x000112FB File Offset: 0x0000F4FB
			public bool MoveNext()
			{
				this.index++;
				return this.index < this.scriptableObjectArray.Length;
			}

			// Token: 0x060003E7 RID: 999 RVA: 0x0001131E File Offset: 0x0000F51E
			public void Reset()
			{
				this.index = -1;
			}

			// Token: 0x040001E3 RID: 483
			private readonly ScriptableObjectArray<T> scriptableObjectArray;

			// Token: 0x040001E4 RID: 484
			private int index = -1;
		}
	}
}
