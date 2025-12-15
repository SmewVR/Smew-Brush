
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class sb : UdonSharpBehaviour
{
    public GameObject brush_ui;
 
    public override void OnPickupUseDown()
    {
        if (brush_ui.activeInHierarchy)
        {
            brush_ui.SetActive(false);
        }
        else if (!brush_ui.activeInHierarchy)
        {
            brush_ui.SetActive(true);
        }
    }

}
