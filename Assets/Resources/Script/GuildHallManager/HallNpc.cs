using UnityEngine;

namespace StoryManager
{
    public class HallNpc : MonoBehaviour
    {
        private void Awake()
        {
            Transform healthBar = transform.Find("UIHealthBar(Clone)");
            if (healthBar != null) healthBar.gameObject.SetActive(false);

            Transform effectGrid = transform.Find("UIEffectGrid(Clone)");
            if (effectGrid != null) effectGrid.gameObject.SetActive(false);

            Transform shieldGrid = transform.Find("Shield(Clone)");
            if (shieldGrid != null) shieldGrid.gameObject.SetActive(false);
        }
    }
}