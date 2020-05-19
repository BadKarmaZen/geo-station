using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFactory
{
  #region Members

  Dictionary<string, FixedObjectFactory> _fixedobjectFactories = new Dictionary<string, FixedObjectFactory>();

  //Dictionary<string, FixedObject> _fixedPrototypes = new Dictionary<string, FixedObject>();
  //Dictionary<string, Func<Tile, string, bool>> _installRules = new Dictionary<string, Func<Tile, string, bool>>();

  #endregion

  #region Methods

  public void CreatePrototype(string type, float movementCost, string neighbourType = null, Func<Tile, string, bool> canInstall = null)
  {
    if (!_fixedobjectFactories.ContainsKey(type))
    {
      var prototype = new FixedObject { Type = type, MovementCost = movementCost };
      var factorty = new FixedObjectFactory(type, prototype, neighbourType);

      factorty.SetRule(canInstall);

      _fixedobjectFactories.Add(type, factorty);
    }
  }

  public FixedObject CreateFixedObject(string type, Tile tile)
  {
    if (!_fixedobjectFactories.ContainsKey(type))
    {
      Debug.LogError($"no factory found for {type}");
    }

    ////  Check Tile
    ////  TODO improve
    ////var canInstall = _installRules[type](tile, );
    //if (tile.FixedObject != null || tile.Type != Tile.TileType.Floor || tile.JobScheduled)
    //{
    //  return null;
    //}

    return _fixedobjectFactories[type].CreateObject(tile);
  }

  public string GetNeighbourType(FixedObject fixedObject)
  {
    if (!_fixedobjectFactories.ContainsKey(fixedObject.Type))
    {
      Debug.LogError($"no factory found for {fixedObject.Type}");
      return string.Empty;
    }

    return _fixedobjectFactories[fixedObject.Type].NeighbourType;
  }

  #endregion
}
