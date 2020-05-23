using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

#region Events

public class TileEvent : Event
{
  public Tile Tile { get; set; }
}

public class CreateTileEvent : TileEvent { }

public class UpdateTileEvent : TileEvent { }

public class LoadTileEvent : TileEvent { }

public class ItemEvent : Event
{
  public Item Item { get; set; }
}

public class CreateItemEvent : ItemEvent { }

public class UpdateItemEvent : ItemEvent { }

#endregion

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

  public Position Position { get; set; }
  
  //  a tile can only be in 1 room
  public Room Room { get; set; }

  public TileType Type { get; set; } = TileType.Space;

  public Item Item { get; set; }

  public Job ActiveJob { get; set; }

  public World World { get; set; }

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

      new UpdateTileEvent { Tile = this }.Publish();

      //IoC.Get<EventAggregator>().Publish(new TileUpdateEvent { Tile = this });
    }
  }

  public void InstallItem(Item item)
  {
    Item = item;
    ActiveJob = null;

    //  Add item to world collection ?
    World.AddItem(item);
    new CreateItemEvent { Item = item }.Publish();
  }

  public Enterable IsEnterable()
  {
    return Item == null ? Enterable.Yes : Item.IsEnterable(Item);
  }

  public Tile GetNorthTile() => World.GetTile(Position.GetNorth());
  public Tile GetSouthTile() => World.GetTile(Position.GetSouth());
  public Tile GetEastTile() => World.GetTile(Position.GetEast());
  public Tile GetWestTile() => World.GetTile(Position.GetWest());

  #endregion

  #region Save/Load

  public void UpdateFrom(TileData data, World world)
  {
    Type = TileType.Floor;
    ActiveJob = world.GetJobs().FirstOrDefault(job => job.Id == data.job_id);
  }

  public TileData ToData()
  {
    return new TileData
    {
      x = Position.x,
      y = Position.y,
      item = Item?.Type,
      job_id = ActiveJob?.Id ?? 0
    };
  }

  #endregion
}

[Serializable]
public class TileData
{
  public int x;
  public int y;

  public string item;
  public long job_id;
}