using UnityEngine;
using System.Collections;

public class BuildingBlock : MonoBehaviour {
	
	public float	distanceToMove;
	public float	RateToMove;
	
	private Vector3	m_DefaultPos;
	private Color	m_DefaultColor;
	
	private bool	m_MustRaise;
	private bool	m_MustLower;
	private bool	m_IsMoving;
	
	private int		m_Level;
	
	// Direction used for filling algorithm {0 = invalid (null), 1 = horizontal (left/right), 2 = vertical (up/down), 3 = diagonal}
	private int		m_DirectionCameFrom;
	
	private Vector2	m_GridIndex;
	
	// Controls for chaining
	private BuildingBlock	m_PrevBlock;
	private Player	m_TrailOwner;
	private Player	m_Owner;
	private Player	m_PlayerPresent;
	
	// Color controls
	private Color			m_OwnerColor;
	private Color			m_TrailColor;
	
	// Backup vars for undo of capture
	private int				m_LevelBackup;
	private Player	m_OwnerBackup;
	
	
	// Use this for initialization
	void Start() 
	{
		m_DefaultColor = Color.grey;
		this.renderer.material.color = m_DefaultColor;
		
		Reset();
	}
	
	// Reset the blocks variables
	public void Reset()
	{
		m_MustLower = false;
		m_MustRaise = false;
		m_IsMoving	= false;
		
		m_Level		= 0;
		
		m_DirectionCameFrom = 0;
		
		m_PrevBlock	= null;
		m_TrailOwner= null;
		m_Owner		= null;
		m_PlayerPresent = null;
		
		m_OwnerColor= m_DefaultColor;
		m_TrailColor= m_DefaultColor;
		
		m_LevelBackup = m_Level;
		m_OwnerBackup = m_Owner;
		
		this.transform.position = m_DefaultPos;
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
	
	// Accessor/Mutator for the direction value
	public int DirectionCameFrom
	{
		get { return(m_DirectionCameFrom); }
		set { m_DirectionCameFrom = value; }
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
	
	
	
	// Update is called once per frame
	void Update()
	{
		// Controls for raising or lowering of the blocks
		if (m_MustRaise)
			RaiseBlock();
		else if (m_MustLower)
			LowerBlock();
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
		if(m_TrailOwner != null)
		{
		if (m_TrailOwner.LastHitBlock != null)
			m_TrailOwner.LastHitBlock.m_PlayerPresent = null;
		}
		// Taking ownership of fresh block
		if (m_PlayerPresent == null)
		{
			m_PlayerPresent = collidingPlayer;
		}
		// Enemy already has this block! So kill colliding player
		else if (m_PlayerPresent != collidingPlayer)
		{
			// Kill player
			collidingPlayer.KillPlayer();
		}
		
		// Triggered a neutral block
		if (m_TrailOwner == null)
		{
			m_TrailOwner = collidingPlayer;
			m_PrevBlock = m_TrailOwner.LastHitBlock;
			m_TrailOwner.LastHitBlock = this;
			
			// Set the direction for filling algo, see top for number def
			DetermineDirection(m_PrevBlock);
			
			// Set the trail color
			m_TrailColor = m_TrailOwner.PlayerColor;
			
			m_MustRaise = true;
			m_MustLower = false;
			m_IsMoving = true;
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
				
				Debug.Log("Mode: " + m_DirectionCameFrom);
				DetermineDirection(m_TrailOwner.LastHitBlock);
				Debug.Log("Mode: " + m_DirectionCameFrom);
				
				// Now perform a capture of the enclosed area
				m_TrailOwner.PerformCapture();
			}
		}
		// Hit an enemy block
		else
		{
			Debug.Log("Hit enemy block");
			ChainNukeBlocks();
			OnTriggerEnter(collider);
		}
	}
	
	
	
	// Work out the direction the snake came from
	void DetermineDirection(BuildingBlock prevBlock)
	{
		int direction = 0;
		
		// No previous item
		if (m_PrevBlock == null)
			return;
		else
		{
			Vector2 prevGridIndex = prevBlock.m_GridIndex;
			
			// Y's same
			if (prevGridIndex.y == m_GridIndex.y)
			{
				// Y's the same and X's differ == horizontal
				if (prevGridIndex.x != m_GridIndex.x)
				{
					direction = 1;
				}
			}
			// Y's differ
			else
			{
				// Y's differ and X's same == vertical
				if (prevGridIndex.x == m_GridIndex.x)
				{
					direction = 2;
				}
				// Both axes differ so must be a diagonal
				else
				{
					direction = 3;
				}
			}
		}
		
		// Set the previous blocks position
		DirectionCameFrom = direction;
		
		// Used to set the first blocks direction at step 2
		if (prevBlock.m_PrevBlock == null)
			prevBlock.DirectionCameFrom = direction;
		/*else
		{
			if (prevBlock.m_DirectionCameFrom == 2 && DirectionCameFrom == 1)
			{
				prevBlock.m_DirectionCameFrom = 1;
			}
		}*/
	}
	
	
	
	// Chain nuke all previous blocks!
	// This will destroy a tail that has been hit!
	void ChainNukeBlocks()
	{
		m_TrailOwner = null;
		
		m_MustLower = true;
		m_MustRaise = false;
		m_IsMoving = true;
		
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
			m_PrevBlock = null;
		}
	}
	
