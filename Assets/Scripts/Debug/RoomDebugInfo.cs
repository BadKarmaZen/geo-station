using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomDebugInfo : MonoBehaviour
  , IHandle<MouseUpdateEvent>
{
  Text _text;

  public void OnHandle(MouseUpdateEvent message)
  {
    if (message.Tile.Room == null)
    {
      _text.text = $"*** Tile lost Room ***";
      return;
    }

    _text.text = $"Room {message.Tile.Room.id}";
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
}