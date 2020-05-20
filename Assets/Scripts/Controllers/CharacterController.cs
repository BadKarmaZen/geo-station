using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour, IHandle<CharacterCreatedEvent>, IHandle<CharacterUpdatedEvent>
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

  public void OnHandle(CharacterCreatedEvent message)
  {
    var characterInfo = new CharacterInfo();    
    _characterGraphics.Add(message.Character, characterInfo);

    characterInfo.main = new GameObject();

    characterInfo.main.name = "astro" + _characterGraphics.Count;
    characterInfo.main.transform.position = new Vector3(message.Character.CurrentTile.Position.x, message.Character.CurrentTile.Position.y);
    characterInfo.main.transform.SetParent(this.transform, true);    

    var renderer = characterInfo.main.AddComponent<SpriteRenderer>();
    renderer.sprite = _resourceCollection.GetSprite("Astronaut_B");
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
    if (message.Hand)
    {
      info.hand.transform.position = new Vector3(info.hand.transform.position.x, message.Character.HandY, -1);
    }
    else
    {
      info.main.transform.position = new Vector3(message.Character.X, message.Character.Y, 0);
    }
    
  }

  #endregion

  #region Unity

  public Sprite astro;

  void Awake()
  {
    IoC.Get<EventAggregator>().Subscribe(this);

    _resourceCollection = new ResourceCollection("Other");
  }

  // Start is called before the first frame update
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {
  }


  #endregion

  #region Helpers

  #endregion

}
