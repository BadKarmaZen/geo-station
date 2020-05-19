using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Position
{
  public int x;
  public int y;

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

  public Position GetNorth() => new Position { x = x, y = y + 1 };
  public Position GetEast() => new Position { x = x + 1, y = y };
  public Position GetSouth() => new Position { x = x, y = y - 1 };
  public Position GetWest() => new Position { x = x - 1, y = y };

  public bool IsNorthOf(Position position) => (x, y) == (position.x, position.y + 1);
  public bool IsEastOf(Position position) => (x, y) == (position.x + 1, position.y);
  public bool IsSouthOf(Position position) => (x, y) == (position.x, position.y - 1);
  public bool IsWestOf(Position position) => (x, y) == (position.x - 1, position.y);

  public override int GetHashCode() => (x, y).GetHashCode();
  public override bool Equals(object obj) => (obj is Position position) ? (x, y) == (position.x, position.y) : false;

  public Vector3 GetVector() => new Vector3(x, y);

  public override string ToString()
  {
    return $"({x},{y})";
  }
}

