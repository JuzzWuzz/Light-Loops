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
	private Queue			m_FloodFillQueue;
	private bool			m_UndoCapture;	
	
	private bool 			killed = false;
	private float 			timer = 0f;
	
	public float 			respawnTime = 5f;
	
	// Use this for initialization
	void Start ()
	{		
		m_LastHitBlock		= null;
		m_ProcessedBlocks	= new ArrayList();
		m_UndoList			= new ArrayList();
		m_KillPlayers		= new ArrayList();
		m_FloodFillQueue	= new Queue();
		m_UndoCapture		= false;
		
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
		m_LastHitBlock.ChainNukeBlocks();
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
	public void PerformCapture(BuildingBlock first)
	{
		// Reset the processed blocks and undo lists
		m_ProcessedBlocks.Clear();
		m_UndoList.Clear();
		m_KillPlayers.Clear();
		m_FloodFillQueue.Clear();
		
		if (m_LastHitBlock != null)
			m_LastHitBlock.ChainCaptureBlocks();
		
		m_LastHitBlock.NextBlock	= first;
		first.PrevBlock				= m_LastHitBlock;
		
		m_LastHitBlock = null;
		
		// Ask for a flood fill operation
		Debug.Log("Before Flood-Fill");
		FloodFillInit();
		FloodFill();
		Debug.Log("After Flood-Fill");
		
		// Loop over all processed blocks to reset their variables
		foreach (BuildingBlock block in m_ProcessedBlocks)
		{
			// If need to undo a capture call the method here
			if (m_UndoCapture)
			{
				block.UndoCapture();
			}
			block.DirectionCameFrom	= 0;
			block.Visited			= false;
			block.Captured			= false;
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

	void FloodFillInit()
	{
		// Variables
		BuildingBlock biggest= GenerateBlocks.blocks[GenerateBlocks.gridSize - 1, GenerateBlocks.gridSize - 1];
		BuildingBlock block1 = GenerateBlocks.blocks[0,0];
		BuildingBlock block2 = null;
		BuildingBlock block3 = null;
		
		bool anticlockwise = false;
		int count = 0;
		while (true)
		{
			count++;
			// Set block2 to be the highest top corner block
			block2 = biggest;
			
			// Loop through all the boundary blocks to find the minimum block that occurs progressivly more right than block1
			foreach (BuildingBlock block in m_ProcessedBlocks)
			{
				if (block.GridIndex.x < block2.GridIndex.x)
				{
					if (block.GridIndex.x > block1.GridIndex.x)
						block2 = block;
				}
				else if (block.GridIndex.x == block2.GridIndex.x)
				{
					if (block.GridIndex.y < block2.GridIndex.y)
						block2 = block;
				}
			}
			
			// Update the block values then move into the next iteration
			block3 = block1;
			block1 = block2;
			
			block3.m_OverrideColor = Color.red;
			block3.m_MustOverrideColor = true;
			
			block1.m_OverrideColor = Color.cyan;
			block1.m_MustOverrideColor = true;
			
			// Check to see which direction!
			if (block3.NextBlock == block1)
			{
				// Use the next block order!
				anticlockwise = true;
				break;
			}
			else if (block3.PrevBlock == block1)
			{
				// Use the prev block order!
				anticlockwise = false;
				break;
			}
			else
			{
				// Keep iterating!
			}
		}
		Debug.Log("Took " + count + " iterations to solve!");
		
		
		BuildingBlock first		= block3;
		BuildingBlock current	= first;
		BuildingBlock next		= current.NextBlock;
		
		// Loop over all of the next blocks
		Debug.Log("Blocks to process: " + m_ProcessedBlocks.Count);
		while (!current.Visited)
		{
			AddCandidate(current.Direction(next), (int)current.GridIndex.x, (int)current.GridIndex.y, anticlockwise);
			
			current.Visited = true;
			
			current	= next;
			next	= current.NextBlock;
		}
		Debug.Log("Calculated flood fill nodes!");
	}
	
	
	// Adds a candidate based on the direction moving, current x & y and the rotation direction
	void AddCandidate(int direction, int x, int y, bool anticlockwise)
	{
		// This is a flip for the direction
		int flip = 1;
		if (!anticlockwise)
			flip *= -1;
		
		BuildingBlock candidate = null;
		
		// Determine where to get the candidate
		switch (direction)
		{
		case 0:
			candidate = GenerateBlocks.blocks[x, y + flip];
			break;
			
		case 1:
			candidate = GenerateBlocks.blocks[x - flip, y];
			break;
			
		case 2:
			candidate = GenerateBlocks.blocks[x, y - flip];
			break;
			
		case 3:
			candidate = GenerateBlocks.blocks[x + flip, y];
			break;
			
		default:
			break;
		}
		
		// Add the candidate to the queue
		if (!candidate.Captured)
			m_FloodFillQueue.Enqueue(candidate);
	}
	
	// Perform the flood fill on the queue generated!
	void FloodFill()
	{
		int x;
		int y;
		
		while (m_FloodFillQueue.Count > 0)
		{
			BuildingBlock block = (BuildingBlock)m_FloodFillQueue.Dequeue();
			
			// Check if block has already been captured!
			if (!block.Captured)
			{
				x = (int)block.GridIndex.x;
				y = (int)block.GridIndex.y;
				
				// Check each of the 4 directions!
				// Check the top
				AddToQueueIfValid(x, y + 1);
				// Check the right
				AddToQueueIfValid(x + 1, y);
				// Check the bottom
				AddToQueueIfValid(x, y + 1);
				// Check the left
				AddToQueueIfValid(x - 1, y);
				
				// Capture the current block
				block.CaptureSpecificBlock(this);
				block.m_MustOverrideColor = true;
			}
		}
	}
	
	void AddToQueueIfValid(int x, int y)
	{
		// If the parameters are in bounds then get the object there
		if (x >= 0 && x < GenerateBlocks.gridSize && y >= 0 && y < GenerateBlocks.gridSize)
		{
			BuildingBlock block = GenerateBlocks.blocks[x,y];
			// If the block is uncaptured then add to the flood fill queue
			if (!block.Captured)
			{
				m_FloodFillQueue.Enqueue(block);
			}
		}
	}
}