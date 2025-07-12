using UnityEngine;

namespace Map 
{
    public enum NodeType 
    {
        Undefined,
        Enemy,
        MiniBoss,
        Encounter,
        Shop,
        MajorBoss
    }
}

namespace Map {
    [CreateAssetMenu]
    public class NodeBlueprint : ScriptableObject 
    {
        public Sprite sprite;
        public NodeType nodeType;
    }
}