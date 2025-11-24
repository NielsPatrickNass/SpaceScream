using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SentenceSimilarityUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.InferenceEngine;
using UnityEngine;
public class SentenceSimilarity : MonoBehaviour
{
    [Tooltip("Model asset imported for Sentis")]
    public ModelAsset modelAsset;

    private Model runtimeModel;
    private Worker worker;
    // allocator + ops removed — newer API uses Tensor<T> and functional ops instead
    // e.g. BackendType etc remain

    private void Awake()
    {
        // Load the model
        runtimeModel = ModelLoader.Load(modelAsset);

        // Choose backend: e.g. GPUCompute or CPU
        worker = new Worker(runtimeModel, BackendType.GPUCompute);

        // If you want CPU fallback you could also instantiate another Worker or specify in config
    }

    private void OnDisable()
    {
        worker?.Dispose();
        worker = null;
    }

    /// <summary>
    /// Encode the input sentences and return normalized embeddings.
    /// </summary>
    public Tensor<float> Encode(List<string> input)
    {
        // Tokenize input sentences into tensors
        Dictionary<string, Tensor> inputTokens = SentenceSimilarityUtils_.TokenizeInput(input);

        // Set inputs on the worker, newer API uses SetInput(s)
        foreach (var kv in inputTokens)
        {
            worker.SetInput(kv.Key, kv.Value);
        }

        // Schedule the execution
        worker.Schedule();

        // Get the output tensor (assuming model’s output named "last_hidden_state")
        Tensor<float> outputTensor = (Tensor<float>)worker.PeekOutput("last_hidden_state");

        // Perform mean pooling
        Tensor attentionMask = inputTokens["attention_mask"];
        Tensor<float> meanPooled = SentenceSimilarityUtils_.MeanPooling(((Tensor<int>)attentionMask), outputTensor);

        // L2-normalize
        Tensor<float> normalized = SentenceSimilarityUtils_.L2Norm(meanPooled);

        // Clean up intermediate tensors if necessary
        foreach (var t in inputTokens.Values) t.Dispose();
        outputTensor.Dispose();
        meanPooled.Dispose();

        return normalized;
    }

    /// <summary>
    /// Compute cosine similarities between input embedding and comparison embeddings.
    /// </summary>
    public Tensor<float> ComputeSimilarity(Tensor<float> inputEmb, Tensor<float> comparisonEmbeddings)
    {
        // Use functional ops or direct mat‐mul depending on API
        // Example: Tensor<float> scores = Ops.MatMul2D(inputEmb, comparisonEmbeddings, transposeB:true);
        // For simplicity assume a utility method:
        return SentenceSimilarityUtils_.CosineSimilarity(inputEmb, comparisonEmbeddings);
    }

    /// <summary>
    /// Returns index & score of best matching sentence.
    /// </summary>
    public (int bestIndex, float bestScore) RankSimilarity(string inputSentence, string[] comparisonSentences)
    {
        List<string> inputList = new List<string> { inputSentence };
        List<string> compList = comparisonSentences.ToList();

        using (Tensor<float> inputEmb = Encode(inputList))
        using (Tensor<float> compEmb = Encode(compList))
        using (Tensor<float> scores = ComputeSimilarity(inputEmb, compEmb))
        {
            // Copy scores to managed array
            float[] scoreArray = scores.DownloadToArray();  // replaced ToReadOnlyArray in new API :contentReference[oaicite:2]{index=2}

            int bestIdx = 0;
            float bestVal = scoreArray[0];
            for (int i = 1; i < scoreArray.Length; i++)
            {
                if (scoreArray[i] > bestVal)
                {
                    bestVal = scoreArray[i];
                    bestIdx = i;
                }
            }

            return (bestIdx, bestVal);
        }
    }
}