using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEvent
{
  public Character Character { get; set; }
}

public class CharacterCreatedEvent : CharacterEvent { }
public class CharacterUpdatedEvent : CharacterEvent
{
  public bool Hand { get; internal set; }
}

public class Character
{
  #region Members

  private float _handMovement;
  private float _movementCompletePercentage; //	goes from 0 to 1
  private Stack<Tile> _path = null;


  #endregion

  #region Properties

  public Tile CurrentTile { get; set; }
  public Tile DestinationTile { get; set; }
  public Tile NextTile { get; set; }
  public float Speed { get; set; }

  public Job CurrentJob { get; set; }

  public float X
  {
    get
    {
      return Mathf.Lerp(CurrentTile.Position.x, NextTile.Position.x, _movementCompletePercentage);
    }
  }

  public float Y
  {
    get
    {
      return Mathf.Lerp(CurrentTile.Position.y, NextTile.Position.y, _movementCompletePercentage);
    }
  }

  public float HandY
  {
    get
    {
      //Debug.Log($"Hand => {_handMovement} ");
      return Mathf.Lerp(CurrentTile.Position.y - 0.2f, CurrentTile.Position.y + 0.1f, _handMovement);
    }
  }

  public BuildingResource SelectedResourcePile { get; private set; }
  public bool HasResource { get; set; }

  #endregion

  #region Construction

  public Character(Tile tile)
  {
    CurrentTile = tile;
    DestinationTile = null;
  }

  #endregion

  #region Methods

  public void Update(float deltaTime)
  {
    UpdateTask(deltaTime);
    UpdateMovement(deltaTime);
  }


  private void MoveIdle()
  {
    if (DestinationTile == null)
    {
      var tiles = (from t in IoC.Get<World>().GetNeighbourTiles(CurrentTile)
                   where t.Type == Tile.TileType.Floor
                   select t).ToList();

      if (tiles.Count != 0)
      {
        var foo = UnityEngine.Random.Range(0, tiles.Count);

        SetDestination(tiles[foo]);
      }
    }
  }
  private void UpdateTask(float deltaTime)
  {
    if (CurrentJob == null)
    {
      //  idle movement
      MoveIdle();

      return;
    }

    if (!HasResource)
    {
      //  has job, do we have the resource
      if (SelectedResourcePile == null)
      {
        //  no resource, get it
        SelectedResourcePile = IoC.Get<BuildingResourceController>().SelectResourcePile(CurrentJob.Item.Type);
        if (SelectedResourcePile != null)
        {
          SelectedResourcePile.Reserve();
        }
      }

      if (SelectedResourcePile != null && DestinationTile == null)
      {
        SetDestination(SelectedResourcePile.Tile);
      }

      if (CurrentTile == SelectedResourcePile?.Tile)
      {
        SelectedResourcePile.TakeResource();
        HasResource = true;
        SelectedResourcePile = null;
      }

      //  no resources or cannot reach resource
      if (SelectedResourcePile == null || DestinationTile == null)
      {
        MoveIdle();
      }
    }

    if (HasResource && DestinationTile == null)
    {
      SetDestination(CurrentJob.Tile);
    }

    if (HasResource && CurrentTile == CurrentJob.Tile)
    {
      //  we have a job and we are at the location
      if (CurrentJob.ProcessJob(deltaTime))
      {
        IoC.Get<EventAggregator>().Publish(new JobUpdateEvent
        {
          Worker = this,
          Job = CurrentJob
        });

        CurrentJob = null;
        HasResource = false;
      }

      _handMovement += 0.03f;

      if (_handMovement > 1.0f)
      {
        _handMovement = 0f;
      }

      IoC.Get<EventAggregator>().Publish(new CharacterUpdatedEvent { Character = this, Hand = true });
    }
  }

  private void UpdateMovement(float deltaTime)
  {
    if (CurrentTile != DestinationTile)
    {
      if (NextTile == null)
      {
        //  get next step
        if (_path != null && _path.Count > 0)
        {
          NextTile = _path.Pop();
        }
        else
        {
          _path = null;
        }
      }
    }
    else
    {
      //  Arrived

      DestinationTile = null;


      //if (CurrentJob == null)
      //{
      //  Debug.Log("Job = null");
      //}
      //else if (CurrentJob.ResourcePile == null)
      //{
      //  Debug.Log("ResourcePile = null");
      //}

      //if (DestinationTile == CurrentJob.ResourcePile.Tile)
      //{
      //  CurrentJob.ResourcePile.TakeResource();
      //  if (!SetDestination(CurrentJob.Tile))
      //  {
      //    //  TODO - cannot complete job
      //  }

      //}
      //else
      //{
      //  DestinationTile = null;
      //}
    }

    if (NextTile?.Item?.Type == Item.Door)
    {
      if (NextTile.Item.CurrentState == "Closed")
      {
        Debug.Log("Character at closed door");
        if (CurrentJob != null)
        {
          NextTile.Item.SetAction("OpenDoor");
        }
        else
        {
          //  no job, no need to open door
          DestinationTile = null;
          NextTile = null;
          return;
        }        
      }

      if (NextTile.Item.CurrentState != "Opened")
      {
        //  we need to wait
        return;
      }
    }


    if (NextTile != null)
    {
      float totalDistance = Tile.Distance(CurrentTile, NextTile);
      float incrementDistance = Speed * deltaTime;
      float percentage = incrementDistance / totalDistance;

      _movementCompletePercentage += percentage;


      if (_movementCompletePercentage >= 1)
      {
        _movementCompletePercentage = 0;
        CurrentTile = NextTile;
        NextTile = null;
      }
      else
      {
        IoC.Get<EventAggregator>().Publish(new CharacterUpdatedEvent { Character = this });
      }
    }
  }

  public bool AssignJob(Job job)
  {
    if (CurrentJob != null)
    {
      Debug.LogError($"Already has job");
      return false;
    }

    CurrentJob = job;
    return true;

    ////  get resource for job

    //var resource = IoC.Get<BuildingResourceController>().SelectResourcePile(job.FixedObject.Type);
    //if (resource == null)
    //{


    //}

    //job.FixedObject.Type


    //{
    //  //  first need to got to resource then to job

    //  if (SetDestination(job.ResourcePile.Tile))
    //  {
    //    CurrentJob = job;
    //    return true;
    //  }
    //}

    //return false;
  }

  public bool SetDestination(Tile tile)
  {
    //  TODO improve ? for direct neigbours

    // Debug.Log($"Charcter.SetDestination{tile.Position}");

    //if (tile.IsNeighbour(CurrentTile))
    //{
    //  DestinationTile = tile;
    //  NextTile = DestinationTile;
    //}
    //else
    //{
    if (tile.Type == Tile.TileType.Floor)
    {
      //  Need to find path to
      var pathfinder = new PathFinding();
      _path = pathfinder.FindPath(CurrentTile, tile);

      if (_path != null)
      {
        DestinationTile = tile;
      }
    }
    // }

    return DestinationTile != null;
  }
  #endregion

}
