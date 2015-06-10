using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;

public class SceneControllerGame : SceneController {

	public UIInput InputChattingMessage;
	public UILabel LabelTitle;
	public UILabel LabelGameStart;
	public User[] Users;


	public override void Start ()
	{
		base.Start ();

		LabelTitle.text = "Let\'s start a game...";

		SendRequestRoomMemberUpdate ();
	}

	public override void OnMessageReceived() 
	{
		JSONObject json = JSONObject.Parse (SocketWrapper.Instance.Pop ());
		print (json);
		if (!json.ContainsKey ("result")) {
			return;
		}
		
		int resultCode = (int)json.GetNumber ("result");
		if (resultCode == ResultCodes.RESULT_OK_STATE_GAME_ROOM) {
			UpdateGameRoom (json);
		} else if (resultCode == ResultCodes.RESULT_OK_MAKE_QUIZ) {
			UpdateQuiz (json);
		} else if (resultCode == ResultCodes.RESULT_OK_ROUND_RESULT) {
			ShowRoundResult (json);
		} else if (resultCode == ResultCodes.RESULT_OK_TOTAL_RESULT) {
			ShowTotalResult (json);
		} else if (resultCode == ResultCodes.RESULT_OK_REQUEST_ROOM_MEMBER_UPDATE) {
			SendRequestRoomMemberUpdate ();
		} else if (resultCode == ResultCodes.RESULT_OK_CHAT_MESSAGE) {
			ReceiveChatMessage(json);
		}
	}

	public void OnChattingMessageSubmit() {
		string message = InputChattingMessage.value;
		InputChattingMessage.value = "";

		JSONObject json = new JSONObject ();
		json.Add ("target", ServerAPITargets.TARGET_CHATTING);
		json.Add ("access_token", SocketWrapper.Instance.accessToken);
		json.Add ("message", message);
		SocketWrapper.Instance.WriteSocket (json.ToString ());
	}

	public void OnClickGameStart() {
		LabelGameStart.GetComponent<Animator> ().Play ("Dismiss");

		JSONObject json = new JSONObject ();
		json.Add ("target", ServerAPITargets.TARGET_GAME_START);
		json.Add ("access_token", SocketWrapper.Instance.accessToken);
		SocketWrapper.Instance.WriteSocket (json.ToString ());
	}

	void UpdateGameRoom(JSONObject json) {
		JSONObject jsonData = json.GetObject ("data");
		JSONArray jsonUserList = jsonData.GetArray ("user_list");
		for (int i = 0; i < jsonUserList.Length; i ++) {
			JSONObject jsonUser = jsonUserList[i].Obj;
			Users[i].Init(jsonUser);
		}
		for (int i = jsonUserList.Length; i < 4; i ++) {
			Users[i].gameObject.SetActive(false);
		}
	}

	void SendRequestRoomMemberUpdate() {
		JSONObject json = new JSONObject ();
		json.Add ("target", ServerAPITargets.TARGET_CHECK_ROOM);
		json.Add ("access_token", SocketWrapper.Instance.accessToken);
		SocketWrapper.Instance.WriteSocket (json.ToString ());
	}

	void UpdateQuiz(JSONObject json) {
		LabelTitle.text = json.GetString ("quiz_string");
	}

	void ShowRoundResult(JSONObject json) {
		string winnerID = json.GetString ("winner_id");
		int remainRound = (int)json.GetNumber ("remain_round");
		LabelTitle.text = string.Format ("{0} is win on this round!\n{1} round is left...", winnerID, remainRound);
	}

	void ShowTotalResult(JSONObject json) {
		string winnerID = json.GetString ("winner_id");
		LabelTitle.text = string.Format ("Final winner is {0}!", winnerID);

		StartCoroutine (InitTitleWithDelay (3.0f));
	}

	IEnumerator InitTitleWithDelay(float delay) {
		yield return new WaitForSeconds (delay);
		LabelTitle.text = "Let\'s start a game...";

		LabelGameStart.GetComponent<Animator> ().Play ("Show");
	}

	void ReceiveChatMessage(JSONObject json) {
		JSONObject jsonMessage = json.GetObject ("message");
		string senderID = jsonMessage.GetString ("sender_id");
		string content = jsonMessage.GetString ("content");

		for( int i = 0; i < Users.Length; i ++ ) {
			Users[i].SetMessageIf(senderID, content);
		}
	}
}
