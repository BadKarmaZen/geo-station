using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class World
{
  #region Members

  public int Width;
  public int Height;

  private Tile[,] _tiles;
  private List<Character> _characters = new List<Character>();
  private List<Item> _items = new List<Item>();
  private List<Room> _rooms = new List<Room>();

  #endregion

  #region Properties

  public bool IsPaused { get; private set; }

  public Room GetOutside() => _rooms[0];

  #endregion

  #region Construction

  public World()
  {
    IsPaused = true;
    CreatePrototypes();
  }

  public World(int width, int height) : this()
  {
    Debug.Log($"Create World: Size {width}x{height}");

    var outside = new Room { type = "outside" };
    _rooms.Add(outside);

    (Width, Height) = (width, height);

    _tiles = new Tile[width, height];
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        _tiles[x, y] = new Tile(this, new Position(x, y));
        _tiles[x, y].Room = outside;
      }
    }
  }

  #endregion

  #region Methods

  public void Pause() => IsPaused = true;
  public void Unpause()
  {
    Debug.Log("Unpause");

    IsPaused = false;
  }

  public void Update(float deltaTime)
  {
    if (IsPaused) return;

    foreach (var character in _characters)
    {
      character.Update(deltaTime);
    }

    foreach (var item in _items)
    {
      item.Update(deltaTime);
    }

    UpdateJobs(deltaTime);
    UpdateBuildingResources(deltaTime);
  }

  private List<Tile> _deliveryTiles = new List<Tile>();
  internal void UpdateTile(Tile tile, TileType oldType, TileType newType)
  {
    if (oldType == TileType.Delivery)
    {
      //  We lost a place to put our deliveries
      _deliveryTiles.Remove(tile);
    }
    else if (newType == TileType.Delivery)
    {
      //  we have gained an delivery tile
      _deliveryTiles.Add(tile);
    }
  }

  public IEnumerable<Tile> GetFreeDeliveryTiles()
  {
    return _deliveryTiles.Where(tile => tile.ResourcePile == null);
  }

  public Character CreateCharacter(Tile tile)
  {
    var character = new Character(tile, this, 2f);
    _characters.Add(character);

    return character;
  }

  internal void AddItem(Item item)
  {
    _items.Add(item);

    //  World has changed 
    //
    if (item.RoomEnclosure)
    {
      //  Update Room Detection
      DetectRooms(item);
    }
  }

  #endregion

  #region Rooms

  long nextRoomId = 1;

  private void DetectRooms(Item item)
  {
    Debug.Log($"Start => DetectRooms for {item.Tile.Position} : Rooms {_rooms.Count}");

    //  this item can create a room, as the final enclosing piece
    //  this item can also split a room in 4 
    //
    var oldRoom = item.Tile.Room;

    Debug.Log($"DetectRooms start flood");

    //  try building room to the north
    foreach (var neigbour in GetNeighbourTiles(item.Tile))
    {
      FloodFill(neigbour, oldRoom);
    }

    Debug.Log($"DetectRooms flood ended");

    //  this is now a room encloser, so it cannot be part of a room
    oldRoom.Unlink(item.Tile);

    if (oldRoom != GetOutside())
    {
      Debug.Log($"DetectRooms remove old room");
      RemoveRoom(oldRoom);
    }

    Debug.Log($"END => DetectRooms for {item.Tile.Position} : Rooms {_rooms.Count}");
  }

  private void FloodFill(Tile sourceTile, Room oldRoom)
  {
    if (sourceTile == null)
      return;

    if (sourceTile.Room != oldRoom)
      return; //  this tile was alread assigned to a new room, no need to create new room

    if (sourceTile.Item?.RoomEnclosure == true)
      return; //  this is a wall, ..., and cannot be part of a room

    if (sourceTile.Type == TileType.Space)
      return;

    //  create a new room
    var room = new Room();
    room.id = nextRoomId++;

    var tilesToCheck = new Queue<Tile>();
    tilesToCheck.Enqueue(sourceTile);

    while (tilesToCheck.Count > 0)
    {
      var tile = tilesToCheck.Dequeue();
      if (tile.Room == oldRoom)
      {
        room.AssignTile(tile);

        foreach (var neighbour in GetNeighbourTiles(tile))
        {
          if (neighbour.Type == TileType.Space)
          {
            //  we have reach the outside of
            //  clear the room
            room.UnassignAllTiles();

            //  bail out and 
            return;
          }

          if (neighbour.Room == oldRoom && (neighbour.Item == null || neighbour.Item.RoomEnclosure == false))
          {
            tilesToCheck.Enqueue(neighbour);
          }
        }
      }
    }

    //  a new room has been created, add to to the world
    //
    _rooms.Add(room);
  }

  //internal BuildingResource CreateBuildingResource(string resource)
  //{
  //  return new BuildingResource(resource, null, 20);
  //  //  TODO need to add this to the save game
  //}

  public void RemoveRoom(Room room)
  {
    if (room._tiles.Count != 0)
    {
      Debug.LogError("RemoveRoom: Error trying to remove ");
    }

    if (room == GetOutside())
      return;

    _rooms.Remove(room);
    room.UnassignAllTiles();
  }

  #endregion

  #region Jobs

  private List<Job> _jobs = new List<Job>();

  public void ScheduleJob(Job job)
  {
    _jobs.Add(job);
  }

  internal IEnumerable<Character> GetCharacters(Func<Character, bool> predicate = null)
  {
    foreach (var character in _characters)
    {
      if (predicate == null || predicate(character))
      {
        yield return character;
      }
    }
  }

  internal IEnumerable<Job> GetJobs()
  {
    return _jobs.AsEnumerable();
  }

  internal IEnumerable<BuildingResource> GetBuildiongResource()
  {
    return _buildingResources.AsEnumerable();
  }

  public Inventory _inventory = new Inventory();
  internal Inventory GetInventory() => _inventory;

  public void CancelJob(Job job)
  {
    //  TODO
    //  _jobs.Remove(job);
  }

  public void RemoveCompletedJob(Job job)
  {
    if (!job.IsCompleted())
    {
      Debug.LogError("Job is not yet completed");
      return;
    }
    _jobs.Remove(job);
  }


  public void UpdateJobs(float deltaTime)
  {
    //  remove all completed jobs from queue
    //_jobs.RemoveAll(job => job.IsCompleted());

    //var jobTodo = _jobs.FirstOrDefault(job => !job.Busy);
    //if (jobTodo != null)
    //{
    //  //  work to to
    //  //  TODO improve how to find worker
    //  var freeWorker = _characters.FirstOrDefault(worker => worker.CurrentJob == null);

    //  if (freeWorker != null)
    //  {
    //    freeWorker.AssignJob(jobTodo);
    //  }
    //}
  }

  #endregion

  #region Building resources


  private List<BuildingResource> _buildingResources = new List<BuildingResource>();

  //public BuildingResource CreateBuildingResource(Tile tile, string type)
  //{
  //  //tile.ResourcePile = new BuildingResource(type, tile, amount: 5)
  //  //{
  //  //  Id = _nextBuildingId++,
  //  //  World = this
  //  //};

  //  //_buildingResources.Add(tile.ResourcePile);
  //  //return tile.ResourcePile;
  //}

  //internal BuildingResource SelectResourcePile(string type)
  //  => _buildingResources?.FirstOrDefault(resource => resource.Type == type && resource.CanTakeResource());

  //  TODO
  //public void RemoveBuildingResource(BuildingResource resource)
  //{
  //  resource.Tile.ResourcePile = null;
  //  _buildingResources.Remove(resource);
  //}

  public void UpdateBuildingResources(float deltaTime)
  {
    //_buildingResources?.RemoveAll(r => r.Amount == 0);
  }

  #endregion

  #region Helpers

  public IEnumerable<Tile> GetAllTiles(Func<Tile, bool> predicate = null)
  {
    for (int x = 0; x < Width; x++)
    {
      for (int y = 0; y < Height; y++)
      {
        if (predicate == null || predicate(_tiles[x, y]))
        {
          yield return _tiles[x, y];
        }
      }
    }
  }

  public IEnumerable<Tile> GetNeighbourTiles(Tile tile)
  {
    //  norh
    var neighbour = GetTile(tile.Position.GetNorth());

    if (neighbour != null)
    {
      yield return neighbour;
    }

    //  east
    neighbour = GetTile(tile.Position.GetEast());

    if (neighbour != null)
    {
      yield return neighbour;
    }

    //  soutch
    neighbour = GetTile(tile.Position.GetSouth());

    if (neighbour != null)
    {
      yield return neighbour;
    }

    //  south
    neighbour = GetTile(tile.Position.GetWest());

    if (neighbour != null)
    {
      yield return neighbour;
    }
  }

  public Tile GetTile(Position position) => GetTile(position.x, position.y);

  public Tile GetTile(int x, int y)
  {
    if (x >= Width || x < 0 || y >= Height || y < 0)
    {
      //  Debug.Log($"Tile. Index out of range ({x},{y})");
      return null;
    }

    return _tiles[x, y];
  }

  #endregion

  #region Save & load

  internal WorldData CreateSaveGame()
  {
    var data = new WorldData { width = Width, height = Height };

    data.tiles = GetAllTiles(tile => tile.Type != TileType.Space).Select(tile => tile.ToData()).ToList();
    data.items = _items.Select(item => item.ToData()).ToList();
    data.jobs = _jobs.Select(job => job.ToData()).ToList();
    data.characters = _characters.Select(character => character.ToData()).ToList();
    data.building_resources = _buildingResources.Select(res => res.ToData()).ToList();

    return data;
  }

  internal static World LoadSaveGame(WorldData data)
  {
    var world = new World(data.width, data.height);

    //  reload tile information
    foreach (var tile in data.tiles)
    {
      world._tiles[tile.x, tile.y].Type = tile.type;
    }

    //  load items
    foreach (var item in data.items)
    {
      var worldItem = IoC.Get<AbstractItemFactory>().LoadItem(item.type, world.GetTile(item.x, item.y), item.rotation);
      world._items.Add(worldItem);
    }

    //  create jobs
    foreach (var job in data.jobs)
    {
      world._jobs.Add(new Job(job, world));
    }

    //  create resource
    foreach (var buildingResource in data.building_resources)
    {
      var resource = new BuildingResource(buildingResource, world);
      world._buildingResources.Add(resource);
    }

    //  create characters
    foreach (var character in data.characters)
    {
      world._characters.Add(new Character(character, world));
    }

    return world;
  }

  #endregion

  #region Prototype Definition

  private void CreatePrototypes()
  {
    var abstractFactory = IoC.Get<AbstractItemFactory>();
    abstractFactory.Initialize();

    //  wall
    var wallFactory = abstractFactory.CreateItemFactory(new Item(Item.Wall, 0f, true));
    wallFactory.BuildTime = 1f;
    wallFactory.BuildSound = "welding";
    wallFactory.SetBuildRule((tiles, factory) =>
    {
      if (tiles.Count != 1)
      {
        Debug.LogError("Walls are single tiles");
        return false;
      }

      var tile = tiles[0];
      //  can
      return tile.Type == TileType.Floor &&  //  a wall must be build on a floor tile
             tile.IsOccupied == false &&          //  must not be occupied
             tile.ActiveJob == null;              //  no job active
    });

    var doorPrototype = new Item(Item.Door, 1.2f, true);
    var doorFactory = abstractFactory.CreateItemFactory(doorPrototype, Item.Wall);

    doorFactory.BuildSound = "welding";
    doorFactory.BuildTime = 0.5f;
    doorPrototype.Parameters["openness"] = 0f;
    doorPrototype.Parameters["is_opening"] = 0f;
    doorPrototype.UpdateActions += ItemActions.DoorUpdateAction;
    doorPrototype.IsEnterable = ItemActions.DoorIsEnterable;

    doorFactory.SetBuildRule((tiles, factory) =>
    {
      if (tiles.Count != 1)
      {
        Debug.LogError("Doors are single tiles");
        return false;
      }

      var tile = tiles[0];

      if (tile.Type != TileType.Floor || tile.IsOccupied || tile.ActiveJob != null)
        return false;

      //  door must have wall to north & south, or east & west
      var north = GetTile(tile.Position.GetNorth());
      var south = GetTile(tile.Position.GetSouth());

      if (factory.IsValidNeighbour(north?.Item?.Type) &&
          factory.IsValidNeighbour(south?.Item?.Type))
      {
        return true;
      }

      var west = GetTile(tile.Position.GetWest());
      var east = GetTile(tile.Position.GetEast());

      if (factory.IsValidNeighbour(west?.Item?.Type) &&
          factory.IsValidNeighbour(east?.Item?.Type))
      {
        return true;
      }

      return false;
    });

    //  Oxygen generator
    var o2Factory = abstractFactory.CreateItemFactory(new Item(Item.O2_generator, 0f, false));
    o2Factory.BuildTime = 1f;
    o2Factory.SetBuildRule((tiles, factory) =>
    {
      if (tiles.Count != 2)
      {
        Debug.LogError("O2_generator are 1x2 tiles");
        return false;
      }

      return tiles.All(tile =>
        tile.Type == TileType.Floor &&  //  a wall must be build on a floor tile
        tile.IsOccupied == false &&          //  must not be occupied
        tile.ActiveJob == null);
    });
  }

  #endregion
}

[Serializable]
public class WorldData
{
  public int width;
  public int height;

  public List<TileData> tiles;
  public List<ItemData> items;
  public List<JobData> jobs;
  public List<CharacterData> characters;
  public List<BuildingResourceData> building_resources;
}