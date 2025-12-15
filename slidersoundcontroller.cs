
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class slidersoundcontroller : UdonSharpBehaviour
{
    public AudioSource btndwn;
    public AudioSource btnup;

    private void buttondown()
    {
        btndwn.Play();
    }

    private void buttonup()
    {
        btnup.Play();
    }
}
