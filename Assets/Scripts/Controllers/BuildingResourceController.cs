using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// This controller manages the Resources
/// </summary>
public class BuildingResourceController : MonoBehaviour
  , IHandle<WorldUpdateEvent>
{
  #region Members

  List<BuildingResource> _resources;

  #endregion

  #region Methods

  public void CreateResource(string type, Tile tile)
  {
    var resource = new BuildingResource(type, tile);
    _resources.Add(resource);
  }

  public void OnHandle(WorldUpdateEvent message)
  {
    if (message.Reset)
    {
    //  foreach (var resource in _resources)
    //  {
    //    Destroy(resource);
    //  }

      _resources = new List<BuildingResource>();
    }
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
    IoC.Get<EventAggregator>().Subscribe(this);
  }

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    _resources?.RemoveAll(r => r.Amount == 0);
  }

  #endregion

}
