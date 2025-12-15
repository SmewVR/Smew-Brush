
using UdonSharp;
using UnityEngine;

public class modeswitch_new : UdonSharpBehaviour
{
    public RectTransform uiTransform;
    private Vector3 leftVector;
    private Vector3 rightVector;
    public AudioSource sound;
    private void Start()
    {
        leftVector = new Vector3(-0.1074f, 0.1229f, -0.0528f);
        rightVector = new Vector3(0.1581f, -0.1825f, -0.0528f);
        //uiTransform.anchoredPosition3D = leftVector;
    }
    public override void Interact()
    {
        sound.Play();
        if (uiTransform != null) {

            if (uiTransform.anchoredPosition3D == leftVector)
            {
                uiTransform.anchoredPosition3D = rightVector;
            }
            else
            {
                uiTransform.anchoredPosition3D = leftVector;
            }
        }
    }
}
