using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

public class Inventory : ObjectBase
{
  #region Members

  private List<BuildingResource> _resourcesAtBase = new List<BuildingResource>();
  private List<BuildingResource> _resourcesOrdered = new List<BuildingResource>();

  #endregion

  #region construction

  public Inventory()
  {
  }

  #endregion

  #region Methods

  public int GetAvailableAmount(string resource)
  {
    var available = _resourcesAtBase.Where(r => r.Type == resource).Sum(r => r.Amount - r.ReservedBySystem);

    return available;
  }

  public int GetAvailableAmountForWorkers(string resource)
  {
    var available = _resourcesAtBase.Where(r => r.Type == resource).Sum(r => r.Amount - r.ReservedByWorker);

    return available;
  }

  //  returns available amount + ordered amount
  public int GetOrderedAmount(string resource)
  {
    var ordered = _resourcesOrdered.Where(r => r.Type == resource).Sum(r => r.Amount - r.ReservedBySystem);

    return ordered;
  }

  public void AddOrderResourceToInventory(BuildingResource resource)
  {
    _resourcesOrdered.Remove(resource);
    _resourcesAtBase.Add(resource);
  }

  //  the resource is empty 
  //  remove it from the world
  internal void RemoveResource(BuildingResource buildingResource)
  {
    _resourcesAtBase.Remove(buildingResource);
    new BuildingResourceUpdatedEvent { Resource = buildingResource }.Publish();
  }

  public BuildingResource SelectBuildingResource(string resource)
  {
    return _resourcesAtBase.FirstOrDefault(r => r.Type == resource && (r.Amount - r.ReservedByWorker) != 0);
  }

  internal void OrderResourceShipment(string resource)
  {
    var order = new BuildingResource(resource, 20);

    order.ReserveResourceBySystem();
    _resourcesOrdered.Add(order);

    IoC.Get<ShipmentController>().OrderResourceShipment(order);
  }

  internal IEnumerable<BuildingResource> GetResourcesAtBase() => _resourcesAtBase;

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
    var resource = _resourcesAtBase.First(res => res.Type == resourceType && (res.Amount - res.ReservedBySystem) > 0);
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
    var resource = _resourcesOrdered.First(res => res.Type == resourceType && (res.Amount - res.ReservedBySystem) > 0);
    resource.ReserveResourceBySystem();
  }


  #endregion

  #region Save/Load

  public Inventory(InventoryData data, World world)
  {
    _resourcesAtBase = data.resourcesAtBase.Select(resource => new BuildingResource(resource, world)).ToList();
    _resourcesOrdered = data.resourcesOrdered.Select(resource => new BuildingResource(resource, world)).ToList();
  }

  public InventoryData ToData() => new InventoryData
  {
    resourcesAtBase = _resourcesAtBase.Select(resource => resource.ToData()).ToList(),
    resourcesOrdered = _resourcesOrdered.Select(resource => resource.ToData()).ToList(),
  };

  internal BuildingResource LoadResource(long resourceId)
  {
    var resourcePile = _resourcesAtBase.FirstOrDefault(resource => resource.Id == resourceId);
    if (resourcePile != null)
    {
      Debug.LogError($"Cannot find resource ({resourceId}) in _resourcesAtBase");
    }
    return resourcePile;
  }

  #endregion
}


[Serializable]
public class InventoryData
{
  public List<BuildingResourceData> resourcesAtBase;
  public List<BuildingResourceData> resourcesOrdered;
}
