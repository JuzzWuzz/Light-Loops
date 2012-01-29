using UnityEngine;
using System.Collections;

public class MyEditable3DText : MonoBehaviour {
	
	string hostIp = "127.0.0.1";
	bool editMode = false;	
	public TextMesh textView;
	public TextMesh shadow;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void OnMouseDown()
	{
		
		if(editMode)
		{
			NetworkingScript.host = hostIp;
			textView.text = hostIp;		
			shadow.text = hostIp;
			editMode = false;
		}
		else
		{
			editMode = true;	
		}
	}
	
	public void OnGUI()
	{
		if(editMode)
		{
			GUI.Label (new Rect (Screen.width/2 - 100, Screen.height/2 - 10, 200,20), "Enter IP Address");
			hostIp = GUI.TextField (new Rect (Screen.width/2 - 120, Screen.height/2 + 20, 200,20), hostIp, 40);			
		}
		
		if(editMode)
		{
			if(Input.GetKeyDown("enter"))
			{
				editMode = false;
				
			}
			   
		}
	}
}
