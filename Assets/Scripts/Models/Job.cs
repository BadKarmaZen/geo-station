using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job
{
	#region Properties

	public Tile Tile { get; set; }
	public Item	Item { get; set; }

	public float BuildTime { get; set; }
	public int Retry { get; internal set; }
  public BuildingResource ResourcePile { get; internal set; }


	//	TODO sound countroller
	bool _startSound = true;
	public AudioSource AudioSource { get; set; }
	#endregion

	#region Methods

	public bool ProcessJob(float deltaTime)
	{
		//	TODO TEST CODE
		if (_startSound)
		{
			_startSound = false;
			this.AudioSource.Play();


			//AudioSource.PlayClipAtPoint(_ac, Tile.Position.GetVector() /*Camera.main.transform.position*/);
		}

		BuildTime -= deltaTime;

		if (BuildTime  < 0)
		{
			this.AudioSource.Stop();
		}
		return BuildTime < 0;
	}

	#endregion
}
