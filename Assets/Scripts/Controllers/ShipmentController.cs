using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

public class ShipmentController : MonoBehaviour
{
  #region const

  enum State
  {
    OnEarth,
    Docking,
    Docked,
    WaitingForPickup,
    UnDocking,
  }

  #endregion

  #region Members

  private State _currentState = State.OnEarth;

  public float start_position_x;
  public float start_position_y;

  public Tile _dockedTile;

  public GameObject rocket;
  public float movementSpeed = 1;

  public float _movementCompletePercentage;
  public float _shipmentTimer = 10; //  every 10 seconds

  public Queue<BuildingResource> _shipmentRequest = new Queue<BuildingResource>();
  public Queue<BuildingResource> _currentShipment = new Queue<BuildingResource>();
  public List<BuildingResource> _waitingOnPickup = new List<BuildingResource>();

  public List<GameObject> Deliveries = new List<GameObject>();

  #endregion

  #region Unity

  void Awake()
  {
    IoC.RegisterInstance(this);
  }

  // Start is called before the first frame update
  void Start()
  {
    rocket.transform.position = new Vector3(start_position_x, start_position_y);

    foreach (var delivery in Deliveries)
    {
      delivery.SetActive(false);
    }
  }

  // Update is called once per frame
  void Update()
  {
    switch (_currentState)
    {
      case State.OnEarth:
        WaitingForShipment();
        break;
      case State.Docking:
        DockingInProgress();
        break;
      case State.Docked:
        Docked();
        break;
      case State.WaitingForPickup:
        WaitForPickup();
        break;
      case State.UnDocking:
        UndockingInProgress();
        break;
    }
  }

  #endregion

  #region Methods

  //  Order a resource shipment from earth
  public void OrderResourceShipment(BuildingResource resource)
  {
    //  Order resource shipment
    _shipmentRequest.Enqueue(resource);
  }

  public void PickUpResource(BuildingResource resource)
  {        
    _waitingOnPickup.Remove(resource);
    Deliveries[_waitingOnPickup.Count].SetActive(false);
  }

  #endregion

  #region Helpers

  private void Docked()
  {
    if (_currentShipment.Count != 0)
    {
      var deliveryTile = IoC.Get<World>().GetAllTiles(tile => tile.Type == TileType.Delivery && tile.ActiveJob == null).FirstOrDefault();

      if (deliveryTile != null)
      {
        var delivery = _currentShipment.Dequeue();
        delivery.Tile = deliveryTile;
        IoC.Get<JobController>().ScheduleDelivery(delivery, _dockedTile, deliveryTile);
        _waitingOnPickup.Add(delivery);
      }
    }
    else
    {
      Debug.Log("All deliveries planned"); 
      _shipmentTimer = 2;  //  reset
      _currentState = State.WaitingForPickup;
    }
  }

  private void WaitForPickup()
  {
    if (_waitingOnPickup.Count == 0)
    {
      _shipmentTimer -= Time.deltaTime;

      if (_shipmentTimer <= 0)
      {
        _shipmentTimer = 2;  //  reset

        Debug.Log("All resources are picked up");
        _currentState = State.UnDocking;

        //  TODO path finding issue
        _dockedTile.SetType(TileType.Docking);
      }
    }
  }

  private void UndockingInProgress()
  {
    float totalDistance = Mathf.Abs(start_position_x - _dockedTile.Position.x);
    float incrementDistance = movementSpeed * Time.deltaTime;
    float percentage = incrementDistance / totalDistance;

    _movementCompletePercentage -= percentage;
    float lerp = Mathf.Lerp(start_position_x, _dockedTile.Position.x, _movementCompletePercentage);
    
    rocket.transform.position = new Vector3(lerp, start_position_y);

    if (_movementCompletePercentage <= 0)
    {
      Debug.Log("Undocking Finished");

      _shipmentTimer = 10;
      _currentState = State.OnEarth;
    }
  }

  private void DockingInProgress()
  {
    float totalDistance = Mathf.Abs(start_position_x - _dockedTile.Position.x);
    float incrementDistance = movementSpeed * Time.deltaTime;
    float percentage = incrementDistance / totalDistance;

    _movementCompletePercentage += percentage;
    float lerp = Mathf.Lerp(start_position_x, _dockedTile.Position.x, _movementCompletePercentage);
    rocket.transform.position = new Vector3(lerp, start_position_y);

    if (_movementCompletePercentage >= 1)
    {
      Debug.Log("Docking Finished");
      _currentState = State.Docked;

      //  TODO path finding issue
      _dockedTile.SetType(TileType.Floor);
    }
  }

  private void WaitingForShipment()
  {
    _shipmentTimer -= Time.deltaTime;

    if (_shipmentTimer <= 0)
    {
      _shipmentTimer = 10;  //  reset

      //  Shipment is due, max delivery is 3
      //
      while (_shipmentRequest.Count != 0 && _currentShipment.Count < 2)
      {
        _currentShipment.Enqueue(_shipmentRequest.Dequeue());
      }

      if (_currentShipment.Count != 0)
      {
        Debug.Log("Something to ship: Start Docking");
        _movementCompletePercentage = 0;
        _currentState = State.Docking;
        _dockedTile = IoC.Get<World>().GetTile(51, 50);

        foreach (var delivery in Deliveries.Take(_currentShipment.Count))
        {
          delivery.SetActive(true);
        } 
      }
    }
  }

  #endregion
}
