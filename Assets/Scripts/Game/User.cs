using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;

public class User : MonoBehaviour {

	public UILabel LabelUserID;
	public UISprite SpriteCharacter;
	public UISprite SpriteBalloon;
	public UILabel LabelMessage;

	string userID;


	public void Init(JSONObject json) {
		userID = json.GetString ("user_id");
		int characterType = (int)json.GetNumber ("character_type") + 1;
		int level = (int)json.GetNumber ("level");

		LabelUserID.text = string.Format ("Lv.{0} {1}", level, userID);
		SpriteCharacter.spriteName = string.Format ("character{0}", characterType);

		DismissBalloon ();
	}

	public void SetMessageIf(string userID, string msg) {
		if (this.userID == userID) {
			SetMessage(msg);
		}
	}

	public void SetMessage(string msg) {
		ShowBalloon ();
		LabelMessage.text = msg;
	}

	public void ShowBalloon() {
		SpriteBalloon.gameObject.SetActive(true);
		Invoke ("DismissBalloon", 3.0f);
	}

	public void DismissBalloon() {
		SpriteBalloon.gameObject.SetActive(false);
	}

}
