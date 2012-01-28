using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

	// Variables
	public int				playerNumber;
	private BuildingBlock	m_LastHitBlock;
	private Color			m_PlayerColor;
	
	// Use this for initialization
	void Start ()
	{
		m_LastHitBlock = null;
		
		switch(playerNumber)
		{
		case 1:
				m_PlayerColor = Color.red;
				renderer.material.color = m_PlayerColor;
			break;
		case 2:
				m_PlayerColor = Color.green;
				renderer.material.color = m_PlayerColor;
			break;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(playerNumber == NetworkingScript.myPlayer)
			transform.Translate(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
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
	
	// Perform a capture of the blocks
	public void PerformCapture()
	{
		if (m_LastHitBlock != null)
			m_LastHitBlock.ChainCaptureBlocks();
		m_LastHitBlock = null;
	}
}
