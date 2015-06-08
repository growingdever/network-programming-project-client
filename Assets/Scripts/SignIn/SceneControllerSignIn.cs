using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;

public class SceneControllerSignIn : SceneController {

	public UIInput InputID;
	public UIInput InputPassword;


	public void OnClickSignIn() {
		JSONObject json = new JSONObject ();
		json.Add ("target", 1);
		json.Add ("id", InputID.value);
		json.Add ("password", InputPassword.value);

		SocketWrapper.Instance.WriteSocket (json.ToString());
	}

	void Update() {
		if (SocketWrapper.Instance.messageQueue.Count > 0) {
			string result = SocketWrapper.Instance.Pop ();
			print (result);
		}
	}

}
