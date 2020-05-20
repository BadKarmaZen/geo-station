using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileUpdateEvent : Event
{
  public Tile Tile { get; set; }
}

//public class FixedObjectUpdateEvent
//{
//  public FixedObject FixedObject { get; set; }
//  public bool UpdateOnly { get; set; }
//  public bool JobFailed { get; internal set; }
//}

public class ItemUpdatedEvent : Event
{
  public Item Item { get; set; }
  public bool UpdateOnly { get; set; }
}

public class Tile
{
  #region const, enum ...

  /// <summary>
  /// We can swith between space and floor
  /// Space: outer furniture: solar panel, satelite, antenna ...
  /// Floos: part of the space station: wall, door, computer, bed, ...
  /// </summary>
  public enum TileType { Space, Floor };

  #endregion

  #region Properties

  public TileType Type { get; protected set; } = TileType.Space;

  //  TODO
  //public FixedObject FixedObject { get; set; }
  public Item Item { get; set; }

  public MovableObject MovableObject { get; set; }


  public World World { get; set; }
  public Position Position { get; set; }
  public bool JobScheduled { get; internal set; }

  internal bool IsNeighbour(Tile tile)
  {
    return Position.IsNorthOf(tile.Position) || Position.IsEastOf(tile.Position) || Position.IsSouthOf(tile.Position) || Position.IsWestOf(tile.Position);
  }

  internal static float Distance(Tile tileA, Tile tileB)
  {
    return Mathf.Sqrt(Mathf.Pow(tileA.Position.x - tileB.Position.x, 2) + Mathf.Pow(tileA.Position.y - tileB.Position.y, 2));
  }

  #endregion

  #region Construction

  public Tile(World world, Position position)
  {
    World = world;
    Position = position;
  }

  #endregion

  #region Helpers

  public void SetType(TileType type)
  {
    if (Type != type)
    {
      Type = type;

      new TileUpdateEvent { Tile = this }.Publish();

      //IoC.Get<EventAggregator>().Publish(new TileUpdateEvent { Tile = this });
    }
  }

  public void InstallItemOnTile(Item item)
  {
    Item = item;

    //  Add item to world collection ?
    World.AddItem(item);

    //  TODO is this the best place ?

    Item.Installing = false;
    JobScheduled = false;

    //  TODO Does this belong here
    new ItemUpdatedEvent { Item = item }.Publish();
  }

  //public void InstallFixedObject(FixedObject fixedObject)
  //{
  //  //Debug.Log($"InstallFixedObject ({fixedObject})");

  //  JobScheduled = false;
  //  this.FixedObject = fixedObject;
  //  FixedObject.Installing = false;

  //  IoC.Get<EventAggregator>().Publish(new FixedObjectUpdateEvent { FixedObject = fixedObject });
  //}


  //public bool NorthOf(Tile tile)
  //{
  //  return Position == (tile.Position.x, tile.Position.y + 1);
  //}

  //internal bool EastOf(Tile tile)
  //{
  //  return Position == (tile.Position.x + 1, tile.Position.y);
  //}

  //internal bool SouthOf(Tile tile)
  //{
  //  return Position == (tile.Position.x, tile.Position.y - 1);
  //}

  //internal bool WestOf(Tile tile)
  //{
  //  return Position == (tile.Position.x - 1, tile.Position.y);
  //}

  //  Job failed
  //internal void CannotCompleteJob(FixedObject fixedObject)
  //{
  //  IoC.Get<EventAggregator>().Publish(new FixedObjectUpdateEvent { FixedObject = fixedObject, JobFailed = true });

  //  JobScheduled = false;
  //  FixedObject = null;
  //}



  #endregion

}
