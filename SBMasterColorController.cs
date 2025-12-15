
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace QvPen.UdonScript.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]

    public class SBMasterColorController : UdonSharpBehaviour
    {
        [SerializeField]
        private GameObject pensParent;

        public TrailRenderer tr;

        public GameObject color_preview;
        // public GameObject toggle_preview;

        //
        private GameObject pen_preview;

        public GameObject value_preview;

        public GameObject sat_preview;
        //for list length 1
        /*
        public Color[] customThemeColors = new Color[4]{
                    Color.yellow,
                    Color.blue,
                    Color.red,
                    Color.green
               };
         */
        [UdonSynced]
        private Color customThemeColor;
        //public int customColorIndex = 0;
        [UdonSynced]
        private bool processGUIevents = true;

        [UdonSynced]
        private bool pointerUp = false;

        public Slider slider_Hue;
        public Slider slider_Saturation;
        public Slider slider_Value;

        [UdonSynced]
        private Color value_color;

        [UdonSynced]
        private Color saturation_color;
        //private Color[] _initCustomThemeColors;

        void Start()
        {
            pen_preview = this.gameObject.transform.parent.parent.parent.Find("Mesh").gameObject;
            //localPlayer = Networking.LocalPlayer;
            customThemeColor = Color.HSVToRGB(
               slider_Hue.value,
               slider_Saturation.value,
               slider_Value.value
           );

            value_color = Color.HSVToRGB(
                slider_Hue.value,
                slider_Saturation.value, //1.0f,
                1.0f
            );

            saturation_color = Color.HSVToRGB(
                slider_Hue.value,
                1.0f,
                1.0f
            );

            if (value_preview != null)
            {
                value_preview.GetComponent<Image>().color = color_preview.GetComponent<Renderer>().material.color;
            }
            
            if (sat_preview != null)
            {
                sat_preview.GetComponent<Image>().color = color_preview.GetComponent<Renderer>().material.color;
            }

            if (pen_preview != null)
            {
                pen_preview.GetComponent<MeshRenderer>().materials[1].color = color_preview.GetComponent<Renderer>().material.color;
            }

            // if (toggle_preview != null)
            // {
            //     toggle_preview.GetComponent<MeshRenderer>().materials[0].color = color_preview.GetComponent<Renderer>().material.color;
            // }
        }
 
        public void OnGUIChange()
        {

            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

            RequestSerialization();
            Debug.Log("OnGUIChange Updating Pen Color...");
            UpdateGUI();
            /*
               if (!processGUIevents)
               {
                   return;
               }*/
            /*
              customThemeColors[customColorIndex] = Color.HSVToRGB(
                slider_Hue.value,
                slider_Saturation.value,
                slider_Value.value
            );
            */
        }
        public override void OnDeserialization()
        {
            Debug.Log("OnDeserialization Updating Pen Color...");
            UpdateGUI();
        }

        public void PointerUpTrue()
        {
            pointerUp = true;
            Debug.Log("pointerUp UdateGUI value: ");
            Debug.Log(pointerUp);
            //for global sync
            RequestSerialization();
            // //for local sync
            pensParent.GetComponent<QvPen_PenManager>().ChangeMatColor(customThemeColor);
            // set back to true for local sync
            // pointerUp = true;
            // UpdateGUI();
        }   

        public void UpdateGUI()
        {

            customThemeColor = Color.HSVToRGB(
                slider_Hue.value,
                slider_Saturation.value,
                slider_Value.value
            );

            saturation_color = Color.HSVToRGB(
                slider_Hue.value,
                1.0f,
                1.0f
            );

            value_color = Color.HSVToRGB(
                slider_Hue.value,
                slider_Saturation.value, //1.0f,
                1.0f
            );
            Debug.Log("pointerUp UdateGUI value: ");
            Debug.Log(pointerUp);
            if (pointerUp){
                pensParent.GetComponent<QvPen_PenManager>().ChangeMatColor(customThemeColor);
                pointerUp = false;
            } 

            tr.material.color = customThemeColor;

            if (color_preview != null)
            {
                color_preview.GetComponent<Renderer>().material.color = customThemeColor;
            }
            
            // if (toggle_preview != null)
            // {
            //     toggle_preview.GetComponent<MeshRenderer>().materials[0].color = color_preview.GetComponent<Renderer>().material.color;
            // }

            if (value_preview != null)
            {
                value_preview.GetComponent<Image>().color = value_color;
            }

            if (sat_preview != null)
            {
                sat_preview.GetComponent<Image>().color = saturation_color;
            }

            if (pen_preview != null)
            {
                pen_preview.GetComponent<MeshRenderer>().materials[1].color = customThemeColor;    
            }
     
            //processGUIevents = false;

            // update HSV sliders
            /*
            float H, S, V;

            Color.RGBToHSV(customThemeColor, out H, out S, out V);

            slider_Hue.value = H;
            slider_Saturation.value = S;
            slider_Value.value = V;
            */

            //pass RGB value to change QV Pen color 

            //pensParent.GetComponent<QvPen_PenManager>().ChangeMatColor(Color.HSVToRGB(H, S, V));
            //tr.material.color = Color.HSVToRGB(H, S, V);

            //processGUIevents = true;
        }
        // public void ChangePenMat(){
        //     pensParent.GetComponent<QvPen_PenManager>().ChangeMatColor(customThemeColor);
        // }
    }
}