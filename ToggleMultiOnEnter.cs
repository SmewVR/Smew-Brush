
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ToggleMultiOnEnter : UdonSharpBehaviour

{
    public GameObject[] targets;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == Networking.LocalPlayer)
        {
            foreach (GameObject obj in targets)
            {
                obj.SetActive(!obj.activeSelf);
            }     
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player == Networking.LocalPlayer)
        {
            foreach (GameObject obj in targets)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
    }

}