using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;

public class SceneControllerLobby : SceneController {

	public UIGrid GridChattingMessages;
	public LobbyChattingMessage PrefabChattingMessage;
	public UIGrid GridGameRoomList;
	public GameRoomListCell PrefabGameRoomListCell;
	public UIGrid GridConnectedUserList;
	public ConnectedUserListCell PrefabConnectedUserListCell;
	public Transform DialogCreateRoom;
	public Transform DialogBackBlock;
	public UIInput InputRoomTitle;
	public UIInput InputChattingMessage;


	public override void Start ()
	{
		base.Start ();

//		StartCoroutine (ChattingTest ());
//		StartCoroutine (UpdateRoomListTest ());
	}

	public void OnClickCreateRoom() {
		DialogCreateRoom.GetComponent<Animator> ().Play ("Show");
		DialogBackBlock.gameObject.SetActive (true);
	}

	public void OnClickCreateRoom2() {
		JSONObject json = new JSONObject ();
		json.Add ("target", ServerAPITargets.TARGET_CREATE_ROOM);
		json.Add ("access_token", SocketWrapper.Instance.accessToken);
		json.Add ("title", InputRoomTitle.value);
		SocketWrapper.Instance.WriteSocket (json.ToString ());

		DialogCreateRoom.GetComponent<Animator> ().Play ("Dismiss");
		DialogBackBlock.gameObject.SetActive (false);
	}

	public void OnSubmitChattingMessage() {

	}

	IEnumerator ChattingTest() {
		for (int i = 0;; i ++) {
			JSONObject json = new JSONObject ();
			json.Add ("result", ResultCodes.RESULT_OK_CHAT_MESSAGE);
			
			JSONObject jsonMessage = new JSONObject ();
			jsonMessage.Add ("sender_id", "testuser1");
			jsonMessage.Add ("content", string.Format("hello world {0}", i));
			json.Add("message", jsonMessage);
			
			SocketWrapper.Instance.messageQueue.AddLast (json.ToString());
			SocketWrapper.Instance.onMessageReceived();

			yield return new WaitForSeconds(2.0f);
		}
	}

	IEnumerator UpdateRoomListTest() {
		while (true) {
			JSONObject json = new JSONObject();
			json.Add("result", ResultCodes.RESULT_OK_STATE_LOBBY);

			JSONObject jsonData = new JSONObject();
			json.Add("data", jsonData);

			JSONArray roomList = new JSONArray();
			for( int i = 1; i < 4; i ++ ) {
				JSONObject jsonRoom = new JSONObject();
				jsonRoom.Add("room_id", i);
				jsonRoom.Add("title", string.Format("title {0}", i));
				jsonRoom.Add("num_of_member", "1/4");
				jsonRoom.Add("state", 0);
				roomList.Add(jsonRoom);
			}
			jsonData.Add("room_list", roomList);

			SocketWrapper.Instance.messageQueue.AddLast(json.ToString());
			yield return new WaitForSeconds(5.0f);
		}
	}

	public override void OnMessageReceived() 
	{
		JSONObject json = JSONObject.Parse (SocketWrapper.Instance.Pop ());
		if (!json.ContainsKey ("result")) {
			return;
		}

		int resultCode = (int)json.GetNumber ("result");
		if (resultCode == ResultCodes.RESULT_OK_CHAT_MESSAGE) {
			if (json.ContainsKey ("message")) {
				JSONObject jsonMessage = json.GetObject ("message");
				AddChattingMessage (jsonMessage);
			}
		} else if (resultCode == ResultCodes.RESULT_OK_STATE_LOBBY) {
			JSONObject jsonData = json.GetObject ("data");
			UpdateLobby(jsonData);
		} else if (resultCode == ResultCodes.RESULT_OK_JOIN_ROOM) {
			Application.LoadLevel ("game");
		} else if (resultCode == ResultCodes.RESULT_OK_CREATE_ROOM) {
			Application.LoadLevel ("game");
		}
	}

	void AddChattingMessage(JSONObject json) {
		string senderid = json.GetString("sender_id");
		string message = json.GetString("content");
		
		GameObject go = NGUITools.AddChild(GridChattingMessages.gameObject, PrefabChattingMessage.gameObject);
		LobbyChattingMessage comp = go.GetComponent<LobbyChattingMessage>();
		comp.SetSenderID(senderid);
		comp.SetMessage(message);
		
		GridChattingMessages.Reposition();
		if( GridChattingMessages.transform.childCount > 7 ) {
			GridChattingMessages.transform.localPosition += new Vector3(0, 30, 0);
		}
	}

	void UpdateLobby(JSONObject json) {
		UpdateRoomList (json.GetArray ("room_list"));
		UpdateUserList (json.GetArray ("user_list"));
	}

	void UpdateRoomList(JSONArray jsonArray) {
		foreach(Transform child in GridGameRoomList.transform) {
			Destroy(child.gameObject);
		}
		GridGameRoomList.transform.DetachChildren ();

		foreach(JSONValue jsonValue in jsonArray) {
			GameObject clone = NGUITools.AddChild(GridGameRoomList.gameObject, PrefabGameRoomListCell.gameObject);
			clone.GetComponent<GameRoomListCell>().UpdateInformation(jsonValue.Obj);
		}
		GridGameRoomList.Reposition ();
		GridGameRoomList.transform.localPosition = new Vector3 (-338, 141, 0);
	}

	void UpdateUserList(JSONArray jsonArray) {
		foreach(Transform child in GridConnectedUserList.transform) {
			Destroy(child.gameObject);
		}
		GridConnectedUserList.transform.DetachChildren ();
		
		foreach(JSONValue jsonValue in jsonArray) {
			GameObject clone = NGUITools.AddChild(GridConnectedUserList.gameObject, PrefabConnectedUserListCell.gameObject);
			clone.GetComponent<ConnectedUserListCell>().UpdateInformation(jsonValue.Obj);
		}
		GridConnectedUserList.Reposition ();
		GridConnectedUserList.transform.localPosition = new Vector3 (-194, 137, 0);
	}

}
