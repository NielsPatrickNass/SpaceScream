using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class OnAnimationSound : MonoBehaviour
{
    public List<AudioClip> sources;

    public float volume;
    public float volumeRdm;
    public float pitchshiftRdm;

    public GameObject audiosourceRef;

    public void PlaySound(int soundID)
    {
        GameObject loc = Instantiate(audiosourceRef);
        AudioSource auds = loc.GetComponent<AudioSource>();
        auds.clip = sources[soundID];
        auds.transform.position = transform.position;
        auds.volume = Random.Range(volume-volumeRdm, volume + volumeRdm);
        auds.pitch = 1f+Random.Range(-pitchshiftRdm, pitchshiftRdm);
        auds.Play();
    }
}
