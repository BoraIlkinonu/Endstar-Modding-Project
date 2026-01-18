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

namespace Endless.Creator.UI
{
	// Token: 0x020002F6 RID: 758
	public class UIWirePropertyModifierView : UIGameObject
	{
		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x06000D24 RID: 3364 RVA: 0x0003F031 File Offset: 0x0003D231
		public string[] StoredParameterValues
		{
			get
			{
				return this.storedParameterValues.ToArray();
			}
		}

		// Token: 0x06000D25 RID: 3365 RVA: 0x0003F040 File Offset: 0x0003D240
		public void DisplayExistingWire(UIWireView wire)
		{
			if (this.verboseLogging)
			{
				string text = "DisplayExistingWire";
				object[] array = new object[1];
				int num = 0;
				UIWireView wire2 = wire;
				array[num] = ((wire2 != null) ? new SerializableGuid?(wire2.WireId) : null);
				DebugUtility.LogMethod(this, text, array);
			}
			if (wire == null)
			{
				base.gameObject.SetActive(false);
				DebugUtility.LogException(new NullReferenceException("wire is null!"), this);
				return;
			}
			if (this.presenters.Count > 0 || this.storedParameterValues.Count > 0)
			{
				this.Clean();
			}
			EndlessEventInfo nodeEvent = wire.ReceiverNode.NodeEvent;
			StoredParameter[] staticParameters = WiringUtilities.GetWireEntry(wire.EmitterNode.InspectedObjectId, wire.EmitterNode.MemberName, wire.ReceiverNode.InspectedObjectId, wire.ReceiverNode.MemberName).StaticParameters;
			for (int i = 0; i < staticParameters.Length; i++)
			{
				if (i >= nodeEvent.ParamList.Count)
				{
					DebugUtility.LogError(string.Format("Mismatch in parameter counts: staticParameters index {0} has no corresponding receiver parameter.", i), this);
				}
				else
				{
					StoredParameter storedParameter = staticParameters[i];
					UIStoredParameterPackage uistoredParameterPackage = new UIStoredParameterPackage(nodeEvent.ParamList[i].DisplayName, storedParameter);
					UIBasePresenter<UIStoredParameterPackage> uibasePresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnModelWithDefaultStyle<UIStoredParameterPackage>(uistoredParameterPackage, this.container);
					this.presenters.Add(uibasePresenter);
					UIStoredParameterPackagePresenter uistoredParameterPackagePresenter = uibasePresenter as UIStoredParameterPackagePresenter;
					if (uistoredParameterPackagePresenter == null)
					{
						uibasePresenter.ReturnToPool();
					}
					else
					{
						UIStoredParameterPackageView storedParameterPackageView = uistoredParameterPackagePresenter.View.Interface as UIStoredParameterPackageView;
						this.storedParameterValues.Add(storedParameter.JsonData);
						int callbackIndex = i;
						Action<object> onUserChangedHandler = delegate(object model)
						{
							this.storedParameterValues[callbackIndex] = JsonConvert.SerializeObject(model);
							this.UpdateExistingWire(wire);
						};
						storedParameterPackageView.OnUserChangedModel += onUserChangedHandler;
						this.eventUnsubscribers.Add(delegate
						{
							storedParameterPackageView.OnUserChangedModel -= onUserChangedHandler;
						});
					}
				}
			}
			if (staticParameters.Length == 0 && nodeEvent.ParamList.Count > 0)
			{
				this.DisplayDefaultParameters(nodeEvent, wire);
				return;
			}
			base.gameObject.SetActive(nodeEvent.ParamList.Count > 0);
		}

