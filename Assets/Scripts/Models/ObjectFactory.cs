using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFactory
{
  #region Members

  private Dictionary<string, ItemFactory> _itemFactories = new Dictionary<string, ItemFactory>();

  #endregion

  #region Methods

  public ItemFactory CreateFactory(Item protoype, params string[] allowedNeighbours)
  {
    var factory = new ItemFactory(protoype, allowedNeighbours);

    _itemFactories.Add(factory.Type, factory);

    return factory;
  }


  public Item CreateItem(string type, Tile tile)
  {
    if (!_itemFactories.ContainsKey(type))
    {
      Debug.LogError($"no factory found for {type}");
    }

    return _itemFactories[type].CreateItem(tile);
  }

  internal ItemFactory GetFactory(Item item)
  {
    if (!_itemFactories.ContainsKey(item.Type))
    {
      Debug.LogError($"no factory found for {item.Type}");
    }

    return _itemFactories[item.Type];
  }

  internal ItemFactory GetFactory(string type)
  {
    if (!_itemFactories.ContainsKey(type))
    {
      Debug.LogError($"no factory found for {type}");
    }

    return _itemFactories[type];
  }

  #endregion
}
