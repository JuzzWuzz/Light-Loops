using UnityEngine;
using System.Collections;

public class BuildingBlock : MonoBehaviour {
	
	public float			distanceToMove;
	public float			RateToMove;
	
	private Vector3			m_DefaultPos;
	private Color			m_DefaultColor;
	private Color			m_FinalColor;
	
	private bool			m_MustRaise;
	private bool			m_MustLower;
	private bool			m_IsMoving;
	private int				m_HeightLevel; // 0 = base, 1 = trail, 2 = max height
	
	public bool				m_MustOverrideColor;
	public Color			m_OverrideColor;
	
	private int				m_Level;
	
	// Direction used for filling algorithm {0 = invalid (null), 1 = horizontal (left/right), 2 = vertical (up/down), 3 = diagonal}
	private int				m_DirectionCameFrom;
	
	private Vector2			m_GridIndex;
	
	// Controls for chaining
	private BuildingBlock	m_PrevBlock;
	private BuildingBlock	m_NextBlock;
	private Player			m_TrailOwner;
	private Player			m_Owner;
	private Player			m_PlayerPresent;
	
	private bool			m_Visited;
	private bool			m_Captured;
	
	// Color controls
	private Color			m_OwnerColor;
	private Color			m_TrailColor;
	
	// Backup vars for undo of capture
	private int				m_LevelBackup;
	private Player			m_OwnerBackup;
	
	
	private static int stacklimit = 0;
	
	// Use this for initialization
	void Start() 
	{
		m_DefaultColor	= Color.grey;
		m_FinalColor	= m_DefaultColor;
		this.renderer.material.color = m_DefaultColor;
		
		m_OverrideColor	= Color.yellow;
		
		Reset();
	}
	
	// Reset the blocks variables
	public void Reset()
	{
		m_MustLower 		= false;
		m_MustRaise			= false;
		m_IsMoving			= false;
		m_HeightLevel		= 0;
		
		m_MustOverrideColor	= false;
		
		m_Level				= 0;
		
		m_DirectionCameFrom	= 0;
		
		m_PrevBlock			= null;
		m_NextBlock			= null;
		m_TrailOwner		= null;
		m_Owner				= null;
		m_PlayerPresent		= null;
		
		m_Visited			= false;
		m_Captured			= false;
		
		m_OwnerColor		= m_DefaultColor;
		m_TrailColor		= m_DefaultColor;
		m_FinalColor		= m_DefaultColor;
		
		m_LevelBackup		= m_Level;
		m_OwnerBackup		= m_Owner;
		
		this.transform.position	= m_DefaultPos;
	}
	
	// Set the initial position of the cube and store this as default!
	public void SetInitPosition(Vector3 position)
	{
		m_DefaultPos = position;
		this.transform.position = m_DefaultPos;
	}
	
	// Accessor/Mutator for the cubes grid index
	public Vector2 GridIndex
	{
		get { return(m_GridIndex); }
		set { m_GridIndex = value; }
	}
	
	// Accessor for the owner object
	public Player Owner
	{
		get { return(m_Owner); }
	}
	
	// Accessor for the blocks level
	public int Level
	{
		get { return(m_Level); }
	}
	
	// Accessor/Mutator for the blocks status of having a player on it or not
	public Player PlayerPresent
	{
		get { return(m_PlayerPresent); }
		set { m_PlayerPresent = value; }
	}
	
	// Accessor/Mutator for the previous block object
	public BuildingBlock PrevBlock
	{
		get { return(m_PrevBlock); }
		set { m_PrevBlock = value; }
	}
	
	// Accessor/Mutator for the next block object
	public BuildingBlock NextBlock
	{
		get { return(m_NextBlock); }
		set { m_NextBlock = value; }
	}
	
	// Accessor/Mutator for visiting a block
	public bool Visited
	{
		get { return(m_Visited); }
		set { m_Visited = value; }
	}
	
	// Accessor/Mutator for capturing a block
	public bool Captured
	{
		get { return(m_Captured); }
		set { m_Captured = value; }
	}
	
	
	
	// Update is called once per frame
	void Update()
	{
		// Controls for raising or lowering of the blocks
		if (m_MustRaise)
			RaiseBlock();
		else if (m_MustLower)
			LowerBlock();
		
		if (m_MustOverrideColor)
			this.renderer.material.color = m_OverrideColor;
		else
			this.renderer.material.color = m_FinalColor;
	}
	
	
	
