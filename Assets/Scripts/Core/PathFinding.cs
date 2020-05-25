using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node
{
  public bool Walkable { get; set; }
  public int gCost { get; set; }
  public int hCost { get; set; }
  public float mCost { get; set; }
  public int fCost => (int)(gCost * mCost) + hCost;

  public Tile Tile { get; set; }
  public Node Parent { get; set; }
}

public class PathFinding
{
  World _world;
  Node[,] _grid;


  public PathFinding()
  {
    _world = IoC.Get<WorldController>().GetWorld();

    CreateGrid();
  }

  private void CreateGrid()
  {
    _grid = new Node[_world.Width, _world.Height];

    for (int x = 0; x < _world.Width; x++)
    {
      for (int y = 0; y < _world.Height; y++)
      {
        var tile = _world.GetTile(new Position(x, y));
        _grid[x, y] = new Node
        {
          Tile = tile,
          Walkable = (tile.Type == TileType.Floor || tile.Type == TileType.Delivery )&&
                    (tile.Item == null || tile.Item.MovementCost != 0),
          mCost = tile.Item?.MovementCost ?? 1f    
        };
      }
    }
  }

  public Stack<Tile> FindPath(Tile from, Tile to)
  {
    var startNode = _grid[from.Position.x, from.Position.y];
    var targetNode = _grid[to.Position.x, to.Position.y];

    List<Node> openSet = new List<Node>();
    HashSet<Node> closedSet = new HashSet<Node>();

    openSet.Add(startNode);

    while (openSet.Count != 0)
    {
      Node currentNode = openSet[0];

      for (int index = 1; index < openSet.Count; index++)
      {
        if (openSet[index].fCost < currentNode.fCost || 
            openSet[index].fCost == currentNode.fCost && openSet[index].hCost < currentNode.hCost)
        {
          currentNode = openSet[index];
        }
      }

      openSet.Remove(currentNode);
      closedSet.Add(currentNode);

      if (currentNode == targetNode)
      {
        var path = new Stack<Tile>();

        var node = targetNode;
        while (node != startNode)       
        {
          path.Push(node.Tile);
          node = node.Parent;
        }

        return path;
      }

      foreach (var neighbour in GetNeighbours(currentNode))      
      {
        if (!neighbour.Walkable || closedSet.Contains(neighbour))
          continue;

        int movementCost = currentNode.gCost + GetDistance(currentNode, neighbour);
        if (movementCost < neighbour.gCost || !openSet.Contains(neighbour))
        {
          neighbour.gCost = movementCost;
          neighbour.hCost = GetDistance(neighbour, targetNode);
          neighbour.Parent = currentNode;

          if (!openSet.Contains(neighbour))
            openSet.Add(neighbour);
        }
      }
    }

    return null;
  }


  public IEnumerable<Node> GetNeighbours(Node node)
  {
    var nodes = from t in _world.GetNeighbourTiles(node.Tile)
                select _grid[t.Position.x, t.Position.y];
    return nodes;
  }

  static int GetDistance(Node a, Node b)
  {
    int distanceX = Mathf.Abs(a.Tile.Position.x - b.Tile.Position.x);
    int distanceY = Mathf.Abs(a.Tile.Position.y - b.Tile.Position.y);

    if (distanceX > distanceY)
    {
      return 14 * distanceY + 10 * (distanceX - distanceY);
    }

    return 14 * distanceX + 10 * (distanceY - distanceX);
  }
}

