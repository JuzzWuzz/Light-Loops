using UnityEngine;
using System.Collections;

public class NetworkingScript : MonoBehaviour {
	public static int myPlayer;
	
	public static string host = "127.0.0.1";
	
	public SmoothLookAt		mainCamera;
	public Player player;
	
	float startTimer = 0;
	bool firstRun = true;
	
	
	// Use this for initialization
	void Start()
	{		
		Network.sendRate = 40;
		if(TextControl.HostGame)
		{
			myPlayer = 1;			
			Network.InitializeServer(4, 22000, false);	
		}
		else
		{
			myPlayer = 2;
			Network.Connect(host, 22000);
			
		}
			
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(startTimer < 2)
		{
			startTimer += Time.deltaTime;
		}
		else
		{
			if(firstRun)
			{
				firstRun = false;
				
				float maxPoint = GenerateBlocks.gridSize * GenerateBlocks.cubeSpacing;
				Vector3 position = new Vector3(Random.value * maxPoint, Random.value * maxPoint, 195);
				
				if(TextControl.HostGame)
				{
					player.playerNumber = 1;
				}
				else
				{
					player.playerNumber = 2;
				}
				
				Player playerInstantiation = (Player)Network.Instantiate(player, position, Quaternion.identity, 0);				
				mainCamera.target = playerInstantiation.transform;
			}
		}
	
	}
}
