using UnityEngine;
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

	public override void OnMessageReceived() 
	{
		string result = SocketWrapper.Instance.Pop ();
		
		JSONObject json = JSONObject.Parse(result);
		if( (int)json.GetNumber("result") == 1001 ) {
			SocketWrapper.Instance.accessToken = json.GetString("access_token");
		}
		
		print (result);
		print (SocketWrapper.Instance.accessToken);
	}

}
