
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BounceSound : UdonSharpBehaviour
{

    public AudioSource SoundFX;
    public Rigidbody rb;


    void OnCollisionEnter(Collision collision)
    {
        SoundFX.Play();
    }
    
    public override void OnPickup()
    {
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public override void OnDrop()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    //throw velocity boost min speed = 0.01
    //throw velocity boost scale = 0.025
}