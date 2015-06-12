using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;

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
	StringBuilder stringBuilder;
	char[] buffer;
	
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

	public bool loggingRead {
		get;
		set;
	}

	void Awake ()
	{
		stringBuilder = new StringBuilder ();
		buffer = new char[1024];
		messageQueue = new LinkedList<string> ();
		SetUpSocket ();
	}

	void Update ()
	{
		if (socketReady && theStream.DataAvailable) {
			ReadSocket ();
		}

		if (stringBuilder.Length > 0) {
			string content = stringBuilder.ToString();
			int index = content.IndexOf("\r\n");
			if( index != -1 ) {
				string line = content.Substring(0, index);
				if( line.Length > 0 ) {
					messageQueue.AddLast(line);
					if( loggingRead ) {
						print (line.Length + "\n" + line);
					}
				}

				stringBuilder = new StringBuilder(content.Substring(index + 2));
			}
		}

		int count = messageQueue.Count;
		for (int i = 0; i < count; i ++) {
			if( onMessageReceived != null ) {
				onMessageReceived();
			}
		}
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
			Application.LoadLevel("login");
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

	public void ReadSocket ()
	{
		if (!socketReady) {
			return;
		}

		if (theStream.DataAvailable) {
			Array.Clear(buffer, 0, buffer.Length);
			int readBytes = theReader.Read(buffer, 0, buffer.Length);
			if( readBytes > 0 ) {
				stringBuilder.Append(new string(buffer, 0, readBytes));
			}
		}
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
