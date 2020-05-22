using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DragEvent
{
  public Tile From { get; set; }
  public Tile To { get; set; }
  public bool Complete { get; set; }
}

/// <summary>
/// This controller manages the build commands from the user
/// Controller Launch order : 5
/// </summary>
public class BuildController : MonoBehaviour, IHandle<DragEvent>
{
  enum BuildAction
  {
    Nothing,
    Destruct,
    Floor,
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

  //List<BuildingResource> _resources = new List<BuildingResource>();

  #endregion

  #region Events

  public void OnHandle(DragEvent message)
  {
    if (message.To != _dragTo)
    {
      //  Update
      _dragFrom = message.From;
      _dragTo = message.To;

      ClearDragPreview();
      ShowDragPreview();
    }

    if (message.Complete)
    {
      //  TODO - TEST
      if (message.From == message.To && _buildAction == BuildAction.Resource)
      {
        IoC.Get<WorldController>().CreateResource(_resourceType, message.From);

        return;
      }

      ClearDragPreview();

      //  TODO - remove test => needs to go to job que
      var tiles = IoC.Get<WorldController>().GetTiles(_dragFrom, _dragTo);

      foreach (var tile in tiles)
      {
        if (_buildAction == BuildAction.Floor)
        {
          tile.SetType(Tile.TileType.Floor);
        }
        else if (_buildAction == BuildAction.Destruct)
        {
          tile.SetType(Tile.TileType.Space);
        }
        else if (_buildAction == BuildAction.FixedObject)
        {
          //  check if we have resources
          //var resource = _resources.FirstOrDefault(r => r.Type == _buildType && r.AmountLeft > 0);

          //if (resource == null)
          //{
          //  //  no resources left
          //  return;
          //}

          //resource.Reserve();

          //  yes we can build a wall
          var item = IoC.Get<ObjectFactory>().CreateItem(_buildType, tile);

          if (item != null)
          {
            //  Create a job for it
            IoC.Get<WorldController>().CreateJob(new Job(tile, item.Type, item.Type == "Door" ? 0.5f : 1f));

            //  TODO ? jobcontroller
            //new ItemUpdatedEvent { Item = item }.Publish();
          }
        }
        else
        {

        }

      }

      _dragFrom = null;
      _dragTo = null;
    }
  }

  #endregion

  #region Unity

  #region Properties

  public GameObject circleCursorPrefab;

  #endregion

  void Awake()
  {
    Debug.Log("BuildController.Awake");
    IoC.RegisterInstance(this);
  }

  public void OnEnable()
  {
    Debug.Log("BuildController.OnEnable");
    IoC.Get<EventAggregator>().Subscribe(this);
  }

  // Start is called before the first frame update
  void Start()
  {
    Debug.Log("BuildController.Start");

    _objectFactory = IoC.Get<GameObjectFactory>();
  }

  // Update is called once per frame
  void Update()
  {

  }

  #region Menu Action

  public void BuildFloor()
  {
    _buildAction = BuildAction.Floor;
  }

  public void DestructTile()
  {
    _buildAction = BuildAction.Destruct;
  }

  public void BuildFixedObect(string type)
  {
    _buildAction = BuildAction.FixedObject;
    _buildType = type;
  }

  //  TODO - remove pathfinding
  public void CreateResource(string resource)
  {
    _buildAction = BuildAction.Resource;
    _resourceType = resource;
  }

  #endregion


  #endregion

  #region Helpers

  private void ShowDragPreview()
  {
    if (_dragTo != null)
    {
      foreach (var position in Position.GetPositions(_dragFrom?.Position, _dragTo?.Position))
      //GetPostitions(_dragFrom, _dragTo))
      {
        var prev = _objectFactory.Spawn(circleCursorPrefab, new Vector3(position.x, position.y), Quaternion.identity);
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
