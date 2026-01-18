using System;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200006A RID: 106
	[Serializable]
	public class InterfaceReference<T> : ISerializationCallbackReceiver where T : class
	{
		// Token: 0x1700008B RID: 139
		// (get) Token: 0x0600035C RID: 860 RVA: 0x0000FD5F File Offset: 0x0000DF5F
		public T Interface
		{
			get
			{
				return this.target as T;
			}
		}

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x0600035D RID: 861 RVA: 0x0000FD71 File Offset: 0x0000DF71
		public bool IsValid
		{
			get
			{
				return this.target != null && this.target is T;
			}
		}

		// Token: 0x0600035E RID: 862 RVA: 0x0000FD91 File Offset: 0x0000DF91
		public void SetTarget(global::UnityEngine.Object target)
		{
			this.target = target;
		}

		// Token: 0x0600035F RID: 863 RVA: 0x0000FD9A File Offset: 0x0000DF9A
		public static implicit operator bool(InterfaceReference<T> interfaceReference)
		{
			return interfaceReference.IsValid;
		}

		// Token: 0x06000360 RID: 864 RVA: 0x000050BB File Offset: 0x000032BB
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		// Token: 0x06000361 RID: 865 RVA: 0x000050BB File Offset: 0x000032BB
		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
		}

		// Token: 0x040001A2 RID: 418
		[SerializeField]
		private global::UnityEngine.Object target;
	}
}
