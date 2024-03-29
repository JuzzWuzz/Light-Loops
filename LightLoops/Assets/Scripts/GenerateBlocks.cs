using UnityEngine;
using System.Collections;
public class GenerateBlocks : MonoBehaviour {
	
	/// <summary>
	/// Load the prefab for the block into this variable.
	/// </summary>
	public BuildingBlock cube;
	public int unityGridSize = 50;
	public static int gridSize = 50;
	public float cubeSize = 3f;
	public static float cubeSpacing = 4.0f;
	
	public static int numberOfLevels = 3;
	
	public static BuildingBlock[,] blocks;
	
	public static float percentagePerBlock = 100 / ((float)gridSize * (float)gridSize);
	
	// Use this for initialization
	void Start () 
	{
		gridSize = unityGridSize;
		// Create the array
		blocks = new BuildingBlock[gridSize, gridSize];
		
		// Setup the cubes transforms
		cube.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);
		
		for (int x = 0; x < gridSize; x++)
		{
			for (int y = 0; y < gridSize; y++)
			{
				UnityEngine.Object cubeInstantiation = Instantiate(cube, new Vector3(), Quaternion.identity);					
				((BuildingBlock)cubeInstantiation).SetInitPosition(new Vector3(x * cubeSpacing, y * cubeSpacing, 200));
				
				// Set the cubes position in the array
				((BuildingBlock)cubeInstantiation).GridIndex = new Vector2(x, y);
				blocks[x,y] = ((BuildingBlock)cubeInstantiation);
			}			
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
}