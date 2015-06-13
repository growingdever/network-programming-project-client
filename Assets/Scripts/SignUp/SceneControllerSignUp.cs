using UnityEngine;
using System.Collections;
using Boomlagoon.JSON;

public class SceneControllerSignUp : SceneController {

	public UIInput InputID;
	public UIInput InputPassword;
	public GameObject AuraEffect;

	public int characterType {
		get;
		set;
	}


	public override void Start ()
	{
		base.Start ();

		characterType = 1;
	}

	public void OnClickSignUp() 
	{
		JSONObject json = new JSONObject ();
		json.Add ("target", ServerAPITargets.TARGET_SIGNUP);
		json.Add ("id", InputID.value);
		json.Add ("password", InputPassword.value);
		json.Add ("character_type", characterType);

		print (json.ToString ());
		SocketWrapper.Instance.WriteSocket (json.ToString());
	}

	public void OnClickBack() 
	{
		Application.LoadLevel ("login");
	}

	public override void OnMessageReceived() 
	{
		string result = SocketWrapper.Instance.Pop ();
		JSONObject json = JSONObject.Parse(result);
		if (!json.ContainsKey ("result")) {
			Debug.LogError("not exist result code");
			return;
		}

		if( (int)json.GetNumber("result") == ResultCodes.RESULT_OK_SIGN_UP ) {
			Application.LoadLevel("login");
		}
	}

}
