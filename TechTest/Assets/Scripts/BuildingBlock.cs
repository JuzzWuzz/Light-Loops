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
	private PlayerScript	m_TrailOwner;
	private PlayerScript	m_Owner;
	
	// Color controls
	private Color			m_OwnerColor;
	private Color			m_TrailColor;
	
	
	
	// Use this for initialization
	void Start() 
	{
		m_DefaultColor = Color.grey;
		this.renderer.material.color = m_DefaultColor;
		
		m_TrailOwner= null;
		m_PrevBlock	= null;
		
		m_Level		= 0;
	}
	
	// Set the initial position of the cube and store this as default!
	public void SetInitPosition(Vector3 position)
	{
		m_DefaultPos = position;
		this.transform.position = m_DefaultPos;
	}
	
	// Accessor/Mutator for the cubes grid index
	public Vector2 GridIndex {
		get { return(m_GridIndex); }
		set { m_GridIndex = value; }
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
		
		PlayerScript collidingPlayer = collider.GetComponent("PlayerScript") as PlayerScript;
		// Triggered a neutral block
		if (m_TrailOwner == null)
		{
			m_TrailOwner = collidingPlayer;
			m_PrevBlock = m_TrailOwner.LastHitBlock;
			m_TrailOwner.LastHitBlock = this;
			
			// Set the direction for filling algo, see top for number def
			DetermineDirection();
			
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
				PlayerScript ownerTemp = m_TrailOwner;
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
				m_TrailOwner.PerformCapture();
			}
		}
		// Hit an enemy block
		else
		{
			Debug.Log("Hit enemy block");
		}
	}
	
	
	
	// Work out the direction the snake came from
	void DetermineDirection()
	{
		// No previous item so set it as diagonal
		if (m_PrevBlock == null)
			m_DirectionCameFrom = 3;
		else
		{
			Vector2 prevGridIndex = m_PrevBlock.m_GridIndex;
			
			// Y's same
			if (prevGridIndex.y == m_GridIndex.y)
			{
				// Y's the same and X's differ == horizontal
				if (prevGridIndex.x != m_GridIndex.x)
				{
					m_DirectionCameFrom = 1;
				}
			}
			// Y's differ
			else
			{
				// Y's differ and X's same == vertical
				if (prevGridIndex.x == m_GridIndex.x)
				{
					m_DirectionCameFrom = 2;	
				}
				// Both axes differ so must be a diagonal
				else
				{
					m_DirectionCameFrom = 3;
				}
			}
		}
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
		m_MustLower = true;
		m_MustRaise = false;
		m_IsMoving = true;
		
		m_Owner = m_TrailOwner;
		m_TrailOwner = null;
		
		if (m_PrevBlock != null)
		{
			m_PrevBlock.ChainCaptureBlocks();
			m_PrevBlock = null;
		}
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
			primaryColor = m_Owner.PlayerColor * 0.5f;
		
		// Move for a set distance!
		if (this.transform.position.z >= (m_DefaultPos.z))
		{
			m_IsMoving	= false;
			m_MustLower	= false;
			
			this.transform.position		= m_DefaultPos;
			this.renderer.material.color= primaryColor;
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