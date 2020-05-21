using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUpdateEvent : Event
{
  public bool Reset { get; set; }
  public bool Paused { get; set; }
  public bool UnPaused { get; set; }
}

public class WorldController : MonoBehaviour
{
  #region Member

  World _world;
  Dictionary<(int x, int y), GameObject> _tiles = new Dictionary<(int x, int y), GameObject>();

  #endregion

  #region Unity

  public void Awake()
  {
    Debug.Log("WorldController.Awake");

    IoC.RegisterType<GameObjectFactory>();
    IoC.RegisterType<ObjectFactory>();
    IoC.RegisterInstance(this);

    //  TODO : improve to some core location
    CreatePrototypes();
  }

  internal World GetWorld() => _world;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    _world?.Update(Time.deltaTime);
  }

  #endregion

  #region Methods

  public void NewWorld()
  {
    //  Creates a new world
    //

    new WorldUpdateEvent { Reset = true }.Publish();

    _world = new World();

    Debug.Log("CreateWorldGame");

    for (int x = 0; x < _world.Size.width; x++)
    {
      for (int y = 0; y < _world.Size.height; y++)
      {
        var tile = _world.GetTile(new Position(x, y));
        if (tile == null) continue;

        new CreateTileEvent { Tile = tile }.Publish();
      }
    }

    Debug.Log("CreateWorldGame.Done");

    //	center camera to the middle of the world view
    //
    var center = new Position(_world.Size.width / 2, _world.Size.height / 2);
    Camera.main.transform.position = new Vector3(_world.Size.width / 2, _world.Size.height / 2, Camera.main.transform.position.z);

    var centerTile = _world.GetTile(center);

    for (int i = 0; i < 1; i++)
    {
      var worker = _world.CreateCharacter(centerTile);
      IoC.Get<EventAggregator>().Publish(new CharacterCreatedEvent { Character = worker });
      IoC.Get<JobController>().AddWorker(worker);
    }

  }

  #endregion


  #region Helper Methods

  internal IEnumerable<Tile> GetNeighbourTiles(Tile tile, Func<Tile, bool> predicate = null)
  {
    var tiles = _world.GetNeighbourTiles(tile);
    if (predicate != null)
    {
      tiles = tiles.Where(predicate);
    }
    return tiles;
  }

  public IEnumerable<Tile> GetTiles(Tile fromTile, Tile toTile)
  {
    if (fromTile == null || toTile == null)
      yield break;

    foreach (var position in Position.GetPositions(fromTile.Position, toTile.Position))
    {
      var tile = _world.GetTile(position);
      if (tile != null)
      {
        yield return tile;
      }
    }
  }

  internal Tile GetTile(Position position) => _world?.GetTile(position);

  #endregion


  #region internal Helpers

  private void CreatePrototypes()
  {
    var objectFactory = IoC.Get<ObjectFactory>();

    //  wall
    var wallFactory = objectFactory.CreateFactory(new Item(Item.Wall, 0f));
    wallFactory.AddBuildRule((tile, factory) =>
   {
     //  a wall must be build on a floor tile
     return tile.Type == Tile.TileType.Floor;
   });

    wallFactory.BuildSound = "welding";

    var doorPrototype = new Item(Item.Door, 1.2f);
    var doorFactory = objectFactory.CreateFactory(doorPrototype, Item.Wall);

    doorPrototype.Parameters["openness"] = 0f;
    doorPrototype.Parameters["is_opening"] = true;
    doorPrototype.UpdateActions += ItemActions.DoorUpdateAction;
    doorPrototype.IsEnterable = ItemActions.DoorIsEnterable;

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
