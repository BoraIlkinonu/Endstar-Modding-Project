using UnityEngine;

namespace Endless.Gameplay.LevelEditing;

public class CreatorComment : MonoBehaviour
{
	[SerializeField]
	[TextArea]
	private string[] comments = new string[1];
}
