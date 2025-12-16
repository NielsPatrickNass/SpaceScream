using UnityEngine;
using System;
using System.Text.RegularExpressions;

public static class TextUtils
{
    public static float CalculateSimilarity(string target, string input)
    {
        if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(input)) return 0f;

        string sTarget = CleanString(target);
        string sInput = CleanString(input);

        // 1. DIRECT SUBSTRING MATCH (Fixes the "Double Recording" bug)
        // If Whisper says "The coast is clear. The coast is clear.", 
        // this checks if "thecoastisclear" exists inside it.
        if (sInput.Contains(sTarget)) 
        {
            return 1.0f; // 100% Match
        }

        // 2. LEVENSHTEIN (Fuzzy Check for typos)
        int dist = ComputeLevenshteinDistance(sTarget, sInput);
        int maxLen = Mathf.Max(sTarget.Length, sInput.Length);
        
        return 1.0f - ((float)dist / maxLen);
    }

    private static string CleanString(string input)
    {
        // 1. Remove all punctuation (dots, commas, symbols)
        string clean = Regex.Replace(input, @"[^\w\s]", "");
        
        // 2. Remove all whitespace (spaces, tabs, newlines)
        // This makes "The coast is clear" become "thecoastisclear"
        // preventing issues where Whisper puts extra spaces between words.
        clean = Regex.Replace(clean, @"\s+", "");

        return clean.ToLower();
    }

    private static int ComputeLevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Mathf.Min(
                    Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }
}