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
	private bool			m_UndoCapture;
	
	// Use this for initialization
	void Start ()
	{
		m_LastHitBlock = null;
		m_ProcessedBlocks = new ArrayList();
		m_UndoList = new ArrayList();
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
				m_PlayerColor = Color.yellow;
				renderer.material.color = m_PlayerColor;
			break;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(playerNumber == NetworkingScript.myPlayer)
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
	
	
		
	// Perform a capture of the blocks
	public void PerformCapture()
	{
		// Reset the processed blocks and undo lists
		m_ProcessedBlocks.Clear();
		m_UndoList.Clear();
		
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
	}
}