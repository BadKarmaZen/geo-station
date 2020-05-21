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

  private Dictionary<Type, List<(object instance, MethodInfo method)>> _handlers = new Dictionary<Type, List<(object, MethodInfo)>>();

  #endregion

  public void Subscribe(IHandle handler)
  {
    if (handler != null)
    {
      var methods = from @interface in handler.GetType().GetInterfaces()
                    let method = @interface.GetMethod("OnHandle")
                    where method != null
                    let type = method.GetParameters()[0].ParameterType
                    select (type, method);

      foreach (var (eventType, method) in methods)
      {
        Debug.Log($"EventAggregator.Subscribe({method.GetParameters()[0].ParameterType})");

        if (!_handlers.ContainsKey(eventType))
        {
          _handlers.Add(eventType, new List<(object instance, MethodInfo method)>());
        }

        _handlers[eventType].Add((handler, method));
      }
    }
  }

  public void Publish<T>(T message)
  {
    if (_handlers.TryGetValue(message.GetType(), out var handlers))
    {
      foreach (var handler in handlers)
      {
        handler.method.Invoke(handler.instance, new object[] { message });
      }
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