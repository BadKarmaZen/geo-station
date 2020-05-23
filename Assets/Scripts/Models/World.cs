using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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

    if (sourceTile.Type == Tile.TileType.Space)
      return;

    //  create a new room
    var room = new Room();
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
          if (neighbour.Type == Tile.TileType.Space)
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

  private long _nextJobId = 1;
  private List<Job> _jobs = new List<Job>();

  public void AddJob(Job job)
  {
    job.Id = _nextJobId++;
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

  public void CancelJob(Job job)
  {
    //  TODO
    //  _jobs.Remove(job);
  }

  public void UpdateJobs(float deltaTime)
  {
    //  remove all completed jobs from queue
    _jobs.RemoveAll(job => job.IsCompleted());

    var jobTodo = _jobs.FirstOrDefault(job => !job.Busy);
    if (jobTodo != null)
    {
      //  work to to
      //  TODO improve how to find worker
      var freeWorker = _characters.FirstOrDefault(worker => worker.CurrentJob == null);

      if (freeWorker != null)
      {
        freeWorker.AssignJob(jobTodo);
      }
    }
  }

  #endregion

  #region Building resources

  private long _nextBuildingId = 1;
  private List<BuildingResource> _buildingResources = new List<BuildingResource>();

  public BuildingResource CreateBuildingResource(Tile tile, string type)
  {
    var resource = new BuildingResource(type, tile, amount: 5);
    resource.Id = _nextBuildingId++;
    resource.World = this;

    _buildingResources.Add(resource);
    return resource;
  }
  
  internal BuildingResource SelectResourcePile(string type)
    => _buildingResources?.FirstOrDefault(resource => resource.Type == type && resource.CanTakeResource());

  public void RemoveBuildingResource(BuildingResource resource)
  {
    _buildingResources.Remove(resource);
  }

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

    data.tiles = GetAllTiles(tile => tile.Type == Tile.TileType.Floor).Select(tile => tile.ToData()).ToList();

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
      world._tiles[tile.x, tile.y].Type = Tile.TileType.Floor;

      if (!string.IsNullOrWhiteSpace(tile.item))
      {
        var item = IoC.Get<AbstractItemFactory>().GetItemFactory(tile.item).LoadItem(world._tiles[tile.x, tile.y]);
        world._items.Add(item);
        world._tiles[tile.x, tile.y].Item = item;
      }
    }

    //  create jobs
    foreach (var job in data.jobs)
    {
      world._jobs.Add(new Job(job, world));
    }

    if (world._jobs.Count > 0)
    {
      world._nextJobId = world._jobs.Max(j => j.Id) + 1;
    }

    //  create resource
    foreach (var buildingResource in data.building_resources)
    {
      var resource = new BuildingResource(buildingResource, world);
      resource.World = world;
      world._buildingResources.Add(resource);

      if (world._nextBuildingId <= resource.Id)
      {
        //  update resource Id;
        world._nextBuildingId = resource.Id + 1;
      }
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

    //  wall
    var wallFactory = abstractFactory.CreateItemFactory(new Item(Item.Wall, 0f, true));
    wallFactory.BuiltTime = 1f;
    wallFactory.BuildSound = "welding";
    wallFactory.SetBuildRule((tile, factory) =>
    {
      return tile.Type == Tile.TileType.Floor &&  //  a wall must be build on a floor tile  and
             tile.Item == null &&                 //  must be empty and
             tile.ActiveJob == null;              //  no job active
    });

    var doorPrototype = new Item(Item.Door, 1.2f, true);
    var doorFactory = abstractFactory.CreateItemFactory(doorPrototype, Item.Wall);

    doorFactory.BuildSound = "welding";
    doorFactory.BuiltTime = 0.5f;
    doorPrototype.Parameters["openness"] = 0f;
    doorPrototype.Parameters["is_opening"] = 0f;
    doorPrototype.UpdateActions += ItemActions.DoorUpdateAction;
    doorPrototype.IsEnterable = ItemActions.DoorIsEnterable;

    doorFactory.SetBuildRule((tile, factory) =>
    {
      if (tile.Type != Tile.TileType.Floor || tile.Item != null || tile.ActiveJob != null)
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
  }

  #endregion
}

[Serializable]
public class WorldData
{
  public int width;
  public int height;

  public List<TileData> tiles;
  public List<JobData> jobs;
  public List<CharacterData> characters;
  public List<BuildingResourceData> building_resources;
}