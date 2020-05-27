using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;


/// <summary>
/// This controller manages the Resources
/// </summary>
public class InventoryController : BaseController
{
  #region Members

  private List<Tile> _occupiedTiles = new List<Tile>();
  private List<Tile> _unoccupiedTiles = new List<Tile>();

  #endregion


  #region Unity

  void Awake()
  {
    IoC.RegisterInstance(this);
  }

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
  }

  #endregion

  #region Methods

  internal void ReserveResource(string resourceType)
  {
    var inventory = IoC.Get<World>().GetInventory();

    Log($"try to reserve the resource ({resourceType}) for the job");

    if (inventory.TryReserveAvailableResource(resourceType) == false)
    {
      Log($"we do not have enough resources ({resourceType}) at the base");

      if (inventory.TryReserveOrderedResource(resourceType) == false)
      {
        Log($"no resources in the back log. order some new resources ({resourceType})");

        inventory.OrderResourceShipment(resourceType);
      }
    }
  }

  internal bool HaveEnoughResourcesAtBase(string resourceType)
  {
    return IoC.Get<World>().GetInventory().GetAvailableAmountForWorkers(resourceType) > 0;
  }

  internal BuildingResource ReserveResourcePile(string resourceType)
  {
    Log(resourceType);
    var resourcePile = IoC.Get<World>().GetInventory().SelectBuildingResource(resourceType);

    if (resourcePile != null)
    {
      Log($"ReserveResourceByWorker {resourcePile.Tile.Position}");
      resourcePile.ReserveResourceByWorker();
    }
    else
    {
      LogError($"No resource availlable: Error in inventroy");
    }

    return resourcePile;
  }

  internal void LoadUI()
  {
    foreach (var resource in IoC.Get<World>().GetInventory().GetResourcesAtBase())
    {
      new BuildingResourceUpdatedEvent { Resource = resource }.Publish();

    }
  }

  internal void TakeResource(BuildingResource resource)
  {
    Log($"{resource.Type} @ {resource.Tile.Position}");

    resource.TakeResource();

    if (resource.IsDepleted)
    {
      Log("resource is depleted");

      IoC.Get<World>().GetInventory().RemoveResource(resource);
      resource.Tile.ResourcePile = null;

      _occupiedTiles.Remove(resource.Tile);
      _unoccupiedTiles.Add(resource.Tile);

      new BuildingResourceUpdatedEvent { Resource = resource }.Publish();
    }
  }

  internal Tile GetUnoccupiedDeliveryTile()
  {
    if (_unoccupiedTiles.Count != 0)
    {
      var deliveryTile = _unoccupiedTiles.First();

      Log($"found @ {deliveryTile.Position}");

      _unoccupiedTiles.Remove(deliveryTile);
      _occupiedTiles.Add(deliveryTile);

      return deliveryTile;
    }

    return null;
  }

  internal void UpdateTile(Tile tile, TileType oldType, TileType newType)
  {
    if (oldType == newType)
      return;

    if (oldType == TileType.Delivery)
    {
      //  We lost a place to put our deliveries
      RemoveDeliveryTile(tile);
    }
    else if (newType == TileType.Delivery)
    {
      //  we have gained an delivery tile
      AddDeliveryTile(tile);
    }
  }

  internal void AddDeliveryTile(Tile deliveryTile)
  {
    Log($"{deliveryTile.Position}");
    _unoccupiedTiles.Add(deliveryTile);
  }

  internal void RemoveDeliveryTile(Tile deliveryTile)
  {
    Log($"{deliveryTile.Position}");
    _unoccupiedTiles.Remove(deliveryTile);
    _occupiedTiles.Remove(deliveryTile);
  }

  #endregion
}