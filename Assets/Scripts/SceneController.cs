using UnityEngine;
using System.Collections;

public class SceneController : MonoBehaviour
{
	public virtual void Awake ()
	{
	}

	public virtual void Start ()
	{
		SocketWrapper.Instance.onMessageReceived = this.OnMessageReceived;
	}

	public virtual void Update ()
	{
	}

	public virtual void OnMessageReceived()
	{
	}
}
