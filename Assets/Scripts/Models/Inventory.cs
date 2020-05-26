using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Inventory
{
  #region Members

  private List<BuildingResource> _resourceInventory = new List<BuildingResource>();
  private List<BuildingResource> _orderedResources = new List<BuildingResource>();

  #endregion

  #region Methods

  //  returns amount available for direct use by workers
  private void DetailedLog(string log,  string resource)
  {
    var available = _resourceInventory.Where(r => r.Type == resource).Sum(r => r.Amount);
    var reserved = _resourceInventory.Where(r => r.Type == resource).Sum(r => r.ReservedBySystem);

    var ordered = _orderedResources.Where(r => r.Type == resource).Sum(r => r.Amount);
    var system_reserved = _orderedResources.Where(r => r.Type == resource).Sum(r => r.ReservedBySystem);

    Debug.Log($"Inventory.{log}: available({available},{reserved}) => {available - reserved}, ordered({ordered},{system_reserved}) => {ordered - system_reserved}");
  }

  public int GetAvailableAmount(string resource)
  {
    var available = _resourceInventory.Where(r => r.Type == resource).Sum(r => r.Amount - r.ReservedBySystem);

    return available;
  }

  public int GetAvailableAmountForWorkers(string resource)
  {
    var available = _resourceInventory.Where(r => r.Type == resource).Sum(r => r.Amount - r.ReservedByWorker);

    return available;
  }

  //  returns available amount + ordered amount
  public int GetOrderedAmount(string resource)
  {
    DetailedLog(nameof(GetOrderedAmount), resource);
    var ordered = _orderedResources.Where(r => r.Type == resource).Sum(r => r.Amount - r.ReservedBySystem);

    return ordered;
  }

  public void AddOrderResourceToInventory(BuildingResource resource)
  {
    DetailedLog(nameof(AddOrderResourceToInventory) + ".Enter", resource.Type);
    _orderedResources.Remove(resource);
    _resourceInventory.Add(resource);
    DetailedLog(nameof(AddOrderResourceToInventory) + ".Leave", resource.Type);
  }

  //  the resource is empty 
  //  remove it from the world
  internal void RemoveResource(BuildingResource buildingResource)
  {
    _resourceInventory.Remove(buildingResource);
    new BuildingResourceUpdatedEvent { Resource = buildingResource }.Publish();
  }

  //public void ReserveResourceForSystem(string resource, int amount = 1)
  //{
  //  DetailedLog(nameof(ReserveResourceForSystem) + ".Enter", resource);

  //  var resourcePile = _resourceInventory.FirstOrDefault(r => r.Type == resource && (r.Amount - r.Reserved) > 0);
  //  if (resourcePile == null)
  //    resourcePile = _orderedResources.FirstOrDefault(r => r.Type == resource && (r.Amount - r.Reserved) > 0);

  //  resourcePile?.ReserveBySystem();
  //}

  public BuildingResource SelectBuildingResource(string resource)
  {
    DetailedLog(nameof(SelectBuildingResource) + ".Enter", resource);
    return _resourceInventory.FirstOrDefault(r => r.Type == resource && (r.Amount - r.ReservedByWorker) != 0);
  }

  internal void OrderResourceShipment(string resource)
  {
    DetailedLog(nameof(OrderResourceShipment) + ".Enter", resource);

    var order = new BuildingResource(resource, this, 20);

    order.ReserveResourceBySystem();
    _orderedResources.Add(order);    

    IoC.Get<ShipmentController>().OrderResourceShipment(order);

    DetailedLog(nameof(OrderResourceShipment) + ".Leave", resource);
  }

  //  Try Reserve Available Resource
  internal bool TryReserveAvailableResource(string resourceType)
  {
    if (GetAvailableAmount(resourceType) > 0)
    {
      ReserveAvailableResource(resourceType);
      return true;
    }
    
    //  no resources available
    return false;
  }

  private void ReserveAvailableResource(string resourceType)
  {
    var resource = _resourceInventory.First(res => res.Type == resourceType && (res.Amount - res.ReservedBySystem) > 0);
    resource.ReserveResourceBySystem();
  }

  internal bool TryReserveOrderedResource(string resourceType)
  {
    if (GetOrderedAmount(resourceType) > 0)
    {
      ReserveOrderedResource(resourceType);
      return true;
    }

    //  no resources available
    return false;
  }

  private void ReserveOrderedResource(string resourceType)
  {
    var resource = _orderedResources.First(res => res.Type == resourceType && (res.Amount - res.ReservedBySystem) > 0);
    resource.ReserveResourceBySystem();
  }


  #endregion
}
