using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
  #region Unity


  #endregion

  #region Member

  World _world;

  Dictionary<(int x, int y), GameObject> _tiles = new Dictionary<(int x, int y), GameObject>();

  //  TODO TEST
  //public Character AstroBoy;

  #endregion

  public void Awake()
  {
    Debug.Log("WorldController.Awake");

    IoC.RegisterType<GameObjectFactory>();
    IoC.RegisterType<ObjectFactory>();
    IoC.RegisterInstance(this);

    _world = new World();
    IoC.RegisterInstance(_world);

    CreatePrototypes();
  }



  // Start is called before the first frame update
  void Start()
  {
    //	center camera to the middle of the world view
    //
    var center = new Position(_world.Size.width / 2, _world.Size.height / 2);
    Camera.main.transform.position = new Vector3(_world.Size.width / 2, _world.Size.height / 2, Camera.main.transform.position.z);

    var centerTile = _world.GetTile(center);

    for (int i = 0; i < 5; i++)
    {
      var worker = _world.CreateCharacter(centerTile);
      IoC.Get<EventAggregator>().Publish(new CharacterCreatedEvent { Character = worker });
      IoC.Get<JobController>().AddWorker(worker);
    }
  }
  
  // Update is called once per frame
  void Update()
  {
    _world.Update(Time.deltaTime);
  }

  #region Helpers
  private void CreatePrototypes()
  {
    var objectFactory = IoC.Get<ObjectFactory>();

    //  wall
    var wallFactory = objectFactory.CreateFactory(new Item(Item.Wall, 0f));
    wallFactory.AddBuildRule( (tile, factory) =>
    {
      //  a wall must be build on a floor tile
      return tile.Type == Tile.TileType.Floor;
    });

    wallFactory.BuildSound = "welding";

    var doorPrototype = new Item(Item.Door, 1.2f) { CurrentState = "Closed" };
    var doorFactory = objectFactory.CreateFactory(doorPrototype, Item.Wall);

    doorFactory.AddAction(action: "OpenDoor", from: "Closed", transition: "Opening", to: "Opened", (Item item, float deltaTime) => 
    {
      Debug.Log($"Opening ({item.ActionTime})");
      item.ActionTime += deltaTime;

      if (item.ActionTime >= 1f)  //  1 second
      {
        Debug.Log($"Open.Done");
        return true;
      }

      return false; 
    }, idle : "CloseDoor");

    doorFactory.AddAction(action: "CloseDoor", from: "Opened", transition: "Closing", to: "Closed", (Item item, float deltaTime) =>
    {
      Debug.Log($"Closing ({item.ActionTime})");
      item.ActionTime += deltaTime;

      if (item.ActionTime >= 1f)  //  1 second
      {
        Debug.Log($"Open.Closed");
        return true;
      }

      return false;
    });


    doorFactory.AddBuildRule((tile, factory) => 
    {
      if (tile.Type != Tile.TileType.Floor)
        return false;

      //  door must have wall to north & south, or east & west
      var north = _world.GetTile(tile.Position.GetNorth());
      var south = _world.GetTile(tile.Position.GetSouth());

      if (factory.IsValidNeighbour(north?.Item?.Type) &&
          factory.IsValidNeighbour(south?.Item?.Type))
      {
        return true;
      }

      var west = _world.GetTile(tile.Position.GetWest());
      var east = _world.GetTile(tile.Position.GetEast());

      if (factory.IsValidNeighbour(west?.Item?.Type) &&
          factory.IsValidNeighbour(east?.Item?.Type))
      {
        return true;
      }

      return false;
    });

    doorFactory.BuildSound = "welding";
  }

  #endregion
}
