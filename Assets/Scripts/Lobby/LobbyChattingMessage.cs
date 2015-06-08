using UnityEngine;
using System.Collections;

public class LobbyChattingMessage : MonoBehaviour {

	public UILabel LabelSenderID;
	public UILabel LabelMessage;

	public void SetSenderID(string senderid) {
		LabelSenderID.text = senderid;
	}

	public void SetMessage(string message) {
		LabelMessage.text = message;
	}

}
