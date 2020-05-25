using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Inventory
{
  #region Members

  private long _nextBuildingId = 1;
  private List<BuildingResource> _resourceInventory = new List<BuildingResource>();
  private List<BuildingResource> _orderedResources = new List<BuildingResource>();

  #endregion

  #region Methods

  public int GetAvailableAmount(string resource)
  {
    var available = _resourceInventory.Where(r => r.Type == resource).Sum(r => r.Amount - r.ReservedBySystem);
    var ordered = _orderedResources.Where(r => r.Type == resource).Sum(r => r.Amount - r.ReservedBySystem);

    return available;
  }

  public void AddOrderResources(string resource, int amount)
  {
    //  TODO
  }

  public void ReserveResourceForSystem(string resource, int amount = 1)
  {
    var resourcePile = _resourceInventory.FirstOrDefault(r => r.Type == resource && (r.Amount - r.ReservedByWorker) > 0);
    if (resourcePile == null)
      resourcePile = _orderedResources.FirstOrDefault(r => r.Type == resource && (r.Amount - r.ReservedByWorker) > 0);

    resourcePile?.ReserveBySystem();
  }

  public BuildingResource SelectBuildingResource(string resource)
  {
    return _resourceInventory.FirstOrDefault(r => r.Type == resource && (r.Amount - r.ReservedByWorker) != 0);
  }

  internal void OrderResourceShipment(string resourceType)
  {
    var order = new BuildingResource(resourceType, null, 20);
    _orderedResources.Add(order);

    IoC.Get<ShipmentController>().OrderResourceShipment(order);
  }

  #endregion
}
