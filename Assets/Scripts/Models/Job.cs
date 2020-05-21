using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobUpdateEvent : Event
{
	public Job Job { get; set; }
	//public Character Worker { get; set; }
}

public class Job
{
	#region Properties
	public long Id { get; set; }

	public Tile Tile { get; set; }

	//public Item Item { get; set; }
	public string Item { get; set; }

	public bool Busy { get; set; }

  public float BuildTime { get; set; }
	//public int Retry { get; internal set; }
	// public BuildingResource ResourcePile { get; internal set; }


	//	TODO sound countroller
	//bool _startSound = true;
	//public AudioSource AudioSource { get; set; }
	#endregion

	#region Methods

	public bool ProcessJob(float deltaTime)
	{
		//	TODO TEST CODE
		//if (_startSound)
		//{
		//	_startSound = false;
		//	this.AudioSource.Play();


		//	//AudioSource.PlayClipAtPoint(_ac, Tile.Position.GetVector() /*Camera.main.transform.position*/);
		//}

		BuildTime -= deltaTime;

		if (IsCompleted())
		{
			//Tile.InstallItemOnTile(Item);
			Tile.InstallItemOnTile(IoC.Get<ObjectFactory>().GetFactory(Item).CreateItem(Tile));
			return true;
		}

		return false;
	}

	public bool IsCompleted() => BuildTime <= 0;

	#endregion

	public JobData ToData() =>
		new JobData
		{
			id = Id,
			x = Tile.Position.x,
			y = Tile.Position.y,
			buildtime = BuildTime,
			//type = Item.Type,
			type = Item,
			busy = Busy
		};
}

[Serializable]
public class JobData
{
	public long id;
  public int x;
	public int y;
	public string type;
	public float buildtime;
	public bool busy;
}