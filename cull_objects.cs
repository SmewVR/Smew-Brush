
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class cull_objects : UdonSharpBehaviour
{

    //public GameObject prev2;
    [SerializeField] private GameObject[] targetObject;
    private void Start()
    {
        
        for (int x = 0; x < targetObject.Length; x += 1)
        {
            if (targetObject[x] != null)
            {
                targetObject[x].SetActive(!targetObject[x].activeSelf);
            }
        }
        
    }
    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == Networking.LocalPlayer)
        {
            for (int x = 0; x < targetObject.Length; x += 1)
            {
                if (targetObject[x] != null)
                {
                    targetObject[x].SetActive(true);
                }
            }
        }
    }
    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player == Networking.LocalPlayer)
        {
            for (int x = 0; x < targetObject.Length; x += 1)
            {
                if (targetObject[x] != null)
                {
                    targetObject[x].SetActive(false);
                }
            }
        }
    }

}
