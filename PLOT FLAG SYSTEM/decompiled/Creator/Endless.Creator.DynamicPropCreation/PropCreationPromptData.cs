using UnityEngine;

namespace Endless.Creator.DynamicPropCreation;

[CreateAssetMenu(menuName = "ScriptableObject/Dynamic Prop Creation/PropCreationPromptData")]
public class PropCreationPromptData : PropCreationData
{
	[TextArea]
	[SerializeField]
	private string message;

	public string Message => message;

	public override bool IsSubMenu => false;
}
