using UnityEngine;
using System.Collections;

public class CharacterSelect : MonoBehaviour {

	public SceneControllerSignUp SceneController;
	public int CharacterTypeNumber;

	void OnClick() {
		SceneController.characterType = CharacterTypeNumber;

		SceneController.AuraEffect.transform.parent = this.transform;
		Vector3 prev = SceneController.AuraEffect.transform.localPosition;
		prev.x = 0;
		SceneController.AuraEffect.transform.localPosition = prev;
	}

}
