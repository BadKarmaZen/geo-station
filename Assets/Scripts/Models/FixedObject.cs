using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class FixedObject
//{
//  #region Properties

//  public Tile Tile { get; protected set; }
//  public string Type { get; set; }

//  public float MovementCost { get; set; }

//  public bool Installing { get; set; }

//  #endregion

//  #region Methods

//  internal FixedObject Clone(Tile tile)
//  {
//    return new FixedObject
//    {
//      Tile = tile,
//      Type = Type,
//      MovementCost = MovementCost,
//      Installing = true
//    };
//  }

//  #endregion
//}

//public class FixedObjectFactory
//{
//  public string Type { get; set; }
//  public string NeighbourType { get; set; }

//  FixedObject Prototype { get; set; }

//  Func<Tile, string, bool> Rule;

//  public FixedObjectFactory(string type, FixedObject protoType, string neighbourType = null)
//  {
//    Type = type;
//    NeighbourType = neighbourType ?? type;
//    Prototype = protoType;
//  }

//  internal void SetRule(Func<Tile, string, bool> canInstall)
//  {
//    if (canInstall == null)
//    {
//      //  default
//      //
//      canInstall = (t, n) =>
//        t.Type == Tile.TileType.Floor &&  //  must build on floor
//        t.FixedObject == null &&          //  no objects installed
//        t.JobScheduled == false;          //  no job scheduled
//      ;
//    }

//    Rule = canInstall;
//  }

//  public bool CanInstall(Tile tile)
//  {
//    return Rule(tile, NeighbourType);
//  }

//  public FixedObject CreateObject(Tile tile)
//  {
//    if (!CanInstall(tile))
//    {
//      return null;
//    }

//    return Prototype.Clone(tile);
//  }
//}
