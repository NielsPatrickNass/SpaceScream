using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Vent : MonoBehaviour
{
    public float intervalMin;
    public float intervalMax;

    public float intervalCurr;
    public float lastTriggered;

    public UnityEvent unityEvent;
    public List<UnityEvent> unityEventOnAnimation;

    public void OnAnimation(int idx)
    {
        unityEventOnAnimation[idx].Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > lastTriggered + intervalCurr)
        {
            lastTriggered = Time.time;
            intervalCurr = Random.Range(intervalMin, intervalMax);
            unityEvent.Invoke();
        }
    }
}
