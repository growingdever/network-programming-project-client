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
	public UIInput InputRoomTitle;
	public UIInput InputChattingMessage;


	public override void Start ()
	{
		base.Start ();

//		StartCoroutine (ChattingTest ());
//		StartCoroutine (UpdateRoomListTest ());

		RequestCheckingLobby ();
	}

	void RequestCheckingLobby() {
		JSONObject json = new JSONObject ();
		json.Add ("target", ServerAPITargets.TARGET_CHECK_LOBBY);
		json.Add ("access_token", SocketWrapper.Instance.accessToken);
		SocketWrapper.Instance.WriteSocket (json.ToString ());
	}

	public void OnClickCreateRoom() {
		DialogCreateRoom.GetComponent<Animator> ().Play ("Show");
	}

	public void OnClickCreateRoom2() {
		JSONObject json = new JSONObject ();
		json.Add ("target", ServerAPITargets.TARGET_CREATE_ROOM);
		json.Add ("access_token", SocketWrapper.Instance.accessToken);
		json.Add ("title", InputRoomTitle.value);
		SocketWrapper.Instance.WriteSocket (json.ToString ());

		DialogCreateRoom.GetComponent<Animator> ().Play ("Dismiss");
	}

	public void OnSubmitChattingMessage() {
		JSONObject json = new JSONObject ();
		json.Add ("target", ServerAPITargets.TARGET_CHATTING);
		json.Add ("access_token", SocketWrapper.Instance.accessToken);
		json.Add ("message", InputChattingMessage.value);
		SocketWrapper.Instance.WriteSocket (json.ToString ());

		InputChattingMessage.value = "";
	}

	public override void OnMessageReceived() 
	{
		JSONObject json = JSONObject.Parse (SocketWrapper.Instance.Pop ());
		print (json);

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
			UpdateLobby (jsonData);
		} else if (resultCode == ResultCodes.RESULT_OK_JOIN_ROOM) {
			Application.LoadLevel ("game");
		} else if (resultCode == ResultCodes.RESULT_OK_CREATE_ROOM) {
		} else if (resultCode == ResultCodes.RESULT_OK_REQUEST_LOBBY_UPDATE) {
			RequestCheckingLobby();
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
