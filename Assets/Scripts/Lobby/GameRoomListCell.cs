using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;

public class GameRoomListCell : MonoBehaviour {

	public UILabel LabelTitle;
	public UILabel LabelStatus;
	public UILabel LabelMemberCount;

	int roomID;

	public void UpdateInformation(JSONObject json) {
		roomID = (int)json.GetNumber ("room_id");
		LabelTitle.text = json.GetString ("title");
		LabelMemberCount.text = json.GetString ("num_of_member");

		int status = (int)json.GetNumber ("state");
		if (status == 0) {
			LabelStatus.text = "READY";
		} else if (status == 1) {
			LabelStatus.text = "PLAYING";
		}
	}

	public void OnClick() {
		print ("room clicked : " + roomID);
	}


}