		// Token: 0x06000D26 RID: 3366 RVA: 0x0003F2A0 File Offset: 0x0003D4A0
		public void DisplayDefaultParameters(EndlessEventInfo receiverEndlessEventInfo, UIWireView wire = null)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayDefaultParameters", new object[] { receiverEndlessEventInfo.MemberName });
			}
			if (this.presenters.Count > 0 || this.storedParameterValues.Count > 0)
			{
				this.Clean();
			}
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}.{1}.Count: {2}", "receiverEndlessEventInfo", "ParamList", receiverEndlessEventInfo.ParamList.Count), this);
			}
			if (receiverEndlessEventInfo.ParamList.Count == 0)
			{
				base.gameObject.SetActive(false);
				return;
			}
			for (int i = 0; i < receiverEndlessEventInfo.ParamList.Count; i++)
			{
				EndlessParameterInfo endlessParameterInfo = receiverEndlessEventInfo.ParamList[i];
				if (this.superVerboseLogging)
				{
					DebugUtility.Log(string.Format("{0} [ {1} ]: {2}: {3}, {4}: {5}", new object[] { "endlessParameterInfo", i, "DisplayName", endlessParameterInfo.DisplayName, "DataType", endlessParameterInfo.DataType }), this);
				}
				UIBasePresenter<EndlessParameterInfo> uibasePresenter = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnModelWithDefaultStyle<EndlessParameterInfo>(endlessParameterInfo, this.container);
				this.presenters.Add(uibasePresenter);
				UIEndlessParameterInfoPresenter uiendlessParameterInfoPresenter = uibasePresenter as UIEndlessParameterInfoPresenter;
				if (uiendlessParameterInfoPresenter == null)
				{
					uibasePresenter.ReturnToPool();
				}
				else
				{
					UIEndlessParameterInfoView endlessParameterInfoView = uiendlessParameterInfoPresenter.View.Interface as UIEndlessParameterInfoView;
					object obj = MonoBehaviourSingleton<UIDynamicTypeFactory>.Instance.Create(endlessParameterInfo.GetReferencedType());
					this.storedParameterValues.Add(JsonConvert.SerializeObject(obj));
					int callbackIndex = i;
					Action<object> onUserChangedHandler = delegate(object model)
					{
						if (this.verboseLogging)
						{
							DebugUtility.Log("model's Type: " + model.GetType().Name, this);
							DebugUtility.Log(string.Format("{0}[{1}]: {2}", "storedParameterValues", callbackIndex, JsonConvert.SerializeObject(model)), this);
							DebugUtility.Log(string.Format("{0}.Count: {1}", "storedParameterValues", this.storedParameterValues.Count), this);
						}
						this.storedParameterValues[callbackIndex] = JsonConvert.SerializeObject(model);
						if (wire != null)
						{
							this.UpdateExistingWire(wire);
						}
					};
					endlessParameterInfoView.OnUserChangedModel += onUserChangedHandler;
					this.eventUnsubscribers.Add(delegate
					{
						endlessParameterInfoView.OnUserChangedModel -= onUserChangedHandler;
					});
				}
			}
			base.gameObject.SetActive(true);
		}

		// Token: 0x06000D27 RID: 3367 RVA: 0x0003F49C File Offset: 0x0003D69C
		public void Clean()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clean", Array.Empty<object>());
			}
			foreach (Action action in this.eventUnsubscribers)
			{
				action();
			}
			this.eventUnsubscribers.Clear();
			this.storedParameterValues.Clear();
			foreach (IUIPresentable iuipresentable in this.presenters)
			{
				iuipresentable.ReturnToPool();
			}
			this.presenters.Clear();
			base.gameObject.SetActive(false);
		}

		// Token: 0x06000D28 RID: 3368 RVA: 0x0003F570 File Offset: 0x0003D770
		private void UpdateExistingWire(UIWireView wire)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateExistingWire", new object[] { wire });
			}
			MonoBehaviourSingleton<UIWiringManager>.Instance.WiringTool.UpdateWire(wire.WireId, wire.EmitterNode.InspectedObjectId, wire.EmitterNode.MemberName, wire.EmitterNode.AssemblyQualifiedTypeName, wire.ReceiverNode.InspectedObjectId, wire.ReceiverNode.MemberName, wire.ReceiverNode.AssemblyQualifiedTypeName, this.storedParameterValues.ToArray(), Array.Empty<SerializableGuid>(), WireColor.NoColor);
		}

		// Token: 0x04000B4E RID: 2894
		[SerializeField]
		private RectTransform container;

		// Token: 0x04000B4F RID: 2895
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000B50 RID: 2896
		[SerializeField]
		private bool superVerboseLogging;

		// Token: 0x04000B51 RID: 2897
		private readonly List<IUIPresentable> presenters = new List<IUIPresentable>();

		// Token: 0x04000B52 RID: 2898
		private readonly List<string> storedParameterValues = new List<string>();

		// Token: 0x04000B53 RID: 2899
		private readonly List<Action> eventUnsubscribers = new List<Action>();
	}
}
