using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;

public class SceneControllerLobby : SceneController {

	public UIGrid GridChattingMessages;
	public LobbyChattingMessage PrefabChattingMessage;
	public UIGrid GridGameRoomList;
	public GameRoomListCell PrefabGameRoomListCell;
	public UIInput InputRoomTitle;


	public override void Start ()
	{
		base.Start ();

//		StartCoroutine (ChattingTest ());
//		StartCoroutine (UpdateRoomListTest ());
	}

	public void OnClickCreateRoom() {

	}

	public void OnClickCreateRoom2() {
		JSONObject json = new JSONObject ();
		json.Add ("target", ServerAPITargets.TARGET_CREATE_ROOM);
		json.Add ("access_token", SocketWrapper.Instance.accessToken);
		json.Add ("title", InputRoomTitle.value);

		print (json.ToString ());
		SocketWrapper.Instance.WriteSocket (json.ToString ());
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
			UpdateRoomList (jsonData.GetArray ("room_list"));
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

	void UpdateRoomList(JSONArray jsonRoomArray) {
		foreach(Transform child in GridGameRoomList.transform) {
			Destroy(child.gameObject);
		}
		GridGameRoomList.transform.DetachChildren ();

		foreach(JSONValue jsonValue in jsonRoomArray) {
			GameObject clone = NGUITools.AddChild(GridGameRoomList.gameObject, PrefabGameRoomListCell.gameObject);
			clone.GetComponent<GameRoomListCell>().UpdateInformation(jsonValue.Obj);
		}
		GridGameRoomList.Reposition ();
		GridGameRoomList.transform.localPosition = new Vector3 (-395, 106, 0);
	}

}
