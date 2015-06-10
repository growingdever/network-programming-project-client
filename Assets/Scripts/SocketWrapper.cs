using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.IO;
using System;
using System.Collections.Generic;

public class SocketWrapper : MonoBehaviour
{
	private static SocketWrapper instance;
	private static GameObject container;

	public static SocketWrapper Instance 
	{  
		get {
			if (!instance) {  
				container = new GameObject ();  
				container.name = "SocketWrapper";  
				instance = container.AddComponent (typeof(SocketWrapper)) as SocketWrapper;  
				DontDestroyOnLoad(container);
				Application.runInBackground = true;
			}  
			return instance;  
		}
	}
	
	TcpClient mySocket;
	NetworkStream theStream;
	StreamWriter theWriter;
	StreamReader theReader;
	String Host = "127.0.0.1";
	Int32 Port = 10101;
	internal Boolean socketReady = false;

	string _accessToken = "";
	public string accessToken {
		get;
		set;
	}

	public LinkedList<string> messageQueue {
		get;
		private set;
	}

	public Action onMessageReceived {
		get;
		set;
	}

	void Awake ()
	{
		SetUpSocket ();

		messageQueue = new LinkedList<string> ();

		StartCoroutine (CheckSocket ());
	}

	IEnumerator CheckSocket() {
		while (true) {
			if (socketReady && theStream.DataAvailable) {
				string read = ReadSocket ();
				messageQueue.AddLast(read);
				onMessageReceived();
			}

			yield return new WaitForFixedUpdate();
		}
	}

	void Update ()
	{

	}

	public string Pop() {
		string first = messageQueue.First.Value;
		messageQueue.RemoveFirst ();
		return first;
	}

	void SetUpSocket ()
	{
		try {
			mySocket = new TcpClient (Host, Port);
			theStream = mySocket.GetStream ();
			theWriter = new StreamWriter (theStream);
			theReader = new StreamReader (theStream);
			socketReady = true;
			Debug.Log("Socket connected");
		} catch (Exception e) {
			Debug.LogError ("Socket error: " + e);
		}
	}

	public void WriteSocket (string theLine)
	{
		if (!socketReady) {
			return;
		}

		string foo = theLine + "\r\n";
		theWriter.Write (foo);
		theWriter.Flush ();
	}

	public String ReadSocket ()
	{
		if (!socketReady) {
			return "";
		}
		if (theStream.DataAvailable) {
			return theReader.ReadLine ();
		}

		return "";
	}

	public void CloseSocket ()
	{
		if (!socketReady) {
			return;
		}

		theWriter.Close ();
		theReader.Close ();
		mySocket.Close ();
		socketReady = false;
	}
}
