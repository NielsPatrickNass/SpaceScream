using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightflicker : MonoBehaviour
{
    public bool debugFlicker;

    public List<float> flickerHoldTime;

    public List<GameObject> flickerShowingObjects;

    public List<bool> showObjects;

    public void Stop()
    {
        StopAllCoroutines();
        enabled = false;
    }

    public void StartFlickerSequence()
    {
        if (lights != null)
        {
            foreach (Light light in lights)
                light.gameObject.SetActive(true);

            foreach (GameObject g in flickerShowingObjects)
                g.SetActive(false);
        }
        StopAllCoroutines();
        StartCoroutine(Flicker());
    }

    // Update is called once per frame
    void Update()
    {
        if (debugFlicker)
        {
            debugFlicker = false;
            StartFlickerSequence();
        }
    }
    Light[] lights = new Light[0];

    public float chanceForShowing;

    IEnumerator Flicker()
    {
        lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);

        List<float> list = new List<float>(flickerHoldTime);
        List<bool> olist = new List<bool>(showObjects);
        float chanceRoll = Random.Range(0f, 1f);
        bool isScare = chanceRoll <= chanceForShowing;
        while (list.Count > 0)
        {

            foreach (Light light in lights)
            {
                light.gameObject.SetActive(!light.gameObject.activeSelf);

            }
            if (isScare)
            {
                foreach (GameObject g in flickerShowingObjects)
                {
                    g.SetActive(olist[0]);

                }
            }

            yield return new WaitForSecondsRealtime(list[0]);
            list.RemoveAt(0);
            olist.RemoveAt(0);
        }
    }
}
