using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// General object class, wall, door
/// </summary>
public class Item
{
  #region type constants

  //	Register item types, so that we don't mis type ;)
  public const string Wall = nameof(Wall);
  public const string Door = nameof(Door);

  #endregion

  #region Members

  #endregion

  #region Properties

  public string Type { get; protected set; }

  public Tile Tile { get; protected set; }

  //	cost to move over object
  public float MovementCost { get; protected set; }

  //  Component Model
  public Dictionary<string, object> Parameters { get; set; }
  public Action<Item, float> UpdateActions;
  public Func<Item, Enterable> IsEnterable = _ => Enterable.Never;

  //public bool Installing { get; internal set; }

  //  Actions

  //public ItemFactory Factory { get; set; }
  //public string IdleAction { get; private set; }

  #endregion

  #region constructors

  //	used for protorype
  public Item(string type, float movement)
  {
    Type = type;
    MovementCost = movement;

    Parameters = new Dictionary<string, object>();
  }

  public Item(Item prototype, Tile tile, ItemFactory factory)
  {
    Type = prototype.Type;
    MovementCost = prototype.MovementCost;

    Parameters = new Dictionary<string, object>();

    //  copy parameters
    foreach (var param in prototype.Parameters)
    {
      Parameters.Add(param.Key, param.Value);
    }

    UpdateActions = prototype.UpdateActions;
    IsEnterable = prototype.IsEnterable;
    Tile = tile;
  }

  #endregion

  #region Methods

  public void Update(float deltaTime)
  {
    UpdateActions?.Invoke(this, deltaTime);
  }

  #endregion

  #region Helper
  public T GetParameter<T>(string parameter) => (T)Parameters[parameter];

  public void UpdateParameters<T>(string parameter, Func<T, T> update) =>
    Parameters[parameter] = update(GetParameter<T>(parameter));

  #endregion
}



