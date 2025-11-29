using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ScoreboardUI : MonoBehaviour
{
    public GameObject entryTemplate;

    private Dictionary<string, TMP_Text> entries = new Dictionary<string, TMP_Text>();

    public void UpdateScores(PlayerState[] players)
    {
        if (players == null) return;

        // Build lookup for active players
        HashSet<string> activeIds = new HashSet<string>();
        foreach (var p in players)
            activeIds.Add(p.id);

        // 1️⃣ REMOVE entries of disconnected players
        List<string> localKeys = new List<string>(entries.Keys);
        foreach (string id in localKeys)
        {
            if (!activeIds.Contains(id))
            {
                Destroy(entries[id].gameObject);
                entries.Remove(id);
            }
        }

        // 2️⃣ Sort players by score (descending)
        List<PlayerState> sorted = new List<PlayerState>(players);
        sorted.Sort((a, b) => b.score.CompareTo(a.score));

        // 3️⃣ Update or create rows
        foreach (var p in sorted)
        {
            // Create entry if missing
            if (!entries.ContainsKey(p.id))
            {
                GameObject entry = Instantiate(entryTemplate, entryTemplate.transform.parent);
                entry.SetActive(true);

                TMP_Text text = entry.GetComponent<TMP_Text>();
                entries[p.id] = text;
            }

            // Update text
            entries[p.id].text = $"{p.name} - {p.score}";
        }

        // 4️⃣ Reorder UI elements according to sorted list
        for (int i = 0; i < sorted.Count; i++)
        {
            var playerId = sorted[i].id;
            entries[playerId].transform.SetSiblingIndex(i + 1);
            // +1 avoids overlapping the template
        }
    }
}
