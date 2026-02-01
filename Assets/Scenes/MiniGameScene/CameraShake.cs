using UnityEngine;
using System.Collections;

/// <summary>
/// Simple camera shake effect for impact feedback.
/// Attach to Main Camera.
/// </summary>
public class CameraShake : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool enableShake = true;
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isShaking = false;
    
    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }
    
    /// <summary>
    /// Trigger a camera shake
    /// </summary>
    public void Shake(float intensity = 0.2f, float duration = 0.1f)
    {
        if (!enableShake) return;
        
        if (isShaking)
        {
            StopAllCoroutines();
        }
        
        StartCoroutine(ShakeCoroutine(intensity, duration));
    }
    
    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        isShaking = true;
        float elapsed = 0f;
        
        Vector3 startPos = transform.localPosition;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            transform.localPosition = startPos + new Vector3(x, y, 0f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Return to original position
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        
        isShaking = false;
    }
    
    /// <summary>
    /// Update original position (call if camera moves)
    /// </summary>
    public void UpdateOriginalPosition()
    {
        if (!isShaking)
        {
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
        }
    }
}
