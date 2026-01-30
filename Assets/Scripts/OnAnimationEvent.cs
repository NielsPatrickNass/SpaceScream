using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnAnimationEvent : MonoBehaviour
{
    public List<UnityEvent> unityEvents;

    public void InvokeEvent(int toInvoke)
    {
        unityEvents[toInvoke].Invoke();
    }

}
