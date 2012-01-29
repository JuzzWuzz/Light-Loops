using UnityEngine;
using System.Collections;

public class TextControl : MonoBehaviour {
	
	public enum ButtonType 
	{
		PLAY_BUTTON_CREATE,
		PLAY_BUTTON_JOIN,
		JOIN_BUTTON_CONNECT,
		JOIN_BUTTON_BACK,
		JOIN_IP_ENTRY_FIELD,
		QUIT_BUTTON
	}
	
	public static bool hostGame = true;
	public static bool HostGame {
		get {return hostGame;}		
	}
	
	public ButtonType buttonType;
	
	public MenuCameraScript menuCamera;
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
				menuCamera.CurrentOrientation = menuCamera.JOIN_ORIENTATION;
				break;
			case ButtonType.JOIN_BUTTON_CONNECT:
				hostGame = false;
				Application.LoadLevel(1);
				break;
			case ButtonType.JOIN_BUTTON_BACK:
				menuCamera.CurrentOrientation = menuCamera.MAIN_ORIENTATION;
				break;			
			case ButtonType.QUIT_BUTTON:
				Application.Quit();
				break;
		}
	}
}
