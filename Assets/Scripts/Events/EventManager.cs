using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages connections between event listeners and event connections
/// </summary>
public static class EventManager
{
    #region Fields

    private static Dictionary<EventName, List<IntEventInvoker>> invokers =
        new Dictionary<EventName, List<IntEventInvoker>>();
    private static Dictionary<EventName, List<UnityAction<int>>> listeners =
        new Dictionary<EventName, List<UnityAction<int>>>();

    #endregion

    #region Public methods
    /// <summary>
    /// Initialize the event manager
    /// </summary>
    public static void Initialize()
    {
        //create empty lists for all the dictionary entries
        foreach(EventName eventName in Enum.GetValues(typeof(EventName))){
            if (!invokers.ContainsKey(eventName))
            {
                invokers.Add(eventName, new List<IntEventInvoker>());
                listeners.Add(eventName, new List<UnityAction<int>>());
            }
            else
            {
                invokers[eventName].Clear();
                listeners[eventName].Clear();
            }
        }
    }

    /// <summary>
    /// Adds the invoker as a caller for the given event
    /// </summary>
    /// <param name="eventName">event name</param>
    /// <param name="invoker">invoker object</param>
    public static void AddInvoker(EventName eventName, IntEventInvoker invoker)
    {
        invokers[eventName].Add(invoker);

        foreach(UnityAction<int> listener in listeners[eventName])
        {
            invoker.AddListener(eventName, listener);
        }
    }

    /// <summary>
    /// Adds the listener to listen for the given event
    /// </summary>
    /// <param name="eventName">event name</param>
    /// <param name="listener">listener delegate</param>
    public static void AddListener(EventName eventName, UnityAction<int> listener)
    {
        listeners[eventName].Add(listener);

        foreach (IntEventInvoker invoker in invokers[eventName])
        {
            invoker.AddListener(eventName, listener);
        }
    }

    /// <summary>
    /// Removes the invoker for the given event name
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="invoker"></param>
    public static void RemoveInvoker(EventName eventName, IntEventInvoker invoker)
    {
        invokers[eventName].Remove(invoker);
    }

    #endregion
}
