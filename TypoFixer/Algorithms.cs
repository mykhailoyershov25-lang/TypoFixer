namespace TypoFixer;

public static class Algorithms
{
    public static int LCS(string word1, string word2)
    {
        if (string.IsNullOrEmpty(word1) || string.IsNullOrEmpty(word2)) return 0;
        
        int n = word1.Length;
        int m = word2.Length;
        
        int[,] matrix = new int[n + 1, m + 1];

        for (int i = 1; i < n; i++)
        {
            for (int j = 1; j < m; j++)
            {
                if (word1[i-1] == word2[j-1])
                {
                    matrix[i, j] = matrix[i - 1, j - 1] + 1;
                }
                else
                {
                    matrix[i, j] = Math.Max(matrix[i - 1, j], matrix[i, j - 1]);
                }
            }
        }
        return matrix[n, m];
    }

    public static int Levenshtein(string word1, string word2)
    {
        if (string.IsNullOrEmpty(word1)) return word2?.Length ?? 0;
        if (string.IsNullOrEmpty(word2)) return word1?.Length ?? 0;
        
        int n = word1.Length;
        int m = word2.Length;
        
        int[,] matrix = new int[n + 1, m + 1];
        for (int i = 0; i <= n; i++) matrix[i, 0] = i;
        for (int i = 0; i <= m; i++) matrix[0, i] = i;
    
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                if (word1[i-1] == word2[j-1])
                {
                    matrix[i, j] = Math.Min(Math.Min(matrix[i-1, j]+1, matrix[i, j-1]+1), matrix[i-1, j-1]);
                }
                else
                {
                    matrix[i, j] = Math.Min(Math.Min(matrix[i-1, j]+1, matrix[i, j-1]+1), matrix[i-1, j-1]+1);
                }
            }
        }

        return matrix[n, m];
    }

    public static int DamerauLevenshtein(string word1, string word2)
    {
        if (string.IsNullOrEmpty(word1)) return word2?.Length ?? 0;
        if (string.IsNullOrEmpty(word2)) return word1?.Length ?? 0;
        
        int n = word1.Length;
        int m = word2.Length;
        
        int[,] matrix = new int[n + 1, m + 1];
        for (int i = 0; i <= n; i++) matrix[i, 0] = i;
        for (int i = 0; i <= m; i++) matrix[0, i] = i;
    
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = 1;
                if (word1[i - 1] == word2[j - 1]) cost = 0;

                matrix[i, j] = Math.Min(Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
                
                if (i > 1 && j > 1 && word1[i - 1] == word2[j - 2] && word1[i - 2] == word2[j - 1])
                {
                    matrix[i, j] = Math.Min(matrix[i, j], matrix[i - 2, j - 2] + 1);
                }

            }
        }

        return matrix[n, m];
    }

    public static List<string> TopSuggestions(string typo, List<string> dictionary)
    {
        var suggestions = new List<(string word, int distance)>();

        foreach (var word in dictionary)
        {
            int distance = DamerauLevenshtein(typo.ToLower(), word.ToLower());
            suggestions.Add((word, distance));
        }

        return suggestions
            .OrderBy(s => s.distance)
            .Take(5)
            .Select(s => s.word)
            .ToList();
    }
}