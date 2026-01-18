using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIWirePropertyModifierView : UIGameObject
{
	[SerializeField]
	private RectTransform container;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	[SerializeField]
	private bool superVerboseLogging;

	private readonly List<IUIPresentable> presenters = new List<IUIPresentable>();

	private readonly List<string> storedParameterValues = new List<string>();

	private readonly List<Action> eventUnsubscribers = new List<Action>();

	public string[] StoredParameterValues => storedParameterValues.ToArray();

	public void DisplayExistingWire(UIWireView wire)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayExistingWire", wire?.WireId);
		}
		if (wire == null)
		{
			base.gameObject.SetActive(value: false);
			DebugUtility.LogException(new NullReferenceException("wire is null!"), this);
			return;
		}
		if (presenters.Count > 0 || storedParameterValues.Count > 0)
		{
			Clean();
		}
		EndlessEventInfo nodeEvent = wire.ReceiverNode.NodeEvent;
		StoredParameter[] staticParameters = WiringUtilities.GetWireEntry(wire.EmitterNode.InspectedObjectId, wire.EmitterNode.MemberName, wire.ReceiverNode.InspectedObjectId, wire.ReceiverNode.MemberName).StaticParameters;
		for (int i = 0; i < staticParameters.Length; i++)
		{
			if (i >= nodeEvent.ParamList.Count)
			{
				DebugUtility.LogError($"Mismatch in parameter counts: staticParameters index {i} has no corresponding receiver parameter.", this);
				continue;
			}
			StoredParameter storedParameter = staticParameters[i];
			UIStoredParameterPackage model = new UIStoredParameterPackage(nodeEvent.ParamList[i].DisplayName, storedParameter);
			UIBasePresenter<UIStoredParameterPackage> uIBasePresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnModelWithDefaultStyle(model, container);
			presenters.Add(uIBasePresenter);
			if (!(uIBasePresenter is UIStoredParameterPackagePresenter uIStoredParameterPackagePresenter))
			{
				uIBasePresenter.ReturnToPool();
				continue;
			}
			UIStoredParameterPackageView storedParameterPackageView = uIStoredParameterPackagePresenter.View.Interface as UIStoredParameterPackageView;
			storedParameterValues.Add(storedParameter.JsonData);
			int callbackIndex = i;
			Action<object> onUserChangedHandler = delegate(object value)
			{
				storedParameterValues[callbackIndex] = JsonConvert.SerializeObject(value);
				UpdateExistingWire(wire);
			};
			storedParameterPackageView.OnUserChangedModel += onUserChangedHandler;
			eventUnsubscribers.Add(delegate
			{
				storedParameterPackageView.OnUserChangedModel -= onUserChangedHandler;
			});
		}
		if (staticParameters.Length == 0 && nodeEvent.ParamList.Count > 0)
		{
			DisplayDefaultParameters(nodeEvent, wire);
		}
		else
		{
			base.gameObject.SetActive(nodeEvent.ParamList.Count > 0);
		}
	}

	public void DisplayDefaultParameters(EndlessEventInfo receiverEndlessEventInfo, UIWireView wire = null)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayDefaultParameters", receiverEndlessEventInfo.MemberName);
		}
		if (presenters.Count > 0 || storedParameterValues.Count > 0)
		{
			Clean();
		}
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0}.{1}.Count: {2}", "receiverEndlessEventInfo", "ParamList", receiverEndlessEventInfo.ParamList.Count), this);
		}
		if (receiverEndlessEventInfo.ParamList.Count == 0)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		for (int i = 0; i < receiverEndlessEventInfo.ParamList.Count; i++)
		{
			EndlessParameterInfo endlessParameterInfo = receiverEndlessEventInfo.ParamList[i];
			if (superVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} [ {1} ]: {2}: {3}, {4}: {5}", "endlessParameterInfo", i, "DisplayName", endlessParameterInfo.DisplayName, "DataType", endlessParameterInfo.DataType), this);
			}
			UIBasePresenter<EndlessParameterInfo> uIBasePresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnModelWithDefaultStyle(endlessParameterInfo, container);
			presenters.Add(uIBasePresenter);
			if (!(uIBasePresenter is UIEndlessParameterInfoPresenter uIEndlessParameterInfoPresenter))
			{
				uIBasePresenter.ReturnToPool();
				continue;
			}
			UIEndlessParameterInfoView endlessParameterInfoView = uIEndlessParameterInfoPresenter.View.Interface as UIEndlessParameterInfoView;
			object value = MonoBehaviourSingleton<UIDynamicTypeFactory>.Instance.Create(endlessParameterInfo.GetReferencedType());
			storedParameterValues.Add(JsonConvert.SerializeObject(value));
			int callbackIndex = i;
			Action<object> onUserChangedHandler = delegate(object model)
			{
				if (verboseLogging)
				{
					DebugUtility.Log("model's Type: " + model.GetType().Name, this);
					DebugUtility.Log(string.Format("{0}[{1}]: {2}", "storedParameterValues", callbackIndex, JsonConvert.SerializeObject(model)), this);
					DebugUtility.Log(string.Format("{0}.Count: {1}", "storedParameterValues", storedParameterValues.Count), this);
				}
				storedParameterValues[callbackIndex] = JsonConvert.SerializeObject(model);
				if (wire != null)
				{
					UpdateExistingWire(wire);
				}
			};
			endlessParameterInfoView.OnUserChangedModel += onUserChangedHandler;
			eventUnsubscribers.Add(delegate
			{
				endlessParameterInfoView.OnUserChangedModel -= onUserChangedHandler;
			});
		}
		base.gameObject.SetActive(value: true);
	}

	public void Clean()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Clean");
		}
		foreach (Action eventUnsubscriber in eventUnsubscribers)
		{
			eventUnsubscriber();
		}
		eventUnsubscribers.Clear();
		storedParameterValues.Clear();
		foreach (IUIPresentable presenter in presenters)
		{
			presenter.ReturnToPool();
		}
		presenters.Clear();
		base.gameObject.SetActive(value: false);
	}

	private void UpdateExistingWire(UIWireView wire)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateExistingWire", wire);
		}
		MonoBehaviourSingleton<UIWiringManager>.Instance.WiringTool.UpdateWire(wire.WireId, wire.EmitterNode.InspectedObjectId, wire.EmitterNode.MemberName, wire.EmitterNode.AssemblyQualifiedTypeName, wire.ReceiverNode.InspectedObjectId, wire.ReceiverNode.MemberName, wire.ReceiverNode.AssemblyQualifiedTypeName, storedParameterValues.ToArray(), Array.Empty<SerializableGuid>(), WireColor.NoColor);
	}
}
