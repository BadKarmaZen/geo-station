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

  #endregion

  #region Factory Methods

  public Item CreateItem(string type, Tile tile)
  {
    if (!_itemFactories.ContainsKey(type))
    {
      Debug.LogError($"no factory found for {type}");
    }

    return _itemFactories[type].CreateItem(tile);
  }

  public bool CanBuildItem(string type, Tile tile)
  {
    if (!_itemFactories.ContainsKey(type))
    {
      Debug.LogError($"no factory found for {type}");
    }

    return _itemFactories[type].CanBuild(tile);
  }

  public float GetBuildTime(string type)
  {
    if (!_itemFactories.ContainsKey(type))
    {
      Debug.LogError($"no factory found for {type}");
    }

    return _itemFactories[type].BuiltTime;
  }

  internal void Initialize()
  {
    _itemFactories = new Dictionary<string, ItemFactory>();
  }

  #endregion

}
