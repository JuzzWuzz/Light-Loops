using UnityEngine;
using System.Collections;

public class SyncTimer : MonoBehaviour {
	
	
	public float gameTime = 250f;
	private float timeLeft = 20f;
	public float TimeLeft {
		get{return timeLeft;}		
	}
	
	private bool started = false;
	
	// Use this for initialization
	void Start () 
	{
		timeLeft = gameTime;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(started && TextControl.HostGame)
		{
			timeLeft -= Time.deltaTime;	
		}
		
		if(timeLeft <= 0)
		{
			timeLeft = 0;
			started = false;
			Scoreboard.TimesUp();
		}
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		float time = 0;
		if (stream.isWriting) {
			time = timeLeft;
			stream.Serialize(ref time);
		} else {
			stream.Serialize(ref time);
			timeLeft = time;
		}
	}
		
	public void StartTimer()
	{
		started = true;
	}
	
	public void ResetTimer()
	{
		if(TextControl.HostGame)
			timeLeft = gameTime;
	}
}