	// Capture this specific block
	public void CaptureSpecificBlock(Player owner)
	{
		// Push this block to the new owners processed block list
		owner.ProcessBlock(this);
		m_OwnerBackup = m_Owner;
		m_LevelBackup = m_Level;
		
		// Set block move state variables
		m_MustLower	= true;
		m_MustRaise	= false;
		m_IsMoving	= true;
		
		bool KillEnemyPlayer = m_PlayerPresent != owner;
		
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
	}
	
	// Undo a capture for an invalid area!
	public void UndoCapture()
	{
		m_Owner = m_OwnerBackup;
		m_Level = m_LevelBackup;
	}
	
	
	
	// Raise the block up high
	void RaiseBlock()
	{
		// Failsafe for making sure a valid move order is issued
		if (!m_IsMoving)
			return;
		
		// Move for a set distance!
		if (this.transform.position.z <= (m_DefaultPos.z - distanceToMove))
		{
			m_IsMoving	= false;
			m_MustRaise	= false;
			
			this.transform.position		= m_DefaultPos - new Vector3(0.0f, 0.0f, distanceToMove);
			this.renderer.material.color= m_TrailOwner.PlayerColor;
		}
		else
		{
			this.transform.Translate(new Vector3(0.0f, 0.0f, -distanceToMove * RateToMove * Time.deltaTime));
			float scale = (m_DefaultPos.z - this.transform.position.z) / distanceToMove;
			Color mixedColor = (m_DefaultColor * (1.0f - scale)) + (m_TrailOwner.PlayerColor * scale);
			this.renderer.material.color = mixedColor;
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
		if (this.transform.position.z >= (m_DefaultPos.z))
		{
			m_IsMoving	= false;
			m_MustLower	= false;
			
			this.transform.position		= m_DefaultPos;
			this.renderer.material.color= primaryColor;
			
			/*if (m_DirectionCameFrom == 1)
				this.renderer.material.color=Color.cyan;
			else if (m_DirectionCameFrom == 2)
				this.renderer.material.color=Color.yellow;
			else if (m_DirectionCameFrom == 3)
				this.renderer.material.color=Color.magenta;*/
		}
		else
		{
			this.transform.Translate(new Vector3(0.0f, 0.0f, distanceToMove * RateToMove * Time.deltaTime));
			float scale = distanceToMove - (m_DefaultPos.z - this.transform.position.z) / distanceToMove;
			Color mixedColor = (primaryColor * (1.0f - scale)) + (m_TrailColor * scale);
			this.renderer.material.color = mixedColor;
		}
	}
}