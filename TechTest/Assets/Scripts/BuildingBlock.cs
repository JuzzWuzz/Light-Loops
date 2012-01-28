using UnityEngine;
using System.Collections;

public class BuildingBlock : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{		
		this.renderer.material.color = Color.grey;		
	}
	
	// Update is called once per frame
	void Update () {
			
	}
	
	void OnCollisionEnter()
	{
		this.renderer.material.color = Color.red;
		Debug.Log("Collision detected on buildingBlock");
	}
}
