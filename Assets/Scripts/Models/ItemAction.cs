using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  TODO: add action like door open ...
public class ItemAction 
{
	#region Properties

	public string ActionType { get; set; }

	public string TransitionState { get; set; }

	public string CompletedState { get; internal set; }

	public Func<Item, float, bool> UpdateAction { get; internal set; }
	public string IdleAction { get; internal set; }

	#endregion
}
