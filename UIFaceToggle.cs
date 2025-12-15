
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UIFaceToggle : UdonSharpBehaviour
{
    public bool isFacingPlayer = false;
    private VRCPlayerApi player;

    // roation and transform of the brush UI when it is not facing the player
    private Quaternion originalRotation;
    public Transform brushUITransform;

    // roation and position of the camera when the brush UI is facing the player
    private Vector3 cameraPosition;
    private Quaternion cameraRotation;
    private Vector3 dir;
    private Vector3 camPos;

    void Start()
    {
        player = Networking.LocalPlayer;
        originalRotation = brushUITransform.localRotation;
    }
    public override void Interact()
    {
        if (isFacingPlayer)
        {
            isFacingPlayer = false;
        }
        else
        {
            isFacingPlayer = true;
        }
    }
    void Update()
    {
        if (player == null) return;
        // cache the camera position from player's head bone 
        camPos = player.GetBonePosition(HumanBodyBones.Head);

        if (brushUITransform == null) return;

        if (!isFacingPlayer)
        {
            // set the brush UI to face the player
            brushUITransform.localRotation = originalRotation;
            return;
        }

        // rotate to face but dont change positions
        // reset the brush UI to its original rotation

        dir = camPos - brushUITransform.position;
        brushUITransform.rotation = Quaternion.LookRotation(camPos - brushUITransform.position) * Quaternion.Euler(0, 180, 0);

    }

}
