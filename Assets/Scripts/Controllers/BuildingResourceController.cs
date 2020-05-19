using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// This controller manages the Resources
/// </summary>
public class BuildingResourceController : MonoBehaviour
{
  #region Members

  List<BuildingResource> _resources = new List<BuildingResource>();

  #endregion

  #region Methods

  public void CreateResource(string type, Tile tile)
  {
    var resource = new BuildingResource(type, tile);
    _resources.Add(resource);
  }

  public BuildingResource SelectResourcePile(string type)
  {
    return _resources.FirstOrDefault(r => r.Type == type && r.AmountLeft != 0);
  }

  #endregion

  #region Unity

  void Awake()
  {
    IoC.RegisterInstance(this);
  }

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    _resources.RemoveAll(r => r.Amount == 0);
  }

  #endregion

}
