using System;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;

public class OnVideoplayer : MonoBehaviour
{
    private VideoPlayer vplayer;

    public UnityEvent OnEndReachedevent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        vplayer = GetComponent<VideoPlayer>();
        vplayer.loopPointReached += OnVideoEndReached;
    }

    private void OnVideoEndReached(VideoPlayer source)
    {
        OnEndReachedevent.Invoke();
    }

}
