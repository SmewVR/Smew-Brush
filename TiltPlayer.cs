
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class TiltPlayer : UdonSharpBehaviour
{
    public Animator anim;
    public Slider slider;

    void Start()
    {
        anim = GetComponent<Animator>();
        anim.speed = 0;
    }

    void Update()
    {
        anim.Play("tiltplayer", -1, slider.normalizedValue);
    }
}
