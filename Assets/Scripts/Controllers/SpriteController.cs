using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteController : MonoBehaviour
  , IHandle<CreateTileEvent>
  , IHandle<TileUpdateEvent>
  , IHandle<ItemUpdatedEvent>
  , IHandle<BuildingResourceUpdatedEvent>
{
  #region Members

  private Dictionary<Position, GameObject> _tileGraphics = new Dictionary<Position, GameObject>();
  private Dictionary<Item, GameObject> _fixedObjectGraphics = new Dictionary<Item, GameObject>();
  private ResourceCollection _resourceCollection;


  private Dictionary<BuildingResource, GameObject> _resourceGraphics = new Dictionary<BuildingResource, GameObject>();

  #endregion

  #region Events
  public void OnHandle(CreateTileEvent message)
  {
    GameObject goTile = new GameObject();
    _tileGraphics.Add(message.Tile.Position, goTile);
    goTile.name = $"Tile_{message.Tile.Position}";
    goTile.transform.position = message.Tile.Position.GetVector();
    goTile.transform.SetParent(this.transform, true);
    goTile.AddComponent<SpriteRenderer>();
  }

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

  public void OnHandle(ItemUpdatedEvent message)
  {
    var item = message.Item;

    //  TODO remove job
    //if (message.JobFailed)
    //{
    //  //  remove job sprite
    //  if (_fixedObjectGraphics.ContainsKey(item))
    //  {
    //    var graphic = _fixedObjectGraphics[item];
    //    Destroy(graphic);
    //    return;
    //  }
    //}

    if (message.UpdateOnly == false)
    {
      GameObject graphic;
      SpriteRenderer renderer;

      if (!_fixedObjectGraphics.ContainsKey(item))
      {
        graphic = new GameObject();

        graphic.name = $"{item.Type}_{item.Tile.Position}";
        graphic.transform.position = new Vector3(item.Tile.Position.x, item.Tile.Position.y);
        graphic.transform.SetParent(this.transform, true);

        _fixedObjectGraphics.Add(item, graphic);
        renderer = graphic.AddComponent<SpriteRenderer>();
      }
      else
      {
        renderer = _fixedObjectGraphics[item].GetComponent<SpriteRenderer>();
      }

      var spriteName = BuildItemSpriteName(item);
      renderer.sprite = _resourceCollection.GetSprite(spriteName);
      renderer.sortingLayerName = "FixedObjects";

      NotifyNeighbours(item);
    }
    else
    {
      var renderer = _fixedObjectGraphics[item].GetComponent<SpriteRenderer>();
      var spriteName = BuildItemSpriteName(item);
      renderer.sprite = _resourceCollection.GetSprite(spriteName);
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
      renderer.sprite = _resourceCollection.GetSprite($"{message.Resource.Type}_Resource");
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

  //public void CreateWorldGame(World world)
  //{
  //  Debug.Log("CreateWorldGame");

  //  for (int x = 0; x < world.Size.width; x++)
  //  {
  //    for (int y = 0; y < world.Size.height; y++)
  //    {
  //      var tile = world.GetTile(new Position(x, y));
  //      if (tile == null) continue;

  //      GameObject goTile = new GameObject();
  //      _tileGraphics.Add(tile.Position, goTile);
  //      goTile.name = $"Tile_{x}_{y}";
  //      goTile.transform.position = new Vector3(tile.Position.x, tile.Position.y);
  //      goTile.transform.SetParent(this.transform, true);
  //      goTile.AddComponent<SpriteRenderer>();
  //    }
  //  }

  //  Debug.Log("CreateWorldGame.Done");
  //}

  #endregion

  #region Unity

  public void Awake()
  {
    Debug.Log($"SpriteController.ctor");
    IoC.Get<EventAggregator>().Subscribe(this);
  }

  public Sprite floorSprite;

  // Start is called before the first frame update
  void Start()
  { 
    Debug.Log($"SpriteController.Start");

    _resourceCollection = new ResourceCollection("Objects");

    //var world = IoC.Get<World>();
    //CreateWorldGame(world);
    //world.Randomioze();
  }

  // Update is called once per frame
  void Update()
  {

  }


  #endregion

  #region Helpers

  //private void LoadResources()
  //{
  //  var sprites = Resources.LoadAll<Sprite>("Objects");

  //  foreach (var sprite in sprites)
  //  {
  //    _fixedObjectSprites.Add(sprite.name, sprite);
  //  }
  //}

  //public Sprite GetSprite(string name)
  //{
  //  if (_fixedObjectSprites.ContainsKey(name) == false)
  //  {
  //    Debug.Log("Fixed object Sprite not found: " + name);
  //    return null;
  //  }

  //  return _fixedObjectSprites[name];
  //}

  private string BuildItemSpriteName(Item item)
  {
    var spriteName = item.Type + "_";
    var factory = IoC.Get<ObjectFactory>().GetFactory(item);

    var neighbours = IoC.Get<WorldController>().GetNeighbourTiles(item.Tile, tile => factory.IsValidNeighbour(tile.Item?.Type));

    foreach (var neighbour in neighbours)
    {
      if (neighbour.Position.IsNorthOf(item.Tile.Position))
      {
        spriteName += "N";
      }
      if (neighbour.Position.IsEastOf(item.Tile.Position))
      {
        spriteName += "E";
      }
      if (neighbour.Position.IsSouthOf(item.Tile.Position))
      {
        spriteName += "S";
      }
      if (neighbour.Position.IsWestOf(item.Tile.Position))
      {
        spriteName += "W";
      }
    }

    return spriteName;
  }

  //  TODO : ? move to job
  private void NotifyNeighbours(Item item)
  {
    var neighbours = IoC.Get<WorldController>().GetNeighbourTiles(item.Tile, tile => tile.Item != null);

    foreach (var neighbour in neighbours)
    {
      new ItemUpdatedEvent { Item = neighbour.Item, UpdateOnly = true }.Publish();
    }
  }


  #endregion
}
