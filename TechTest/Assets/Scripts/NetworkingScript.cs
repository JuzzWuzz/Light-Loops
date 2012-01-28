using UnityEngine;
using System.Collections;

public class NetworkingScript : MonoBehaviour {
	public static int myPlayer;
	
	public Transform		player1;
	public Transform		player2;
	public SmoothLookAt		mainCamera;
	
	
	// Use this for initialization
	void Start()
	{		
		Network.sendRate = 40;
		if(TextControl.HostGame)
		{
			myPlayer = 1;
			Network.InitializeServer(4, 22000, false);
			
			mainCamera.target = player1;
		}
		else
		{
			myPlayer = 2;
			Network.Connect("127.0.0.1", 22000);
			mainCamera.target = player2;
		}
			
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
