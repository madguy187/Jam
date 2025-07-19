using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NodePopUpManager : MonoBehaviour
{
    public static NodePopUpManager _instance;
    public TextMeshProUGUI textComponent;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        // Hide the popup initially
        gameObject.SetActive(false);
    }

    void Update()
    {
        // Update position to follow the mouse
        transform.position = Input.mousePosition;
    }

    public void SetAndShowPopUp(string message)
    {
        textComponent.text = message;
        transform.position = Input.mousePosition;
        gameObject.SetActive(true);
    }

    public void HidePopUp()
    {
        gameObject.SetActive(false);
        textComponent.text = string.Empty;
    }
}
