
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LanternToggle : UdonSharpBehaviour
{
    public GameObject light;
    [SerializeField] private bool isGlobal = true;

    public override void OnPickupUseDown()
    {
        if (isGlobal)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, light.activeInHierarchy ? "set_active_false" : "set_active_true");
        }
        else
        {
            light.SetActive(!light.activeInHierarchy);
        }
    }

    public void set_active_true()
    {
        light.SetActive(true);
    }

    public void set_active_false()
    {
        light.SetActive(false);
    }
}
