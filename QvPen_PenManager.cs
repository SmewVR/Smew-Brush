using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
using System;

#pragma warning disable IDE0090, IDE1006

namespace QvPen.UdonScript
{
    [DefaultExecutionOrder(20)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class QvPen_PenManager : UdonSharpBehaviour
    {
        [SerializeField]
        private QvPen_Pen pen;

        public Gradient colorGradient = new Gradient();

        public float inkWidth;

        // Layer 0 : Default
        // Layer 9 : Player
        public int inkMeshLayer = 0;
        public int inkColliderLayer = 9;

        public Material pcInkMaterial;
        public Material questInkMaterial;
        public Material trailInkMaterial;
        public AudioSource SoundFX;
        public TrailRenderer tr;
        [SerializeField]
        private GameObject respawnButton;
        [SerializeField]
        private GameObject clearButton;

        [SerializeField]
        private Shader _roundedTrailShader;
        public Shader roundedTrailShader => _roundedTrailShader;

        private void Start() => pen._Init(this);

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(pen.gameObject))
                return;

            if (pen.isHeld)
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(StartUsing));
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(pen.gameObject))
                return;
        }

        public void StartUsing()
        {

            if (respawnButton)
                respawnButton.SetActive(false);
            if (clearButton)
                clearButton.SetActive(false);

        }

        public void EndUsing()
        {
            if (respawnButton)
                respawnButton.SetActive(true);
            if (clearButton)
                clearButton.SetActive(true);

        }

        #region API

        public void _SetWidth(float width)
        {
            inkWidth = width;
            pen._UpdateInkData();
        }

        public void _SetMeshLayer(int layer)
        {
            inkMeshLayer = layer;
            pen._UpdateInkData();
        }

        public void _SetColliderLayer(int layer)
        {
            inkColliderLayer = layer;
            pen._UpdateInkData();
        }

        public void _SetUseDoubleClick(bool value) => pen._SetUseDoubleClick(value);

        public void _SetEnabledSync(bool value) => pen._SetEnabledSync(value);

        public void ResetPen()
        {
            pen._Respawn();
            pen._Clear();
        }

        public void Respawn() => pen._Respawn();

        public void Clear()
        {
            _ClearSyncBuffer();
            pen._Clear();
        }

        public InputField export_text_field;

        public string Export()
        {
            if (export_text_field) 
                export_text_field.text = pen._Export();

            return ""; 

        }

        public InputField save_text_field;
        public InputField load_text_field;

        public void Save()
        {
            if (save_text_field)
                save_text_field.text = pen._SaveDrawing();
        }

        public void Load()
        {
            if (load_text_field)
      
                pen._LoadDrawing(load_text_field.text);
        }

        [UdonSynced]
        private Color syncedCustomMatColor;


        // Change pen trail material color
        public void ChangeMatColor(Color customMatColor)
        {
            // Set the owner of the object to the local player
            //Networking.SetOwner(Networking.LocalPlayer, gameObject);

            syncedCustomMatColor = customMatColor;

            // Output a debug message to the console to check the value of the syncedCustomMatColor variable
            Debug.Log("syncedCustomMatColor set to: " + syncedCustomMatColor);

            //pcInkMaterial.color = syncedCustomMatColor;
            //questInkMaterial.color = syncedCustomMatColor;
            //pen._UpdateInkData();

            // Send a custom network event to all clients to trigger the OnMatColorChanged method
            //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(sync_color_change));
            sync_color_change();
        }

        public void sync_color_change()
        {
            // Take ownership of this script and update its synced color variables
            // Set the owner of the object to the local player
            
            // Networking.SetOwner(Networking.LocalPlayer, gameObject);

            // Update the color on all clients using the synced color value
            pcInkMaterial.color = syncedCustomMatColor;
            questInkMaterial.color = syncedCustomMatColor;
            pen._UpdateInkData();
            // Output a debug message to the console
            Debug.Log("Mat color changed to: " + syncedCustomMatColor);
        }

        [UdonSynced]
        private float SyncedLineWidth;

        
        public void ChangeLineWidth(float customLineWidth)
        {
            //Networking.SetOwner(Networking.LocalPlayer, gameObject);
            SyncedLineWidth = customLineWidth; // set the synced width to the slider value
                                               //set the global inkWidth to the synced one
                                               //inkWidth = SyncedLineWidth;
                                               //pen._UpdateInkData();

            // Adjust pitch based on width

            // Send a custom network event to all clients to trigger the OnMatColorChanged method
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(sync_width_change));

        }

        public void sync_width_change()
        {
            //Networking.SetOwner(Networking.LocalPlayer, gameObject);
            inkWidth = SyncedLineWidth;
            pen._UpdateInkData();
        }

        //[UdonSynced]
        private AudioSource SyncedSound;

        //SMEW
        //change pen trail material

        public void OverrideMat(Material customInkMaterial, AudioSource soundFX)
        {
            //customInkMaterial = GetComponent<MeshRenderer>().material
            pcInkMaterial = customInkMaterial;
            questInkMaterial = customInkMaterial;

            //pen trail material
            tr.material = customInkMaterial;

            pen.SoundFX = soundFX;
            pen._UpdateInkData();
        }

        #endregion

        #region Network

        public bool _TakeOwnership()
        {
            if (Networking.IsOwner(gameObject))
            {
                _ClearSyncBuffer();
                return true;
            }
            else
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                return Networking.IsOwner(gameObject);
            }
        }

        private bool isInUseSyncBuffer = false;

        [UdonSynced, System.NonSerialized, FieldChangeCallback(nameof(syncedData))]
        private Vector3[] _syncedData = { };
        private Vector3[] syncedData
        {
            get => _syncedData;
            set
            {
                _syncedData = value;

                RequestSendPackage();

                pen._UnpackData(_syncedData);
            }
        }

        private void RequestSendPackage()
        {
            if (VRCPlayerApi.GetPlayerCount() > 1 && Networking.IsOwner(gameObject) && !isInUseSyncBuffer)
            {
                isInUseSyncBuffer = true;
                RequestSerialization();
            }
        }

        public void _SendData(Vector3[] data)
        {
            if (!isInUseSyncBuffer)
                syncedData = data;
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            isInUseSyncBuffer = false;

            if (!result.success)
                pen.DestroyJustBeforeInk();
        }

        public void _ClearSyncBuffer()
        {
            syncedData = new Vector3[] { };
            isInUseSyncBuffer = false;
        }

        #endregion
    }
}