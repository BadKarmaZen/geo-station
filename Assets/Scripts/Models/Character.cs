using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using UnityEngine.Monetization;
using System.Runtime.CompilerServices;

#region Events

public class CharacterEvent : Event
{
  public Character Character { get; set; }
}

public class CharacterCreatedEvent : CharacterEvent { }

public class CharacterUpdatedEvent : CharacterEvent
{
  public bool Hand { get; internal set; }
}

#endregion


public enum CharacterState
{
  //  Idle
  Idle,
  WanderAround,

  //  Delivery job
  GotoPickUpTile,
  TakeDelivery,
  GotoDeliveryTile,
  DropDelivery,

  //  Construction Job
  SelectResourcePile,
  GotoResourcePile,
  TakeResource,
  GotoJobTile,
  Constructing,
}


public class Character : ObjectBase
{
  #region Members

  private float _handMovement;
  private float _movementCompletePercentage; //	goes from 0 to 1
  private Stack<Tile> _path = null;

  private CharacterState _currentState;

  #endregion

  #region Properties

  public float Speed { get; set; }
  public World World { get; set; }
  public Tile CurrentTile { get; set; }
  public Tile DestinationTile { get; set; }
  public Tile NextTile { get; set; }
  public Job CurrentJob { get; set; }
  public BuildingResource SelectedResourcePile { get; set; }
  public bool HasResource { get; set; }


  //  Ui Updates
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

  #endregion

  #region Construction

  public Character(Tile tile, World world, float speed)
  {
    CurrentTile = tile;
    World = world;
    Speed = speed;

    DestinationTile = null;
    _currentState = CharacterState.Idle;
  }

  #endregion

  #region Methods

  public void Update(float deltaTime)
  {
    //UpdateTask(deltaTime);
    UpdateState(deltaTime);
    UpdateMovement(deltaTime);
  }

  private void SetState(CharacterState newState)
  {
    if (newState != CharacterState.WanderAround &&
        newState != CharacterState.Idle)
    {
      Log($"Set state from {_currentState} to {newState}");
    }

    _currentState = newState;
  }

  private void UpdateState(float deltaTime)
  {
    switch (_currentState)
    {
      default:
      case CharacterState.Idle:
        //  nothing to do 
        MoveIdle();
        break;
      case CharacterState.WanderAround:
        if (CurrentTile == DestinationTile)
        {
          SetState(CharacterState.Idle);
        }
        else if (DestinationTile == null)
        {
          SetState(CharacterState.Idle);
        }
        break;

      case CharacterState.GotoPickUpTile:
        if (CurrentTile == (Tile)CurrentJob.Tag)
        {
          Log("we are at pickup location");
          SetState(CharacterState.TakeDelivery);
        }
        else
          if (DestinationTile == null)
        {
          Log("get path to pickup location");
          SetDestination(CurrentJob.Tag as Tile);
        }
        break;


      case CharacterState.TakeDelivery:

        if (CurrentTile == (Tile)CurrentJob.Tag)
        {
          IoC.Get<ShipmentController>().PickUpResource(CurrentJob.Delivery);
          DestinationTile = null;
          SetState(CharacterState.GotoDeliveryTile);
        }
        else
        {
          LogError("not at the pick up place");
          DestinationTile = null;
          SetState(CharacterState.GotoPickUpTile);
        }
        break;


      case CharacterState.GotoDeliveryTile:
        if (CurrentTile == CurrentJob.Tile)
        {
          Log("at delivery");
          SetState(CharacterState.DropDelivery);
        }
        else if (DestinationTile == null)
        {
          Debug.Log("set path to delivery tile");
          SetDestination(CurrentJob.Tile);
        }
        break;

      case CharacterState.DropDelivery:

        Log("DropDelivery");
        IoC.Get<World>().GetInventory().AddOrderResourceToInventory(CurrentJob.Delivery);

        CurrentJob.Tile.ResourcePile = CurrentJob.Delivery;
        new BuildingResourceUpdatedEvent { Resource = CurrentJob.Delivery }.Publish();

        CurrentJob = null;
        DestinationTile = null;
        SetState(CharacterState.Idle);
        break;

      case CharacterState.SelectResourcePile:

        SelectedResourcePile = IoC.Get<InventoryController>().ReserveResourcePile(CurrentJob.Item);
        
        if (SelectedResourcePile != null)
        {
          SetState(CharacterState.GotoResourcePile);
        }

        break;

      case CharacterState.GotoResourcePile:
        //if (HasResource)
        //{
        //  Debug.LogError("already has resource");
        //  _currentState = CharacterState.GotoJobTile;
        //}
        //else
        if (CurrentTile == SelectedResourcePile.Tile)
        {
          Log("we are at resource location");
          SetState(CharacterState.TakeResource);
        }
        else if (DestinationTile == null)
        {
          Log("Need to get resource");
          SetDestination(SelectedResourcePile.Tile);
        }

        break;
      case CharacterState.TakeResource:
        if (CurrentTile != SelectedResourcePile.Tile)
        {
          LogError("We are not at the resource pile");
          DestinationTile = null;
          SetState(CharacterState.GotoResourcePile);
        }
        else
        {
          IoC.Get<InventoryController>().TakeResource(SelectedResourcePile);

          SetState(CharacterState.GotoJobTile);
          DestinationTile = null;
        }
        break;
      case CharacterState.GotoJobTile:

        //  are we at job tile
        if (CurrentTile == CurrentJob.Tile)
        {
          SetState(CharacterState.Constructing);
        }
        else if (DestinationTile == null)
        {
          SetDestination(CurrentJob.Tile);
        }

        break;
      case CharacterState.Constructing:

        if (CurrentJob.ProcessJob(deltaTime))
        {
          //  jobe is done
          //HasResource = false;
          IoC.Get<JobController>().JobIsFinished(CurrentJob);

          CurrentJob = null;
          DestinationTile = null;
          SetState(CharacterState.Idle);
        }
        else
        {
          //  animate handes
          _handMovement += 0.03f;
          if (_handMovement > 1.0f)
          {
            _handMovement = 0f;
          }

          new CharacterUpdatedEvent { Character = this, Hand = true }.Publish();
        }

        break;
    }
  }

