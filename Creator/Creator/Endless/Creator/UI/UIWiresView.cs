using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020002FC RID: 764
	public class UIWiresView : UIGameObject
	{
		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x06000D3F RID: 3391 RVA: 0x0003F851 File Offset: 0x0003DA51
		public int SpawnedWiresCount
		{
			get
			{
				return this.spawnedWires.Count;
			}
		}

		// Token: 0x06000D40 RID: 3392 RVA: 0x0003F860 File Offset: 0x0003DA60
		public void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			WireBundle wireBundle = WiringUtilities.GetWireBundle(this.emitterWiringInspectorView.TargetId, this.receiverWiringInspectorView.TargetId);
			if (wireBundle != null)
			{
				List<WireEntry> wires = wireBundle.Wires;
				if (wires.Count > 0)
				{
					Canvas.ForceUpdateCanvases();
					foreach (WireEntry wireEntry in wires)
					{
						string emitterMemberName = wireEntry.EmitterMemberName;
						string receiverMemberName = wireEntry.ReceiverMemberName;
						SerializableGuid wireId = wireEntry.WireId;
						UIWireNodeView nodeView = this.emitterWiringInspectorView.GetNodeView(wireEntry.EmitterComponentAssemblyQualifiedTypeName, emitterMemberName);
						UIWireNodeView nodeView2 = this.receiverWiringInspectorView.GetNodeView(wireEntry.ReceiverComponentAssemblyQualifiedTypeName, receiverMemberName);
						if (!(nodeView == null) && !(nodeView2 == null))
						{
							PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
							UIWireView uiwireView = this.wireSource;
							Transform rectTransform = base.RectTransform;
							UIWireView uiwireView2 = instance.Spawn<UIWireView>(uiwireView, default(Vector3), default(Quaternion), rectTransform);
							uiwireView2.RectTransform.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
							bool flag = (nodeView ? nodeView.IsOnLeftOfScreen : nodeView2.IsOnLeftOfScreen);
							uiwireView2.Initialize(wireId, nodeView, nodeView2, flag);
							this.spawnedWires.Add(uiwireView2);
							this.spawnedWireLookup.Add(wireId, uiwireView2);
						}
					}
				}
			}
		}

		// Token: 0x06000D41 RID: 3393 RVA: 0x0003FA00 File Offset: 0x0003DC00
		public UIWireView DisplayTempWire(string emitterTypeName, string emitterMemberName, string receiverTypeName, string receiverMemberName)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayTempWire", new object[] { emitterTypeName, emitterMemberName, receiverTypeName, receiverMemberName });
			}
			if (this.tempWire)
			{
				this.DespawnTempWire();
			}
			UIWireNodeView nodeView = this.emitterWiringInspectorView.GetNodeView(emitterTypeName, emitterMemberName);
			UIWireNodeView nodeView2 = this.receiverWiringInspectorView.GetNodeView(receiverTypeName, receiverMemberName);
			PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIWireView uiwireView = this.wireSource;
			Transform rectTransform = base.RectTransform;
			this.tempWire = instance.Spawn<UIWireView>(uiwireView, default(Vector3), default(Quaternion), rectTransform);
			this.tempWire.RectTransform.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
			SerializableGuid empty = SerializableGuid.Empty;
			bool flag = (nodeView ? nodeView.IsOnLeftOfScreen : nodeView2.IsOnLeftOfScreen);
			this.tempWire.Initialize(empty, nodeView, nodeView2, flag);
			this.spawnedWires.Add(this.tempWire);
			this.spawnedWireLookup.Add(empty, this.tempWire);
			return this.tempWire;
		}

		// Token: 0x06000D42 RID: 3394 RVA: 0x0003FB18 File Offset: 0x0003DD18
		public void DespawnTempWire()
		{
			if (!this.tempWire)
			{
				return;
			}
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DespawnTempWire", Array.Empty<object>());
			}
			this.spawnedWires.Remove(this.tempWire);
			this.spawnedWireLookup.Remove(SerializableGuid.Empty);
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIWireView>(this.tempWire);
			this.tempWire = null;
		}

		// Token: 0x06000D43 RID: 3395 RVA: 0x0003FB88 File Offset: 0x0003DD88
		public UIWireView GetWire(SerializableGuid wireId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetWire", new object[] { wireId });
			}
			if (!this.spawnedWireLookup.ContainsKey(wireId))
			{
				DebugUtility.LogWarning(this, "GetWire", "spawnedWireLookup does not contain that wireId", new object[] { wireId });
				return null;
			}
			return this.spawnedWireLookup[wireId];
		}

		// Token: 0x06000D44 RID: 3396 RVA: 0x0003FBF4 File Offset: 0x0003DDF4
		public void ToggleDarkMode(bool state, UIWireView except)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleDarkMode", new object[]
				{
					state,
					except.DebugSafeName(true)
				});
			}
			for (int i = 0; i < this.spawnedWires.Count; i++)
			{
				if (this.spawnedWires[i] != except)
				{
					if (state)
					{
						this.spawnedWires[i].Darken();
					}
					else
					{
						this.spawnedWires[i].Lighten();
					}
				}
			}
			if (except)
			{
				except.Lighten();
				except.transform.SetAsLastSibling();
			}
		}

		// Token: 0x06000D45 RID: 3397 RVA: 0x0003FC98 File Offset: 0x0003DE98
		public void ToggleDynamicWires(bool state)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleDynamicWires", new object[] { state });
			}
			for (int i = 0; i < this.spawnedWires.Count; i++)
			{
				this.spawnedWires[i].enabled = state;
				if (!state)
				{
					this.spawnedWires[i].UpdateLineRendererPoints();
				}
			}
		}

		// Token: 0x06000D46 RID: 3398 RVA: 0x0003FD04 File Offset: 0x0003DF04
		public void DespawnAllWires()
		{
			this.DespawnTempWire();
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DespawnAllWires", Array.Empty<object>());
			}
			for (int i = 0; i < this.spawnedWires.Count; i++)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIWireView>(this.spawnedWires[i]);
			}
			this.spawnedWires.Clear();
			this.spawnedWireLookup.Clear();
		}

		// Token: 0x06000D47 RID: 3399 RVA: 0x0003FD74 File Offset: 0x0003DF74
		public void DespawnWire(UIWireView wire)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DespawnWire", new object[] { wire.WireId });
			}
			this.spawnedWires.Remove(wire);
			this.spawnedWireLookup.Remove(wire.WireId);
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIWireView>(wire);
		}

		// Token: 0x04000B66 RID: 2918
		[SerializeField]
		private UIWireView wireSource;

		// Token: 0x04000B67 RID: 2919
		[SerializeField]
		private UIWiringObjectInspectorView emitterWiringInspectorView;

		// Token: 0x04000B68 RID: 2920
		[SerializeField]
		private UIWiringObjectInspectorView receiverWiringInspectorView;

		// Token: 0x04000B69 RID: 2921
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000B6A RID: 2922
		private readonly List<UIWireView> spawnedWires = new List<UIWireView>();

		// Token: 0x04000B6B RID: 2923
		private Dictionary<SerializableGuid, UIWireView> spawnedWireLookup = new Dictionary<SerializableGuid, UIWireView>();

		// Token: 0x04000B6C RID: 2924
		private UIWireView tempWire;
	}
}