	// Handling of collision events
	void OnTriggerEnter(Collider collider)
	{
		// Ignore shitty pre-loading hits
		if (Time.timeSinceLevelLoad <= 0.0f)
		{
			Debug.Log("Ignoring preload hit");
			return;
		}
		
		// Get the colliding object (the player)
		Player collidingPlayer = collider.GetComponent("Player") as Player;
		
		// Remove self from last known block
		if(collidingPlayer != null)
		{
			if (collidingPlayer.LastHitBlock != null)
			{
				collidingPlayer.LastHitBlock.m_PlayerPresent	= null;
				//collidingPlayer.LastHitBlock.m_MustOverrideColor= false;
			}
		}
		// Taking ownership of fresh block
		if (m_PlayerPresent == null)
		{
			m_PlayerPresent = collidingPlayer;
			//m_MustOverrideColor = true;
		}
		// Enemy already has this block! So kill colliding player
		else if (m_PlayerPresent != collidingPlayer)
		{
			// Kill player
			collidingPlayer.KillPlayer();
			return;
		}
		
		// Check for ownership of the current block
		float speed		= 1.0f;
		float levelDiff	= (float)m_Level / (float)GenerateBlocks.numberOfLevels;
		// This block is the colliding players territory
		if (m_Owner == collidingPlayer)
		{
			// Speed 100% -> 300%
			speed += (levelDiff * 2.0f);
			collidingPlayer.SetSpeed(speed);
		}
		// Enemy (or neutral) block is hit
		else if (m_Owner != collidingPlayer)
		{
			// Player hit a max level block and must be killed!
			if (m_Level == GenerateBlocks.numberOfLevels)
			{
				// Kill player
				collidingPlayer.KillPlayer();
				return;
			}
			
			// Speed 50% -> 100%
			speed -= (levelDiff * 0.5f);
			collidingPlayer.SetSpeed(speed);
		}
		
		// Triggered a neutral block
		if (m_TrailOwner == null)
		{
			m_TrailOwner = collidingPlayer;
			m_PrevBlock = m_TrailOwner.LastHitBlock;
			if (m_PrevBlock != null)
				m_PrevBlock.m_NextBlock = this;
			m_TrailOwner.LastHitBlock = this;
			
			// Set the trail color
			m_TrailColor = m_TrailOwner.PlayerColor;
			
			// If the block is a level 3 then lower for the trail
			if (m_Level == GenerateBlocks.numberOfLevels)
			{
				m_MustRaise 	= false;
				m_MustLower		= true;
				m_IsMoving		= true;
				m_HeightLevel	= 1;
			}
			// Raise the block to the trails height
			else
			{
				m_MustRaise 	= true;
				m_MustLower		= false;
				m_IsMoving		= true;
				m_HeightLevel	= 1;
			}
		}
		// Hit own block
		else if (m_TrailOwner == collidingPlayer)
		{
			if (m_TrailOwner.LastHitBlock.m_PrevBlock == this)
			{
				Debug.Log("Reversed :/");
				Player ownerTemp = m_TrailOwner;
				ownerTemp.LastHitBlock.ChainNukeBlocks();
				ownerTemp.LastHitBlock = null;
			}
			else
			{
				Debug.Log("Hit own block");
				if (m_PrevBlock != null)
				{	
					m_PrevBlock.ChainNukeBlocks();
					m_PrevBlock = null;
				}
				
				// Now perform a capture of the enclosed area
				m_TrailOwner.PerformCapture(this);
				
				// Recall the trigger method to set tail to start at this current block!
				OnTriggerEnter(collider);
			}
		}
		// Hit an enemy block
		else
		{
			Debug.Log("Hit enemy block");
			// Nuke all of the enemies blocks
			ChainNukeBlocks();
			
			// Recall the trigger method to make sure that the current players trail makes use of the newly freed block
			OnTriggerEnter(collider);
		}
	}
	
	
	
