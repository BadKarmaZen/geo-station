using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class BuildingResourceUpdatedEvent : Event
{
  public BuildingResource Resource { get; set; }
}

public class BuildingResource
{
  private static long NextResourceId = 1;
  public static void Initialize(long nextResourceId) => NextResourceId = nextResourceId;

  #region Properties

  public long Id { get; private set; }
  public string Type { get; set; }

  //  Amount available in the resource pile
  public int Amount { get; set; }

  //  amount requested and reserved by job controller
  public int ReservedBySystem { get; set; }

  //  Amount Reserved by a worker
  public int ReservedByWorker { get; set; }

  //public int AmountLeft => Amount - Reserved;

  public Tile Tile { get; set; }
  public World World { get; internal set; }

  #endregion

  #region Construction

  public BuildingResource(string type, Tile tile, int amount)
  {
    Id = NextResourceId++;
    Type = type;
    Amount = amount;
    ReservedByWorker = 0;
    Tile = tile;
  }

  #endregion

  #region Methods

  public bool CanTakeResource()
  {
    Debug.Log($"BuildingResource.CanTakeResource: {Amount}, {ReservedBySystem}, {ReservedByWorker}");
    return Amount - ReservedByWorker > 0;
  }

  public void Reserve()
  {
    Debug.Log($"BuildingResource.Reserve: {Amount}, {ReservedBySystem}, {ReservedByWorker}");
    ReservedByWorker++;
  }

  public void ReserveBySystem()
  {
    Debug.Log($"BuildingResource.ReserveBySystem: {Amount}, {ReservedBySystem}, {ReservedByWorker}");
    ReservedBySystem++;
  }

  internal void TakeResource()
  {
    Amount--;
    ReservedByWorker--;

    Debug.Log($"BuildingResource.TakeResource: {Amount} => {Amount} - {ReservedByWorker}");

    if (Amount == 0)
    {
      //  TODO something
     // IoC.Get<World>().GetInventory().TakeReser
      //World.RemoveBuildingResource(this);
      //new BuildingResourceUpdatedEvent { Resource = this }.Publish();
    }
  }

  #endregion

  #region Save/Load

  public BuildingResource(BuildingResourceData data, World world)
  {
    Id = data.id;
    Type = data.type;
    Tile = world.GetTile(data.x, data.y);
    Tile.ResourcePile = this;
    World = world;
    Amount = data.amount;
    ReservedByWorker = data.amount_reserved;

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
    amount_reserved = ReservedByWorker
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
}