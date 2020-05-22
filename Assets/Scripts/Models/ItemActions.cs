using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngineInternal;

public static class ItemActions 
{
  #region Door Actions

  public static void DoorUpdateAction(Item item, float deltaTime)
  {
    if (item.Parameters["is_opening"] >= 1)
    {
      item.Parameters["openness"] += deltaTime * 2f;

      if (item.Parameters["openness"] >= 1)
        item.Parameters["is_opening"] = 0;
    }
    else
    {
      item.Parameters["openness"] -= deltaTime * 2f;
    }

    item.Parameters["openness"] = Mathf.Clamp01(item.Parameters["openness"]);
  }

  public static Enterable DoorIsEnterable(Item item)
  {
    //  auto matic open request door to open
    item.Parameters["is_opening"] = 1f;

    return item.Parameters["openness"] >= 1 ? Enterable.Yes : Enterable.Soon;
  }

  #endregion

}
