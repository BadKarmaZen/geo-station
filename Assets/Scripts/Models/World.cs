using System;
using System.Collections;
using System.Collections.Generic;
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
  private List<Character> _characters;
  private List<Item> _items;



  #endregion

  #region Construction

  public World()
  {
    _characters = new List<Character>();
    _items = new List<Item>();
    _jobs = new List<Job>();
  }

  public World(int width, int height) : this()
  {
    Debug.Log($"Create World: Size {width}x{height}");

    (Width, Height) = (width, height);

    _tiles = new Tile[width, height];
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        _tiles[x, y] = new Tile(this, new Position(x, y));
      }
    }
  }

  #endregion

  #region Methods

  public void Update(float deltaTime)
  {

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
    var character = new Character(tile) { World = this, Speed = 2f };
    _characters.Add(character);

    return character;
  }

  internal void AddItem(Item item)
  {
    _items.Add(item);
  }

  #endregion


  #region Jobs

  private long _nextJobId = 1;
  private List<Job> _jobs;

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

    //  remove all completed jobs from queue
    _jobs.RemoveAll(job => job.IsCompleted());
  }

  #endregion

  #region Building resources

  private long _nextBuildingId;
  private List<BuildingResource> _buildingResources = new List<BuildingResource>();

  public BuildingResource CreateBuildingResource(Tile tile, string type)
  {
    var resource = new BuildingResource(type, tile);
    _buildingResources.Add(resource);
    return resource;
  }
  internal BuildingResource SelectResourcePile(string type)
    => _buildingResources?.FirstOrDefault(resource => resource.Type == type);

  public void UpdateBuildingResources(float deltaTime)
  {
    _buildingResources?.RemoveAll(r => r.Amount == 0);
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

    data.tiles = GetAllTiles(tile => tile.Type == Tile.TileType.Floor).Select(tile => new TileData
    {
      x = tile.Position.x,
      y = tile.Position.y
    }).ToList();

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
    }

    //  create jobs
    foreach (var job in data.jobs)
    {
      world._jobs.Add(new Job
      {
        Id = job.id,
        BuildTime = job.buildtime,
        Busy = job.busy,
        Tile = world.GetTile(job.x, job.y),
        Item = job.type
      });
    }

    if (world._jobs.Count > 0)
    {
      world._nextJobId = world._jobs.Max(j => j.Id) + 1;
    }

    //  create resource
    foreach (var buildingResource in data.building_resources)
    {
      world._buildingResources.Add(new BuildingResource(buildingResource.type, world.GetTile(buildingResource.x, buildingResource.y), buildingResource.amount)
      {
        Id = buildingResource.id,
        AmountReserved = buildingResource.amount_reserved
      });
    }

    if (world._buildingResources.Count > 0)
    {
      world._nextBuildingId = world._buildingResources.Max(r => r.Id) + 1;
    }

    //  create characters
    foreach (var character in data.characters)
    {
      world._characters.Add(new Character(world.GetTile(character.x, character.y))
      {
        Speed = 2f,
        CurrentJob = world._jobs.FirstOrDefault(j => j.Id == character.job_id),
        SelectedResourcePile = world._buildingResources.FirstOrDefault(r => r.Id == character.resource_id)
      });
    }

    return world;
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

[Serializable]
public class TileData
{
  public int x;
  public int y;

  public ItemData item;
}

[Serializable]
public class ItemData
{
  public string Type;
}
