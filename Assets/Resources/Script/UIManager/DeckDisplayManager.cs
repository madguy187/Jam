using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckDisplayManager : MonoBehaviour
{
    public static DeckDisplayManager instance;
    public GameObject deckBoxPrefab;
    public Transform deckPanel;

    private List<UnitObject> lastUnits = new List<UnitObject>();
    private List<GameObject> deckBoxes = new List<GameObject>();

    void Awake() 
    {
        if (instance != null) {
            Destroy(instance);
        }
        instance = this;
    }

    void Update()
    {
        var currentUnits = new List<UnitObject>();
        foreach (UnitObject unit in DeckManager.instance.GetDeckByType(eDeckType.PLAYER))
        {
            if (unit != null)
                currentUnits.Add(unit);
        }

        // Only update if the deck has changed
        if (!AreUnitListsEqual(lastUnits, currentUnits))
        {
            RefreshDeckDisplay(currentUnits);
            lastUnits = new List<UnitObject>(currentUnits);
        }
    }

    bool AreUnitListsEqual(List<UnitObject> a, List<UnitObject> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }

    void RefreshDeckDisplay(List<UnitObject> units)
    {
        // Clear old boxes
        foreach (var box in deckBoxes)
        {
            Destroy(box);
        }
        deckBoxes.Clear();

        // Add boxes for current units
        foreach (UnitObject unit in units)
        {
            GameObject box = Instantiate(deckBoxPrefab, deckPanel);
            SpriteRenderer unitSr = unit.GetComponent<SpriteRenderer>();
            box.GetComponent<Image>().sprite = unitSr.sprite;
            deckBoxes.Add(box);
        }
    }
}