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
  static long nextId = 1;

  public long Id { get; set; }
  public string Type { get; set; }
  public int Amount { get; set; }
  public int AmountReserved { get; set; }
  public int AmountLeft => Amount - AmountReserved;
  public Tile Tile { get; set; }

  public BuildingResource(string type, Tile tile,int amount = 5)
  {
    Id = nextId++;
    Type = type;
    Amount = amount;
    AmountReserved = 0;
    Tile = tile;

    //Debug.Log($"BuildingResource.Create: {Type}_{Id}.(a = {Amount}, r={AmountReserved}) => {AmountLeft}");    
  }

  public void Reserve()
  {
    AmountReserved++;

    //Debug.Log($"BuildingResource.Reserve: {Type}_{Id}.(a = {Amount}, r={AmountReserved}) => {AmountLeft}");
  }

  internal void TakeResource()
  {
    Amount--;
    AmountReserved--;

    //Debug.Log($"BuildingResource.TakeResource: {Type}_{Id}.(a = {Amount}, r={AmountReserved}) => {AmountLeft}");

    if (Amount == 0)
    {
      //Debug.Log($"BuildingResource.TakeResource => notify");
      new BuildingResourceUpdatedEvent { Resource = this }.Publish();
    }
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