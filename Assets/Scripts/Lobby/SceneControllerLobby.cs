using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;

public class SceneControllerLobby : SceneController {

	public override void Start ()
	{
		base.Start ();
	}

	public override void OnMessageReceived() 
	{
		JSONObject json = JSONObject.Parse (SocketWrapper.Instance.Pop ());
//		int resultCode = (int)json.GetNumber ("result");

		print (json);
	}

}
