using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job
{
	#region Properties

	public Tile Tile { get; set; }
	public FixedObject FixedObject { get; set; }

	public float BuildTime { get; set; }
	public int Retry { get; internal set; }
  public BuildingResource ResourcePile { get; internal set; }

  #endregion

  #region Methods

  public bool ProcessJob(float deltaTime)
	{
		BuildTime -= deltaTime;
		return BuildTime < 0;
	}

	#endregion
}
