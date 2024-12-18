public class LevenshteinGraph {
    private Dictionary<int, List<int>> outer;

    /**
     * Searched is used both for reconstructing paths after finishing the
     * breadth-first search and ensuring each word is contained in only one
     * layer.
     */
    private Dictionary<int, List<int>> searched;

    /**
     * Initializes the graph, with searched being completely empty and outer only containing the root word with no previous.
     * @param root Word to put in the outer layer of the graph.
     */
    public LevenshteinGraph(int root) {
        searched = new Dictionary<int, List<int>>();
        outer = new Dictionary<int, List<int>>();
        outer.Add(root, new List<int>());
    }
    
    /**
     * This method puts every word in outer into searched and replaces outer with a new outer containing the neighbors of the previous outer.
     * NOTE: A neighbor already in the graph is not added. Any path which arrives at a word later than another path will not be a levenshtein.
     * Generating the graph layer-by-layer and checking for links on each layer prevents generation and checking of words with distances past the target. 
     * It does this by temporarily creating a new Dictionary to store the new outer.
     * For each word in the previous outer, all of its neighbors are added to the new outer.
     * The word is also added to the previous of each neighbor.
     * If a word generates a neighbor that's already in the new outer, the word is simply added to the previous of that neighbor.
     * Once it is finished iterating across outer, it is put into searched and replaced with newOuter.
     * @param dictionary Array of all legal words.
     * @param lengthStartIndexes A map, with the values being the first index of a word in dictionary of a length equal to its key.
     * @return whether generateNewOuter succeeded. When it fails, that
     *         means the search has reached a dead-end.
     */
    public bool generateNewOuter(LevenshteinDatabase database) {
        Dictionary<int, List<int>> newOuter = new Dictionary<int, List<int>>();

        /*searched = searched.Concat(outer).ToDictionary(pair => pair.Key, pair => pair.Value);*/
        // The foreach loop is faster than Concat, which is extremely disappointing.
        foreach (KeyValuePair<int, List<int>> entry in outer) {
            searched.Add(entry.Key, entry.Value);
        }

        foreach (int outerWord in outer.Keys) {
            int[] neighbors = database.findNeighbors(outerWord);

            foreach (int neighbor in neighbors) {
                if (searched.ContainsKey(neighbor)) {
                    continue;
                }

                if (newOuter.ContainsKey(neighbor)) {
                    newOuter[neighbor].Add(outerWord);
                } else {
                    List<int> listToAdd = new List<int>();
                    listToAdd.Add(outerWord);
                    newOuter.Add(neighbor, listToAdd);
                }
            }
        }
        
        if (newOuter.Count == 0) {
            return false;
        }

        outer = newOuter;
        return true;
    }

    /**
     * Finds all paths between wordIndex1 and wordIndex2 after a breadth-first
     * search has been completed.
     *
     * Does this by reading the values of the searched map (Lists containing
     * every previous word), adding each to a copy of the path and recursively
     * calling the helper method with these copies, returning once the path has
     * reached the its destination word.
     *
     * @param wordIndex1 first word index
     * @param wordIndex2 second word index
     * @param reversed if false, search from wordIndex1 -> wordIndex2
     *                 if true, search from wordIndex2 -> wordIndex1
     * @return all paths between wordIndex1 and wordIndex2
     */
    public List<LinkedList<int>> allPathsBetween(int wordIndex1, int wordIndex2, bool reversed) {
        LinkedList<int> previous = new LinkedList<int>();
        previous.AddFirst(wordIndex2);
        List<LinkedList<int>> toReturn = new List<LinkedList<int>>();

        if (wordIndex1 == wordIndex2) {
            toReturn.Add(previous);
        } else if (reversed) {
            allPathsBetween(toReturn, previous, wordIndex1, outer[wordIndex2], (i, p) => p.AddLast(i));
        } else {
            allPathsBetween(toReturn, previous, wordIndex1, outer[wordIndex2], (i, p) => p.AddFirst(i));
        }

        return toReturn;
    }

    private void allPathsBetween(List<LinkedList<int>> paths, LinkedList<int> currentPath, int root, List<int> setToSearch, Action<int, LinkedList<int>> pathAdder) {
        if (setToSearch.Contains(root)) {
            pathAdder(root, currentPath);
            paths.Add(currentPath);
            return;
        }

        foreach (int wordIndex in setToSearch) {
            LinkedList<int> newPrevious = new LinkedList<int>(currentPath);

            pathAdder(wordIndex, newPrevious);
            allPathsBetween(paths, newPrevious, root, searched[wordIndex], pathAdder);
        }
    }

    /**
     * Checks if the outer of this graph contains wordIndex.
     * This method is for single-sided levenshtein algorithms, where each layer is checked against a single target word.
     * @param wordIndex Word to check.
     * @return True if the outer of this graph contains wordIndex, false otherwise.
     */
    public bool outerContains(int wordIndex) {
        return outer.ContainsKey(wordIndex);
    }

    public static List<int> outerIntersection(LevenshteinGraph graph1, LevenshteinGraph graph2) {
        List<int> intersection = new List<int>();
        Dictionary<int, List<int>> outerCopy = graph1.outer;
        Dictionary<int, List<int>> otherOuter = graph2.outer;

        if (outerCopy.Keys.Count > otherOuter.Keys.Count) {
            Dictionary<int, List<int>> temp = otherOuter;
            otherOuter = outerCopy;
            outerCopy = temp;
        }

        foreach (int wordIndex in outerCopy.Keys) {
            if (otherOuter.ContainsKey(wordIndex)) {
                intersection.Add(wordIndex);
            }
        }

        return intersection;
    }

    /** @return The size of outer */
    public int outerSize() {
        return outer.Count;
    }

    /** @return The size of searched */
    public int searchedSize() {
        return searched.Count;
    }

    public String toString() {
        return "Outer: \n" + outer + "\nSearched: \n" + searched;
    }
}
