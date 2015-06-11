using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;

public class GameRoomListCell : MonoBehaviour {

	public UILabel LabelTitle;
	public UILabel LabelStatus;
	public UILabel LabelMemberCount;
	public Color StatusColorWaiting;
	public Color StatusColorPlaying;

	int roomID;

	public void UpdateInformation(JSONObject json) {
		roomID = (int)json.GetNumber ("room_id");
		LabelTitle.text = json.GetString ("title");
		LabelMemberCount.text = json.GetString ("num_of_member");

		int status = (int)json.GetNumber ("state");
		if (status == 0) {
			LabelStatus.text = "READY";
			LabelStatus.color = StatusColorWaiting;
		} else if (status == 1) {
			LabelStatus.text = "PLAYING";
			LabelStatus.color = StatusColorPlaying;
		}
	}

	public void OnClick() {
		JSONObject json = new JSONObject ();
		json.Add ("target", 4);
		json.Add ("access_token", SocketWrapper.Instance.accessToken);
		json.Add ("room_id", roomID);
		SocketWrapper.Instance.WriteSocket (json.ToString ());
	}

}
