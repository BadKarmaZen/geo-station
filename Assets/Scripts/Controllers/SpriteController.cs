using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This controller is resposible for the main world ui, showing tiles, items, ...
/// Controller Launch order : 2
/// </summary>
public class SpriteController : BaseController
  //, IHandle<WorldUpdateEvent>
  , IHandle<CreateTileEvent>
  , IHandle<UpdateTileEvent>
  , IHandle<LoadTileEvent>
  , IHandle<CreateItemEvent>
  , IHandle<UpdateItemEvent>
  , IHandle<BuildingResourceUpdatedEvent>
{
  #region Members

  private ResourceCollection _resourceCollection;

  private Dictionary<Position, GameObject> _tileGraphics = new Dictionary<Position, GameObject>();
  private Dictionary<Item, GameObject> _itemGraphics = new Dictionary<Item, GameObject>();
  private Dictionary<BuildingResource, GameObject> _resourceGraphics = new Dictionary<BuildingResource, GameObject>();

  #endregion

  #region Events

  //public void OnHandle(WorldUpdateEvent message)
  //{
  //  //if (message.Reset)
  //  //{
  //  //  //  a new world has been set up
  //  //  //  _destroy all currenty graphics

  //  //  foreach (var tile in _tileGraphics.Values)
  //  //  {
  //  //    Destroy(tile);
  //  //  }
  //  //  foreach (var item in _itemGraphics.Values)
  //  //  {
  //  //    Destroy(item);
  //  //  }
  //  //  foreach (var resource in _resourceGraphics.Values)
  //  //  {
  //  //    Destroy(resource);
  //  //  }

  //  //  _tileGraphics = new Dictionary<Position, GameObject>();
  //  //  _itemGraphics = new Dictionary<Item, GameObject>();
  //  //  _resourceGraphics = new Dictionary<BuildingResource, GameObject>();
  //  //}
  //}

  public void OnHandle(CreateTileEvent message)
  {
    CreateTile(message.Tile);
  }

  public void OnHandle(UpdateTileEvent message)
  {
    UpdateTile(message.Tile);
  }

  //  messages is used when a tile gets loaded from a savegame
  public void OnHandle(LoadTileEvent message)
  {
    CreateTile(message.Tile);
    UpdateTile(message.Tile);

    if (message.Tile.Item != null)
    {
      CreateItem(message.Tile.Item, false);
    }
  }
  public void OnHandle(CreateItemEvent message)
  {
    CreateItem(message.Item);
  }

  public void OnHandle(UpdateItemEvent message)
  {
    UpdateItem(message.Item);
    //var item = message.Item;

    ////  TODO remove job
    ////if (message.JobFailed)
    ////{
    ////  //  remove job sprite
    ////  if (_fixedObjectGraphics.ContainsKey(item))
    ////  {
    ////    var graphic = _fixedObjectGraphics[item];
    ////    Destroy(graphic);
    ////    return;
    ////  }
    ////}

    ////  TODO improve - generic
    //bool rotate = false;

    //if (item.Type == Item.Door)
    //{
    //  //  default door is EW
    //  //  it there is a wall to the north this is as NS door, so rotate
    //  var north = IoC.Get<WorldController>().GetTile(item.Tile.Position.GetNorth());
    //  var south = IoC.Get<WorldController>().GetTile(item.Tile.Position.GetSouth());

    //  rotate = north?.Item?.Type == Item.Wall && south?.Item?.Type == Item.Wall;
    //}

    //if (message.UpdateOnly == false)
    //{
    //  GameObject graphic;
    //  SpriteRenderer renderer;

    //  if (!_itemGraphics.ContainsKey(item))
    //  {
    //    graphic = new GameObject();

    //    graphic.name = $"{item.Type}_{item.Tile.Position}";
    //    graphic.transform.position = new Vector3(item.Tile.Position.x, item.Tile.Position.y);

    //    if (rotate)
    //    {
    //      graphic.transform.rotation = Quaternion.Euler(0, 0, 90);
    //    }

    //    graphic.transform.SetParent(this.transform, true);

    //    _itemGraphics.Add(item, graphic);
    //    renderer = graphic.AddComponent<SpriteRenderer>();
    //  }
    //  else
    //  {
    //    renderer = _itemGraphics[item].GetComponent<SpriteRenderer>();
    //  }

    //  var spriteName = BuildItemSpriteName(item);
    //  renderer.sprite = _resourceCollection.GetSprite(spriteName);
    //  renderer.sortingLayerName = "FixedObjects";

    //  NotifyNeighbours(item);
    //}
    //else
    //{
    //  var renderer = _itemGraphics[item].GetComponent<SpriteRenderer>();
    //  var spriteName = BuildItemSpriteName(item);
    //  renderer.sprite = _resourceCollection.GetSprite(spriteName);
    //}
  }

  public void OnHandle(BuildingResourceUpdatedEvent message)
  {
    Log($"SpriteController.OnHandle_BuildingResourceUpdatedEvent = {message.Resource.Id} => {message.Resource.Amount}");

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
        Log($"SpriteController.OnHandle_BuildingResourceUpdatedEvent => destroy");
        Destroy(_resourceGraphics[message.Resource]);
      }
    }
  }


  #endregion

  #region Methods

  //public void CreateWorldGame(World world)
  //{
  //  Log("CreateWorldGame");

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

  //  Log("CreateWorldGame.Done");
  //}

  #endregion

  #region Unity

  #region Properties

  #endregion

  public void Awake()
  {
    Log($"SpriteController.Awake");
    _resourceCollection = new ResourceCollection("Tiles", "Objects");
  }

  public void OnEnable()
  {
    Log("SpriteController.OnEnable");

    //  the controller is active subscribe to events
    IoC.Get<EventAggregator>().Subscribe(this);
  }

  // Start is called before the first frame update
  void Start()
  {
    //Log($"SpriteController.Start");
  }

  // Update is called once per frame
  void Update()
  {
    //  TODO need to move?
    foreach (var pair in _itemGraphics)
    {
      var (item, graphic) = (pair.Key, pair.Value);

      if (item.Type == Item.Door)
      {
        var renderer = graphic.GetComponent<SpriteRenderer>();

        var openness = item.Parameters["openness"];

        if (openness < 0.1f)  // door is closed
        {
          renderer.sprite = _resourceCollection.GetSprite("Door_Openness_0");
        }
        else if (openness < 0.5f)  // door is closed
        {
          renderer.sprite = _resourceCollection.GetSprite("Door_Openness_1");
        }
        else if (openness < 0.9f)  // door is closed
        {
          renderer.sprite = _resourceCollection.GetSprite("Door_Openness_2");
        }
        else
        {
          renderer.sprite = _resourceCollection.GetSprite("Door_Openness_3");
        }
      }
    }

  }

  #endregion

  #region Helpers

  private void CreateTile(Tile tile)
  {
    if (_tileGraphics.ContainsKey(tile.Position))
    {
      LogError($"*** Tile {tile.Position} already created ***");
      return;
    }

    var grahic = new GameObject();
    _tileGraphics.Add(tile.Position, grahic);
    grahic.name = $"Tile_{tile.Position}";
    grahic.transform.position = tile.Position.GetVector();
    grahic.transform.SetParent(this.transform, true);
    grahic.AddComponent<SpriteRenderer>();
  }

  private void UpdateTile(Tile tile)
  {
    if (_tileGraphics.TryGetValue(tile.Position, out var graphic))
    {
      SpriteRenderer renderer = graphic.GetComponent<SpriteRenderer>();

      var spriteName = "Tiles_Space";

      if (tile.Type == TileType.Floor)
      {
        spriteName = "Tiles_Floor";
      }
      else if(tile.Type == TileType.Docking)
      {
        spriteName = "Tiles_Docking";
      }
      else if (tile.Type == TileType.Delivery)
      {
        spriteName = "Tiles_Delivery";
      }
      renderer.sprite = _resourceCollection.GetSprite(spriteName);
    }
    else
    {
      LogError("Unknown Tile");
    }
  }

  private void CreateItem(Item item, bool notify = true)
  {
    if (_itemGraphics.ContainsKey(item))
    {
      LogError($"*** Item {item.Type}@{item.Tile.Position} already created ***");
      return;
    }

    var graphic = new GameObject();

    graphic.name = $"{item.Type}_{item.Tile.Position}";
    graphic.transform.position = new Vector3(item.Tile.Position.x, item.Tile.Position.y);

    //  TODO improve - generic
    bool rotate = false;

    if (item.Type == Item.Door)
    {
      //  default door is EW
      //  it there is a wall to the north this is as NS door, so rotate
      var north = IoC.Get<WorldController>().GetTile(item.Tile.Position.GetNorth());
      var south = IoC.Get<WorldController>().GetTile(item.Tile.Position.GetSouth());

      rotate = north?.Item?.Type == Item.Wall && south?.Item?.Type == Item.Wall;
    }

    if (rotate)
    {
      graphic.transform.rotation = Quaternion.Euler(0, 0, 90);
    }

    graphic.transform.SetParent(this.transform, true);

    _itemGraphics.Add(item, graphic);
    var renderer = graphic.AddComponent<SpriteRenderer>();
    var spriteName = BuildItemSpriteName(item);
    renderer.sprite = _resourceCollection.GetSprite(spriteName);
    renderer.sortingLayerName = "FixedObjects";

    if (notify)
      NotifyNeighbours(item);
  }

  private void UpdateItem(Item item, bool notify = false)
  {
    if (_itemGraphics.TryGetValue(item, out var graphic))
    {
      var renderer = graphic.GetComponent<SpriteRenderer>();
      var spriteName = BuildItemSpriteName(item);
      renderer.sprite = _resourceCollection.GetSprite(spriteName);
    }
    else
    {
      LogError($"Unknown Item {item.Type} @ {item.Tile.Position}");
    }

  }

  private string BuildItemSpriteName(Item item)
  {
    var spriteName = IoC.Get<AbstractItemFactory>().GetSpriteName(item);

    //spriteName = item.Type + "_";
    //var factory = IoC.Get<AbstractItemFactory>().GetItemFactory(item);

    //var neighbours = IoC.Get<WorldController>().GetNeighbourTiles(item.Tile, tile => factory.IsValidNeighbour(tile.Item?.Type));

    //foreach (var neighbour in neighbours)
    //{
    //  if (neighbour.Position.IsNorthOf(item.Tile.Position))
    //  {
    //    spriteName += "N";
    //  }
    //  if (neighbour.Position.IsEastOf(item.Tile.Position))
    //  {
    //    spriteName += "E";
    //  }
    //  if (neighbour.Position.IsSouthOf(item.Tile.Position))
    //  {
    //    spriteName += "S";
    //  }
    //  if (neighbour.Position.IsWestOf(item.Tile.Position))
    //  {
    //    spriteName += "W";
    //  }
    //}

    return spriteName;
  }

  //  TODO : ? move to job
  private void NotifyNeighbours(Item item)
  {
    var neighbours = IoC.Get<WorldController>().GetNeighbourTiles(item.Tile, tile => tile.Item != null);

    foreach (var neighbour in neighbours)
    {
      new UpdateItemEvent { Item = neighbour.Item }.Publish();
    }
  }

  #endregion
}
