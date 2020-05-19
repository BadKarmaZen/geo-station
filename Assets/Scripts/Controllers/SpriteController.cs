using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteController : MonoBehaviour
  , IHandle<TileUpdateEvent>, IHandle<FixedObjectUpdateEvent>
  , IHandle<BuildingResourceUpdatedEvent>
{
  #region Members

  private Dictionary<Position, GameObject> _tileGraphics = new Dictionary<Position, GameObject>();
  private Dictionary<FixedObject, GameObject> _fixedObjectGraphics = new Dictionary<FixedObject, GameObject>();
  private Dictionary<string, Sprite> _fixedObjectSprites = new Dictionary<string, Sprite>();


  private Dictionary<BuildingResource, GameObject> _resourceGraphics = new Dictionary<BuildingResource, GameObject>();

  #endregion

  #region Events

  public void OnHandle(TileUpdateEvent message)
  {
    var tile = message.Tile;

    if (!_tileGraphics.ContainsKey(tile.Position))
    {
      Debug.LogError("Unknown Tile");
      return;
    }

    SpriteRenderer renderer = _tileGraphics[tile.Position].GetComponent<SpriteRenderer>();
    if (tile.Type == Tile.TileType.Floor)
    {
      renderer.sprite = floorSprite;
    }
    else
    {
      renderer.sprite = null;
    }
  }

  public void OnHandle(FixedObjectUpdateEvent message)
  {
    var fixedObject = message.FixedObject;

    if (message.JobFailed)
    {
      //  remove job sprite
      if (_fixedObjectGraphics.ContainsKey(fixedObject))
      {
        var graphic = _fixedObjectGraphics[fixedObject];
        Destroy(graphic);
        return;
      }
    }

    if (message.UpdateOnly == false)
    {
      GameObject graphic;
      SpriteRenderer renderer;

      if (!_fixedObjectGraphics.ContainsKey(fixedObject))
      {
        graphic = new GameObject();

        graphic.name = $"{fixedObject.Type}_{fixedObject.Tile.Position}";
        graphic.transform.position = new Vector3(fixedObject.Tile.Position.x, fixedObject.Tile.Position.y);
        graphic.transform.SetParent(this.transform, true);

        _fixedObjectGraphics.Add(fixedObject, graphic);
        renderer = graphic.AddComponent<SpriteRenderer>();
      }
      else
      {
        renderer = _fixedObjectGraphics[fixedObject].GetComponent<SpriteRenderer>();
      }

      var spriteName = BuildFixedObjectSpriteName(fixedObject);
      renderer.sprite = GetSprite(spriteName);
      renderer.sortingLayerName = "FixedObjects";

      NotifyNeighbours(fixedObject);
    }
    else
    {
      var renderer = _fixedObjectGraphics[fixedObject].GetComponent<SpriteRenderer>();
      var spriteName = BuildFixedObjectSpriteName(fixedObject);
      renderer.sprite = GetSprite(spriteName);
    }
  }
  
  public void OnHandle(BuildingResourceUpdatedEvent message)
  {
    Debug.Log($"SpriteController.OnHandle_BuildingResourceUpdatedEvent = {message.Resource.Id} => {message.Resource.Amount}");

    if (!_resourceGraphics.ContainsKey(message.Resource))
    {
      var graphic = new GameObject();

      graphic.name = $"{message.Resource.Type}_{message.Resource.Tile.Position}";
      graphic.transform.position = message.Resource.Tile.Position.GetVector();
      graphic.transform.SetParent(this.transform, true);

      _resourceGraphics.Add(message.Resource, graphic);
      var renderer = graphic.AddComponent<SpriteRenderer>();
      renderer.sprite = GetSprite($"{message.Resource.Type}_Resource");
      renderer.sortingLayerName = "FixedObjects";
    }
    else
    {
      // is depleted
      if (message.Resource.Amount == 0)
      {
        Debug.Log($"SpriteController.OnHandle_BuildingResourceUpdatedEvent => destroy");
        Destroy(_resourceGraphics[message.Resource]);
      }
    }
  }

  #endregion

  #region Methods

  public void CreateWorldGame(World world)
  {
    Debug.Log("CreateWorldGame");

    for (int x = 0; x < world.Size.width; x++)
    {
      for (int y = 0; y < world.Size.height; y++)
      {
        var tile = world.GetTile(new Position(x, y));
        if (tile == null) continue;

        GameObject goTile = new GameObject();
        _tileGraphics.Add(tile.Position, goTile);
        goTile.name = $"Tile_{x}_{y}";
        goTile.transform.position = new Vector3(tile.Position.x, tile.Position.y);
        goTile.transform.SetParent(this.transform, true);
        goTile.AddComponent<SpriteRenderer>();
      }
    }

    Debug.Log("CreateWorldGame.Done");
  }

  #endregion

  public void Awake()
  {
    Debug.Log($"SpriteController.ctor");
    IoC.Get<EventAggregator>().Subscribe(this);
  }

  #region Unity

  public Sprite floorSprite;

  // Start is called before the first frame update
  void Start()
  {
    Debug.Log($"SpriteController.Start");


    LoadResources();

    var world = IoC.Get<World>();
    CreateWorldGame(world);
    world.Randomioze();
  }

  // Update is called once per frame
  void Update()
  {

  }


  #endregion

  #region Helpers

  private void LoadResources()
  {
    var sprites = Resources.LoadAll<Sprite>("Objects");

    foreach (var sprite in sprites)
    {
      _fixedObjectSprites.Add(sprite.name, sprite);
    }
  }

  public Sprite GetSprite(string name)
  {
    if (_fixedObjectSprites.ContainsKey(name) == false)
    {
      Debug.Log("Fixed object Sprite not found: " + name);
      return null;
    }

    return _fixedObjectSprites[name];
  }

  private string BuildFixedObjectSpriteName(FixedObject fixedObject)
  {
    if (fixedObject.Installing)
      return fixedObject.Type + "Job_";


    var spriteName = fixedObject.Type + "_";
    var neighbourType = IoC.Get<ObjectFactory>().GetNeighbourType(fixedObject);

    //  check surrounding tiles
    var neighbours = from t in IoC.Get<World>().GetNeighbourTiles(fixedObject.Tile)
                     where t.FixedObject?.Type == neighbourType
                     select t;

    foreach (var neighbour in neighbours)
    {
      if (neighbour.Position.IsNorthOf(fixedObject.Tile.Position))
      {
        spriteName += "N";
      }
      if (neighbour.Position.IsEastOf(fixedObject.Tile.Position))
      {
        spriteName += "E";
      }
      if (neighbour.Position.IsSouthOf(fixedObject.Tile.Position))
      {
        spriteName += "S";
      }
      if (neighbour.Position.IsWestOf(fixedObject.Tile.Position))
      {
        spriteName += "W";
      }
    }

    return spriteName;
  }

  //  TODO : ? move to job
  private void NotifyNeighbours(FixedObject fixedObject)
  {
    var neighbours = from t in IoC.Get<World>().GetNeighbourTiles(fixedObject.Tile)
                     where t.FixedObject != null
                     select t;

    foreach (var neighbour in neighbours)
    {
      IoC.Get<EventAggregator>().Publish(new FixedObjectUpdateEvent { FixedObject = neighbour.FixedObject, UpdateOnly = true });
    }
  }

  #endregion
}
