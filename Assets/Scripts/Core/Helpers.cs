using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Position
{
  #region Fields

  public int x;
  public int y;

  #endregion

  #region Constructors

  public Position(int x = 0, int y = 0)
  {
    this.x = x;
    this.y = y;
  }

  public Position(float x = 0, float y = 0)
  {
    this.x = Mathf.FloorToInt(x);
    this.y = Mathf.FloorToInt(y);
  }

  #endregion

  #region Methods

  public Position GetNorth() => new Position { x = x, y = y + 1 };
  public Position GetEast() => new Position { x = x + 1, y = y };
  public Position GetSouth() => new Position { x = x, y = y - 1 };
  public Position GetWest() => new Position { x = x - 1, y = y };

  public bool IsNorthOf(Position position) => (x, y) == (position.x, position.y + 1);
  public bool IsEastOf(Position position) => (x, y) == (position.x + 1, position.y);
  public bool IsSouthOf(Position position) => (x, y) == (position.x, position.y - 1);
  public bool IsWestOf(Position position) => (x, y) == (position.x - 1, position.y);
  
  public Vector3 GetVector() => new Vector3(x, y);
  #endregion

  #region Overrides

  public override int GetHashCode() => (x, y).GetHashCode();
  public override bool Equals(object obj) => (obj is Position position) ? (x, y) == (position.x, position.y) : false;
  public override string ToString() => $"({x},{y})";

  #endregion

  #region Static Helpers

  public static IEnumerable<Position> GetPositions(Position? from, Position? to)
  {
    if (from == null || to == null)
      yield break;

    var range = Normalize(from.Value, to.Value);

    for (int x = range.bottomLeft.x; x <= range.upperRight.x; x++)
    {
      for (int y = range.bottomLeft.y; y <= range.upperRight.y; y++)
      {
        yield return new Position { x = x, y = y };
      }
    }
  }

  public static (Position bottomLeft, Position upperRight) Normalize(Position a, Position b) =>
    (bottomLeft: new Position(Mathf.Min(a.x,b.x), Mathf.Min(a.y,b.y)), 
     upperRight: new Position(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y)));

  #endregion
}

