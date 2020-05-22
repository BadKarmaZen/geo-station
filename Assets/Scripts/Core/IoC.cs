using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IoC
{
  #region Members

  private static Dictionary<Type, object> _services;

  #endregion

  #region Construction

  static IoC()
  {
    Debug.Log("*** IoC created ***");
  }

  #endregion

  #region Methods

  public static void Initialize()
  {
    Debug.Log("*** IoC Initialize ***");

    _services = new Dictionary<Type, object>();

    RegisterType<EventAggregator>();
  }

  public static void RegisterInstance<T>(T service)
  {
    Debug.Log($"IoC.RegisterInstance: {typeof(T)}");

    _services.Add(typeof(T), service);
  }
  public static T RegisterType<T>()
    where T : class, new()
  {
    Debug.Log($"IoC.RegisterType: {typeof(T)}");
    var instance = new T();
    _services.Add(typeof(T), instance);
    return instance;
  }

  public static T Get<T>()
  {
    return (T)_services[typeof(T)];
  }

  #endregion
}
