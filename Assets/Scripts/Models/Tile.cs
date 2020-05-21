using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CreateTileEvent : Event
{
  public Tile Tile { get; set; }
}

public class TileUpdateEvent : Event
{
  public Tile Tile { get; set; }
}

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
  
  public Item Item { get; set; }

  //public MovableObject MovableObject { get; set; }


  public World World { get; set; }
  public Position Position { get; set; }
  public bool JobScheduled { get; internal set; }

  internal bool IsNeighbour(Tile tile)
  {
    return Position.IsNorthOf(tile.Position) || Position.IsEastOf(tile.Position) || Position.IsSouthOf(tile.Position) || Position.IsWestOf(tile.Position);
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

    //Item.Installing = false;
    JobScheduled = false;

    //  TODO Does this belong here
    new ItemUpdatedEvent { Item = item }.Publish();
  }

  #endregion

}
