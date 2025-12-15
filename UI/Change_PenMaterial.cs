
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace QvPen.UdonScript.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class Change_PenMaterial : UdonSharpBehaviour
    {
        [SerializeField]
        private GameObject pensParent;
        public GameObject button;

        //material to change to
        public Material CustomInkMat;

        public GameObject color_preview;
        //
        private GameObject pen_preview;

        //for pickup ink brushes
        public GameObject pickup_ink_renderer;

        //sound fx "pop"
        public AudioSource popSound;
        public AudioSource soundFX;

        //Set Color button 
        public GameObject set_color;
        // public GameObject toggle_button_mat;
        void Start()
        {
            pen_preview = gameObject; // This script is on the pen_preview object
            Transform current = pen_preview.transform.parent;

            while (current != null)
            {
                Transform mesh = current.Find("Mesh");
                if (mesh != null)
                {
                    Debug.Log("Found 'Mesh' under: " + current.name);
                    pen_preview = mesh.gameObject;
                    return;
                }

                current = current.parent; // Go up one level
            }
            // pen_preview = this.gameObject.transform.parent.parent.parent.parent.parent.parent.Find("Mesh").gameObject;
        }

        //Set width button 
        //public GameObject set_width;

        //rect transform of button component
        /*
        private RectTransform rectTransform;
        private float initialScale_x;
        private float initialScale_y;
        private float initialScale_z;
        */

        /*
        private void Start()
        {
            // Get the RectTransform component on the button GameObject
            rectTransform = button.GetComponent<RectTransform>();

            // Save the initial scale of the RectTransform
            initialScale_x = rectTransform.localScale.x;
            initialScale_y = rectTransform.localScale.y;
            initialScale_z = rectTransform.localScale.z;

        }*/

        public override void Interact()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ChangeMat));
            if (popSound)
            {
                popSound.Play();
            }

        
        }

        /*
        public void OnHover()
        {
            // Scale the RectTransform to half of its initial scale
            rectTransform.localScale = new Vector3(initialScale_x * 2f, initialScale_y * 2f, initialScale_z / 2f);

        }*/

        /*
        public void OnHoverExit()
        {
            rectTransform.localScale = new Vector3(initialScale_x, initialScale_y, initialScale_z);
        }*/

        public void ChangeMat()
        {
            pensParent.GetComponent<QvPen_PenManager>().OverrideMat(CustomInkMat, soundFX);
            pensParent.GetComponent<QvPen_PenManager>().tr.material = CustomInkMat;
            set_color.GetComponent<MeshRenderer>().material = CustomInkMat;
            
            //if brush is a pickup ink pen
            if (pickup_ink_renderer){
                pickup_ink_renderer.GetComponent<MeshRenderer>().material = CustomInkMat;
            }

            //set_width.GetComponent<MeshRenderer>().material = CustomInkMat;
            // toggle_button_mat.GetComponent<MeshRenderer>().material = CustomInkMat;

            if (pen_preview != null && color_preview != null)
            {
                
                pen_preview.GetComponent<MeshRenderer>().materials[1].color = color_preview.GetComponent<Renderer>().material.color;
            }

        }
    }
}