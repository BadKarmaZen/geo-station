using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingResourceUpdatedEvent : Event
{
  public BuildingResource Resource { get; set; }
}

public class BuildingResource
{
  //  todo debug
  static int nextId = 1;

  public int Id { get; private set; }

  public string Type { get; private set; }
  public int Amount { get; private set; }
  public int AmountReserved { get; private set; }
  public int AmountLeft => Amount - AmountReserved;
  public Tile Tile { get; set; }

  public BuildingResource(string type, Tile tile,int amount = 5)
  {
    Id = nextId++;
    Type = type;
    Amount = amount;
    AmountReserved = 0;
    Tile = tile;

    Debug.Log($"BuildingResource.Create: {Type}_{Id}.(a = {Amount}, r={AmountReserved}) => {AmountLeft}");
    new BuildingResourceUpdatedEvent { Resource = this }.Publish();
  }

  public void Reserve()
  {
    AmountReserved++;

    Debug.Log($"BuildingResource.Reserve: {Type}_{Id}.(a = {Amount}, r={AmountReserved}) => {AmountLeft}");
  }

  internal void TakeResource()
  {
    Amount--;
    AmountReserved--;

    Debug.Log($"BuildingResource.TakeResource: {Type}_{Id}.(a = {Amount}, r={AmountReserved}) => {AmountLeft}");

    if (Amount == 0)
    {
      Debug.Log($"BuildingResource.TakeResource => notify");
      new BuildingResourceUpdatedEvent { Resource = this }.Publish();
    }
  }
}
