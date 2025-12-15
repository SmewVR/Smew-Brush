
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
public class UIPickerManager : UdonSharpBehaviour
{
    public Image[] uiTextures; // Array of UI textures to apply
    public GameObject uiParent; // Parent GameObject containing UI elements
    public void SetUITextures(int index)
    {
        if (index < 0 || index >= uiTextures.Length)
        {
            Debug.LogError("Index out of bounds for UI textures array.");
            return;
        }

        // Loop through all first-level children of the parent
        for (int i = 0; i < uiParent.transform.childCount; i++)
        {
            // Get the child game object
            GameObject child = uiParent.transform.GetChild(i).gameObject;
            // Get the Image component of the child
            Image childImage = child.GetComponent<Image>();
            if (childImage != null)
            {
                // Set the sprite and color of the child image to match the selected UI texture
                childImage.sprite = uiTextures[index].sprite;
                childImage.color = uiTextures[index].color;
            }
        }
    }

}
