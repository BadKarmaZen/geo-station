using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// This controller manages the displayed characters
/// Controller Launch order : 3 
/// </summary>
public class CharacterController : MonoBehaviour
 // , IHandle<WorldUpdateEvent>
  , IHandle<CharacterCreatedEvent>
  , IHandle<CharacterUpdatedEvent>
{
  #region internals

  class CharacterInfo
  {
    public GameObject main;
    public GameObject hand;
  }

  #endregion

  #region Members

  private ResourceCollection _resourceCollection;

  Dictionary<Character, CharacterInfo> _characterGraphics = new Dictionary<Character, CharacterInfo>();

  #endregion

  #region Events

  //public void OnHandle(WorldUpdateEvent message)
  //{
  //  //if (message.Reset)
  //  //{
  //  //  //  a new world has been set up
  //  //  //
  //  //  foreach (var character in _characterGraphics.Values)
  //  //  {
  //  //    Destroy(character.main);
  //  //  }

  //  //  _characterGraphics = new Dictionary<Character, CharacterInfo>();
  //  //}
  //}

  public void OnHandle(CharacterCreatedEvent message)
  {
    Debug.Log("CharacterController.CharacterCreatedEvent");

    var characterInfo = new CharacterInfo();
    _characterGraphics.Add(message.Character, characterInfo);

    characterInfo.main = new GameObject();

    characterInfo.main.name = "astro" + _characterGraphics.Count;
    characterInfo.main.transform.position = new Vector3(message.Character.CurrentTile.Position.x, message.Character.CurrentTile.Position.y);
    characterInfo.main.transform.SetParent(this.transform, true);

    var renderer = characterInfo.main.AddComponent<SpriteRenderer>();
    renderer.sprite = _resourceCollection.GetSprite("Astronaut_B_South");
    renderer.sortingLayerName = "Character";

    //  create hand object
    characterInfo.hand = new GameObject();
    characterInfo.hand.name = "hand" + characterInfo.main.name;

    //  hands need to be in unitspace
    characterInfo.hand.transform.position = new Vector3(characterInfo.main.transform.position.x - 0.3f, characterInfo.main.transform.position.y, -1);
    characterInfo.hand.transform.SetParent(characterInfo.main.transform, true);

    renderer = characterInfo.hand.AddComponent<SpriteRenderer>();
    renderer.sprite = _resourceCollection.GetSprite("Hands_B");
    renderer.sortingLayerName = "Character";
  }

  public void OnHandle(CharacterUpdatedEvent message)
  {
    // find game object
    var info = _characterGraphics[message.Character];
    try
    {
      if (message.Hand)
      {
        info.hand.SetActive(true);
        info.main.GetComponent<SpriteRenderer>().sprite = _resourceCollection.GetSprite($"Astronaut_B_{message.Direction}");
        info.hand.transform.position = new Vector3(info.hand.transform.position.x, message.Character.HandY, -1);
      }
      else
      {
        info.hand.SetActive(false);
        info.main.GetComponent<SpriteRenderer>().sprite = _resourceCollection.GetSprite($"Astronaut_B_{message.Direction}");
        info.main.transform.position = new Vector3(message.Character.X, message.Character.Y, 0);
      }
    }
    catch (System.Exception e)
    {
      Debug.LogError(e.Message);
    }    
  }

  #endregion

  #region Unity

  public Sprite astro;

  void Awake()
  {
    Debug.Log($"CharacterController.Awake => {_characterGraphics.Count}");

    _resourceCollection = new ResourceCollection("Other");
  }

  public void OnEnable()
  {
    Debug.Log("CharacterController.OnEnable");

    IoC.Get<EventAggregator>().Subscribe(this);
  }

  // Start is called before the first frame update
  void Start()
  {
    Debug.Log($"CharacterController.Start");
  }

  // Update is called once per frame
  void Update()
  {
    foreach (var character in IoC.Get<World>().GetCharacters())
    {
      character.Update(Time.deltaTime);
    }
  }

  #endregion

  #region Helpers

  #endregion

}
