using UnityEngine;
using System.Collections;
using System.IO;

public class Scoreboard : MonoBehaviour {
	
	public static ArrayList playerList;
	private static ArrayList percantageItems;	
	public static bool GameStarted {
		get{return gameStarted;}		
	}
	private static bool gameStarted = false;	
	private static Scoreboard instance;
	
	public TextMesh HUDItemTemplate;
	public SyncTimer timer;
	
	private TextMesh timeRemaining;	
	private TextMesh ownageHeader;	
	
	private TextMesh winText;
	
	public float victoryPercent = 50f;
	
	
	// Use this for initialization
	void Start () 
	{
		instance = this;
		playerList = new ArrayList();
		percantageItems = new ArrayList();		
	}
	
	// Update is called once per frame
	void Update () 
	{		
		if(gameStarted)
		{
			UpdateScores();
			
			timeRemaining.text = "Time Left: "+timer.TimeLeft.ToString("0.00");
		}		
	}
	
	public static void UpdateScores()
	{
		if(gameStarted)
		{
			if(percantageItems.Count < playerList.Count)
			{
				for(int i = 0; i < (playerList.Count - percantageItems.Count); i++)
				{
					TextMesh item = (TextMesh)Instantiate(instance.HUDItemTemplate, new Vector3(0, 0, 0), Quaternion.identity);
					item.transform.parent = instance.transform;
					item.transform.localEulerAngles = new Vector3(0, 0, 0);
					percantageItems.Add(item);
				}
			}
			
			for(int i = 0; i < playerList.Count ; i++)
			{				
				if((i+1) <= percantageItems.Count)
				{
					((TextMesh)percantageItems[i]).renderer.material.color = ((Player)playerList[i]).PlayerColor;
					((TextMesh)percantageItems[i]).text = ((Player)playerList[i]).PercentageOwned.ToString("0.00") + "%";
					((TextMesh)percantageItems[i]).transform.localPosition = new Vector3(7, 7-i, 0);
					
					//Do a quick victory check
					if(((Player)playerList[i]).PercentageOwned >= instance.victoryPercent)
					{
						//End game.	
						gameStarted = false;
						
						//Show end game message
						if(instance.winText == null)
						{
							instance.winText = (TextMesh)Instantiate(instance.HUDItemTemplate, new Vector3(0, 0, 0), Quaternion.identity);
							instance.winText.transform.parent = instance.transform;
							instance.winText.transform.localEulerAngles = new Vector3(0, 0, 0);
							instance.winText.transform.localPosition = (new Vector3(-4f, 0f, 0f));
							instance.winText.renderer.material.color = ((Player)playerList[i]).PlayerColor;
						}
						if(NetworkingScript.myPlayer == (i+1))
						{
							instance.winText.text = "You won the game!";
						}
						else
						{
							instance.winText.text = "Player "+(i+1) +" won the game!";	
						}
					}
				}
			
			}
			
			
		}
	}
	
	public static void StartGame()
	{
		if(!gameStarted)
		{
			if(instance.timeRemaining == null)
			{
				instance.timeRemaining = (TextMesh)Instantiate(instance.HUDItemTemplate, new Vector3(0, 0, 0), Quaternion.identity);
				instance.timeRemaining.text = "Time Left: ";
				instance.timeRemaining.transform.parent = instance.transform;
				instance.timeRemaining.transform.localPosition = new Vector3(-11f, 8f, 0f);
				instance.timeRemaining.transform.localEulerAngles = new Vector3(0, 0, 0);
			}
			
			if(instance.ownageHeader == null)
			{
				instance.ownageHeader = (TextMesh)Instantiate(instance.HUDItemTemplate, new Vector3(0, 0, 0), Quaternion.identity);
				instance.ownageHeader.text = "Area Owned";
				instance.ownageHeader.transform.parent = instance.transform;
				instance.ownageHeader.transform.localPosition = new Vector3(5f, 8f, 0f);
				instance.ownageHeader.transform.localEulerAngles = new Vector3(0, 0, 0);
			}
			
			gameStarted = true;
			UpdateScores();
			
			if(TextControl.HostGame)
			{
				instance.timer.StartTimer();
			}
		}
		
	}
	
	public static void TimesUp()
	{
		//End game due to time over;
		gameStarted = false;	
		
		//Determine which player has highest score
		float owned = 0f;
		int playerID = -1;
		
		foreach(Player p in playerList)
		{
			if(p.PercentageOwned > owned)
			{
				owned = p.PercentageOwned;
				playerID = p.playerNumber;
			}
		}
		
		//Show end game message
		if(instance.winText == null)
		{
			instance.winText = (TextMesh)Instantiate(instance.HUDItemTemplate, new Vector3(0, 0, 0), Quaternion.identity);
			instance.winText.transform.parent = instance.transform;
			instance.winText.transform.localEulerAngles = new Vector3(0, 0, 0);
			instance.winText.transform.localPosition = (new Vector3(-6f, 0f, 0f));
			instance.winText.renderer.material.color = ((Player)playerList[playerID - 1]).PlayerColor;
		}
		
		if(NetworkingScript.myPlayer == playerID)
		{
			instance.winText.text = "Times up! You won the game!!";
		}
		else if(playerID == -1)
        {
			instance.winText.text = "Tie! Everyone fails!";
		}
		else
		{
			instance.winText.text = "Times up! Player "+playerID +" won the game!";	
		}
		
	}
	
	public static void Reset()
	{
		playerList.Clear();
		percantageItems.Clear();
		instance.timer.ResetTimer();		
	}
}
