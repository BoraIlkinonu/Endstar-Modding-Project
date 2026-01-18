using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPublishView : UIGameObject
{
	[SerializeField]
	private UIPublishModel model;

	[SerializeField]
	private UIDropdownVersion betaDropdown;

	[SerializeField]
	private UIDropdownVersion publicDropdown;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private readonly string[] betaValue = new string[1];

	private readonly string[] publicValue = new string[1];

	public void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		model.OnSynchronizeStart += OnSynchronizeStart;
		model.OnVersionsSet += SetDropdownValues;
		model.OnBetaVersionSet += DisplayBetaVersion;
		model.OnPublicVersionSet += DisplayPublicVersion;
		model.OnClientGameRoleSet += OnClientGameRoleSet;
	}

	private void OnDestroy()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		model.OnSynchronizeStart -= OnSynchronizeStart;
		model.OnVersionsSet -= SetDropdownValues;
		model.OnBetaVersionSet -= DisplayBetaVersion;
		model.OnPublicVersionSet -= DisplayPublicVersion;
		model.OnClientGameRoleSet -= OnClientGameRoleSet;
	}

	public void Display()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Display");
		}
		SetInteractable(interactable: false);
		model.Synchronize();
	}

	private void OnSynchronizeStart()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Display");
		}
		betaDropdown.SetValueText(string.Empty);
		publicDropdown.SetValueText(string.Empty);
	}

	private void SetDropdownValues(List<string> versions)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetDropdownValues", versions.Count);
		}
		betaValue[0] = versions[0];
		publicValue[0] = versions[0];
		betaDropdown.SetOptionsAndValue(versions, betaValue[0], triggerOnValueChanged: false);
		publicDropdown.SetOptionsAndValue(versions, publicValue[0], triggerOnValueChanged: false);
	}

	private void DisplayBetaVersion(string version)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayBetaVersion", version);
		}
		betaDropdown.SetValue(version, triggerOnValueChanged: false);
	}

	private void DisplayPublicVersion(string version)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayPublicVersion", version);
		}
		publicDropdown.SetValue(version, triggerOnValueChanged: true);
	}

	private void OnClientGameRoleSet(Roles clientGameRole)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnClientGameRoleSet", clientGameRole);
		}
		bool interactable = clientGameRole.IsGreaterThanOrEqualTo(Roles.Publisher);
		SetInteractable(interactable);
	}

	private void SetInteractable(bool interactable)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInteractable", interactable);
		}
		betaDropdown.SetIsInteractable(interactable);
		publicDropdown.SetIsInteractable(interactable);
	}
}
