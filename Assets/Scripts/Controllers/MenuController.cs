using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
  public GameObject floorMenu;
  public GameObject itemsMenu;

  void Awake()
  {
    floorMenu.SetActive(false);
    itemsMenu.SetActive(false);
  }

  // Start is called before the first frame update
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {

  }

  public void ToggleFloors()
  {
    if (floorMenu.activeSelf)
    {
      floorMenu.SetActive(false);
    }
    else
    {
      floorMenu.SetActive(true);
    }   
  }

  public void ToggleInstallItems()
  {
    if (itemsMenu.activeSelf)
    {
      itemsMenu.SetActive(false);
    }
    else
    {
      itemsMenu.SetActive(true);
    }
  }
}
