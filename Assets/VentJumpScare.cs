using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.Events;

public class VentJumpScare : MonoBehaviour
{
    public float lastJumpscare;
    public float timeUntilScareCurr;
    public float timeUntilScareMin;
    public float timeUntilScareMax;

    public AnimationCurve rustleCurve;

    public Vent vent;

    public UnityEvent jumpscareEvent;

    public AudioSource tensionMusic;

    public float tensionVolumeMin;
    public float tensionVolumeMax;

    private void OnEnable()
    {
        lastJumpscare = Time.time;
        timeUntilScareCurr = Random.Range(timeUntilScareMin, timeUntilScareMax);
    }

    // Update is called once per frame
    void Update()
    {
        vent.intervalMax = rustleCurve.Evaluate((Time.time - lastJumpscare) / timeUntilScareCurr);

        tensionMusic.volume = Mathf.Lerp(tensionVolumeMin, tensionVolumeMax, (Time.time - lastJumpscare) / timeUntilScareCurr);
        if (Time.time-lastJumpscare > timeUntilScareCurr)
        {
            tensionMusic.volume = 0;
            timeUntilScareCurr = Random.Range(timeUntilScareMin, timeUntilScareMax);
            lastJumpscare = Time.time;
            jumpscareEvent.Invoke();
        }
    }
}
