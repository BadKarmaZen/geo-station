using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
  #region Members

  public string type;
  public long id;

  public List<Tile> _tiles = new List<Tile>();

  #endregion

  #region Properties

  public float atmos_O2 = 0f;
  public float atmos_N = 0f;
  public float atmos_CO2 = 0f;

  #endregion

  #region Methods

  public void AssignTile(Tile tile)
  {
    if (this == tile.Room)
      return;

    if (tile.Room != null)
      tile.Room.Unlink(tile);

    tile.Room = this;
    _tiles.Add(tile);
  }

  public void Unlink(Tile tile)
  {
    tile.Room = null;
    _tiles.Remove(tile);
  }

  public void UnassignAllTiles()
  {
    foreach (Tile tile in _tiles)
    {
      tile.Room = tile.World.GetOutside();
    }
  }

  #endregion

}


