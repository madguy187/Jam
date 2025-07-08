using UnityEngine;
using UnityEngine.UI;

public class NodeUI : MonoBehaviour {
    public Text label;

    public void Init(NodeType type) {
        label.text = type.ToString();

        // Optional: Change color based on type
        Color color = Color.white;
        switch (type) {
            case NodeType.Fight: color = Color.red; break;
            case NodeType.Encounter: color = Color.green; break;
            case NodeType.Shop: color = Color.blue; break;
            case NodeType.Boss: color = Color.black; break;
        }

        GetComponent<Image>().color = color;
    }
}
