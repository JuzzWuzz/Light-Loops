using UnityEngine;
using System.Collections;

public class MenuCameraScript : MonoBehaviour {
	
	
	
	public Vector3 MAIN_ORIENTATION;
	public Vector3 JOIN_ORIENTATION;
	
	public float speed = 3.0f;	
	
	public Vector3 CurrentOrientation {		
		set{currentOrientation = value;}
	}
	Vector3 currentOrientation;
	
	public float elapsedTime = 0f;
	
	// Use this for initialization	
	void Start () {
		currentOrientation = transform.eulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void LateUpdate () 
	{			
		if(currentOrientation != null)
		{
			//Delayed start
			if(elapsedTime > 0.1)
			{
				var newRot = Quaternion.Euler(currentOrientation); // get the equivalent quaternion
    			transform.rotation = Quaternion.Slerp(transform.rotation, newRot, speed*Time.deltaTime);					
			}		
			else
			{
				elapsedTime += Time.deltaTime;
			}
		}
		
	}
}
