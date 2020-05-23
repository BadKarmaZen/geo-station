using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticResizer : MonoBehaviour
{
  public float childHeight;
  public float childWidth;
  public bool verticalMode;

  // Start is called before the first frame update
  void Start()
  {
    AdjustSize();
  }

  public void AdjustSize()
  {
    var size = GetComponent<RectTransform>().sizeDelta;

    if (verticalMode)
    {
      size.y = this.transform.childCount * childHeight;
    }
    else
    {
      size.x = this.transform.childCount * childWidth;
    }    

    GetComponent<RectTransform>().sizeDelta = size;
  }
}
