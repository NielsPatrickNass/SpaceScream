using HuggingFace.SharpTransformers.Normalizers;
using HuggingFace.SharpTransformers.PostProcessors;
using HuggingFace.SharpTransformers.PreTokenizers;
using HuggingFace.SharpTransformers.Tokenizers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.InferenceEngine;
using UnityEngine;

namespace SentenceSimilarityUtils
{
    public static class SentenceSimilarityUtils_
    {
        // --------------------------
        // LOAD TOKENIZER
        // --------------------------

        public static JObject LoadTokenizerJson()
        {
            TextAsset tok = Resources.Load<TextAsset>("Model/tokenizer");
            return JObject.Parse(tok.text);
        }

        // --------------------------
        // TOKENIZATION
        // --------------------------

        static Tuple<List<List<int>>, List<List<int>>, List<List<int>>> Tokenize(List<string> candidates)
        {
            JObject tokenizerJson = LoadTokenizerJson();

            var norm = new BertNormalizer((JObject)tokenizerJson["normalizer"]);
            var pre = new BertPreTokenizer((JObject)tokenizerJson["pre_tokenizer"]);
            var wp = new WordPieceTokenizer((JObject)tokenizerJson["model"]);
            var template = new TemplateProcessing((JObject)tokenizerJson["post_processor"]);

            List<List<int>> ids = new();
            foreach (string text in candidates)
            {
                string normalized = norm.Normalize(text);
                List<string> preTok = pre.PreTokenize(normalized);
                List<string> tok = wp.Encode(preTok);
                List<string> processed = template.PostProcess(tok);
                ids.Add(wp.ConvertTokensToIds(processed));
            }

            int maxLen = (int)tokenizerJson["truncation"]["max_length"];

            var (attentionMask, tokenIds) =
                PaddingOrTruncate(ids, maxLen);

            var tokenTypeIds = tokenIds
                .Select(row => row.Select(_ => 0).ToList())
                .ToList();

            return Tuple.Create(tokenIds, attentionMask, tokenTypeIds);
        }

        // --------------------------
        // CREATE SENTIS TENSORS
        // --------------------------

        public static Dictionary<string, Tensor> TokenizeInput(List<string> sentences)
        {
            var t = Tokenize(sentences);

            List<List<int>> ids = t.Item1;
            List<List<int>> mask = t.Item2;
            List<List<int>> tokTypes = t.Item3;

            int batch = ids.Count;
            int seq = ids[0].Count;

            Tensor<int> tIds = new Tensor<int>(new TensorShape( batch, seq ), Flatten(ids).ToArray());
            Tensor<int> tMask = new Tensor<int>(new TensorShape ( batch, seq ), Flatten(mask).ToArray());
            Tensor<int> tTypes = new Tensor<int>(new TensorShape ( batch, seq ), Flatten(tokTypes).ToArray());

            return new Dictionary<string, Tensor>
            {
                { "input_ids", tIds },
                { "attention_mask", tMask },
                { "token_type_ids", tTypes }
            };
        }

        static IEnumerable<int> Flatten(List<List<int>> list)
        {
            foreach (var inner in list)
                foreach (var x in inner)
                    yield return x;
        }

        // --------------------------
        // MEAN POOLING (modern)
        // --------------------------

        public static Tensor<float> MeanPooling(Tensor<int> attentionMask, Tensor<float> hidden)
        {
            int b = hidden.shape[0];
            int s = hidden.shape[1];
            int h = hidden.shape[2];

            float[] att = attentionMask.DownloadToArray().Select(v => v > 0 ? 1f : 0f).ToArray();
            float[] hid = hidden.DownloadToArray();

            float[] output = new float[b * h];

            for (int batch = 0; batch < b; batch++)
            {
                for (int dim = 0; dim < h; dim++)
                {
                    float sum = 0;
                    float count = 0;

                    for (int tok = 0; tok < s; tok++)
                    {
                        float m = att[batch * s + tok];
                        if (m == 1f)
                        {
                            sum += hid[(batch * s + tok) * h + dim];
                            count += 1f;
                        }
                    }
                    output[batch * h + dim] = sum / Mathf.Max(count, 1e-9f);
                }
            }

            return new Tensor <float>(new TensorShape ( b, h ), output);
        }

        // --------------------------
        // L2 NORMALIZATION (modern)
        // --------------------------

        public static Tensor<float> L2Norm(Tensor<float> t)
        {
            int b = t.shape[0];
            int h = t.shape[1];
            float[] x = t.DownloadToArray();
            float[] outArr = new float[b * h];

            for (int i = 0; i < b; i++)
            {
                float norm = 0f;
                for (int d = 0; d < h; d++)
                {
                    float v = x[i * h + d];
                    norm += v * v;
                }

                norm = Mathf.Sqrt(norm) + 1e-9f;

                for (int d = 0; d < h; d++)
                {
                    outArr[i * h + d] = x[i * h + d] / norm;
                }
            }

            return  new Tensor <float>(new TensorShape(b, h), outArr);
        }

        // --------------------------
        // COSINE SIMILARITY (manual)
        // --------------------------

        public static Tensor<float> CosineSimilarity(Tensor<float> a, Tensor<float> b)
        {
            // a: [1, H]
            // b: [N, H]
            int N = b.shape[0];
            int H = b.shape[1];

            float[] va = a.DownloadToArray();
            float[] vb = b.DownloadToArray();

            float[] result = new float[N];

            for (int i = 0; i < N; i++)
            {
                float dot = 0f;

                for (int d = 0; d < H; d++)
                    dot += va[d] * vb[i * H + d];

                result[i] = dot;
            }

            return new Tensor<float>(new TensorShape ( 1, N ), result);
        }

        // --------------------------
        // PADDING / TRUNCATION
        // --------------------------

        static Tuple<List<List<int>>, List<List<int>>> PaddingOrTruncate(
            List<List<int>> tokens, int maxLen)
        {
            List<List<int>> ids = new();
            List<List<int>> mask = new();

            foreach (var row in tokens)
            {
                List<int> r = row;
                if (r.Count > maxLen)
                    r = r.Take(maxLen).ToList();

                if (r.Count < maxLen)
                    r = r.Concat(Enumerable.Repeat(0, maxLen - r.Count)).ToList();

                ids.Add(r);
                mask.Add(r.Select(v => v == 0 ? 0 : 1).ToList());
            }

            return Tuple.Create(mask, ids);
        }
    }
}
