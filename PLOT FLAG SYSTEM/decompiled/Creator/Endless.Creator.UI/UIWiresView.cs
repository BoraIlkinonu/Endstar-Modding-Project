using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIWiresView : UIGameObject
{
	[SerializeField]
	private UIWireView wireSource;

	[SerializeField]
	private UIWiringObjectInspectorView emitterWiringInspectorView;

	[SerializeField]
	private UIWiringObjectInspectorView receiverWiringInspectorView;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private readonly List<UIWireView> spawnedWires = new List<UIWireView>();

	private Dictionary<SerializableGuid, UIWireView> spawnedWireLookup = new Dictionary<SerializableGuid, UIWireView>();

	private UIWireView tempWire;

	public int SpawnedWiresCount => spawnedWires.Count;

	public void Display()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Display");
		}
		WireBundle wireBundle = WiringUtilities.GetWireBundle(emitterWiringInspectorView.TargetId, receiverWiringInspectorView.TargetId);
		if (wireBundle == null)
		{
			return;
		}
		List<WireEntry> wires = wireBundle.Wires;
		if (wires.Count <= 0)
		{
			return;
		}
		Canvas.ForceUpdateCanvases();
		foreach (WireEntry item in wires)
		{
			string emitterMemberName = item.EmitterMemberName;
			string receiverMemberName = item.ReceiverMemberName;
			SerializableGuid wireId = item.WireId;
			UIWireNodeView nodeView = emitterWiringInspectorView.GetNodeView(item.EmitterComponentAssemblyQualifiedTypeName, emitterMemberName);
			UIWireNodeView nodeView2 = receiverWiringInspectorView.GetNodeView(item.ReceiverComponentAssemblyQualifiedTypeName, receiverMemberName);
			if (!(nodeView == null) && !(nodeView2 == null))
			{
				PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
				UIWireView prefab = wireSource;
				Transform parent = base.RectTransform;
				UIWireView uIWireView = instance.Spawn(prefab, default(Vector3), default(Quaternion), parent);
				uIWireView.RectTransform.SetAnchor(AnchorPresets.StretchAll);
				bool flowLeftToRight = (nodeView ? nodeView.IsOnLeftOfScreen : nodeView2.IsOnLeftOfScreen);
				uIWireView.Initialize(wireId, nodeView, nodeView2, flowLeftToRight);
				spawnedWires.Add(uIWireView);
				spawnedWireLookup.Add(wireId, uIWireView);
			}
		}
	}

	public UIWireView DisplayTempWire(string emitterTypeName, string emitterMemberName, string receiverTypeName, string receiverMemberName)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayTempWire", emitterTypeName, emitterMemberName, receiverTypeName, receiverMemberName);
		}
		if ((bool)tempWire)
		{
			DespawnTempWire();
		}
		UIWireNodeView nodeView = emitterWiringInspectorView.GetNodeView(emitterTypeName, emitterMemberName);
		UIWireNodeView nodeView2 = receiverWiringInspectorView.GetNodeView(receiverTypeName, receiverMemberName);
		PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
		UIWireView prefab = wireSource;
		Transform parent = base.RectTransform;
		tempWire = instance.Spawn(prefab, default(Vector3), default(Quaternion), parent);
		tempWire.RectTransform.SetAnchor(AnchorPresets.StretchAll);
		SerializableGuid empty = SerializableGuid.Empty;
		bool flowLeftToRight = (nodeView ? nodeView.IsOnLeftOfScreen : nodeView2.IsOnLeftOfScreen);
		tempWire.Initialize(empty, nodeView, nodeView2, flowLeftToRight);
		spawnedWires.Add(tempWire);
		spawnedWireLookup.Add(empty, tempWire);
		return tempWire;
	}

	public void DespawnTempWire()
	{
		if ((bool)tempWire)
		{
			if (verboseLogging)
			{
				DebugUtility.LogMethod(this, "DespawnTempWire");
			}
			spawnedWires.Remove(tempWire);
			spawnedWireLookup.Remove(SerializableGuid.Empty);
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(tempWire);
			tempWire = null;
		}
	}

	public UIWireView GetWire(SerializableGuid wireId)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetWire", wireId);
		}
		if (!spawnedWireLookup.ContainsKey(wireId))
		{
			DebugUtility.LogWarning(this, "GetWire", "spawnedWireLookup does not contain that wireId", wireId);
			return null;
		}
		return spawnedWireLookup[wireId];
	}

	public void ToggleDarkMode(bool state, UIWireView except)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleDarkMode", state, except.DebugSafeName());
		}
		for (int i = 0; i < spawnedWires.Count; i++)
		{
			if (spawnedWires[i] != except)
			{
				if (state)
				{
					spawnedWires[i].Darken();
				}
				else
				{
					spawnedWires[i].Lighten();
				}
			}
		}
		if ((bool)except)
		{
			except.Lighten();
			except.transform.SetAsLastSibling();
		}
	}

	public void ToggleDynamicWires(bool state)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleDynamicWires", state);
		}
		for (int i = 0; i < spawnedWires.Count; i++)
		{
			spawnedWires[i].enabled = state;
			if (!state)
			{
				spawnedWires[i].UpdateLineRendererPoints();
			}
		}
	}

	public void DespawnAllWires()
	{
		DespawnTempWire();
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DespawnAllWires");
		}
		for (int i = 0; i < spawnedWires.Count; i++)
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(spawnedWires[i]);
		}
		spawnedWires.Clear();
		spawnedWireLookup.Clear();
	}

	public void DespawnWire(UIWireView wire)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DespawnWire", wire.WireId);
		}
		spawnedWires.Remove(wire);
		spawnedWireLookup.Remove(wire.WireId);
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(wire);
	}
}
