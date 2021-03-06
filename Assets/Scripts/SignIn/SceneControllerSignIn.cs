﻿using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;

public class SceneControllerSignIn : SceneController {

	public UIInput InputID;
	public UIInput InputPassword;


	public override void Start ()
	{
		base.Start ();
	}

	public void OnClickSignIn() 
	{
		JSONObject json = new JSONObject ();
		json.Add ("target", 1);
		json.Add ("id", InputID.value);
		json.Add ("password", InputPassword.value);

		SocketWrapper.Instance.WriteSocket (json.ToString());
	}

	public void OnClickSignUp() 
	{
		Application.LoadLevel ("signup");
	}

	public override void OnMessageReceived() 
	{
		string result = SocketWrapper.Instance.Pop ();
		JSONObject json = JSONObject.Parse(result);
		if (!json.ContainsKey ("result")) {
			Debug.LogError("not exist result code");
			return;
		}

		if( (int)json.GetNumber("result") == ResultCodes.RESULT_OK_SIGN_IN ) {
			SocketWrapper.Instance.accessToken = json.GetString("access_token");
			Application.LoadLevel("lobby");
		}
	}

}
