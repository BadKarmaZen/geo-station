using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class holds all the item factories
/// </summary>
public class AbstractItemFactory
{
  #region Members

  private Dictionary<string, ItemFactory> _itemFactories;

  #endregion

  #region Methods

  public ItemFactory CreateItemFactory(Item protoype, params string[] allowedNeighbours)
  {
    var factory = new ItemFactory(protoype, allowedNeighbours);

    _itemFactories.Add(factory.Type, factory);

    return factory;
  }


  public ItemFactory GetItemFactory(Item item) => GetItemFactory(item?.Type);

  public ItemFactory GetItemFactory(string itemType)
  {
    if (!_itemFactories.ContainsKey(itemType))
    {
      Debug.LogError($"no factory found for {itemType}");
    }

    return _itemFactories[itemType];
  }

  public Item LoadItem(string type, Tile master, float rotation)
  {
    if (!_itemFactories.ContainsKey(type))
    {
      Debug.LogError($"no factory found for {type}");
    }

    var item = _itemFactories[type].LoadItem(master, rotation);
    master.Item = item;
    return item;
  }

  #endregion

  #region Factory Methods

  public bool IsMultiTile(string type)
  {
    //  TODO
    return (type == Item.O2_generator);
  }

  public Item CreateItem(string type, Tile tile, float rotation = 0)
  {
    if (!_itemFactories.ContainsKey(type))
    {
      Debug.LogError($"no factory found for {type}");
    }

    return _itemFactories[type].CreateItem(tile, rotation);
  }

  public bool CanBuildItem(string type, Tile tile)
  {
    if (!_itemFactories.ContainsKey(type))
    {
      Debug.LogError($"no factory found for {type}");
    }

    return _itemFactories[type].CanBuild(new List<Tile> { tile });
  }

  internal bool CanBuildItem(string type, List<Tile> tiles)
  {
    if (!_itemFactories.ContainsKey(type))
    {
      Debug.LogError($"no factory found for {type}");
    }

    return _itemFactories[type].CanBuild(tiles);
  }

  public float GetBuildTime(string type)
  {
    if (!_itemFactories.ContainsKey(type))
    {
      Debug.LogError($"no factory found for {type}");
    }

    return _itemFactories[type].BuildTime;
  }

  internal void Initialize()
  {
    _itemFactories = new Dictionary<string, ItemFactory>();
  }

  internal List<Tile> GetTilesToBuildOn(string type, Tile master, float rotate)
  {
    var tiles = new List<Tile> { master };

    //  TODO
    if (type == Item.O2_generator)
    {

      if (rotate == 0) //  default
      {
        tiles.Add(master.GetEastTile());
      }
      else if (rotate == 90)
      {
        tiles.Add(master.GetSouthTile());
      }
      else if (rotate == 180)
      {
        tiles.Add(master.GetWestTile());
      }
      else // if (_rotate == 270)
      {
        tiles.Add(master.GetNorthTile());
      }
    }

    return tiles;
  }

  internal string GetSpriteName(Item item)
  {
    if (!_itemFactories.ContainsKey(item.Type))
    {
      Debug.LogError($"no factory found for {item.Type}");
      return string.Empty;
    }

    var spriteName = item.Type + "_";

    if (IsMultiTile(item.Type))
    {
      spriteName += $"{item.Rotation}";
    }
    else
    {
      var neighbours = IoC.Get<WorldController>().GetNeighbourTiles(item.Tile, tile => GetItemFactory(item).IsValidNeighbour(tile.Item?.Type));

      foreach (var neighbour in neighbours)
      {
        if (neighbour.Position.IsNorthOf(item.Tile.Position))
        {
          spriteName += "N";
        }
        if (neighbour.Position.IsEastOf(item.Tile.Position))
        {
          spriteName += "E";
        }
        if (neighbour.Position.IsSouthOf(item.Tile.Position))
        {
          spriteName += "S";
        }
        if (neighbour.Position.IsWestOf(item.Tile.Position))
        {
          spriteName += "W";
        }
      }
    }

    return spriteName;
  }

  internal string GetSpriteName(Job job)
  {
    if (!_itemFactories.ContainsKey(job.Item))
    {
      Debug.LogError($"no factory found for {job.Item}");
      return string.Empty;
    }

    var spriteName = job.Item + "_";

    if (IsMultiTile(job.Item))
    {
      spriteName += $"{job.Rotation}";
    }
    else
    {
      var neighbours = IoC.Get<WorldController>().GetNeighbourTiles(job.Tile, tile => GetItemFactory(job.Item).IsValidNeighbour(tile.Item?.Type));

      foreach (var neighbour in neighbours)
      {
        if (neighbour.Position.IsNorthOf(job.Tile.Position))
        {
          spriteName += "N";
        }
        if (neighbour.Position.IsEastOf(job.Tile.Position))
        {
          spriteName += "E";
        }
        if (neighbour.Position.IsSouthOf(job.Tile.Position))
        {
          spriteName += "S";
        }
        if (neighbour.Position.IsWestOf(job.Tile.Position))
        {
          spriteName += "W";
        }
      }
    }

    return spriteName;
  }

  #endregion

}
