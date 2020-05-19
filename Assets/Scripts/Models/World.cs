using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World
{
  #region Members

  private Tile[,] _tiles;
  private List<Character> _characters = new List<Character>();

  public (int width, int height) Size { get; set; }

  #endregion

  #region Constuction

  public World(int width = 100, int height = 100)
  {
    Size = (width, height);

    _tiles = new Tile[width, height];

    for (int x = 0; x < Size.width; x++)
    {
      for (int y = 0; y < Size.height; y++)
      {
        _tiles[x, y] = new Tile(this, new Position(x, y));
      }
    }


    Debug.Log($"Create World: Size {Size}");
  }

  #endregion

  #region Methods

  public void Update(float deltaTime)
  {
    foreach (var character in _characters)
    {
      character.Update(deltaTime);
    }
  }


  public Tile GetTile(Position position)
  {
    if (position.x >= Size.width || position.x < 0 || position.y >= Size.height || position.y < 0)
    {
      //  Debug.Log($"Tile. Index out of range ({x},{y})");
      return null;
    }

    return _tiles[position.x, position.y];
  }

  //public IEnumerable<Tile> GetSurrondingTiles(Tile tile)
  //{
  //  //  order == 
  //  for (int x = tile.Position.x - 1; x <= tile.Position.x + 1; x++)
  //  {
  //    for (int y = tile.Position.y - 1; y <= tile.Position.y + 1; y++)
  //    {
  //      Debug.Log($"GetSurrondingTiles({x},{y})");
  //      if (x != tile.Position.x && y != tile.Position.y)
  //      {
  //        Debug.Log($"GetSurrondingTiles({x},{y}).1");
  //        var neighbour = GetTile(x, y);
  //        if (neighbour != null)
  //        {
  //          Debug.Log($"GetSurrondingTiles({x},{y}).Neighbour");
  //          yield return neighbour;
  //        }
  //      }
  //    }
  //  }
  //}

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

  public Character CreateCharacter(Tile tile)
  {
    var character = new Character(tile) { Speed = 2f };
    _characters.Add(character);

    return character;
  }



  //  TODO - mock
  public void Randomioze()
  {
    Debug.Log($"World.Randomioze");

    for (int x = 0; x < Size.width; x++)
    {
      for (int y = 0; y < Size.height; y++)
      {
        _tiles[x, y].SetType(UnityEngine.Random.Range(0, 2) == 0 ? Tile.TileType.Space : Tile.TileType.Floor);
      }
    }

    //  ensure mid tile is floor
    _tiles[50, 50].SetType(Tile.TileType.Floor);
  }

  #endregion

}
