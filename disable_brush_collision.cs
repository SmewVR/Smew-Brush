
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class disable_brush_collision : UdonSharpBehaviour
{
    public GameObject[] colliders;
    public GameObject on;
    public GameObject off;
    public AudioSource soundfx;

    public override void Interact()
    {
        if (on.activeInHierarchy)
        {
            foreach (GameObject collider in colliders)
            {
                collider.SetActive(false);
            }
            off.SetActive(true);
            on.SetActive(false);

        }
        else
        {
            foreach (GameObject collider in colliders)
            {
                collider.SetActive(true);
            }
            off.SetActive(false);
            on.SetActive(true);
        }
        soundfx.Play();
        
    }
}