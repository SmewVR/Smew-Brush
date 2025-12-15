
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class toggleobjectenter : UdonSharpBehaviour
{

    public GameObject[] targets;

    private void OnCollisionEnter(Collision collision)
    {
        foreach (GameObject obj in targets)
        {
            obj.SetActive(!obj.activeSelf);
        }
    }


    public void OnCollisionExit(Collision collision)
    {

        foreach (GameObject obj in targets)
        {
            obj.SetActive(!obj.activeSelf);
        }
       
    }

}