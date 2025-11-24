using UnityEngine;

public class WalkieTalkieVisualizer : MonoBehaviour
{
    public float intensity;

    public float speed;

    public Transform visualizerHolder;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < visualizerHolder.childCount; i++) {
            visualizerHolder.GetChild(i).localScale = new Vector3(Mathf.Lerp(visualizerHolder.GetChild(i).localScale.x, intensity, speed), visualizerHolder.GetChild(i).localScale.y, visualizerHolder.GetChild(i).localScale.z);
        }
    }
}