  private void MoveIdle()
  {
    if (DestinationTile != null)
    {
      LogError("MoveIdle. already have destination");
      return;
    }

    var tiles = World.GetNeighbourTiles(CurrentTile).Where(tile => tile.Type == TileType.Floor || tile.Type == TileType.Delivery).ToList();
    if (tiles.Count == 0)
    {
      LogError("MoveIdle. I'm at location hwer i cannot move anywhere");
      return;
    }

    var foo = UnityEngine.Random.Range(0, tiles.Count);
    SetDestination(tiles[foo]);

    SetState(CharacterState.WanderAround);
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


    }

    //  Enter behaviour
    if (NextTile?.IsEnterable() == Enterable.Never)
    {
      //  cannot enter
      DestinationTile = NextTile = null;
    }
    else if (NextTile?.IsEnterable() == Enterable.Soon)
    {
      //  wait until we can enter the tile
      return;
    }

    if (NextTile != null)
    {
      float totalDistance = Position.Distance(CurrentTile.Position, NextTile.Position);
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

  public void AssignJob(Job job)
  {
    Log($"Assign new job");

    if (CurrentJob != null)
    {
      LogError($"Already has job");
      return;
    }

    CurrentJob = job;
    CurrentJob.AcceptJob();

    //  What kind of job
    if (CurrentJob.Type == JobType.Delivery)
    {
      SetState(CharacterState.GotoPickUpTile);
    }
    else
    {
      SetState(CharacterState.SelectResourcePile);
    }
  }

  public bool SetDestination(Tile tile)
  {
    if (tile.Type != TileType.Space)
    {
      //  Need to find path to
      var pathfinder = new PathFinding();
      _path = pathfinder.FindPath(CurrentTile, tile);

      if (_path != null)
      {
        DestinationTile = tile;
      }
      else
      {
        if (_currentState != CharacterState.WanderAround &&
            _currentState != CharacterState.Idle)
        {
          Log($"No Path found [{_currentState}] from {CurrentTile.Position} to {tile.Position}");
        }
      }
    }

    return DestinationTile != null;
  }

  #endregion

  #region Save/Load

  public Character(CharacterData data, World world)
  {
    World = world;
    Speed = data.speed;

    CurrentTile = world.GetTile(data.x, data.y);
    HasResource = data.has_resource;

    CurrentJob = world.GetJobs().FirstOrDefault(job => job.Id == data.job_id);
    SelectedResourcePile = world.GetBuildiongResource().FirstOrDefault(resource => resource.Id == data.resource_id);
  }

  public CharacterData ToData() => new CharacterData
  {
    speed = Speed,
    x = CurrentTile.Position.x,
    y = CurrentTile.Position.y,
    has_resource = HasResource,
    job_id = CurrentJob?.Id ?? 0,
    resource_id = SelectedResourcePile?.Id ?? 0
  };

  #endregion
}

[Serializable]
public class CharacterData
{
  //public string name;
  public float speed;

  public int x;
  public int y;
  public bool has_resource;
  public long job_id;
  public long resource_id;
}