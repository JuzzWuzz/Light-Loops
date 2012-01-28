using UnityEngine;
using System.Collections;

public class LoadingScreenController : MonoBehaviour {
	
	public TextMesh shadow;
	float elapsedTime = 0;
	
	private static bool countDownComplete = false;
	
	public static bool CountDownComplete {
		get { return countDownComplete; }
	}
	
	// Use this for initialization
	void Start () 
	{
		
		GetComponent<TextMesh>().text = "Loading";
		GetComponent<TextMesh>().transform.localPosition = new Vector3(-2.1f, 2.4f, 6.2f);
		shadow.text = "Loading";
		countDownComplete = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(elapsedTime < 5)
		{
			elapsedTime += Time.deltaTime;
			if(elapsedTime > 0.1)
			{
				if(elapsedTime < 2)
				{
					GetComponent<TextMesh>().text = "Get Ready!";
					GetComponent<TextMesh>().transform.localPosition = new Vector3(-3.4f, 2.4f, 5.9f);
					shadow.text = "Get Ready!";
				}
				else if(elapsedTime < 3)
				{
					GetComponent<TextMesh>().text = "3...";
					GetComponent<TextMesh>().transform.localPosition = new Vector3(0.5f, 2.4f, 6.8f);
					shadow.text = "3...";
				}
				else if(elapsedTime < 4)
				{
					GetComponent<TextMesh>().text = "2...";
					shadow.text = "2...";
				}
				else if(elapsedTime < 5)
				{
					GetComponent<TextMesh>().text = "1...";
					shadow.text = "1...";
				}		
			}
		}	
		else if(elapsedTime < 6)
		{
			shadow.text = "Go!";
			GetComponent<TextMesh>().text = "Go!";
			float goneLevel = 6 - elapsedTime;
			GetComponent<TextMesh>().transform.localPosition = Vector3.Slerp(
			                                                                 GetComponent<TextMesh>().transform.localPosition, 
			                                                                 new Vector3(-18f, 1.7f, 2.2f), Time.deltaTime);
			countDownComplete = true;	
		}
			
	}
}
