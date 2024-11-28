import java.io.*;
import java.util.*;

public class WildcardDatabase extends LevenshteinDatabase {
    protected HashMap<String, HashSet<String>> wildcardMap;

    public WildcardDatabase(String dictionaryPath) throws FileNotFoundException {
        super(dictionaryPath);
        initializeWildcardMap();

        //System.out.println(this.wildcardMap.toString());
    }

    protected WildcardDatabase(String dictionaryPath, boolean initializeWildcardMap) throws FileNotFoundException {
        super(dictionaryPath);
        if (initializeWildcardMap) {
            initializeWildcardMap();
        }
    }

    public ArrayList<String> localWildcardIdentities(String word) {
        ArrayList<String> identities = new ArrayList();

        addEachWildcard(word, identities, (wordToAdd, wildcardIdentity, 
                wildcardListObject) -> {
            if (wildcardMap.containsKey(wildcardIdentity)) {
                ((ArrayList<String>)wildcardListObject).add(wildcardIdentity);
            }
        });
        return identities;
    }

    public static ArrayList<String> allWildcardIdentities(String word) {
        ArrayList<String> identities = new ArrayList();

        addEachWildcard(word, identities, (wordToAdd, wildcardIdentity, 
                wildcardMapObject) -> {
            ((ArrayList<String>)wildcardMapObject).add(wildcardIdentity);
        });
        return identities;
    };

    private void putEachWildcard(String word) {
        addEachWildcard(word, wildcardMap, (wordToAdd, wildcardIdentity, 
                wildcardMapObject) -> {
            HashMap<String, HashSet<String>> wildcardMap = (HashMap<String, HashSet<String>>)wildcardMapObject;

            wildcardMap.putIfAbsent(wildcardIdentity, new HashSet<>());
            wildcardMap.get(wildcardIdentity).add(wordToAdd.toString());
        });
    }

    private static void addEachWildcard(String word, Object dataStructure, wildcardDataStructureAdder wildcardDataStructureAdder) {
        StringBuilder cardBuilder = new StringBuilder(word);
        int wordLength = word.length();

        cardBuilder.setCharAt(0, '*');
        wildcardDataStructureAdder.addIdentityToStructure(word, cardBuilder.toString(), dataStructure);
        for (int i = 1; i < wordLength; i++) {
            cardBuilder.setCharAt(i - 1, word.charAt(i - 1));
            cardBuilder.setCharAt(i, '*');
            wildcardDataStructureAdder.addIdentityToStructure(word, cardBuilder.toString(), dataStructure);
        }

        cardBuilder.append('*');
        cardBuilder.setCharAt(wordLength - 1, word.charAt(wordLength - 1));
        wildcardDataStructureAdder.addIdentityToStructure(word, cardBuilder.toString(), dataStructure);
        for (int i = wordLength; i > 0; i--) {
            cardBuilder.setCharAt(i, word.charAt(i - 1));
            cardBuilder.setCharAt(i - 1, '*');
            wildcardDataStructureAdder.addIdentityToStructure(word, cardBuilder.toString(), dataStructure);
        }
    }

    public boolean areNeighbors(String word1, String word2) {
        HashSet<String> word1Neighbors = this.findNeighbors(word1, new HashSet());
        return word1Neighbors.contains(word2);
    }

    public HashSet<String> findNeighbors(String word, HashSet<String> blacklist) {
        HashSet<String> returnSet = new HashSet();

        for (String wildcard : this.localWildcardIdentities(word)) {
            returnSet.addAll(wildcardMap.get(wildcard));
            returnSet.removeAll(blacklist);
        }
        return returnSet;
    };

    private void initializeWildcardMap() {
        wildcardMap = new HashMap();

        for (int i = 0; i < this.dictionary.length; i++) {
            this.putEachWildcard(this.dictionary[i]);
        }
        
        Iterator<Map.Entry<String, HashSet<String>>> wildcardIterator = wildcardMap.entrySet().iterator();

        while (wildcardIterator.hasNext()) {
            Map.Entry<String, HashSet<String>> entry = wildcardIterator.next();

            if (entry.getValue().size() == 1) {
                wildcardIterator.remove();
            }
        }
    }

    public String wildcardMapToString() {
        TreeSet<String> mapKeys = new TreeSet(WildcardDatabase.COMPARE_BY_LENGTH);
        mapKeys.addAll(wildcardMap.keySet());
        StringBuilder mapBuilder = new StringBuilder();

        for (String key : mapKeys) {
            StringBuilder entryBuilder = new StringBuilder();
            TreeSet<String> mapValue = new TreeSet(WildcardDatabase.COMPARE_BY_LENGTH);

            mapValue.addAll(wildcardMap.get(key));
            entryBuilder.append(key + " ->");

            for (String word : mapValue) {
                entryBuilder.append(" " + word);
            }

            mapBuilder.append(entryBuilder + "\n");
        }

        return mapBuilder.toString();
    }

    public static final Comparator<String> COMPARE_BY_LENGTH = (o1, o2) -> {
        int delta = o1.length() - o2.length();
        if (delta == 0) {
            return o1.compareTo(o2);
        } else {
            return delta;
        }
    };
}

@FunctionalInterface
interface wildcardDataStructureAdder {
    void addIdentityToStructure(String word, String wildcardIdentity, Object dataStructure);
}
