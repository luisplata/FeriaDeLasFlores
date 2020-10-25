using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Supports invoking one integer argument UnityEvents
/// </summary>
public class IntEventInvoker : MonoBehaviour
{
    protected Dictionary<EventName, UnityEvent<int>> unityEvents = new Dictionary<EventName, UnityEvent<int>>();    

    /// <summary>
    /// Adds the given listener for the given event
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="listener"></param>
    public void AddListener(EventName eventName, UnityAction<int> listener)
    {
        //Only adds listeners for supported events
        if (unityEvents.ContainsKey(eventName))
        {
            unityEvents[eventName].AddListener(listener);
        }
    }
}
