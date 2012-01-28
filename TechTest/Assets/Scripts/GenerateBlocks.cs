using UnityEngine;
using System.Collections;
public class GenerateBlocks : MonoBehaviour {
	
	/// <summary>
	/// Load the prefab for the block into this variable.
	/// </summary>
	public BuildingBlock cube;
	public UnityEngine.Transform plane;
	public int gridSize = 50;
	public float cubeSize = 3f;
	public float cubeSpacing = 4.0f;
	
	// Use this for initialization
	void Start () 
	{
		cube.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);
		for (int x = 0; x < gridSize; x++) 
		{
			for (int y = 0; y < gridSize; y++) 
			{
				UnityEngine.Object cubeInstantiation = Instantiate(cube, new Vector3(x*4, y*4, 200), Quaternion.identity);					
			}			
		}
		
		plane.transform.localPosition = new Vector3((gridSize * cubeSpacing) / 2,
		                                            (gridSize * cubeSpacing) / 2,
		                                            200 + cubeSize + 1);		
		plane.transform.localScale = new Vector3(gridSize * cubeSpacing, gridSize * cubeSpacing, gridSize * cubeSpacing);
		plane.renderer.material.color = Color.black;
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
