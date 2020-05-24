using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ShipmentController : MonoBehaviour
{
  public float start_position_x;
  public float start_position_y;

  public GameObject rocket;
  public float movementSpeed = 1;
  public bool IsDocking;
  public bool IsUndocking;

  public float _movementCompletePercentage;
  public float _shipmentTimer = 10; //  every 10 seconds

  public Queue<string> _shipmentRequest = new Queue<string>();
  public List<BuildingResource> _buildingResources = new List<BuildingResource>();

  void Awake()
  {
    IoC.RegisterInstance(this);
  }

  // Start is called before the first frame update
  void Start()
  {
    rocket.transform.position = new Vector3(start_position_x, start_position_y);
  }

  // Update is called once per frame
  void Update()
  {
    _shipmentTimer -= Time.deltaTime;

    if (_shipmentTimer <= 0)
    {      
      _shipmentTimer = 10;  //  reset

      //  do action
      while (_shipmentRequest.Count != 0 && _buildingResources.Count < 2)
      {
        //  create the resource
        var resoure = IoC.Get<WorldController>().RequestShipment(_shipmentRequest.Dequeue());
        _buildingResources.Add(resoure);
      }

      if (_buildingResources.Count != 0)
      {
        Debug.Log("Start Docking");
        
        _movementCompletePercentage = 0;
        IsDocking = true;
      }
    }

    if (IsDocking)
    {
      float totalDistance = Mathf.Abs(start_position_x - 51);
      float incrementDistance = movementSpeed * Time.deltaTime;
      float percentage = incrementDistance / totalDistance;

      _movementCompletePercentage += percentage;
      float lerp = Mathf.Lerp(start_position_x, 51, _movementCompletePercentage);


      Debug.Log($"Docking @ {_movementCompletePercentage} => {lerp}");
      rocket.transform.position = new Vector3(lerp, start_position_y);

      if (_movementCompletePercentage >= 1)
      {
        Debug.Log("Docking Finished");
        IsDocking = false;
      }
    }
    else if (IsUndocking)
    {
      float totalDistance = Mathf.Abs(start_position_x - 51);
      float incrementDistance = movementSpeed * Time.deltaTime;
      float percentage = incrementDistance / totalDistance;

      _movementCompletePercentage -= percentage;
      float lerp = Mathf.Lerp(start_position_x, 51, _movementCompletePercentage);
      Debug.Log($"Undocking @ {_movementCompletePercentage} => {lerp}");

      rocket.transform.position = new Vector3(lerp, start_position_y);

      if (_movementCompletePercentage <= 0)
      {
        Debug.Log("Undocking Finished");
        IsUndocking = false;
      }
    }
  }

  public void RequestResources(string resource)
  {
    //  check free resources
    var controller = IoC.Get<WorldController>();
    var inventory = controller.GetInventory();

    if (inventory.GetAvailableAmount(resource) == 0)
    {
      //  Order resource shipment
      _shipmentRequest.Enqueue(resource);
    }
  }

  public void TakeShipment()
  {
    Debug.Log("Start UnDocking");
    _movementCompletePercentage = 1;
    IsUndocking = true;
  }
}
