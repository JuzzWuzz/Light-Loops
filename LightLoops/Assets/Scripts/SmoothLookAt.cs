using UnityEngine;
using System.Collections;

public class SmoothLookAt : MonoBehaviour
{
	public Transform target;
	public float damping = 6.0f;
	public bool smooth = true;
	
	public float desiredDistance = 100f;
	
	public float elapsedTime = 0f;

	public void LateUpdate () {
		if (target) {	
			//Delayed start
			if(elapsedTime > 0.1)
			{
				if (smooth)
				{
					// Look at and dampen the rotation
					var rotation = Quaternion.LookRotation(target.position - transform.position);
					transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
					
					//Move to and dampen movement
					var targetPosition = new Vector3(target.position.x, target.position.y, target.position.z - desiredDistance);
					transform.position = Vector3.Slerp(transform.position, targetPosition, Time.deltaTime * damping);					
				}
				else
				{
					// Just lookat
				    transform.LookAt(target);
				}
			}		
			else
			{
				elapsedTime += Time.deltaTime;
			}
		}
	}

	public void Start () {
		// Make the rigid body not change rotation
	   	if (rigidbody)
			rigidbody.freezeRotation = true;
	}
}