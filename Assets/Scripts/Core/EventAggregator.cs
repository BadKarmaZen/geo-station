using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public interface IHandle { }
public interface IHandle<T> : IHandle
{
  void OnHandle(T message);
}

public class EventAggregator
{
  #region Members

  private Dictionary<Type, (object instance, MethodInfo method)> _handlers = new Dictionary<Type, (object, MethodInfo)>();

  #endregion

  public void Subscribe(IHandle handler)
  {
    if (handler != null)
    {
      var type = handler.GetType();
      var methods = from i in type.GetInterfaces()
                    let method = i.GetMethod("OnHandle")
                    where method != null
                    select method;

      foreach (var method in methods)
      {
        Debug.Log($"EventAggregator.Subscribe({method.GetParameters()[0].ParameterType})");
        _handlers.Add(method.GetParameters()[0].ParameterType, (handler, method));
      }
    }
  }

  public void Publish<T>(T message)
  {
    if (_handlers.TryGetValue(message.GetType(), out var handler))
    {
      handler.method.Invoke(handler.instance, new object[] { message });
    }
    else
    {
      Debug.LogWarning("No instance subcribed to this event type");
    }
  }
}

public class Event
{
  public virtual void Publish()
  {
    IoC.Get<EventAggregator>().Publish(this);
  }
}