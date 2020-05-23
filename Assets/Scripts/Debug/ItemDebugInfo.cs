using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDebugInfo : MonoBehaviour
  , IHandle<MouseUpdateEvent>
{
  Text _text;

  public void OnHandle(MouseUpdateEvent message)
  {
    if (message.Tile.Item != null)
    {
      _text.text = $"Item : {message.Tile.Item.Type}";
    }
    else
    {
      _text.text = $"Item :  Null";
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
    //_text.text = $"{Time.deltaTime}";
  }
}