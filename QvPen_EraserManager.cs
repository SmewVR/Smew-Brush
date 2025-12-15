using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

namespace QvPen.UdonScript
{
    [DefaultExecutionOrder(20)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class QvPen_EraserManager : UdonSharpBehaviour
    {
        [SerializeField]
        private QvPen_Eraser eraser;

        // Layer 9 : Player
        public int inkColliderLayer = 9;

        private void Start() => eraser._Init(this);

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Networking.LocalPlayer.IsOwner(eraser.gameObject))
                return;

            if (eraser.isHeld)
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(StartUsing));
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(eraser.gameObject))
                return;

           // if (!eraser.isHeld)
                //SendCustomNetworkEvent(NetworkEventTarget.All, nameof(EndUsing));
        }

        public void StartUsing()
        {
            var owner = Networking.GetOwner(eraser.gameObject);
        }

        public void EndUsing()
        {

        }

        public void ResetEraser() => eraser._Respawn();

        public void Respawn() => eraser._Respawn();

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
        private Vector3[] syncedData {
            get => _syncedData;
            set {
                _syncedData = value;

                RequestSendPackage();

                eraser._UnpackData(_syncedData);
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

        public override void OnPostSerialization(SerializationResult result) => isInUseSyncBuffer = false;

        public void _ClearSyncBuffer()
        {
            syncedData = new Vector3[] { };
            isInUseSyncBuffer = false;
        }

        #endregion
    }
}
