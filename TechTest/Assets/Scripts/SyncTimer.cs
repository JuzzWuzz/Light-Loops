using UnityEngine;
using System.Collections;

public class SyncTimer : MonoBehaviour {
	
	public float timeLeft = 240f;
	
	private bool started = false;
	
	// Use this for initialization
	void Start () 
	{
		
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
}
