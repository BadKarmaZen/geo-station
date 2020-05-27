using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class BuildingResourceUpdatedEvent : Event
{
  public BuildingResource Resource { get; set; }
}

public class BuildingResource : ObjectBase
{
  #region Identifier

  private static long NextResourceId = 1;
  public static void Initialize(long nextResourceId) => NextResourceId = nextResourceId;
  public long Id { get; private set; }

  #endregion

  #region Properties

  public string Type { get; set; }

  //  Amount available in the resource pile
  public int Amount { get; set; }

  //  amount requested and reserved by job controller, so that we don't order to much
  public int ReservedBySystem { get; set; }

  //  Amount Reserved by a worker, so that 2 workers don't fight over the same resource
  public int ReservedByWorker { get; set; }

  public Tile Tile { get; set; }
  public bool IsDepleted => Amount == 0;

  #endregion

  #region Construction
  public BuildingResource(string type, int amount)
  {
    Id = NextResourceId++;
    Type = type;
    Amount = amount;
    ReservedByWorker = 0;
    ReservedBySystem = 0;
  }

  //public BuildingResource(string type, Tile tile, int amount)
  //{
  //  Id = NextResourceId++;
  //  Type = type;
  //  Amount = amount;
  //  ReservedByWorker = 0;
  //  ReservedBySystem = 0;
  //  Tile = tile;
  //}

  #endregion

  #region Methods

  

  public void ReserveResourceByWorker()
  {
    Log($"BuildingResource.ReserveByWorker: {Amount}, {ReservedByWorker}");
    ReservedByWorker++;
  }
  public void ReserveResourceBySystem()
  {
    Log($"BuildingResource.ReserveBySystem: {Amount}, {ReservedBySystem}");
    ReservedBySystem++;
  }

  internal void TakeResource()
  {
    Amount--;
    ReservedByWorker--;
    ReservedBySystem--;
  }

  #endregion

  #region Save/Load

  public BuildingResource(BuildingResourceData data, World world)
  {
    Id = data.id;
    Type = data.type;
    Tile = world.GetTile(data.x, data.y);
    Tile.ResourcePile = this;
    Amount = data.amount;
    ReservedByWorker = data.amount_reserved;
    ReservedBySystem = data.amount_system_reserved;

    if (Id >= NextResourceId)
    {
      Initialize(Id + 1);
    }
  }

  public BuildingResourceData ToData() => new BuildingResourceData
  {
    id = Id,
    type = Type,
    x = Tile.Position.x,
    y = Tile.Position.y,
    amount = Amount,
    amount_reserved = ReservedByWorker,
    amount_system_reserved = ReservedBySystem,
  };

  #endregion
}

[Serializable]
public class BuildingResourceData
{
  public long id;
  public string type;
  public int x;
  public int y;
  public int amount;
  public int amount_reserved;
  public int amount_system_reserved;
}