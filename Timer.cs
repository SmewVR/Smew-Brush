
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Dilbert
{
    /// <summary>
    /// PickupResetTimer
    /// Pickup with built in reset timer
    /// also has a extra option for edible pickups
    /// Last edit: 17-09-2022 Ver 2.0
    /// </summary>
    public class Timer : UdonSharpBehaviour
    {
        private float Time_mem = 0;
        private float BoneTimeMEM = 0;
        private bool paused = false;

        private bool InUse = false;
        private VRC_Pickup Pickup;
        private Collider PickupCollider;

        [Tooltip("Location it respawns to when timer runs out")]
        public Transform RespawnPoint;


        [Tooltip("Time untill despawned")]
        public float Countdown = 60;
        [Tooltip("Countdown is never resetted (only paused)")]
        public bool NoCountdownReset = false;


        [Tooltip("Sound to play")]
        public AudioSource SoundEffect;

        [Header("Synching")]
        [Tooltip("All players in world are affected.")]
        public bool GlobalSynched = true;

        [Header("Events")]
        public bool PauseOnPickup = true;
        public override void OnPickup() { if (PauseOnPickup) { SetTimer(false); } }
        public override void OnDrop() { if (PauseOnPickup) { SetTimer(true); } }

        public bool PauseOnPickupUseDown = false;
        public override void OnPickupUseDown()
        {
            if (PauseOnPickupUseDown)
            { SetTimer(false); }

        }
        public override void OnPickupUseUp() { if (PauseOnPickupUseDown) { SetTimer(true); } }

        public bool ResetOnInteract = false;
        public override void Interact() { if (ResetOnInteract) { SetTimer(true); } }

        public bool PauseOnCollisionEnter = false;
        void OnCollisionEnter(Collision other) { if (PauseOnCollisionEnter) { SetTimer(false); } }
        void OnCollisionExit(Collision other) { if (PauseOnCollisionEnter) { SetTimer(true); } }
        public override void OnPlayerCollisionEnter(VRCPlayerApi player) { if (PauseOnCollisionEnter && player.isLocal) { SetTimer(false); } }
        public override void OnPlayerCollisionExit(VRCPlayerApi player) { if (PauseOnCollisionEnter && player.isLocal) { SetTimer(true); } }

        public bool PauseOnTriggerEnter = false;
        void OnTriggerEnter(Collider other) { if (PauseOnTriggerEnter) { SetTimer(false); } }
        void OnTriggerExit(Collider other) { if (PauseOnTriggerEnter) { SetTimer(true); } }
        public override void OnPlayerTriggerEnter(VRCPlayerApi player) { if (PauseOnTriggerEnter && player.isLocal) { SetTimer(false); } }
        public override void OnPlayerTriggerExit(VRCPlayerApi player) { if (PauseOnTriggerEnter && player.isLocal) { SetTimer(true); } }

        public void PlaySound()
        {
            if (SoundEffect != null)
            {
                SoundEffect.Play();
            }
        }

        public void Start()
        {
            if (Networking.LocalPlayer == null)
            { GlobalSynched = false;  }

            if (!ResetOnInteract)
            {
                ((UdonBehaviour)this.gameObject.GetComponent(nameof(UdonBehaviour))).DisableInteractive = true;
            }

            if (!(Utilities.IsValid(Pickup)))
            {
                Pickup = (VRC_Pickup)this.gameObject.GetComponent(typeof(VRC_Pickup));
                if (!(Utilities.IsValid(Pickup)))
                {
                    Debug.LogError("PickupResetTimer: no VRC_Pickup found (mandatory)", this.gameObject);
                }
            }

            if (PickupCollider == null)
            {
                PickupCollider = (Collider)this.gameObject.GetComponent(nameof(Collider));
                if (PickupCollider == null)
                {
                    Debug.LogWarning("PickupResetTimer: no collider found.", this.gameObject);
                }
            }
        }


        public void LateUpdate()
        {
            if (InUse)
            {
                if (!paused && Time.time > Time_mem && this.gameObject.transform != RespawnPoint.transform)
                {
                    ResetPickup();
                }

            }
        }

        public void ResetPickup()
        {
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject) && this.gameObject.transform != RespawnPoint.transform)
            {
                if (Pickup.IsHeld)
                {
                    Pickup.Drop();
                }

                ((Rigidbody)this.gameObject.GetComponent(nameof(Rigidbody))).velocity = new Vector3(0, 0, 0);
                InUse = false;
                PickupCollider.enabled = false;
                this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y - 200, this.gameObject.transform.position.z);
                SendCustomEventDelayedSeconds("ResetLocation", 0.3f, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
            }
        }

        public void ResetLocation()
        {
            ((Rigidbody)this.gameObject.GetComponent(nameof(Rigidbody))).velocity = new Vector3(0, 0, 0);
            this.gameObject.transform.position = RespawnPoint.position;
            this.gameObject.transform.rotation = RespawnPoint.rotation;
            PickupCollider.enabled = true;
        }

        private void SetTimer(bool dropped)
        {
            if (dropped)
            {
                Reset();
            }
            else
            {
                SetOwner(Networking.LocalPlayer);

                Pause();
            }
        }

        public void Reset()
        {
            if (GlobalSynched)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Reset_Timer"); }
            else
            { SendCustomEvent("Reset_Timer"); }
        }

        public void Reset_Timer()
        {
            Time_mem = Time.time + Countdown;
            paused = false;
            InUse = true;
        }

        public void Pause()
        {
            if (GlobalSynched)
            { SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Pause_Timer"); }
            else
            { SendCustomEvent("Pause_Timer"); }
        }

        public void Pause_Timer()
        {
            InUse = true;
            paused = true;
        }

        #region setOwner
        /// <summary>
        /// Sets owner if this script owner does not match.
        /// </summary>
        /// <param name="player">Player to set as owner.</param>
        /// <returns>True if player exist (not null).</returns>
        private bool SetOwner(VRCPlayerApi player)
        {
            if (player != null)
            {
                if (!Networking.IsOwner(this.gameObject))
                {
                    Networking.SetOwner(player, this.gameObject);
                }
                return true;
            }
            return false;
        }
        #endregion


    }
}