	// Determine the position of this block in relation to another, returns a 4-way int
	public int Direction(BuildingBlock block)
	{
		int ret = -1;
		// Block on right
		if (block.GridIndex.x > GridIndex.x)
		{
			ret = 0;
		}
		// Block on left
		else if (block.GridIndex.x < GridIndex.x)
		{
			ret = 2;
		} 
		
		// Block is above
		if (block.GridIndex.y > GridIndex.y)
		{
			ret = 1;
		}
		// Block is below
		else if (block.GridIndex.y < GridIndex.y)
		{
			ret = 3;
		}
		
		// Return the direction
		return(ret);
	}
	
	
	
	// Chain nuke all previous blocks!
	// This will destroy a tail that has been hit!
	public void ChainNukeBlocks()
	{
		if (stacklimit >= 100)
		{
			stacklimit = 0;
			return;
		}
		stacklimit++;
		m_TrailOwner = null;
		
		// If the block is a level 3 then raise back to its normal height
		if (m_Level == GenerateBlocks.numberOfLevels)
		{
			m_MustRaise 	= true;
			m_MustLower		= false;
			m_IsMoving		= true;
			m_HeightLevel	= 2;
		}
		// Otherwise lower block to base
		else
		{
			m_MustRaise 	= false;
			m_MustLower		= true;
			m_IsMoving		= true;
			m_HeightLevel	= 0;
		}
		
		// Reset direction came from
		m_DirectionCameFrom = 0;
		
		if (m_PrevBlock != null)
		{	
			m_PrevBlock.ChainNukeBlocks();
			m_PrevBlock = null;
		}
	}
	
	// Capture all the surrounding blocks
	public void ChainCaptureBlocks()
	{
		CaptureSpecificBlock(m_TrailOwner);
		m_TrailOwner = null;
		
		if (m_PrevBlock != null)
		{
			m_PrevBlock.ChainCaptureBlocks();
		}
	}
	
	// Return if the current block is a boundary matching the input block
	public bool IsBorder(BuildingBlock block)
	{
		bool result = true;
		
		// Check levels
		if (block.m_Level != m_Level)
			result = false;
		
		// Check that they are same owner
		if (block.m_Owner != m_Owner)
			result = false;
		
		// Return result
		return(result);
	}
	
	// Capture this specific block
	public void CaptureSpecificBlock(Player owner)
	{
		// Push this block to the new owners processed block list
		owner.ProcessBlock(this);
		m_OwnerBackup = m_Owner;
		m_LevelBackup = m_Level;
		
		// Set block as captured
		m_Captured	= true;
		
		bool KillEnemyPlayer = m_PlayerPresent != owner;
		if (m_PlayerPresent != null)
			Debug.Log(m_PlayerPresent.ToString());// + "=====" + m_Owner.ToString());
		
		// Calculate the score for the current block
		Player prevOwner	= m_Owner;
		float oldScore		= (float)m_Level / (float)GenerateBlocks.numberOfLevels;
		float newScore		= 0.0f;
		
		// If already owning the block then increase its level
		if (m_Owner == owner)
		{
			m_Level++;
			if (m_Level > GenerateBlocks.numberOfLevels)
				m_Level = GenerateBlocks.numberOfLevels;
		}
		// Neutral block
		else if (m_Owner == null)
		{
			// Block is available for capture, so change to level 1 player
			if (m_Level <= 0)
			{
				m_Level = 1;
				m_Owner = owner;
			}
			// Cannot capture level 3 neutral zone!
			else if (m_Level == GenerateBlocks.numberOfLevels)
			{
				// Tell parent to reverse the capture
				owner.UndoCapture(this);
			}
			// Else decrease level and remain neutral
			else
			{
				m_Level--;
			}
		}
		// Enemy held block
		else if (m_Owner != owner)
		{
			if (KillEnemyPlayer && m_PlayerPresent == m_Owner)
				KillEnemyPlayer = false;
			
			// Cannot Capture this teritory so reject the fill!
			if (m_Level == GenerateBlocks.numberOfLevels)
			{
				// Tell parent to reverse the capture
				owner.UndoCapture(this);
			}
			// Attack the enemies zone
			else
			{
				// Reduce the blocks effectiveness
				m_Level--;
				
				// Level has been hit hard enough so move it to neutral
				if (m_Level <= 0)
				{
					m_Level = 0;
					m_Owner = null;
				}
			}
		}
		
		if (KillEnemyPlayer)
			owner.KillEnemyPlayer(m_PlayerPresent);
		
		// Raise or lower the block!
		DetermineRaisingOrLowering();
		
		// Calculate the new score
		newScore = (float)m_Level / (float)GenerateBlocks.numberOfLevels;
		
		// Reduce old owners score
		if (prevOwner != null)
			prevOwner.ChangeScore(-oldScore);
		
		// Increase new owners score
		if (m_Owner != null)
			m_Owner.ChangeScore(newScore);
	}
	
