using System.Collections;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    public float delay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
