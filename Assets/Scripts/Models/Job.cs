using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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

  public string Item { get; set; }

  public bool Busy { get; set; }

  public float BuildTime { get; set; }
  public float Rotation { get; set; }

  //public int Retry { get; internal set; }
  // public BuildingResource ResourcePile { get; internal set; }


  //	TODO sound countroller
  //bool _startSound = true;
  //public AudioSource AudioSource { get; set; }
  #endregion

  #region Construction

  public Job(string type, Tile tile, float buildTime, float rotation = 0)
  {
    Tile = tile;
    Item = type;
    BuildTime = buildTime;
    Tile.ActiveJob = this;
    Rotation = rotation;
  }

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
      Busy = false;

      //  create iteme
      var item = IoC.Get<AbstractItemFactory>().CreateItem(Item, Tile, Rotation);
      Tile.InstallItem(item);


      return true;
    }

    return false;
  }

  internal void AcceptJob()
  {
    Busy = true;
  }

  public bool IsCompleted() => BuildTime <= 0;

  #endregion

  #region Save/Load

  public Job(JobData data, World world)
  {
    Id = data.id;
    Tile = world.GetTile(data.x, data.y);
    BuildTime = data.buildtime;
    Item = data.type;
    Busy = data.busy;
    Rotation = data.rotation;
  }

  public JobData ToData() =>
    new JobData
    {
      id = Id,
      x = Tile.Position.x,
      y = Tile.Position.y,
      buildtime = BuildTime,
      //type = Item.Type,
      type = Item,
      busy = Busy,
      rotation = Rotation
    };

  internal void FinishJob() => new JobUpdateEvent { Job = this }.Publish();

  #endregion
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
  public float rotation;
}