using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;

public class SceneControllerGame : SceneController {

	public UIInput InputChattingMessage;
	public UILabel LabelTitle;
	public UILabel LabelGameStart;
	public User[] Users;


	public override void Awake ()
	{
		base.Awake ();

		for (int i = 0; i < Users.Length; i++) {
			Users[i].gameObject.SetActive(false);
		}
	}

	public override void Start ()
	{
		base.Start ();

		LabelTitle.text = "Let\'s start a game...";

		SocketWrapper.Instance.accessToken = "user0";

//		SendRequestRoomMemberUpdate ();
		StartCoroutine (TestRoundWinner ());
	}

	IEnumerator TestRoundWinner() {
		JSONObject jsonRoomState = new JSONObject();
		jsonRoomState.Add("result", 1015);

		JSONObject jsonData = new JSONObject ();
		jsonData.Add ("room_id", 1);
		jsonRoomState.Add ("data", jsonData);

		JSONArray userList = new JSONArray ();
		for (int i = 1; i <= 2; i ++) {
			JSONObject jsonUser = new JSONObject();
			jsonUser.Add("user_id", "testuser" + i);
			jsonUser.Add("character_type", i);
			jsonUser.Add("level", i * 3);
			userList.Add (jsonUser);
		}
		jsonData.Add ("user_list", userList);

		SocketWrapper.Instance.messageQueue.AddLast (jsonRoomState.ToString());


		while (true) {
			JSONObject json2 = new JSONObject();
			json2.Add("result", 1012);
			json2.Add("winner_id", "testuser1");
			json2.Add("remain_round", 9);
			SocketWrapper.Instance.messageQueue.AddLast(json2.ToString());
			SocketWrapper.Instance.onMessageReceived();
			print (json2);

			yield return new WaitForSeconds(3.0f);
		}
	}

	public override void OnMessageReceived() 
	{
		JSONObject json = JSONObject.Parse (SocketWrapper.Instance.Pop ());
		print (json);
		if (!json.ContainsKey ("result")) {
			return;
		}
		
		int resultCode = (int)json.GetNumber ("result");
		switch (resultCode) {
		case ResultCodes.RESULT_OK_STATE_GAME_ROOM:
			UpdateGameRoom (json);
			break;
		case ResultCodes.RESULT_OK_START_GAME:
			LabelGameStart.GetComponent<Animator> ().Play ("Dismiss");
			break;
		case ResultCodes.RESULT_OK_NOTIFYING_START_GAME:
			LabelGameStart.GetComponent<Animator> ().Play ("Dismiss");
			break;
		case ResultCodes.RESULT_OK_MAKE_QUIZ:
			UpdateQuiz (json);
			break;
		case ResultCodes.RESULT_OK_ROUND_RESULT:
			ShowRoundResult (json);
			break;
		case ResultCodes.RESULT_OK_TOTAL_RESULT:
			ShowTotalResult (json);
			break;
		case ResultCodes.RESULT_OK_REQUEST_ROOM_MEMBER_UPDATE:
			SendRequestRoomMemberUpdate ();
			break;
		case ResultCodes.RESULT_OK_CHAT_MESSAGE:
			ReceiveChatMessage (json);
			break;
		case ResultCodes.RESULT_ERROR_INVALID_CONNECTION:
		default:
			Application.LoadLevel("login");
			break;
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
		JSONObject json = new JSONObject ();
		json.Add ("target", ServerAPITargets.TARGET_GAME_START);
		json.Add ("access_token", SocketWrapper.Instance.accessToken);
		SocketWrapper.Instance.WriteSocket (json.ToString ());

		for (int i = 0; i < Users.Length; i ++) {
			Users[i].ShowMedalIf("");
		}
	}

	void UpdateGameRoom(JSONObject json) {
		JSONObject jsonData = json.GetObject ("data");
		JSONArray jsonUserList = jsonData.GetArray ("user_list");
		for (int i = 0; i < jsonUserList.Length; i ++) {
			JSONObject jsonUser = jsonUserList[i].Obj;
			Users[i].gameObject.SetActive(true);
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
		if (winnerID == "") {
			LabelTitle.text = string.Format ("There isn\'t winner!\n{0} round is left...", remainRound);
		} else {
			LabelTitle.text = string.Format ("{0} is win on this round!\n{1} round is left...", winnerID, remainRound);
			for( int i = 0; i < Users.Length; i ++ ) {
				Users[i].ShowMedalIf(winnerID);
			}
		}
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
