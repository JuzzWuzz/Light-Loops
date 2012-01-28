using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	// Variables
	public static int numberOfPlayers = 0;
	public int				playerNumber;
	private BuildingBlock	m_LastHitBlock;
	private Color			m_PlayerColor;
	
	private ArrayList		m_ProcessedBlocks;
	private ArrayList		m_UndoList;
	private ArrayList		m_KillPlayers;
	private bool			m_UndoCapture;	
	
	private bool 			killed = false;
	private float 			timer = 0f;	
	
	public float 			respawnTime = 5f;
	
	// Use this for initialization
	void Start ()
	{		
		m_LastHitBlock = null;
		m_ProcessedBlocks = new ArrayList();
		m_UndoList = new ArrayList();
		m_KillPlayers = new ArrayList();
		m_UndoCapture = false;
		
		numberOfPlayers++;
		playerNumber = numberOfPlayers;
		switch(playerNumber)
		{
		case 1:
				m_PlayerColor = Color.blue;
				renderer.material.color = m_PlayerColor;
			break;
		case 2:
				m_PlayerColor = Color.red;
				renderer.material.color = m_PlayerColor;
			break;
		case 3: 
				m_PlayerColor = Color.green;
				renderer.material.color = m_PlayerColor;
			break;
		case 4:
				m_PlayerColor = Color.magenta;
				renderer.material.color = m_PlayerColor;
			break;
		case 5:
				m_PlayerColor = Color.white;
				renderer.material.color = m_PlayerColor;
			break;
		case 6:
				m_PlayerColor = Color.cyan;
				renderer.material.color = m_PlayerColor;
			break;
			
		}
		
		//If the this is a joining player, reset our world.		
		if(playerNumber != NetworkingScript.myPlayer)
		{
			NetworkingScript.localPlayer.KillPlayer();
			for (int y = 0; y < GenerateBlocks.blocks.GetLength(0); y++)
			{			
				for (int x = 0; x < GenerateBlocks.blocks.GetLength(1); x++)
				{
					GenerateBlocks.blocks[x ,y].Reset();
				}
			}
		}
				
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(playerNumber == NetworkingScript.myPlayer)
		{
			if(!killed)
			{
				transform.Translate(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
				if(transform.position.x < 0)
				{				
					transform.position = new Vector3(0, transform.position.y, transform.position.z);
				}
				float max =  GenerateBlocks.cubeSpacing * (GenerateBlocks.gridSize - 1);
				if(transform.position.x > max)
				{
					transform.position = new Vector3(max, transform.position.y, transform.position.z);
				}
				if(transform.position.y < 0)
				{
					transform.position = new Vector3(transform.position.x, 0, transform.position.z);
				}
				if(transform.position.y > max)
				{
					transform.position = new Vector3(transform.position.x, max, transform.position.z);
				}
			}
			else
			{
				timer += Time.deltaTime;
				if(timer > respawnTime)
				{
					Respawn();	
				}
			}
			
		}
	}
	
	/// <summary>
	/// Sends a command for the player to die, wait a while, and then respawn.
	/// </summary>
	public void KillPlayer()
	{
		transform.position = new Vector3(transform.position.x, transform.position.y, 215);
		timer = 0f;
		killed = true;
	}
	
	/// <summary>
	/// Respawns the player in a non-lvl 3 block on the map.
	/// </summary>
	private void Respawn()
	{		
		Vector3 position = new Vector3();
		int x, y;
		float maxPoint = GenerateBlocks.gridSize * GenerateBlocks.cubeSpacing;
		
		
		while(true)
		{
			position = new Vector3(Random.value * maxPoint, Random.value * maxPoint, 195);			
			x = (int) (position.x / GenerateBlocks.cubeSpacing);
			y = (int) (position.y / GenerateBlocks.cubeSpacing);			
			
			if(GenerateBlocks.blocks[x, y].Level != GenerateBlocks.numberOfLevels)
			{
				break;
			}
		}
					
		transform.position = position;
		killed = false;
	}
	
	// Accessor for the players color
	public Color PlayerColor
	{
		get { return(m_PlayerColor); }
	}
	
	// Accessor/Mutator for the last hit block by the player
	public BuildingBlock LastHitBlock
	{
		get { return(m_LastHitBlock); }
		set { m_LastHitBlock = value; }
	}
	
	// Push block to processed list!
	public void ProcessBlock(BuildingBlock block)
	{
		m_ProcessedBlocks.Add(block);
	}
	
		// Undo Capture
	public void UndoCapture(BuildingBlock block)
	{
		m_UndoCapture = true;
		m_UndoList.Add(block);
	}
	
	// Store a kill that might need to be imposed!
	public void KillEnemyPlayer(Player enemy)
	{
		m_KillPlayers.Add(enemy);
	}
	
	
		
	// Perform a capture of the blocks
	public void PerformCapture()
	{
		// Reset the processed blocks and undo lists
		m_ProcessedBlocks.Clear();
		m_UndoList.Clear();
		m_KillPlayers.Clear();
		
		if (m_LastHitBlock != null)
			m_LastHitBlock.ChainCaptureBlocks();
		m_LastHitBlock = null;
		
		for (int y = 0; y < GenerateBlocks.blocks.GetLength(0); y++)
		{
			bool on = false;
			for (int x = 0; x < GenerateBlocks.blocks.GetLength(1); x++)
			{
				
				BuildingBlock block = GenerateBlocks.blocks[x,y];
				
				if (block.DirectionCameFrom != 0)
				{
					if (block.DirectionCameFrom == 2 || block.DirectionCameFrom == 3)
						on = !on;
				}
				else
				{
					if (on)
						block.CaptureSpecificBlock(this);
				}
			}
		}
		
		// Loop over all processed blocks to reset their variables
		foreach (BuildingBlock block in m_ProcessedBlocks)
		{
			// If need to undo a capture call the method here
			if (m_UndoCapture)
			{
				block.UndoCapture();
			}
			block.DirectionCameFrom = 0;
		}
		
		// Kill off the enemies entrapped if you can succcessfully capture the territory
		if (!m_UndoCapture)
		{
			foreach (Player enemy in m_KillPlayers)
			{
				enemy.KillPlayer();
			}
		}
	}
}