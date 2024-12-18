public class LazyDatabase : LevenshteinDatabase {
    private readonly Dictionary<int, int> lengthStartIndexes;

    public LazyDatabase(String dictionaryPath) : base(dictionaryPath) {
        lengthStartIndexes = new Dictionary<int, int>();
        for (int i = 0; i < this.dictionary.Length; i++) {
            lengthStartIndexes.TryAdd(this.dictionary[i].Length, i);
        }
    }

    public override bool areNeighbors(int wordIndex1, int wordIndex2) {
        return areNeighboring(this.wordAt(wordIndex1), this.wordAt(wordIndex2));
    }

    public override int[] findNeighbors(int wordIndex) {
        List<int> neighbors = new List<int>();

        String w = this.wordAt(wordIndex);
        int endIndex = dictionary.Length;
        lengthStartIndexes.TryGetValue(w.Length + 2, out endIndex);

        // Reduces the searching scope to only words with a length that allows them to be adjacent
        int i = 0;
        for (lengthStartIndexes.TryGetValue(w.Length - 1, out i); i < endIndex; i++) {
            if (areNeighboring(w, this.dictionary[i])) {
                neighbors.Add(i);
            }
        }

        return neighbors.ToArray();
    }

    protected static bool areNeighboring(String w1, String w2) {
        int w1l = w1.Length;
        int w2l = w2.Length;
        int lengthDifference = w1l - w2l;
        bool foundDifference = false;

        if (lengthDifference == 0) {
            for (int i = 0; i < w1l; i++) {
                if (w1.ElementAt(i) != w2.ElementAt(i)) {
                    if (foundDifference) {
                        return false;
                    } else {
                        foundDifference = true;
                    }
                }
            }
            // If a difference was never found, the words are equal, and false is still returned
            return foundDifference;
        }

        // The next part requires word1 to be shorter than word2,
        // which is why they need to be swapped
        if (lengthDifference > 0) {
            String t = w2;
            w2 = w1;
            w1 = t;
            w1l = w2l;
        }

        for (int i = 0, w2Index = 0; i < w1l; i++, w2Index++) {
            if (w1.ElementAt(i) != w2.ElementAt(w2Index)) {
                if (foundDifference) {
                    return false;
                } else {
                    foundDifference = true;
                    i--;
                }
            }
        }
        return true;
    }
};
