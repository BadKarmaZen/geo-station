using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General object class, wall, door
/// </summary>
public class Item 
{
	#region type constants

	//	Register item types, so that we don't mis type ;)
	public const string Wall = nameof(Wall);
	public const string Door = nameof(Door);

	#endregion
	
	#region Properties

	public string Type { get; protected set; }

	public Tile Tile { get; protected set; }

	//	cost to move over object
	public float MovementCost { get; protected set; }

	public bool Installing { get; internal set; }

	#endregion

	#region constructors

	//	used for protorype
	public Item(string type, float movement)
	{
		Type = type;
		MovementCost = movement;
	}

	public Item(Item prototype, Tile tile)
	{
		Type = prototype.Type;
		MovementCost = prototype.MovementCost;

		Tile = tile;
	}


  #endregion
}

