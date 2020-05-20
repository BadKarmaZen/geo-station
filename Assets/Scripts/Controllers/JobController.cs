using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JobUpdateEvent
{
  public Job Job { get; set; }
  public Character Worker { get; set; }
}

public class JobController : MonoBehaviour, IHandle<JobUpdateEvent>
{
  #region Members

  ResourceCollection _resourceCollection;
  Dictionary<Job, GameObject> _jobGraphics = new Dictionary<Job, GameObject>();

  bool updateWorkForce;

  List<Job> _scheduledJob = new List<Job>();
  List<Character> _freeWorkers = new List<Character>();
  List<Character> _activeWorkers = new List<Character>();

  #endregion

  #region Unity

  void Awake()
  {
    IoC.RegisterInstance(this);
  }

  // Start is called before the first frame update
  void Start()
  {
    IoC.Get<EventAggregator>().Subscribe(this);

    _resourceCollection = new ResourceCollection("Jobs");
  }

  // Update is called once per frame
  void Update()
  {
    //  Start Job if needed
    StartJob();

    if (_activeWorkers.Count != 0)
    {
      foreach (var worker in _activeWorkers)
      {
        worker.Update(Time.deltaTime);
      }
    }

    UpdateWorkForce();

    //if (_currentJob != null)
    //{
    //  if (_currentJob.ProcessJob(Time.deltaTime))
    //  {
    //    //  Job is done
    //    _currentJob.Tile.InstallFixedObject(_currentJob.FixedObject);
    //    _currentJob.Tile.JobScheduled = false;
    //    _currentJob = null;
    //  }
    //}
  }

  #endregion

  #region Methods

  public void AddJob(Job job)
  {
    AddGraphics(job);

    job.Tile.JobScheduled = true;
    _scheduledJob.Add(job);
  }

  private void AddGraphics(Job job)
  {
    var graphic = new GameObject();
    graphic.name = $"{job.Item.Type}_{job.Tile.Position}";
    graphic.transform.position = job.Tile.Position.GetVector();
    graphic.transform.SetParent(this.transform, true);

    _jobGraphics.Add(job, graphic);
    var renderer = graphic.AddComponent<SpriteRenderer>();
    renderer.sprite = _resourceCollection.GetSprite(GetSpriteName(job));
    renderer.sortingLayerName = "FixedObjects";

    //var audio = new GameObject();
    //audio.transform.position = job.Tile.Position.GetVector();
    //audio.transform.SetParent(this.transform, true);

    //  doesn't seem to be working correct
    job.AudioSource = graphic.AddComponent<AudioSource>();

    // get sound
    var factory = IoC.Get<ObjectFactory>().GetFactory(job.Item);
    job.AudioSource.clip = Resources.Load<AudioClip>($"Sounds/{factory.BuildSound}"); ;

    //job.AudioSource.loop = true;
    job.AudioSource.volume = 0.1f;
    job.AudioSource.spatialBlend = 1.0f;
    job.AudioSource.rolloffMode = AudioRolloffMode.Linear;
    job.AudioSource.minDistance = 5;
    job.AudioSource.maxDistance = 11.5f;
  }

  private string GetSpriteName(Job job)
  {
    var spriteName = job.Item.Type + "_";

    //  TODO improve
    if (job.Item.Type == Item.Door)
    {
      var factory = IoC.Get<ObjectFactory>().GetFactory(job.Item);

      //  check surrounding tiles
      var neighbours = from t in IoC.Get<World>().GetNeighbourTiles(job.Tile)
                       where factory.IsValidNeighbour(t.Item?.Type)
                       select t;

      foreach (var neighbour in neighbours)
      {
        if (neighbour.Position.IsNorthOf(job.Tile.Position))
        {
          spriteName += "N";
        }
        if (neighbour.Position.IsEastOf(job.Tile.Position))
        {
          spriteName += "E";
        }
        if (neighbour.Position.IsSouthOf(job.Tile.Position))
        {
          spriteName += "S";
        }
        if (neighbour.Position.IsWestOf(job.Tile.Position))
        {
          spriteName += "W";
        }
      }

      return spriteName;

    }
    else
    {
      return spriteName;
    }
  }

  private void RemoveGraphics(Job job)
  {
    if (_jobGraphics.ContainsKey(job))
    {
      //  TODO  to replace with spawn
      Destroy(_jobGraphics[job]);
      _jobGraphics.Remove(job);
    }
  }

  void StartJob()
  {
    if (_scheduledJob.Count > 0 && _freeWorkers.Count > 0)
    {
      //  TODO find worker (nearest to job, resource ...)

      for (int workId = 0; workId < _freeWorkers.Count; workId++)
      {
        if (_freeWorkers[workId].AssignJob(_scheduledJob[0]))
        {
          //  success
          var worker = _freeWorkers[workId];
          _freeWorkers.Remove(worker);
          _activeWorkers.Add(worker);

          _scheduledJob.RemoveAt(0);

          return;
        }
      }

      //  none of the free workers could do the job
      //  remove and add to back
      Debug.LogWarning("none of the free workers could do the job");
      var job = _scheduledJob[0];
      //job.Retry++;

      if (_activeWorkers.Count > 0/* job.Retry < 3*/)
      {
        //  Still workers busy
        _scheduledJob.Add(job);
      }
      else
      {
        //  TODO
        //  job is delete
        //  
        //job.Tile.CannotCompleteJob(job.FixedObject);
      }
      _scheduledJob.RemoveAt(0);
    }
  }

  public void AddWorker(Character worker)
  {
    _freeWorkers.Add(worker);
  }

  public void OnHandle(JobUpdateEvent message)
  {
    updateWorkForce = true;
    ////  free worker
    //_activeWorkers.Remove(message.Worker);
    //_freeWorkers.Add(message.Worker);

    //  finish job
    var job = message.Job;

    //  TODO - replace
    //job.Tile.InstallFixedObject(job.FixedObject);
    job.Tile.InstallItemOnTile(job.Item);
    //job.Tile.JobScheduled = false;

    RemoveGraphics(job);
  }

  void UpdateWorkForce()
  {
    if (updateWorkForce)
    {
      updateWorkForce = false;

      _freeWorkers.AddRange(_activeWorkers.FindAll(w => w.CurrentJob == null));
      _activeWorkers.RemoveAll(w => w.CurrentJob == null);
    }
  }

  #endregion

}
