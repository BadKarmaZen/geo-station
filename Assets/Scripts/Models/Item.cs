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

  //  Component Model
  Dictionary<string, object> _itemParameters = new Dictionary<string, object>();
  Action<Item> _updateAction;

  #endregion

  #region Properties

  public string Type { get; protected set; }

  public Tile Tile { get; protected set; }

  //	cost to move over object
  public float MovementCost { get; protected set; }

  //public bool Installing { get; internal set; }

  //  Actions
  public string CurrentState { get; set; }
  public float ActionTime { get; set; }
  public float IdleTime { get; set; }

  public ItemAction CurrentAction { get; set; }

  public ItemFactory Factory { get; set; }
  public string IdleAction { get; private set; }

  #endregion

  #region constructors

  //	used for protorype
  public Item(string type, float movement)
  {
    Type = type;
    MovementCost = movement;
  }

  public Item(Item prototype, Tile tile, ItemFactory factory)
  {
    Type = prototype.Type;
    MovementCost = prototype.MovementCost;
    CurrentState = prototype.CurrentState;

    //  copy parameters
    foreach (var param  in _itemParameters)
    {
      _itemParameters.Add(param.Key, param.Value);
    }

    Factory = factory;
    Tile = tile;
  }

  #endregion

  #region Methods

  public void SetAction(string action)
  {
    Debug.Log($"SetAction({action})");
    //	can do action
    CurrentAction = Factory.GetItemAction(CurrentState, action);
    if (CurrentAction != null)
    {
      ActionTime = 0f;
      SetState(CurrentAction.TransitionState);
    }
  }

  public void Update(float deltaTime)
  {
    _updateAction?.Invoke(this);

    if (CurrentAction != null)
    {
      if (CurrentAction.UpdateAction(this, deltaTime) == true)
      {
        //  Action  completed
        SetState(CurrentAction.CompletedState);
        IdleAction = CurrentAction.IdleAction;
        CurrentAction = null;
        IdleTime = IdleAction == null ? 0f : 1f;  //  set idle time 1 sec
      }
    }
    else
    {
      if (IdleAction != null)
      {
        IdleTime -= deltaTime;
        if (IdleTime <= 0f)
        {
          SetAction(IdleAction);
        }
      }      
    }
  }

  #endregion

  private void SetState(string newState)
  {
    Debug.Log($"ItemState: {CurrentState} => {newState}");
    CurrentState = newState;
  }
}

