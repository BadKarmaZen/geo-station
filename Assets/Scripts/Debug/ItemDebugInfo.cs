using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDebugInfo : MonoBehaviour
  , IHandle<MouseUpdateEvent>
{
  Text _text;
  Tile _tile;

  public void OnHandle(MouseUpdateEvent message)
  {
    _tile = message.Tile;
    if (_tile == null)
    {
      _text.text = string.Empty;
    }   
  }

  // Start is called before the first frame update
  void Start()
  {
    _text = GetComponent<Text>();
    if (_text == null)
    {
      //  no text element
      enabled = false;
    }
    else
    {
      IoC.Get<EventAggregator>().Subscribe(this);
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (_tile == null)
      return;

    var resource = _tile.ResourcePile;
    if(resource != null)
    {
      _text.text = $"Resource : {resource.Type} ({resource.Amount}, {resource.ReservedByWorker} [{resource.ReservedBySystem}])";
    }

    if (!_tile.IsOccupied)
    {
      _text.text = $"not occupied";
      return;
    }

    //  either item or resource pile present
    if (_tile.Item != null)
    {
      _text.text = $"Item : {_tile.Item.Type}";
    }
  }
}