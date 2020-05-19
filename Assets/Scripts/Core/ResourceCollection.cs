using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCollection 
{
  #region Members

  private string _folder;
  private Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

  #endregion

  public ResourceCollection(string folder)
  {
    _folder = folder;
    Debug.Log($"Create Resource Collection: {_folder}");

    LoadResources(folder);
  }

  public Sprite GetSprite(string name)
  {
    if (!_sprites.ContainsKey(name))
    {
      Debug.LogWarning($"Sprite '{name}' not found in {_folder}");
      return null;
    }

    return _sprites[name];
  }

  #region Helper
  
  private void LoadResources(string folder)
  {
    var sprites = Resources.LoadAll<Sprite>(folder);

    foreach (var sprite in sprites)
    {
      _sprites.Add(sprite.name, sprite);
    }
  }

  #endregion

}
