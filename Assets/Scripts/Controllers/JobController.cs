using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobUpdateEvent
{
  public Job Job { get; set; }
  public Character Worker { get; set; }
}

public class JobController : MonoBehaviour, IHandle<JobUpdateEvent>
{
  #region Members

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
    job.Tile.JobScheduled = true;
    _scheduledJob.Add(job);
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
        //  job is delete
        job.Tile.CannotCompleteJob(job.FixedObject);
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
    job.Tile.InstallFixedObject(job.FixedObject);
    job.Tile.JobScheduled = false;
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
