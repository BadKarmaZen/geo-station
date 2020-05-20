using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  The factory for a specific Item

public class ItemFactory
{
  #region Members

  private Func<Tile, ItemFactory, bool> _buildRule;

  #endregion

  #region Properties

  public string Type { get; protected set; }
  public List<string> AllowedNeighbourTypes { get; protected set; }
  public Item Protoype { get; protected set; }
  public string BuildSound { get; set; }

  #endregion

  #region Constructors

  public ItemFactory(Item prototype, params string[] allowedNeighbours)
  {
    Type = prototype.Type;
    Protoype = prototype;

    if (allowedNeighbours.Length == 0)
    {
      AllowedNeighbourTypes = new List<string> { Type };
    }
    else
    {
      AllowedNeighbourTypes = new List<string>(allowedNeighbours);
    }

    _buildRule = (t, f) => false;
  }

  #endregion

  #region Methods

  public void AddBuildRule(Func<Tile, ItemFactory, bool> rule) 
    => _buildRule = rule;

  public bool IsValidNeighbour(string type) 
    => AllowedNeighbourTypes.Contains(type);

  public Item CreateItem(Tile tile)
    => _buildRule(tile, this) ? new Item(Protoype, tile) : null;

  #endregion
}
