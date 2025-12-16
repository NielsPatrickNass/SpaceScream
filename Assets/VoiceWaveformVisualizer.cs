using UnityEngine;
using System.Collections.Generic;
using Whisper.Utils;

[RequireComponent(typeof(LineRenderer))]
public class VoiceWaveformVisualizer : MonoBehaviour
{
    [Header("References")]
    public MicrophoneRecord micRecord;
    public VoiceCalibrationManager calibrationManager;

    [Header("Visual Settings")]
    public int pointCount = 600;      // High res for smoothness
    public float width = 1200.0f;     // Width of the line in pixels/units
    public float lineWidth = 1.0f;    // UPDATED: Thinner line (was 6.0)
    
    [Header("Wave Behavior")]
    [Range(1f, 20f)] public float waveFrequency = 10f; // How many "humps" in the wave
    [Range(1f, 50f)] public float waveSpeed = 15f;     // How fast the sine wave wiggles
    [Range(1f, 500f)] public float sensitivity = 200f; // How tall the spikes get
    [Range(0.1f, 1f)] public float decay = 0.90f;      // UPDATED: Faster decay (0.90) for lighter feel

    [Header("Colors")]
    public Color idleColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public Color goodColor = Color.green;
    public Color loudColor = Color.red;
    public Color quietColor = Color.yellow;

    private LineRenderer _lineRenderer;
    private float[] _volumeHistory; // Stores the "Envelope" (loudness) history
    private float _currentSmoothedVol;

    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = pointCount;
        _lineRenderer.useWorldSpace = false; 
        
        // Make joints round so the line looks like a liquid
        _lineRenderer.numCornerVertices = 10;
        _lineRenderer.numCapVertices = 10;

        _volumeHistory = new float[pointCount];

        // Initial Style
        _lineRenderer.material.color = idleColor;
        _lineRenderer.startColor = idleColor;
        _lineRenderer.endColor = idleColor;
    }

    void Update()
    {
        _lineRenderer.startWidth = lineWidth;
        _lineRenderer.endWidth = lineWidth;

        UpdateColor();
        UpdateVolumeHistory();
        DrawSiriWave();
    }

    private void UpdateVolumeHistory()
    {
        float rawVolume = 0f;

        // 1. Get RMS (Volume) from Mic
        if (micRecord != null && micRecord.IsRecording && micRecord._clip != null)
        {
            // We calculate RMS manually for the visualizer to keep it separate from the logic
            float[] tempSamples = new float[128];
            int micPos = Microphone.GetPosition(micRecord.RecordStartMicDevice);
            if (micPos < tempSamples.Length) micPos = tempSamples.Length;
            micRecord._clip.GetData(tempSamples, micPos - tempSamples.Length);

            float sum = 0f;
            foreach (var s in tempSamples) sum += s * s;
            rawVolume = Mathf.Sqrt(sum / tempSamples.Length); // RMS
        }
        else
        {
            // Idle breathing
            rawVolume = Mathf.PerlinNoise(Time.time, 0) * 0.005f; 
        }

        // 2. Smooth the volume (Attack/Decay)
        _currentSmoothedVol = Mathf.Lerp(_currentSmoothedVol, rawVolume, Time.deltaTime * 20f);

        // 3. Scroll the History Buffer Left
        for (int i = 0; i < pointCount - 1; i++)
        {
            _volumeHistory[i] = _volumeHistory[i + 1];
        }

        // 4. Insert new volume at the Right edge
        // Apply sensitivity
        float displayVal = Mathf.Clamp(_currentSmoothedVol * sensitivity, 0f, 10f); 
        _volumeHistory[pointCount - 1] = displayVal;
    }

    private void DrawSiriWave()
    {
        if (_lineRenderer.positionCount != pointCount) _lineRenderer.positionCount = pointCount;

        Vector3[] positions = new Vector3[pointCount];
        float xStep = width / pointCount;
        float startX = -(width / 2f);

        for (int i = 0; i < pointCount; i++)
        {
            float x = startX + (i * xStep);
            
            // 1. The Math Wave (Perfect Sine)
            // Calculates a wiggling sine wave based on time and position
            float baseWave = Mathf.Sin((i * 0.1f * waveFrequency) - (Time.time * waveSpeed));

            // 2. The Envelope (Volume History)
            // We multiply the perfect wave by the volume history at this position.
            // If volume history is 0, the wave is flat. If volume is high, the wave expands.
            float envelope = _volumeHistory[i];

            // 3. Taper the edges (So the line fades to 0 at left and right sides of screen)
            // This prevents the line from "popping" in at the edge
            float edgeFade = 1.0f;
            if (i < 20) edgeFade = i / 20f;
            if (i > pointCount - 20) edgeFade = (pointCount - i) / 20f;

            float y = baseWave * envelope * 50f * edgeFade; // 50f is base amplitude multiplier

            positions[i] = new Vector3(x, y, 0);
        }

        _lineRenderer.SetPositions(positions);
    }

    private void UpdateColor()
    {
        if (calibrationManager == null || calibrationManager.detector == null) return;

        Color targetColor = idleColor;

        if (calibrationManager.isTestActive)
        {
            // Instant feedback from detector
            string phase = calibrationManager.currentPhaseString;
            if (calibrationManager.detector.HasViolatedUpperLimit(phase))
                targetColor = loudColor;
            else if (calibrationManager.detector.HasViolatedLowerLimit(phase))
                targetColor = quietColor;
            else
                targetColor = goodColor;
        }

        Color nextColor = Color.Lerp(_lineRenderer.startColor, targetColor, Time.deltaTime * 10f);
        _lineRenderer.startColor = nextColor;
        _lineRenderer.endColor = nextColor;
        _lineRenderer.material.color = nextColor;
    }
}