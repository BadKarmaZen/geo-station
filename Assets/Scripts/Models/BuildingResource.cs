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
  #region Properties

  public long Id { get; set; }
  public string Type { get; set; }

  public int Amount { get; set; }
  public int AmountReserved { get; set; }
  public int AmountLeft => Amount - AmountReserved;

  public Tile Tile { get; set; }
  public World World { get; internal set; }

  #endregion

  #region Construction

  public BuildingResource(string type, Tile tile, int amount)
  {
    Type = type;
    Amount = amount;
    AmountReserved = 0;
    Tile = tile;
  }

  #endregion

  #region Methods

  public bool CanTakeResource()
  {
    Debug.Log($"BuildingResource.CanTakeResource: {AmountLeft} => {Amount} - {AmountReserved}");
    return AmountLeft > 0;
  }

  public void Reserve()
  {
    Debug.Log($"BuildingResource.Reserve: {AmountLeft} => {Amount} - {AmountReserved}");
    AmountReserved++;
  }

  internal void TakeResource()
  {
    Amount--;
    AmountReserved--;

    Debug.Log($"BuildingResource.TakeResource: {AmountLeft} => {Amount} - {AmountReserved}");

    if (Amount == 0)
    {
      World.RemoveBuildingResource(this);
      new BuildingResourceUpdatedEvent { Resource = this }.Publish();
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
    AmountReserved = data.amount_reserved;
  }

  public BuildingResourceData ToData() => new BuildingResourceData
  {
    id = Id,
    type = Type,
    x = Tile.Position.x,
    y = Tile.Position.y,
    amount = Amount,
    amount_reserved = AmountReserved
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