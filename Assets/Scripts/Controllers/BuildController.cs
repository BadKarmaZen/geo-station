using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// This controller manages the build commands from the user
/// Controller Launch order : 5
/// </summary>
public class BuildController : MonoBehaviour
  , IHandle<MouseUpdateEvent>
  , IHandle<MouseDragEvent>
  , IHandle<MouseClickEvent>
  , IHandle<MouseWheelEvent>
{
  enum BuildAction
  {
    Nothing,
    Destruct,
    Floor,
    Delivery,
    FixedObject,
    Resource,
  }

  #region Members

  //  Build Drag
  Tile _dragFrom;
  Tile _dragTo;

  List<GameObject> _dragPreview = new List<GameObject>();

  GameObjectFactory _objectFactory;
  //World _world;

  BuildAction _buildAction = BuildAction.Nothing;
  private string _resourceType;
  private string _buildType;

  private float _rotate = 0;

  ResourceCollection _resouceCollection;
  GameObject _previewGameObject;
  GameObject _defaultBuildCursor;

  #endregion

  #region Events

  public void OnHandle(MouseUpdateEvent message)
  {
    //  update location for build preview
    //
    _previewGameObject.transform.position = message.Tile.Position.GetVector();
  }

  public void OnHandle(MouseClickEvent message)
  {
    DoBuild(message.Tile);
    _rotate = 0;
  }

  public void OnHandle(MouseDragEvent message)
  {
    if (message.To != _dragTo)
    {
      //  Update
      _dragFrom = message.Tile;
      _dragTo = message.To;

      ClearDragPreview();
      ShowDragPreview();
    }

    if (message.Complete)
    {
      ClearDragPreview();

      var worldcontroller = IoC.Get<WorldController>();
      var tiles = worldcontroller.GetTiles(_dragFrom, _dragTo);

      foreach (var tile in tiles)
      {
        DoBuild(tile);
      }


      _dragFrom = null;
      _dragTo = null;
    }
  }

  public void OnHandle(MouseWheelEvent message)
  {
    //  rotate element preview
    if (message.Up)
    {
      _rotate += 90;
    }
    else
    {
      _rotate -= 90;
    }

    if (_rotate < 0)
    {
      _rotate += 360;
    }
    else if (_rotate >= 360)
    {
      _rotate -= 360;
    }

    Debug.Log(_rotate);

    UpdatePreviewSprite();
  }

  #endregion

  #region Unity

  #region Properties

  //public GameObject circleCursorPrefab;

  #endregion

  void Awake()
  {
    Debug.Log("BuildController.Awake");
    IoC.RegisterInstance(this);

    _objectFactory = IoC.Get<GameObjectFactory>();    
    _resouceCollection = new ResourceCollection("Objects");

    CreatePrefabs();
  }

  private void CreatePrefabs()
  {
    _defaultBuildCursor = new GameObject();
    _defaultBuildCursor.name = $"build cursor";
    _defaultBuildCursor.layer = 5;  // "UI"

    _defaultBuildCursor.transform.position = new Vector3();
    _defaultBuildCursor.transform.SetParent(this.transform, true);
    var renderer = _defaultBuildCursor.AddComponent<SpriteRenderer>();
    renderer.sprite = _resouceCollection.GetSprite("build");
    renderer.sortingLayerName = "BuildPreview";


    _previewGameObject = new GameObject();
    _previewGameObject.name = "build preview";
    _previewGameObject.layer = 5;  // "UI"

    _previewGameObject.transform.position = new Vector3();
    _previewGameObject.transform.SetParent(this.transform, true);
    renderer = _previewGameObject.AddComponent<SpriteRenderer>();
    renderer.sortingLayerName = "BuildPreview";
  }

  public void OnEnable()
  {
    Debug.Log("BuildController.OnEnable");
  }

  // Start is called before the first frame update
  void Start()
  {
    Debug.Log("BuildController.Start");
    IoC.Get<EventAggregator>().Subscribe(this);
  }

  // Update is called once per frame
  void Update()
  {

  }

  #endregion

  #region Menu Action

  public void BuildFloor()
  {
    _buildAction = BuildAction.Floor;
  }

  public void BuildDelivery()
  {
    _buildAction = BuildAction.Delivery;
  }

  public void DestructTile()
  {
    _buildAction = BuildAction.Destruct;
  }

  public void BuildFixedObect(string type)
  {
    _buildAction = BuildAction.FixedObject;
    _buildType = type;

    UpdatePreviewSprite();
  }

  //  TODO - remove pathfinding
  public void CreateResource(string resource)
  {
    _buildAction = BuildAction.Resource;
    _resourceType = resource;
  }

  #endregion

  #region Helpers

  private void DoBuild(Tile tile)
  {
    //if (_buildAction == BuildAction.Resource)
    //{
    //  IoC.Get<WorldController>().CreateResource(_resourceType, tile);
    //}
    //else
    if (_buildAction != BuildAction.FixedObject)
    {
      if (_buildAction == BuildAction.Floor)
      {
        tile.SetType(TileType.Floor);
      }
      else if (_buildAction == BuildAction.Delivery)
      {
        tile.SetType(TileType.Delivery);
      }
      else if (_buildAction == BuildAction.Destruct)
      {
        tile.SetType(TileType.Space);
      }

      //  the word needs to know about this
      IoC.Get<WorldController>().UpdateTileInfo(tile);
    }
    else if (_buildAction == BuildAction.FixedObject)
    {
      //  can we build here
      var factory = IoC.Get<AbstractItemFactory>();

      if (factory.IsMultiTile(_buildType))
      {
        var buildOnTiles = factory.GetTilesToBuildOn(_buildType, tile, _rotate);

        if (factory.CanBuildItem(_buildType, buildOnTiles))
        {
          //  TODO: FIX jobs
          IoC.Get<JobController>().ScheduleJob(_buildType, tile, _rotate);
          //IoC.Get<WorldController>().CreateJob(new Job(_buildType, tile, factory.GetBuildTime(_buildType), _rotate));
        }
      }
      else
      {
        if (factory.CanBuildItem(_buildType, tile))
        {
          IoC.Get<JobController>().ScheduleJob(_buildType, tile);
        }
      }
    }
  }

  private void UpdatePreviewSprite()
  {
    //  update preview
    //if (_buildType == Item.O2_generator)
    {
      var spritename = $"{_buildType}_{_rotate}";
      _previewGameObject.GetComponent<SpriteRenderer>().sprite = _resouceCollection.GetSprite(spritename);
    }
  }

  private void ShowDragPreview()
  {
    if (_dragTo != null)
    {
      foreach (var position in Position.GetPositions(_dragFrom?.Position, _dragTo?.Position))
      //GetPostitions(_dragFrom, _dragTo))
      {
        //  TODO
        var prev = _objectFactory.Spawn(_defaultBuildCursor, new Vector3(position.x, position.y), Quaternion.identity);
        //	set parent in hier. view
        prev.transform.SetParent(this.transform, true);
        _dragPreview.Add(prev);
      }
    }
  }

  private void ClearDragPreview()
  {
    foreach (var go in _dragPreview)
    {
      _objectFactory.Despawn(go);
    }

    _dragPreview.Clear();
  }



  //private IEnumerable<Position> GetPostitions(Tile from, Tile to)
  //{
  //  if (from == null || to == null)
  //    yield break;

  //  //  normalize
  //  int fromX = Math.Min(from.Position.x, to.Position.x);
  //  int toX = Math.Max(from.Position.x, to.Position.x);

  //  int fromY = Math.Min(from.Position.y, to.Position.y);
  //  int toY = Math.Max(from.Position.y, to.Position.y);

  //  for (int x = fromX; x <= toX; x++)
  //  {
  //    for (int y = fromY; y <= toY; y++)
  //    {
  //      yield return new Position { x = x, y = y };
  //    }
  //  }
  //}


  #endregion
}
