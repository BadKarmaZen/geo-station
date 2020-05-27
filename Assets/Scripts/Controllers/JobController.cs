using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This controller manages the jobs
/// Controller Launch order : 4
/// </summary>
public class JobController : BaseController
//, IHandle<WorldUpdateEvent>
// , IHandle<JobUpdateEvent>
{
  #region Members

  ResourceCollection _resourceCollection;

  Dictionary<Job, GameObject> _jobGraphics = new Dictionary<Job, GameObject>();

  //WorldController _worldController;

  //bool updateWorkForce;

  //List<Job> _scheduledJob = new List<Job>();
  //List<Character> _freeWorkers = new List<Character>();
  //List<Character> _activeWorkers = new List<Character>();

  #endregion

  #region Events

  //public void OnHandle(WorldUpdateEvent message)
  //{
  //  if (message.Reset)
  //  {
  //    //  a new world has been set up
  //    //
  //    foreach (var job in _jobGraphics.Values)
  //    {
  //      Destroy(job);
  //    }

  //    _jobGraphics = new Dictionary<Job, GameObject>();
  //  }
  //}
  //public void OnHandle(JobUpdateEvent message)
  //{
  //  if (message.Job.IsCompleted())
  //  {
  //    RemoveGraphics(message.Job);
  //  }
  //  else
  //  {
  //    CreateGraphics(message.Job);
  //  }
  //  //// updateWorkForce = true;
  //  // ////  free worker
  //  // //_activeWorkers.Remove(message.Worker);
  //  // //_freeWorkers.Add(message.Worker);

  //  // //  finish job
  //  // var job = message.Job;

  //  // //  TODO - replace
  //  // //job.Tile.InstallFixedObject(job.FixedObject);
  //  // job.Tile.InstallItemOnTile(job.Item);
  //  // //job.Tile.JobScheduled = false;

  //  // RemoveGraphics(job);
  //}

  #endregion

  #region Unity

  void Awake()
  {
    Log("JobController.Awake");

    IoC.RegisterInstance(this);

    _resourceCollection = new ResourceCollection("Jobs");
  }

  internal void LoadJob(Job job)
  {
    //  just create game object
    CreateGraphics(job);
  }

  public void OnEnable()
  {
    Log("JobController.OnEnable");

    //IoC.Get<EventAggregator>().Subscribe(this);
  }

  // Start is called before the first frame update
  void Start()
  {
    Log("JobController.Start");
  }

  // Update is called once per frame
  void Update()
  {
    //  anything todo
    //
    var freeWorkers = IoC.Get<World>().GetCharacters(character => character.CurrentJob == null).ToList();

    if (freeWorkers.Count != 0)
    {
      var deliveryJobs = IoC.Get<World>().GetJobs().Where(job => job.Type == JobType.Delivery && job.Busy == false).Take(freeWorkers.Count).ToList();
      while (deliveryJobs.Count > 0)
      {
        freeWorkers[0].AssignJob(deliveryJobs[0]);
        deliveryJobs.RemoveAt(0);
        freeWorkers.RemoveAt(0);
      }


      //var inventory = IoC.Get<World>().GetInventory();

      var constructionJobs = IoC.Get<World>().GetJobs()
        .Where(job => job.Type == JobType.Construction &&
                      job.Busy == false &&
                      IoC.Get<InventoryController>().HaveEnoughResourcesAtBase(job.Item) )
        .Take(freeWorkers.Count).ToList();

      while (constructionJobs.Count > 0)
      {
        freeWorkers[0].AssignJob(constructionJobs[0]);
        constructionJobs.RemoveAt(0);
        freeWorkers.RemoveAt(0);
      }
    }
  }

  #endregion

  #region Methods

  //public void AddJob(Job job)
  //{   
  //  CreateGraphics(job);

  //  IoC.Get<WorldController>().AddJob(job);



  //  job.Tile.JobScheduled = true;
  //}

  private void CreateGraphics(Job job)
  {
    var graphic = new GameObject();
    //graphic.name = $"{job.Item.Type}_{job.Tile.Position}";
    graphic.name = $"{job.Item}_{job.Tile.Position}";
    graphic.transform.position = job.Tile.Position.GetVector();
    graphic.transform.SetParent(this.transform, true);

    _jobGraphics.Add(job, graphic);
    var renderer = graphic.AddComponent<SpriteRenderer>();
    renderer.sprite = _resourceCollection.GetSprite(GetSpriteName(job));
    renderer.sortingLayerName = "FixedObjects";

    //var audio = new GameObject();
    //audio.transform.position = job.Tile.Position.GetVector();
    //audio.transform.SetParent(this.transform, true);

    //  TODO SOUND
    //  doesn't seem to be working correct
    //job.AudioSource = graphic.AddComponent<AudioSource>();

    //// get sound
    //var factory = IoC.Get<ObjectFactory>().GetFactory(job.Item);
    //job.AudioSource.clip = Resources.Load<AudioClip>($"Sounds/{factory.BuildSound}"); ;

    ////job.AudioSource.loop = true;
    //job.AudioSource.volume = 0.1f;
    //job.AudioSource.spatialBlend = 1.0f;
    //job.AudioSource.rolloffMode = AudioRolloffMode.Linear;
    //job.AudioSource.minDistance = 5;
    //job.AudioSource.maxDistance = 11.5f;
  }

  private string GetSpriteName(Job job)
  {
    return IoC.Get<AbstractItemFactory>().GetSpriteName(job);
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

  //  Schedules a single one-tile job
  //
  public void ScheduleJob(string itemType, Tile tile, float rotation = 0)
  {
    var factory = IoC.Get<AbstractItemFactory>();
    var job = Job.CreateConstructionJob(itemType, tile, factory.GetBuildTime(itemType), rotation);

    //  Reserve a resource, order one if needed
    //
    IoC.Get<InventoryController>().ReserveResource(itemType);    

    //  Add the job to the module
    //
    IoC.Get<World>().ScheduleJob(job);

    CreateGraphics(job);
  }

  public void ScheduleDelivery(BuildingResource resource, Tile from, Tile to)
  {
    var delivery = Job.CreateDeliveryJob(resource, from, to);
    IoC.Get<World>().ScheduleJob(delivery);
  }

  internal void JobIsFinished(Job job)
  {
    IoC.Get<World>().RemoveCompletedJob(job);

    RemoveGraphics(job);

    //new JobUpdateEvent { Job = job }.Publish();
  }

  #endregion

}
