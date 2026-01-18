using System;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;

namespace Endless.Gameplay
{
	// Token: 0x0200028F RID: 655
	[Serializable]
	public class PropLibraryReference : InspectorPropReference, INetworkSerializable, IEquatable<PropLibraryReference>
	{
		// Token: 0x170002C0 RID: 704
		// (get) Token: 0x06000E7D RID: 3709 RVA: 0x0001965C File Offset: 0x0001785C
		internal override ReferenceFilter Filter
		{
			get
			{
				return ReferenceFilter.None;
			}
		}

		// Token: 0x06000E7E RID: 3710 RVA: 0x0004DA48 File Offset: 0x0004BC48
		internal virtual PropLibrary.RuntimePropInfo GetReference()
		{
			PropLibrary.RuntimePropInfo runtimePropInfo;
			try
			{
				runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(this.Id);
			}
			catch
			{
				runtimePropInfo = null;
			}
			return runtimePropInfo;
		}

		// Token: 0x06000E7F RID: 3711 RVA: 0x0004DA84 File Offset: 0x0004BC84
		public string GetReferenceName()
		{
			PropLibrary.RuntimePropInfo reference = this.GetReference();
			if (reference == null)
			{
				return "None";
			}
			return reference.PropData.Name;
		}

		// Token: 0x06000E80 RID: 3712 RVA: 0x0004DAAC File Offset: 0x0004BCAC
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<SerializableGuid>(ref this.Id, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue<SerializableGuid>(ref this.CosmeticId, default(FastBufferWriter.ForNetworkSerializable));
		}

		// Token: 0x06000E81 RID: 3713 RVA: 0x0004DAE5 File Offset: 0x0004BCE5
		public override int GetHashCode()
		{
			return HashCode.Combine<SerializableGuid, SerializableGuid>(this.Id, this.CosmeticId);
		}

		// Token: 0x06000E82 RID: 3714 RVA: 0x0004DAF8 File Offset: 0x0004BCF8
		public override bool Equals(object obj)
		{
			PropLibraryReference propLibraryReference = obj as PropLibraryReference;
			return propLibraryReference != null && this.Id == propLibraryReference.Id && this.CosmeticId == propLibraryReference.CosmeticId;
		}

		// Token: 0x06000E83 RID: 3715 RVA: 0x0004DB35 File Offset: 0x0004BD35
		public static bool operator ==(PropLibraryReference a, PropLibraryReference b)
		{
			return a == b || (a != null && b != null && a.Equals(b));
		}

		// Token: 0x06000E84 RID: 3716 RVA: 0x0004DB4E File Offset: 0x0004BD4E
		public static bool operator !=(PropLibraryReference a, PropLibraryReference b)
		{
			return !(a == b);
		}

		// Token: 0x06000E85 RID: 3717 RVA: 0x0004DB5A File Offset: 0x0004BD5A
		public bool Equals(PropLibraryReference other)
		{
			return other != null && this.Id == other.Id && this.CosmeticId == other.CosmeticId;
		}

		// Token: 0x06000E86 RID: 3718 RVA: 0x0004DB8B File Offset: 0x0004BD8B
		public override string ToString()
		{
			return string.Format("{0}, {1}: {2}", base.ToString(), "CosmeticId", this.CosmeticId);
		}

		// Token: 0x04000D12 RID: 3346
		internal SerializableGuid CosmeticId;
	}
}
