using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{
  #region Members

  Vector3 _currentFramePosition;
  Vector3 _lastFramePosition;

  Tile _startDragTile;

  World _world;

  #endregion

  #region Unity

  public GameObject cursor;

  // Start is called before the first frame update
  void Start()
  {
    _world = IoC.Get<World>();
  }

  // Update is called once per frame
  void Update()
  {
    _currentFramePosition = GetMousePosition();

    UpdateCursor();
    UpdateCameraPosition();
    UpdateDrag();

    _lastFramePosition = GetMousePosition();
  }


  #endregion

  #region Helper

  void UpdateCameraPosition()
  {
    //  right or middle mouse button
    if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
    {
      var diff = _lastFramePosition - _currentFramePosition;
      Camera.main.transform.Translate(diff);
    }

    var scroll = Input.GetAxis("Mouse ScrollWheel");
    if (scroll != 0)
    {
      var newSize = Camera.main.orthographicSize - (Camera.main.orthographicSize * scroll);
      Camera.main.orthographicSize = Mathf.Clamp(newSize, 3f, 50f);
    }
  }

  void UpdateCursor()
  {
    Tile tile = GetTileAtWorldCoordinate(_currentFramePosition);
    if (tile != null)
    {
      cursor.SetActive(true);
      cursor.transform.position = new Vector3(tile.Position.x, tile.Position.y);
    }
    else
    {
      cursor.SetActive(false);
    }
  }

  private void UpdateDrag()
  {
    //	Prevent dragging over menu
    //
    if (EventSystem.current.IsPointerOverGameObject())
    {
      return;
    }

    //	Start Left Mouse Button Down
    if (Input.GetMouseButtonDown(0))
    {
      _startDragTile = GetTileAtWorldCoordinate(GetMousePosition());
    }

    if (Input.GetMouseButton(0) && _startDragTile != null)
    {
      var currentTile = GetTileAtWorldCoordinate(GetMousePosition());
      IoC.Get<EventAggregator>().Publish(new DragEvent { From = _startDragTile, To = currentTile });
    }

    if (Input.GetMouseButtonUp(0))
    {
      var currentTile = GetTileAtWorldCoordinate(GetMousePosition());
      IoC.Get<EventAggregator>().Publish(new DragEvent { From = _startDragTile, To = currentTile, Complete = true });
    }
  }

  Vector3 GetMousePosition()
  {
    var mouse = Input.mousePosition;
    mouse.z = 10;
    return Camera.main.ScreenToWorldPoint(mouse);
  }

  Tile GetTileAtWorldCoordinate(Vector3 coord)
  {
    return _world.GetTile(new Position(coord.x + 0.5f, coord.y + 0.5f));
  }

  #endregion



}
