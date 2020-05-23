using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomDebugInfo : MonoBehaviour
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

  void Update()
  {
    if (_tile == null)
      return;

    if (_tile.Room == null)
    {
      _text.text = $"*** Tile lost Room ***";
      return;
    }

    _text.text = $"Room {_tile.Room.id}";
  }
}