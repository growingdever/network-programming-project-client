using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;

public class ConnectedUserListCell : MonoBehaviour {

	public UILabel LabelID;
	string userID;
	int level;

	public void UpdateInformation(JSONObject json) {
		userID = json.GetString ("user_id");
		int characterType = (int)json.GetNumber ("character_type");
		level = (int)json.GetNumber ("exp") / 300;

		LabelID.text = string.Format ("Lv.{0} {1}", level, userID);
	}

}
