
// using UdonSharp;
// using UnityEngine;
// using VRC.SDKBase;
// using VRC.Udon;

// public class text_face_player : UdonSharpBehaviour
// {
//     private VRCPlayerApi player;
//     public Transform[] textMeshTransforms;
//     public UIFaceToggle[] bools;

//     private Quaternion originalRotation;
//     void Start()
//     {
//         player = Networking.LocalPlayer;
//         originalRotation = textMeshTransforms[0].rotation;
//     }

//     private Vector3 cameraPosition;
//     private Quaternion cameraRotation;

//     void Update()
//     {
//         if (player != null)
//         {
//             // Update the player's camera position and rotation
//             cameraPosition = player.GetBonePosition(HumanBodyBones.Head);
//             cameraRotation = player.GetBoneRotation(HumanBodyBones.Head);

//             // Rotate each game object in the list to face the camera
//             for (int i = 0; i < textMeshTransforms.Length; i++)
//             {
//                 if(bools[i].isFacingPlayer){

//                     if (textMeshTransforms[i] != null && bools[i] != null)
//                     {
//                         textMeshTransforms[i].LookAt(cameraPosition);
//                         textMeshTransforms[i].rotation *= Quaternion.Euler(0, 180, 0); // Flip the object 180 degrees to face the camera
//                     }
//                 }
//                 else{
//                     textMeshTransforms[i].rotation = originalRotation;   
                    
//                 }
//             }
//         }

//     }
// }

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class text_face_player : UdonSharpBehaviour
{
    public bool istitles; 
    private VRCPlayerApi player;
    public Transform[] textMeshTransforms;
    public UIFaceToggle[] bools;
    
    private Quaternion[] originalRotations; // Store original local rotations

    void Start()
    {

        player = Networking.LocalPlayer;

        // Store the local rotations of each text mesh at the start
        originalRotations = new Quaternion[textMeshTransforms.Length];
        for (int i = 0; i < textMeshTransforms.Length; i++)
        {
            if (textMeshTransforms[i] != null)
                originalRotations[i] = textMeshTransforms[i].localRotation;
        }
    }

    private Vector3 cameraPosition;
    private Quaternion cameraRotation;

    void Update()
    {
        if (player == null) return;

        // cache the camera position from player's head bone 
        Vector3 camPos = player.GetBonePosition(HumanBodyBones.Head);

        for (int i = 0; i < textMeshTransforms.Length; i++)
        {
            var textTransform = textMeshTransforms[i];

            if (textTransform == null) continue;

            if (!istitles && bools[i] != null && !bools[i].isFacingPlayer)
            {
                // Reset to the original local rotation if not facing the player
                textTransform.localRotation = originalRotations[i];
                continue;
            }

            Vector3 dir = camPos - textTransform.position;

            // textTransform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180, 0);
            // stabalize world position of text only
            // retain world position of text transform
            textTransform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180, 0);
            // adjust the position to not be directly at the camera
        }
        
    }
    
}