using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;

public class SceneControllerGame : SceneController {

	public UIInput InputChattingMessage;
	public UILabel LabelTitle;
	public UIButton ButtonStartGame;
	public UIButton ButtonLeaveGame;
	public UILabel LabelTimer;
	public User[] Users;
	int numOfUser;


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

		SendRequestRoomMemberUpdate ();
	}

//	IEnumerator TestRoundWinner() {
//		JSONObject jsonRoomState = new JSONObject();
//		jsonRoomState.Add("result", 1015);
//
//		JSONObject jsonData = new JSONObject ();
//		jsonData.Add ("room_id", 1);
//		jsonRoomState.Add ("data", jsonData);
//
//		JSONArray userList = new JSONArray ();
//		for (int i = 1; i <= 2; i ++) {
//			JSONObject jsonUser = new JSONObject();
//			jsonUser.Add("user_id", "testuser" + i);
//			jsonUser.Add("character_type", i);
//			jsonUser.Add("level", i * 3);
//			userList.Add (jsonUser);
//		}
//		jsonData.Add ("user_list", userList);
//
//		SocketWrapper.Instance.messageQueue.AddLast (jsonRoomState.ToString());
//		SocketWrapper.Instance.onMessageReceived ();
//
//		JSONObject jsonGameStart = new JSONObject ();
//		jsonGameStart.Add ("result", ResultCodes.RESULT_OK_NOTIFYING_START_GAME);
//		SocketWrapper.Instance.messageQueue.AddLast (jsonGameStart.ToString());
//		SocketWrapper.Instance.onMessageReceived ();
//
//
//		for( int i = 0; i < 5; i ++ ) {
//			JSONObject json;
//
//
//			json = GetTestJSONRoundStart();
//			SocketWrapper.Instance.messageQueue.AddLast(json.ToString());
//			SocketWrapper.Instance.onMessageReceived();
//			yield return new WaitForSeconds(5.0f);
//
//
//			json = GetTestJSONRoundResult();
//			SocketWrapper.Instance.messageQueue.AddLast(json.ToString());
//			SocketWrapper.Instance.onMessageReceived();
//			yield return new WaitForSeconds(3.0f);
//		}
//	}
//
//	JSONObject GetTestJSONRoundStart() {
//		JSONObject json = new JSONObject();
//		json.Add("result", ResultCodes.RESULT_OK_MAKE_QUIZ);
//		json.Add("quiz_string", "test test");
//		json.Add("time", 5.0f);
//		
//		return json;
//	}
//
//	JSONObject GetTestJSONRoundResult() {
//		JSONObject json = new JSONObject();
//		json.Add("result", ResultCodes.RESULT_OK_ROUND_RESULT);
//		json.Add("winner_id", "testuser" + Random.Range(1, 3));
//		json.Add("remain_round", 9);
//
//		return json;
//	}

	public override void OnMessageReceived() 
	{
		JSONObject json = JSONObject.Parse (SocketWrapper.Instance.Pop ());
		if (!json.ContainsKey ("result")) {
			return;
		}
		
		int resultCode = (int)json.GetNumber ("result");
		switch (resultCode) {
		case ResultCodes.RESULT_OK_STATE_GAME_ROOM:
			UpdateGameRoom (json);
			break;
		case ResultCodes.RESULT_OK_START_GAME:
			break;
		case ResultCodes.RESULT_OK_NOTIFYING_START_GAME:
			StartGame ();
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
		case ResultCodes.RESULT_OK_LEAVE_ROOM:
			LeaveGameRoom();
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
	}

	public void OnClickLeaveGame() {
		JSONObject json = new JSONObject ();
		json.Add ("target", ServerAPITargets.TARGET_LEAVE_GAME_ROOM);
		json.Add ("access_token", SocketWrapper.Instance.accessToken);
		SocketWrapper.Instance.WriteSocket (json.ToString ());
	}

	void StartGame() {
		ButtonStartGame.GetComponent<Animator> ().Play ("Dismiss");
		ButtonLeaveGame.GetComponent<Animator> ().Play ("Dismiss");

		LabelTitle.text = "Game is starting soon!";
		LabelTimer.GetComponent<Animator> ().Play ("TimerShow");
		LabelTimer.text = "00";
	}

	void LeaveGameRoom() {
		SocketWrapper.Instance.onMessageReceived = null;

		ButtonStartGame.GetComponent<Animator> ().Play ("Dismiss");
		ButtonLeaveGame.GetComponent<Animator> ().Play ("Dismiss");
		Invoke ("LeaveGameRoom2", 1.5f);
	}

	void LeaveGameRoom2() {
		Application.LoadLevel ("lobby");
	}

	void UpdateGameRoom(JSONObject json) {
		JSONObject jsonData = json.GetObject ("data");
		JSONArray jsonUserList = jsonData.GetArray ("user_list");
		numOfUser = jsonUserList.Length;
		for (int i = 0; i < numOfUser; i ++) {
			JSONObject jsonUser = jsonUserList[i].Obj;
			Users[i].gameObject.SetActive(true);
			Users[i].Init(jsonUser);
		}
		for (int i = numOfUser; i < 4; i ++) {
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

		int time = (int)json.GetNumber ("time") / 1000;
		StartCoroutine (TimerStart (time));
	}

	IEnumerator TimerStart(int time) {
		for( int i = time; i >= 0; i -- ) {
			LabelTimer.text = string.Format ("{0}", i);
			yield return new WaitForSeconds(1.0f);
		}
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

		LabelTimer.GetComponent<Animator> ().Play ("TimerDismiss");

		StartCoroutine (InitTitleWithDelay (3.0f));

		SendRequestRoomMemberUpdate ();
	}

	IEnumerator InitTitleWithDelay(float delay) {
		yield return new WaitForSeconds (delay);
		LabelTitle.text = "Let\'s start a game...";

		ButtonStartGame.GetComponent<Animator> ().Play ("Show");
		ButtonLeaveGame.GetComponent<Animator> ().Play ("Show");
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
