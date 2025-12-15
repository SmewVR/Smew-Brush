
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UIPickerButton : UdonSharpBehaviour
{
    public int buttonIndex;
   
    public UIPickerManager pickerManager;

    public override void Interact()
    {
        Debug.Log(buttonIndex);
        pickerManager.SetUITextures(buttonIndex);
    }
}
