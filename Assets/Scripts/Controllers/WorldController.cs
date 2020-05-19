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

    _world = new World();
    IoC.RegisterInstance(_world);

    CreatePrototypes();

    //  TEST
    IoC.RegisterInstance(this);
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

  //internal void CreateResourcePile(string resourceType, Tile from)
  //{
    
  //}

  // Update is called once per frame
  void Update()
  {
    _world.Update(Time.deltaTime);
  }

  #region Helpers
  private void CreatePrototypes()
  {
    var factory = IoC.Get<ObjectFactory>();

    factory.CreatePrototype("Wall", 0f);  //  0 => non movable object
    factory.CreatePrototype("Door", 1.2f, "Wall", (tile, type) =>
    {
      var n = _world.GetTile(tile.Position.GetNorth());
      var s = _world.GetTile(tile.Position.GetNorth());
      if (n?.FixedObject?.Type == type && s?.FixedObject?.Type == type)
      {
        return true;
      }

      var e = _world.GetTile(tile.Position.GetEast());
      var w = _world.GetTile(tile.Position.GetWest());
      if (e?.FixedObject?.Type == type && w?.FixedObject?.Type == type)
      {
        return true;
      }

      return false; 
    });
  }

  #endregion
}
