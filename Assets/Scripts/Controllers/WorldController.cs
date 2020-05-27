using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Unity.Collections;
using UnityEngine.UIElements;

/// <summary>
/// This is the wolrd controller, important changes to the world are managed by this controller
/// This controller is launched first: lauch order = 1
/// </summary>
public class WorldController : BaseController
{
  #region Member

  World _world;

  static string _loadGameFileName = @"d:\[unity]\geo_new_game.txt";

  #endregion

  #region Unity

  public void Awake()
  {
    Log("WorldController.Awake");

    //  Initialize the Ioc so that this 'game' - session has all brand new object instances ...
    IoC.Initialize();

    IoC.RegisterType<GameObjectFactory>();
    IoC.RegisterType<AbstractItemFactory>();
    IoC.RegisterInstance(this);
  }

  public void OnEnable()
  {
    Log("WorldController.OnEnable");
  }

  // Start is called before the first frame update

  void Start()
  {
    Log("WorldController.Start");

    if (_loadGameFileName != null)
    {
      //    load an existing game
      LoadGame();
    }
    else
    {
      NewGame();
    }   
  }

  private void LoadUiElements()
  {
    //  create tiles
    foreach (var tile in _world.GetAllTiles())
    {
      new LoadTileEvent { Tile = tile }.Publish();
    }

    //  create characters
    foreach (var character in _world.GetCharacters())
    {
      new CharacterCreatedEvent { Character = character }.Publish();
    }

    //  create jobs
    foreach (var job in _world.GetJobs())
    {
      IoC.Get<JobController>().LoadJob(job);
    }

    IoC.Get<InventoryController>().LoadUI();
  }

  // Update is called once per frame
  void Update()
  {
    _world?.Update(Time.deltaTime);
  }

  #endregion

  #region Menu

  public void OnNewGame()
  {
    Log("WorldController.Menu.OnNewGame");

    //  pause the current world
    _world?.Pause();

    //  clear the load file name
    _loadGameFileName = @"d:\[unity]\geo_new_game.txt";

    //  Reload Scene
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }

  public void OnLoadGame()
  {
    Log("WorldController.Menu.OnLoadGame");

    //  pause the current world
    _world?.Pause();

    //  set the load game file name
    _loadGameFileName = @"d:\[unity]\geo.txt";

    //  Reload Scene
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }

  public void OnSaveGame()
  {
    Log("WorldController.Menu.OnSaveGame");

    //  pause the current world
    _world?.Pause();

    SaveGame();

    //  un pause the current world
    _world?.Unpause();
  }

  #endregion

  #region Methods

  internal World GetWorld() => _world;

  public void NewGame()
  {
    Log("NewGame");

    //  Creates a new world
    //
    _world = new World(100, 100);
    IoC.RegisterInstance(_world);

    //  Create all ui tiles
    //
    for (int x = 0; x < _world.Width; x++)
    {
      for (int y = 0; y < _world.Height; y++)
      {
        var tile = _world.GetTile(new Position(x, y));
        if (tile == null) continue;

        new CreateTileEvent { Tile = tile }.Publish();
        new UpdateTileEvent { Tile = tile }.Publish();
      }
    }


    //	center camera to the middle of the world view
    //
    var center = new Position(_world.Width / 2, _world.Height / 2);
    Camera.main.transform.position = new Vector3(_world.Width / 2, _world.Height / 2, Camera.main.transform.position.z);

    var centerTile = _world.GetTile(center);

    for (int i = 0; i < 5; i++)
    {
      new CharacterCreatedEvent { Character = _world.CreateCharacter(centerTile) }.Publish();
    }

    _world?.Unpause();

    Log("CreateWorldGame.Done");
  }

  public void LoadGame()
  {
    Log("WorldController.LoadGame");

    var json = File.ReadAllText(_loadGameFileName);
    var saveGame = JsonUtility.FromJson<GameData>(json);

    _world = World.LoadSaveGame(saveGame.world);
    IoC.RegisterInstance(_world);

    //  World is create we need to create all the ui elemements

    LoadUiElements();

    //  set the camera
    Camera.main.transform.position = new Vector3(saveGame.camera_x, saveGame.camera_y, -10);

    Log("WorldController.LoadGame.Done");

    _world.Unpause();
  }

  public void SaveGame()
  {
    var savegame = new GameData
    {
      camera_x = Camera.main.transform.position.x,
      camera_y = Camera.main.transform.position.y,

      world = _world.CreateSaveGame()
    };

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

  internal void UpdateTileInfo(Tile tile)
  {
  }

  #endregion

  #region internal Helpers

  #endregion
}

[Serializable]
public class GameData
{
  public float camera_x;
  public float camera_y;

  public WorldData world;
}
