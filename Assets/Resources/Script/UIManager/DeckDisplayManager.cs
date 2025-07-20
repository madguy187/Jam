using UnityEngine;
using UnityEngine.UI;

public class DeckDisplayManager : MonoBehaviour
{
    public GameObject deckBoxPrefab;
    public Transform deckPanel;
    public Sprite[] deckSprites;

    void Start()
    {
        foreach (Sprite sprite in deckSprites) {
            GameObject box = Instantiate(deckBoxPrefab, deckPanel);
            box.GetComponent<Image>().sprite = sprite;
        }
    }
}
