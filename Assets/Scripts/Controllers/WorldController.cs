using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Unity.Collections;
using UnityEngine.UIElements;

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

  static string _filename;

  #endregion

  #region Unity

  public void Awake()
  {
    Debug.Log("WorldController.Awake");

    IoC.Initialize();

    IoC.RegisterType<GameObjectFactory>();
    IoC.RegisterType<ObjectFactory>();
    IoC.RegisterInstance(this);

    //  TODO : improve to some core location
    CreatePrototypes();
  }
  public void OnEnable()
  {
    Debug.Log("WorldController.OnEnable");

   
  }

  // Start is called before the first frame update
  void Start()
  {
    Debug.Log("WorldController.Start");

    if (_filename != null)
    {
      var json = File.ReadAllText(_filename);
      var saveGame = JsonUtility.FromJson<WorldData>(json);

      _world = World.LoadSaveGame(saveGame);

      //  World is create we need to create all the ui elemements

      LoadUiElements();
    }
  }

  private void LoadUiElements()
  {
    //  create tiles
    foreach (var tile in _world.GetAllTiles(t => t != null))
    {
      new CreateTileEvent { Tile = tile }.Publish();
      new TileUpdateEvent { Tile = tile }.Publish();
    }

    //  create characters
    foreach (var character in _world.GetCharacters())
    {
      new CharacterCreatedEvent { Character = character }.Publish();
    }

    //  create jobs
    foreach (var job in _world.GetJobs())
    {
      new JobUpdateEvent { Job = job }.Publish();
    }

    //  create resources
    foreach (var resource  in _world.GetBuildiongResource())
    {
      new BuildingResourceUpdatedEvent { Resource = resource }.Publish();
    }
  }

  // Update is called once per frame
  void Update()
  {
    _world?.Update(Time.deltaTime);
  }

  #endregion

  #region Methods

  internal World GetWorld() => _world;

  public void NewWorld()
  {
    //  Creates a new world
    //
    new WorldUpdateEvent { Reset = true }.Publish();

    _world = new World(100, 100);

    Debug.Log("CreateWorldGame");

    for (int x = 0; x < _world.Width; x++)
    {
      for (int y = 0; y < _world.Height; y++)
      {
        var tile = _world.GetTile(new Position(x, y));
        if (tile == null) continue;

        new CreateTileEvent { Tile = tile }.Publish();
      }
    }

    Debug.Log("CreateWorldGame.Done");

    //	center camera to the middle of the world view
    //
    var center = new Position(_world.Width / 2, _world.Height / 2);
    Camera.main.transform.position = new Vector3(_world.Width / 2, _world.Height / 2, Camera.main.transform.position.z);

    var centerTile = _world.GetTile(center);

    for (int i = 0; i < 1; i++)
    {
      new CharacterCreatedEvent { Character = _world.CreateCharacter(centerTile) }.Publish();
    }
  }

  internal void CreateResource(string resourceType, Tile tile)
  {
    var resource = _world.CreateBuildingResource(tile, resourceType);
    new BuildingResourceUpdatedEvent { Resource = resource }.Publish();
  }

  public void LoadWorld()
  {
    new WorldUpdateEvent { Reset = true }.Publish();

    SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    _filename = @"d:\[unity]\geo.txt";
  }



  internal void CreateJob(Job job)
  {
    _world.AddJob(job);

    new JobUpdateEvent { Job = job }.Publish();
  }

  public void SaveWorld()
  {
    var savegame = _world.CreateSaveGame();

    var json = JsonUtility.ToJson(savegame, true);

    File.WriteAllText(@"d:\[unity]\geo.txt", json);
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
