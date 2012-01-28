using UnityEngine;
using System.Collections;

public class TextControl : MonoBehaviour {
	
	public enum ButtonType 
	{
		PLAY_BUTTON_CREATE,
		PLAY_BUTTON_JOIN,
		QUIT_BUTTON
	}
	
	public static bool hostGame = false;
	public static bool HostGame {
		get {return hostGame;}		
	}
	
	public ButtonType buttonType;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnMouseEnter()
	{
		//Change color of text on enter
		renderer.material.color = Color.blue;
	}
	
	void OnMouseExit()
	{
		renderer.material.color = Color.white;
	}
	
	void OnMouseUp()
	{
		switch(buttonType)
		{
			case ButtonType.PLAY_BUTTON_CREATE:
				hostGame = true;
				Application.LoadLevel(1);
				break;
			case ButtonType.PLAY_BUTTON_JOIN:
				hostGame = false;
				Application.LoadLevel(1);
				break;
			case ButtonType.QUIT_BUTTON:
				Application.Quit();
				break;
		}
	}
}
