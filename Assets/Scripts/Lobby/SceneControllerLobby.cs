using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;

public class SceneControllerLobby : SceneController {

	public UIGrid GridChattingMessages;
	public LobbyChattingMessage PrefabChattingMessage;

	public override void Start ()
	{
		base.Start ();

		StartCoroutine (ChattingTest ());
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

	public override void OnMessageReceived() 
	{
		JSONObject json = JSONObject.Parse (SocketWrapper.Instance.Pop ());
		if (!json.ContainsKey ("result")) {
			return;
		}

		int resultCode = (int)json.GetNumber ("result");
		if (resultCode == ResultCodes.RESULT_OK_CHAT_MESSAGE) {
			if (json.ContainsKey ("message")) {
				JSONObject jsonMessage = json.GetObject("message");
				string senderid = jsonMessage.GetString("sender_id");
				string message = jsonMessage.GetString("content");
				
				GameObject go = NGUITools.AddChild(GridChattingMessages.gameObject, PrefabChattingMessage.gameObject);
				LobbyChattingMessage comp = go.GetComponent<LobbyChattingMessage>();
				comp.SetSenderID(senderid);
				comp.SetMessage(message);

				GridChattingMessages.Reposition();
				if( GridChattingMessages.transform.childCount > 7 ) {
					GridChattingMessages.transform.localPosition += new Vector3(0, 30, 0);
				}
			}
		}
	}

}
