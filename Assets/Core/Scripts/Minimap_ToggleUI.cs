using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap_ToggleUI : MonoBehaviour
{
    public GameObject uiElement;
    public KeyCode toggleKey = KeyCode.I; 

    private bool isVisible = true;

    void Update() {
        if (Input.GetKeyDown(toggleKey)) {
            ToggleVisibility();
        }
    }

    // Toggle the UI element's visibility
    public void ToggleVisibility() {
        isVisible = !isVisible;
        uiElement.SetActive(isVisible);
    }
}
