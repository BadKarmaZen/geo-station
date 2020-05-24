using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
  #region Members

  Dictionary<string, int> _resourceInventory = new Dictionary<string, int>();

  #endregion

  #region Methods

  public int GetAvailableAmount(string resource)
  {
    if (!_resourceInventory.ContainsKey(resource))
    {
      _resourceInventory.Add(resource, 0);
    }

    return _resourceInventory[resource];
  }

  public void AddToInventory(string resource, int amount)
  {
    if (!_resourceInventory.ContainsKey(resource))
    {
      _resourceInventory.Add(resource, amount);
    }
    else
    {
      _resourceInventory[resource] += amount;
    }

  }

  #endregion
}