	// Work out requirement to move the block (raise / lower)
	void DetermineRaisingOrLowering()
	{
		int requiredHeight = 0;
		if (m_Level == GenerateBlocks.numberOfLevels)
		{
			requiredHeight = 2;
		}
		
		if (m_HeightLevel > requiredHeight)
		{
			m_MustRaise 	= false;
			m_MustLower		= true;
			m_IsMoving		= true;
			m_HeightLevel	= requiredHeight;
		}
		else if (m_HeightLevel <= requiredHeight)
		{
			m_MustRaise 	= true;
			m_MustLower		= false;
			m_IsMoving		= true;
			m_HeightLevel	= requiredHeight;
		}
	}
	
	// Undo a capture for an invalid area!
	public void UndoCapture()
	{
		m_Owner = m_OwnerBackup;
		m_Level = m_LevelBackup;
		
		// Raise or lower the block!
		DetermineRaisingOrLowering();
	}
	
	
	
	// Raise the block up high
	void RaiseBlock()
	{
		// Failsafe for making sure a valid move order is issued
		if (!m_IsMoving)
			return;
		
		Color primaryColor = m_DefaultColor;
		if (m_Owner != null)
		{
			float level = 0.8f - (0.6f * (float)m_Level / (float)GenerateBlocks.numberOfLevels);
			primaryColor = level * m_Owner.PlayerColor;
		}
		
		// Move for a set distance!
		if (this.transform.position.z <= (m_DefaultPos.z - (distanceToMove * m_HeightLevel)))
		{
			m_IsMoving	= false;
			m_MustRaise	= false;
			
			this.transform.position	= m_DefaultPos - new Vector3(0.0f, 0.0f, distanceToMove * m_HeightLevel);
			if (m_TrailOwner != null)
				m_FinalColor = m_TrailOwner.PlayerColor;
			else
				m_FinalColor = primaryColor;
		}
		else
		{
			this.transform.Translate(new Vector3(0.0f, 0.0f, -distanceToMove * RateToMove * Time.deltaTime));
			float scale		= (m_DefaultPos.z - this.transform.position.z) / (distanceToMove * m_HeightLevel);
			
			if (m_Owner != null)
				m_FinalColor	= (primaryColor * (1.0f - scale)) + (m_Owner.PlayerColor * scale);
			else
				m_FinalColor	= (primaryColor * (1.0f - scale)) + m_TrailColor * scale;
 		}
	}
	
	// Drop the block back down
	void LowerBlock()
	{
		// Failsafe for making sure a valid move order is issued
		if (!m_IsMoving)
			return;
		
		Color primaryColor = m_DefaultColor;
		if (m_Owner != null)
		{
			float level = 0.8f - (0.6f * (float)m_Level / (float)GenerateBlocks.numberOfLevels);
			primaryColor = level * m_Owner.PlayerColor;
		}
		
		// Move for a set distance!
		if (this.transform.position.z >= (m_DefaultPos.z - (distanceToMove * m_HeightLevel)))
		{
			m_IsMoving	= false;
			m_MustLower	= false;
			
			this.transform.position	= m_DefaultPos - new Vector3(0.0f, 0.0f, distanceToMove * m_HeightLevel);
			if (m_TrailOwner != null)
				m_FinalColor = m_TrailOwner.PlayerColor;
			else
				m_FinalColor = primaryColor;
		}
		else
		{
			this.transform.Translate(new Vector3(0.0f, 0.0f, distanceToMove * RateToMove * Time.deltaTime));
			float scale		= (m_DefaultPos.z - this.transform.position.z) / (distanceToMove);
			
			m_FinalColor	= (primaryColor * (1.0f - scale)) + (m_TrailColor * scale);
		}
	}
}