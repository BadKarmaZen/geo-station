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
    if (item.GetParameter<bool>("is_opening"))
    {
      item.UpdateParameters<float>("openness", value => value + (deltaTime * 2f));

      if (item.GetParameter<float>("openness") >= 1)
        item.UpdateParameters<bool>("is_opening", _ => false);
    }
    else
    {
      item.UpdateParameters<float>("openness", value => value - (deltaTime * 2f));
    }


    item.UpdateParameters<float>("openness", value => Mathf.Clamp01(value));
  }

  public static Enterable DoorIsEnterable(Item item)
  {
    //  auto matic open request door to open
    item.UpdateParameters<bool>("is_opening", _ => true);

    return item.GetParameter<float>("openness") >= 1 ? Enterable.Yes : Enterable.Soon;
  }

  #endregion

}
