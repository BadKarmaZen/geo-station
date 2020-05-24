using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//  The factory for a specific Item

public class ItemFactory
{
  #region Members

  private Func<List<Tile>, ItemFactory, bool> _canBuildRule;
  private Dictionary<string, List<ItemAction>> _itemStates = new Dictionary<string, List<ItemAction>>();

  #endregion

  #region Properties

  public string Type { get; protected set; }
  public List<string> AllowedNeighbourTypes { get; protected set; }
  public Item Protoype { get; protected set; }


  public string BuildSound { get; set; }
  public float BuildTime { get; set; }

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

    _canBuildRule = (t, f) => false;
  }

  #endregion

  #region Methods

  public void SetBuildRule(Func<List<Tile>, ItemFactory, bool> rule) 
    => _canBuildRule = rule;

  public bool CanBuild(List<Tile> tiles)
    => _canBuildRule(tiles, this);

  public bool IsValidNeighbour(string type) 
    => AllowedNeighbourTypes.Contains(type);

  public Item CreateItem(Tile tile, float rotation = 0) => new Item(Protoype, tile, this, rotation);
    //=> CanBuild(tile) ? new Item(Protoype, tile, this) : null;

  public Item LoadItem(Tile tile, float rotation) => new Item(Protoype, tile, this, rotation);

  internal void AddAction(string action, string from, string transition, string to, Func<Item, float, bool> updateAction, string idle = null)
  {
    if (!_itemStates.ContainsKey(from))
    {
      _itemStates.Add(from, new List<ItemAction>());
    }

    _itemStates[from].Add(new ItemAction
    {
      ActionType = action,
      TransitionState = transition,
      CompletedState = to,
      UpdateAction = updateAction,
      IdleAction = idle
    }); ;
  }

  internal ItemAction GetItemAction(string currentState, string action)
  {
    if (_itemStates.ContainsKey(currentState))
    {
      return _itemStates[currentState].FirstOrDefault(a => a.ActionType == action);
    }
    return null;
  }
  
  #endregion
}
