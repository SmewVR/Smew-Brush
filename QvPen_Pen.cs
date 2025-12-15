using UdonSharp;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

#pragma warning disable IDE0090, IDE1006
#pragma warning disable IDE0066, IDE0074

namespace QvPen.UdonScript
{
    [DefaultExecutionOrder(10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class QvPen_Pen : UdonSharpBehaviour
    {
        private const string _version = "v3.1.2";

        [Header("Pen")]
        [SerializeField]
        private TrailRenderer trailRenderer;

        public string pickuptext;

        [SerializeField]
        private TrailRenderer previewTrailRenderer;

        [SerializeField]
        private LineRenderer inkPrefab;

        [SerializeField]
        private Transform inkPosition;
        [SerializeField]
        private Transform inkPoolRoot;
        [System.NonSerialized]
        public Transform inkPool;
        [System.NonSerialized]
        public Transform inkPoolSynced;// { get; private set; }
        private Transform inkPoolNotSynced;

        public AudioSource SoundFX; //SMEW

        private QvPen_SequentialSync synchronizer;

        [Header("Pointer")]
        [SerializeField]
        private Transform pointer;
        [FieldChangeCallback(nameof(pointerRadius))]
        private float _pointerRadius = 0f;
        private float pointerRadius
        {
            get
            {
                if (_pointerRadius > 0f)
                    return _pointerRadius;
                else
                {
                    var sphereCollider = pointer.GetComponent<SphereCollider>();
                    sphereCollider.enabled = false;
                    var s = pointer.lossyScale;
                    return _pointerRadius = Mathf.Min(s.x, s.y, s.z) * sphereCollider.radius;
                }
            }
        }
        [SerializeField, FieldChangeCallback(nameof(pointerRadiusMultiplierForDesktop))]
        private float _pointerRadiusMultiplierForDesktop = 5f;
        private float pointerRadiusMultiplierForDesktop => isUserInVR ? 1f : Mathf.Abs(_pointerRadiusMultiplierForDesktop);
        [SerializeField]
        private Material pointerMaterialNormal;
        [SerializeField]
        private Material pointerMaterialActive;
        [SerializeField]
        private bool canBeErasedWithOtherPointers = true;

        private bool enabledSync = true;

        [FieldChangeCallback(nameof(inkPrefabCollider))]
        private MeshCollider _inkPrefabCollider;
        private MeshCollider inkPrefabCollider
            => _inkPrefabCollider ? _inkPrefabCollider : (_inkPrefabCollider = inkPrefab.GetComponentInChildren<MeshCollider>(true));
        private GameObject lineInstance;

        private bool isUser;

        // Components
        [FieldChangeCallback(nameof(pickup))]
        private VRC_Pickup _pickup;
        private VRC_Pickup pickup => _pickup ? _pickup : (_pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup)));

        [FieldChangeCallback(nameof(objectSync))]
        private VRCObjectSync _objectSync;
        private VRCObjectSync objectSync
            => _objectSync ? _objectSync : (_objectSync = (VRCObjectSync)GetComponent(typeof(VRCObjectSync)));

        // PenManager
        private QvPen_PenManager manager;

        // Ink
        private int inkMeshLayer;
        private int inkColliderLayer;
        private const float followSpeed = 30f;
        private int inkNo;

        // Pointer
        private bool isPointerEnabled;
        private Renderer pointerRenderer;

        // Double click
        private bool useDoubleClick = true;
        private const float clickTimeInterval = 0.2184f;
        private float prevClickTime;
        private readonly float clickPosInterval = (Vector3.one * 0.00552f).sqrMagnitude;
        private Vector3 prevClickPos;

        // State
        private const int StatePenIdle = 0;
        private const int StatePenUsing = 1;
        private const int StateEraserIdle = 2;
        private const int StateEraserUsing = 3;
        private int currentState = StatePenIdle;
        private string nameofCurrentState
        {
            get
            {
                switch (currentState)
                {
                    case StatePenIdle: return nameof(StatePenIdle);
                    case StatePenUsing: return nameof(StatePenUsing);
                    case StateEraserIdle: return nameof(StateEraserIdle);
                    case StateEraserUsing: return nameof(StateEraserUsing);
                    default: return string.Empty;
                }
            }
        }

        // Sync state
        [System.NonSerialized]
        public int currentSyncState = SYNC_STATE_Idle;
        public const int SYNC_STATE_Idle = 0;
        public const int SYNC_STATE_Started = 1;
        public const int SYNC_STATE_Finished = 2;

        // Ink pool
        private const string inkPoolRootName = "QvPen_InkPool";
        private const string inkPoolName = "InkPool";
        private int penId;
        public Vector3 penIdVector { get; private set; }
        private string penIdString;

        private const string inkPrefix = "Ink";
        private float inkWidth;

        [FieldChangeCallback(nameof(localPlayer))]
        private VRCPlayerApi _localPlayer;
        private VRCPlayerApi localPlayer => _localPlayer ?? (_localPlayer = Networking.LocalPlayer);

        public void _Init(QvPen_PenManager manager)
        {
            this.manager = manager;
            _UpdateInkData();

            inkPool = inkPoolRoot.Find(inkPoolName);

            var inkPoolRootGO = GameObject.Find($"/{inkPoolRootName}");
            if (inkPoolRootGO)
                inkPoolRoot = inkPoolRootGO.transform;
            else
            {
                inkPoolRootGO = inkPoolRoot.gameObject;
                inkPoolRootGO.name = inkPoolRootName;
                SetParentAndResetLocalTransform(inkPoolRootGO.transform, null);
                inkPoolRootGO.transform.SetAsFirstSibling();
#if !UNITY_EDITOR
                Log($"{nameof(QvPen)} {_version}");
#endif
            }

            SetParentAndResetLocalTransform(inkPool, inkPoolRootGO.transform);

            penId = string.IsNullOrEmpty(Networking.GetUniqueName(gameObject))
                ? 0 : Networking.GetUniqueName(gameObject).GetHashCode();
            penIdVector = new Vector3((penId >> 24) & 0x00ff, (penId >> 12) & 0x0fff, penId & 0x0fff);
            penIdString = $"0x{(int)penIdVector.x:x2}{(int)penIdVector.y:x3}{(int)penIdVector.z:x3}";
            inkPool.name = $"{inkPoolName} ({penIdString})";

            synchronizer = inkPool.GetComponent<QvPen_SequentialSync>();
            if (synchronizer)
                synchronizer.pen = this;

            inkPoolSynced = inkPool.Find("Synced");
            inkPoolNotSynced = inkPool.Find("NotSynced");

#if !UNITY_EDITOR
            Log($"I'm {penIdString}");
#endif

            pickup.InteractionText = pickuptext;
            //  "<color=#FF0000>P</color><color=#FF7F00>i</color><color=#FFFF00>c</color><color=#00FF00>k</color> <color=#0000FF>U</color><color=#4B0082>p</color>";
            //nameof(QvPen);
            pickup.UseText = "Draw";

            pointerRenderer = pointer.GetComponent<Renderer>();
            pointer.gameObject.SetActive(false);
            pointer.transform.localScale *= pointerRadiusMultiplierForDesktop;
        }

        public void _UpdateInkData()
        {
            inkWidth = manager.inkWidth;
            inkMeshLayer = manager.inkMeshLayer;
            inkColliderLayer = manager.inkColliderLayer;

            inkPrefab.gameObject.layer = inkMeshLayer;
            inkPrefabCollider.gameObject.layer = inkColliderLayer;

#if UNITY_ANDROID
            var material = manager.questInkMaterial;
            inkPrefab.widthMultiplier = inkWidth;
            trailRenderer.widthMultiplier = inkWidth;
#else
            var material = manager.pcInkMaterial;
            if (material && material.shader == manager.roundedTrailShader)
            {
                inkPrefab.widthMultiplier = 0f;
                trailRenderer.widthMultiplier = 0f;
                material.SetFloat("_Width", inkWidth);
            }
            else
            {
                inkPrefab.widthMultiplier = inkWidth;
                trailRenderer.widthMultiplier = inkWidth;
            }
#endif

            inkPrefab.material = material;
            trailRenderer.material = material;
            inkPrefab.colorGradient = manager.colorGradient;
            trailRenderer.colorGradient = manager.colorGradient;
        }

        private void LateUpdate()
        {
            if (!isHeld || isPointerEnabled)
                return;

            if (isUser)
                trailRenderer.transform.SetPositionAndRotation(
                    Vector3.Lerp(trailRenderer.transform.position, inkPosition.position, Time.deltaTime * followSpeed),
                    Quaternion.Lerp(trailRenderer.transform.rotation, inkPosition.rotation, Time.deltaTime * followSpeed));
            else
                trailRenderer.transform.SetPositionAndRotation(inkPosition.position, inkPosition.rotation);
        }

        public bool _CheckId(Vector3 idVector) => idVector == penIdVector;

        #region Data protocol

        #region Base

        // Mode
        public const int MODE_UNKNOWN = -1;
        public const int MODE_DRAW = 1;
        public const int MODE_ERASE = 2;
        public const int MODE_DRAW_PLANE = 3;
        private int GetFooterSize(int mode)
        {
            switch (mode)
            {
                case MODE_UNKNOWN: return 0;
                case MODE_DRAW: return 7;
                case MODE_ERASE: return 6;
                default: return 0;
            }
        }

        private int currentDrawMode = MODE_DRAW;

        #endregion

        private Vector3 GetData(Vector3[] data, int index)
            => data.Length >= index ? data[data.Length - index] : Vector3.negativeInfinity;

        private void SetData(Vector3[] data, int index, Vector3 element)
        {
            if (data.Length >= index)
                data[data.Length - index] = element;
        }

        private int GetMode(Vector3[] data) => data.Length >= 1 ? (int)GetData(data, 1).y : MODE_UNKNOWN;

        private int GetFooterLength(Vector3[] data) => data.Length >= 1 ? (int)GetData(data, 1).z : 0;

        #endregion

        private readonly Collider[] results = new Collider[4];
        public override void PostLateUpdate()
        {
            if (!isHeld || !isPointerEnabled || !isUser)
                return;

            var count = Physics.OverlapSphereNonAlloc(pointer.position, pointerRadius, results, 1 << inkColliderLayer, QueryTriggerInteraction.Ignore);
            for (var i = 0; i < count; i++)
            {
                var other = results[i];

                if (other && other.transform.parent && other.transform.parent.parent)
                {
                    if (canBeErasedWithOtherPointers
                      ? other.transform.parent.parent.parent && other.transform.parent.parent.parent.parent == inkPoolRoot
                      : other.transform.parent.parent.parent == inkPool
                    )
                    {
                        var lineRenderer = other.GetComponentInParent<LineRenderer>();
                        if (lineRenderer && lineRenderer.positionCount > 0)
                        {
                            var data = new Vector3[GetFooterSize(MODE_ERASE)];

                            SetData(data, 1, new Vector3(localPlayer.playerId, MODE_ERASE, GetFooterSize(MODE_ERASE)));
                            SetData(data, 2, penIdVector);
                            SetData(data, 3, Vector3.right * lineRenderer.positionCount);
                            SetData(data, 4, lineRenderer.GetPosition(0));
                            SetData(data, 5, lineRenderer.GetPosition(lineRenderer.positionCount / 2));
                            SetData(data, 6, lineRenderer.GetPosition(Mathf.Max(0, lineRenderer.positionCount - 1)));

                            _SendData(data);

                        }
                    }
                    //else if (
                    //    false
                    //)
                    //{
                    //
                    //}
                }

                results[i] = null;
            }
        }

        #region Events

        private bool isSwitchedUseText = false;

        public override void OnPickup()
        {
            isUser = true;

            manager._TakeOwnership();
            manager.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(QvPen_PenManager.StartUsing));

            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChangeStateToPenIdle));

            //pickup.UseText = (isSwitchedUseText ^= true) ? "Draw" : "Double qk UseDown : Switch modes";
        }

        public override void OnDrop()
        {
            isUser = false;

            manager.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(QvPen_PenManager.EndUsing));

            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChangeStateToPenIdle));

            manager._ClearSyncBuffer();
        }
        public TrailRenderer copyTrail;
        public override void OnPickupUseDown()
        {
            prevClickTime = Time.time;
            prevClickPos = inkPosition.position;


            switch (currentState)
            {
                case StatePenIdle:
                    {
                        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChangeStateToPenUsing));
                        //SMEW
                        if (copyTrail)
                        {
                            copyTrail.emitting = true;

                        }

                        break;
                    }
                case StateEraserIdle:
                    {
                        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChangeStateToEraseUsing));
                        break;
                    }
                default:
                    {
                        Error($"Unexpected state : {nameofCurrentState} at {nameof(OnPickupUseDown)}");
                        break;
                    }
            }
        }



        public override void OnPickupUseUp()
        {
            switch (currentState)
            {
                case StatePenUsing:
                    {
                        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChangeStateToPenIdle));
                        //SMEW
                        if (copyTrail)
                        {
                            copyTrail.emitting = false;
                        }
                        break;
                    }
                case StateEraserUsing:
                    {
                        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChangeStateToEraseIdle));
                        break;
                    }
                case StatePenIdle:
                    {
                        Log($"Change state : {nameof(StateEraserIdle)} to {nameofCurrentState}");
                        break;
                    }
                case StateEraserIdle:
                    {
                        Log($"Change state : {nameof(StatePenIdle)} to {nameofCurrentState}");
                        break;
                    }
                default:
                    {
                        Error($"Unexpected state : {nameofCurrentState} at {nameof(OnPickupUseUp)}");
                        break;
                    }
            }
        }

        public void _SetUseDoubleClick(bool value)
        {
            useDoubleClick = value;
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ChangeStateToPenIdle));
        }

        public void _SetEnabledSync(bool value)
        {
            enabledSync = value;
        }

        private GameObject justBeforeInk;
        public void DestroyJustBeforeInk()
        {
            Destroy(justBeforeInk);
            inkNo--;
        }

        private void OnEnable()
        {
            if (inkPool)
                inkPool.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if (inkPool)
                inkPool.gameObject.SetActive(false);
        }

        private void OnDestroy() => Destroy(inkPool);

        #endregion

        #region ChangeState

        public void ChangeStateToPenIdle()
        {
            switch (currentState)
            {
                case StatePenUsing:
                    {
                        FinishDrawing();
                        break;
                    }
                case StateEraserIdle:
                    {
                        ChangeToPen();
                        break;
                    }
                case StateEraserUsing:
                    {
                        DisablePointer();
                        ChangeToPen();
                        break;
                    }
            }
            currentState = StatePenIdle;
        }

        public void ChangeStateToPenUsing()
        {
            switch (currentState)
            {
                case StatePenIdle:
                    {
                        StartDrawing();
                        break;
                    }
                case StateEraserIdle:
                    {
                        ChangeToPen();
                        StartDrawing();
                        break;
                    }
                case StateEraserUsing:
                    {
                        DisablePointer();
                        ChangeToPen();
                        StartDrawing();
                        break;
                    }
            }
            currentState = StatePenUsing;
        }

        public void ChangeStateToEraseIdle()
        {
            switch (currentState)
            {
                case StatePenIdle:
                    {
                        ChangeToEraser();
                        break;
                    }
                case StatePenUsing:
                    {
                        FinishDrawing();
                        ChangeToEraser();
                        break;
                    }
                case StateEraserUsing:
                    {
                        DisablePointer();
                        break;
                    }
            }
            currentState = StateEraserIdle;
        }

        public void ChangeStateToEraseUsing()
        {
            switch (currentState)
            {
                case StatePenIdle:
                    {
                        ChangeToEraser();
                        EnablePointer();
                        break;
                    }
                case StatePenUsing:
                    {
                        FinishDrawing();
                        ChangeToEraser();
                        EnablePointer();
                        break;
                    }
                case StateEraserIdle:
                    {
                        EnablePointer();
                        break;
                    }
            }
            currentState = StateEraserUsing;
        }

        #endregion

        private bool isCheckedIsUserInVR = false;
        [FieldChangeCallback(nameof(isUserInVR))]
        private bool _isUserInVR;
        private bool isUserInVR => isCheckedIsUserInVR ? _isUserInVR
            : (isCheckedIsUserInVR = true) && (_isUserInVR = localPlayer != null && localPlayer.IsUserInVR());

        public bool isHeld => pickup.IsHeld;

        public void _Respawn()
        {
            pickup.Drop();

            if (Networking.IsOwner(gameObject))
                objectSync.Respawn();
        }

    
        public void _Clear()
        {
            /*
            foreach (Transform ink in inkPoolSynced)
                // call GetPositions() with the array to populate it with line positions
                lineRenderer = ink.GetComponent<LineRenderer>();
                positions = new Vector3[lineRenderer.positionCount];

                lineRenderer.GetPositions(positions);

                for (int i = 0; i < lineRenderer.positionCount; i++)
                {
                    formatPos = string.Format("v {0} {1} {2}", positions[i].x, positions[i].y, positions[i].z);
                    //Debug.Log("Position " + i + ": " + positions[i]);
                    Debug.Log(formatPos);
                }
                */

  
            foreach (Transform ink in inkPoolSynced)
                Destroy(ink.gameObject);

            foreach (Transform ink in inkPoolNotSynced)
                Destroy(ink.gameObject);

            inkNo = 0;
        }

        // Variables for LineRenderer _Export (smew 6/28/2024)
        
        private LineRenderer lineRenderer;
        private string formatPos;

        //for Baked Mesh export method
        private Mesh testmesh;
        private Vector3[] mesh_vertices;
        private int[] mesh_tris;
        private int v1;
        private int v2;
        private int v3;
        private int object_counter;

        private int vertexOffset;

        public string _Export()
        {
            formatPos = "";
            formatPos += string.Format("# OBJ file created in the Smew Brush! VRChat world \v {0} \v https://vrchat.com/home/world/wrld_0097c2e4-634a-445b-a43d-9585cb6df959 \v\v", System.DateTime.Now.ToString());
            object_counter = 0;
            vertexOffset = 0;

            foreach (Transform ink in inkPoolSynced)
            {

                // call GetPositions() with the array to populate it with line positions
                lineRenderer = ink.GetComponent<LineRenderer>();
                //simplify number of points (uncomment once working)
                lineRenderer.Simplify(0.001f);

                Mesh testmesh = new Mesh();

                //bake mesh to access full vertex data
                lineRenderer.BakeMesh(testmesh, true);

                mesh_vertices = testmesh.vertices;
                mesh_tris = testmesh.triangles;

                Debug.Log(mesh_vertices);
                Debug.Log(mesh_vertices.Length);

                formatPos += "o Object_" + object_counter.ToString() + "\v";
                object_counter++;

                // adjust to keep the uv map
                // Adjust vertices to keep the UV map
                // 1. Calculate the UV coordinates for each vertex based on its position.
                // 2. Create a new array to store the UV coordinates.
                // 3. Iterate through the vertices and assign UV coordinates to each vertex.
                // 4. Assign the UV coordinates array to the mesh's UV property.

                // export vertex data
                for (int i = 0; i < mesh_vertices.Length; i++)
                    formatPos += string.Format("v {0} {1} {2}", mesh_vertices[i].x, mesh_vertices[i].y, mesh_vertices[i].z) + "\v";

                Debug.Log(formatPos);

                // export face data
                for (int i = 0; i < mesh_tris.Length; i += 3)
                    // Note: OBJ format uses 1-based 
                    formatPos += string.Format("f {0}/1/1 {1}/1/1 {2}/1/1", mesh_tris[i] + 1 + vertexOffset, mesh_tris[i + 1] + 1 + vertexOffset, mesh_tris[i + 2] + 1 + vertexOffset) + "\v";

                vertexOffset += mesh_vertices.Length;
            }
    
            // Trim any trailing whitespace or newline characters
            formatPos = formatPos.Trim();
            return formatPos;
        }

        // end _Export (smew 6/28/2024)
        /*export linerenderer method (no faces)
         
          public string _Export_Test()
        {
            formatPos = "";
            foreach (Transform ink in inkPoolSynced)
            {
                // call GetPositions() with the array to populate it with line positions
                lineRenderer = ink.GetComponent<LineRenderer>();
                positions = new Vector3[lineRenderer.positionCount];
                lineRenderer.Simplify(0.005f);
                lineRenderer.GetPositions(positions);

                for (int i = 0; i < lineRenderer.positionCount; i++)
                {
                    formatPos += string.Format("v {0} {1} {2}", positions[i].x, positions[i].y, positions[i].z) + "\v";

                    //replace with print to-txt-box 
                    Debug.Log(formatPos);

                }

            }
            
            return formatPos;
        }
             */

        private string savedata;
        private Vector3[] positions;

        public string _SaveDrawing_linedata()
        {
            savedata = "";

            foreach (Transform ink in inkPoolSynced)
            {

                // call GetPositions() with the array to populate it with line positions
                lineRenderer = ink.GetComponent<LineRenderer>();
                //simplify number of points (uncomment once working)
                positions = new Vector3[lineRenderer.positionCount];
                lineRenderer.GetPositions(positions);
                
                
                for (int i = 0; i < lineRenderer.positionCount; i++)
                {
                    savedata += string.Format("{0} {1} {2} ", positions[i].x, positions[i].y, positions[i].z);
                }

            }
            return savedata;
        }

        private Mesh testmesh_save;

        private string save_vertice_data;
        private string save_face_data;

        private Vector3[] mesh_verts;
        private int[] mesh_faces;

        //Save Mesh data
        public string _SaveDrawing()
        {
            savedata = "";
            save_vertice_data = "";
            save_face_data = "";

            foreach (Transform ink in inkPoolSynced)
            {
                // call GetPositions() with the array to populate it with line positions
                lineRenderer = ink.GetComponent<LineRenderer>();
                //simplify number of points (uncomment once working)
                //lineRenderer.Simplify(0.001f);

                Mesh testmesh_save = new Mesh();
                
                //bake mesh to access full vertex data
                lineRenderer.BakeMesh(testmesh_save, true);

                mesh_verts = testmesh_save.vertices;
                mesh_tris = testmesh_save.triangles;
                
                // export vertex data
                for (int i = 0; i < mesh_verts.Length; i++)
                    save_vertice_data += string.Format("{0} {1} {2} ", mesh_verts[i].x, mesh_verts[i].y, mesh_verts[i].z) + "\v";

                save_face_data += "f\v";
                // export face data
                for (int i = 0; i < mesh_tris.Length; i += 3)
                    save_face_data += string.Format("{0} {1} {2} ", mesh_tris[i], mesh_tris[i + 1], mesh_tris[i + 2]) + "\v";
            }

            savedata = save_vertice_data + save_face_data;

            return savedata;
        }

        private string coord;
        private string[] array;
        private int index;

        public string[] ConvertStringToArray(string input, string v_or_t)
        {
            
            //udon exception occurs, why? 

            if (v_or_t == "v")
            {
                //input = input.Trim();
                // Count the number of spaces to determine the size of the array needed
                int count = 1; // Start with 1 to account for at least one value
                coord = "";

                foreach (char s in input)
                {
                    if (s == 'f')
                    {
                        break;
                    }

                    if (s == ' ')
                    {
                        count++;
                    }
                }

                // Initialize the array with the determined size
                array = new string[count];

                index = 0; // Index to track current position in the array

                // Iterate through each character in the input string
                foreach (char s in input)
                {
                    // Debug.Log(s);

                    if (s == 'f')
                    {
                        array[index] = coord;
                        Debug.Log("v " + array[index]);
                        break;
                    }

                    if (s == ' ')
                    {
                        // Debug.Log(coord);
                        array[index] = coord;
                        coord = "";
                        Debug.Log("v " + array[index]);
                        //Debug.Log(index);
                        index += 1;
                    }

                    else
                    {
                        coord += s;
                    }
                }
            }

            if (v_or_t == "t")
            {

                bool _foundF = false;

                //input = input.Trim();
                // Count the number of spaces to determine the size of the array needed
                int count = 1; // Start with 1 to account for at least one value
                coord = "";

                foreach (char s in input)
                {
                    if (s == 'f')
                    {
                        _foundF = true;
                        continue;
                    }

                    if (_foundF)
                    {
                        if (s == ' ')
                        {
                            count++;
                        }
                    }
                        
                }
          

                // Initialize the array with the determined size
                array = new string[count];

                index = 0; // Index to track current position in the array


                bool foundF = false;

                // Iterate through each character in the input string
                foreach (char s in input)
                {
                    // Debug.Log(s);

                    if (s == 'f')
                    {
                        foundF = true;
                        continue;
                    }

                    if (foundF)
                    {
                        if (s == ' ')
                        {
                            // Debug.Log(coord);
                            array[index] = coord;
                            coord = "";
                            Debug.Log("t " + array[index]);

                            //Debug.Log(array[index]);
                            //Debug.Log(index);
                            index += 1;
                        }

                        else
                        {
                            coord += s;
                            //index += 1;
                        }
                    }   
                }
                //add coord last value to last index 
                array[index] = coord;
            }

            // Output each element of the resulting array (for demonstration)
            /*foreach (string item in array)
            {
                Debug.Log("String: " + item);
            }*/
           
            return array;
        }

        private Vector3[] load_positions;
  
        private string[] coord_list;
        private int coord_index = 0;
        private float x;
        private float y;
        private float z;

        /*
        public void _LoadDrawing_version_1(string positions)
        {
            Debug.Log("positions");

            Debug.Log(positions);
            
            coord_list = ConvertStringToArray(positions);
            Debug.Log(coord_list.Length);

            load_positions = new Vector3[coord_list.Length / 3];

            Debug.Log(coord_list);
            Debug.Log(coord_list[0]);

            for (int i = 0; i < coord_list.Length/3; i++)
            {
                Debug.Log(coord_list[i]);
                Debug.Log(float.Parse(coord_list[i * 3]));
                Debug.Log(float.Parse(coord_list[i * 3 + 1]));
                Debug.Log(float.Parse(coord_list[i * 3 + 2]));
                x = float.Parse(coord_list[i * 3]);
                y = float.Parse(coord_list[i * 3 + 1]);
                z = float.Parse(coord_list[i * 3 + 2]);
                load_positions[i] = new Vector3(x, y, z);
            }

            Debug.Log(load_positions);
            Debug.Log(load_positions.Length);

            lineInstance = Object.Instantiate(inkPrefab.gameObject);
            var line = lineInstance.GetComponent<LineRenderer>();

            line.material = manager.pcInkMaterial;
            line.widthMultiplier = 0.02f;
            line.positionCount = load_positions.Length;
            line.SetPositions(load_positions);

            lineInstance.SetActive(true);

            //lineInstance.SetActive(false);

        }*/

        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        //public GameObject meshInstance;

        private Vector3[] vertice_array;
        private int[] tri_array;

        private string[] coord_list_v;

        private string[] coord_list_t;

        private GameObject meshInstance;

        private Mesh mesh; 

        public void _LoadDrawing(string positions)
        {
            positions = positions.Trim();
            // get mesh vertex Vector3[] Array 
            coord_list_v = ConvertStringToArray(positions, "v");
            vertice_array = new Vector3[coord_list_v.Length / 3];

            // get mesh triangle Vector3[] Array 
            coord_list_t = ConvertStringToArray(positions, "t");
            tri_array = new int[coord_list_t.Length];


            //populate vertice array

            Debug.Log("populate vertice array");
            Debug.Log("coord list v length ");
            Debug.Log(coord_list_v.Length);

            for (int i = 0; i < coord_list_v.Length / 3; i++)
            {

                Debug.Log(float.Parse(coord_list_v[i * 3]));
                Debug.Log(float.Parse(coord_list_v[i * 3 + 1]));
                Debug.Log(float.Parse(coord_list_v[i * 3 + 2]));

                x = float.Parse(coord_list_v[i * 3]);
                y = float.Parse(coord_list_v[i * 3 + 1]);
                z = float.Parse(coord_list_v[i * 3 + 2]);
                vertice_array[i] = new Vector3(x, y, z);
                
            }

            Debug.Log("populate triangle array");
            Debug.Log("coord list t length ");
            Debug.Log(coord_list_t.Length);

            Debug.Log("coord_list_t:");
            foreach (var item in coord_list_t)
            {
                Debug.Log(item);
            }

            //populate triangle array
            for (int i = 0; i < coord_list_t.Length; i++)
            {
                Debug.Log(int.Parse(coord_list_t[i]));   
                tri_array[i] = int.Parse(coord_list_t[i]);
            }
            Debug.Log("triarray length");
            Debug.Log(coord_list_t.Length);

            // Assign material and other properties as needed
            
            Debug.Log("creating new mesh");
            mesh = new Mesh();

            mesh.vertices = vertice_array;
            mesh.triangles = tri_array;

           //Debug.Log("assign created mesh to mesh filter");
            // Assign the created mesh to the MeshFilter component
            meshFilter.mesh = mesh;

            //meshRenderer.GetComponent<MeshFilter>().mesh = mesh;

            Debug.Log("assign material to mesh renderer");
            // Assign material to the MeshRenderer component
            meshRenderer.material = manager.pcInkMaterial;
            //GameObject meshInstance = Instantiate(GetComponent<MeshRenderer>().gameObject);

            //meshInstance.SetActive(true);
            //meshRenderer.enabled = true;
        }

        private void StartDrawing()
        {
            trailRenderer.gameObject.SetActive(true);
            previewTrailRenderer.gameObject.SetActive(false);
            //var newobj = VRCInstantiate(BrushAudio);
            if (SoundFX)
            {
                SoundFX.Stop();
                SoundFX.Play();
            }
        }

        private void FinishDrawing()
        {
            previewTrailRenderer.gameObject.SetActive(true);
            if (isUser)
            {
                var data = PackData(trailRenderer, currentDrawMode);

                _SendData(data);
            }

            trailRenderer.gameObject.SetActive(false);
            if (SoundFX)
            {
                SoundFX.Pause();
            }

            trailRenderer.Clear();

        }

        private Vector3[] PackData(TrailRenderer trailRenderer, int mode)
        {
            if (!trailRenderer)
                return null;

            var positionCount = trailRenderer.positionCount;
            var data = new Vector3[positionCount + GetFooterSize(mode)];

            trailRenderer.GetPositions(data);

            System.Array.Reverse(data, 0, positionCount);

            SetData(data, 1, new Vector3(localPlayer.playerId, mode, GetFooterSize(mode)));
            SetData(data, 2, penIdVector);
            SetData(data, 3, new Vector3(inkMeshLayer, inkColliderLayer, enabledSync ? 1f : 0f));
            SetData(data, 4, Vector3.right * positionCount);
            SetData(data, 5, data[0]);
            SetData(data, 6, data[positionCount / 2]);
            SetData(data, 7, data[Mathf.Max(0, positionCount - 1)]);

            return data;
        }

        public Vector3[] _PackData(LineRenderer lineRenderer, int mode)
        {
            if (!lineRenderer)
                return null;

            var positionCount = lineRenderer.positionCount;
            var data = new Vector3[positionCount + GetFooterSize(mode)];

            lineRenderer.GetPositions(data);

            var inkMeshLayer = (float)lineRenderer.gameObject.layer;
            var inkColliderLayer = (float)lineRenderer.GetComponentInChildren<Collider>(true).gameObject.layer;

            SetData(data, 1, new Vector3(localPlayer.playerId, mode, GetFooterSize(mode)));
            SetData(data, 2, penIdVector);
            SetData(data, 3, new Vector3(inkMeshLayer, inkColliderLayer, enabledSync ? 1f : 0f));
            SetData(data, 4, Vector3.right * positionCount);
            SetData(data, 5, data[0]);
            SetData(data, 6, data[positionCount / 2]);
            SetData(data, 7, data[Mathf.Max(0, positionCount - 1)]);

            return data;
        }

        public void _SendData(Vector3[] data) => manager._SendData(data);

        private void EnablePointer()
        {
            isPointerEnabled = true;
            pointerRenderer.sharedMaterial = pointerMaterialActive;
        }

        private void DisablePointer()
        {
            isPointerEnabled = false;
            pointerRenderer.sharedMaterial = pointerMaterialNormal;
        }

        private void ChangeToPen()
        {
            DisablePointer();
            pointer.gameObject.SetActive(false);
        }

        private void ChangeToEraser()
        {
            pointer.gameObject.SetActive(true);
        }

        public void _UnpackData(Vector3[] data)
        {
            if (data.Length == 0)
                return;

            switch (GetMode(data))
            {
                case MODE_DRAW:
                    {
                        CreateInkInstance(data);
                        break;
                    }
                case MODE_ERASE:
                    {
                        EraseInk(data);
                        break;
                    }
            }
        }

        #region Draw Line

        private void CreateInkInstance(Vector3[] data)
        {

            // print drawing mesh data 
            //Debug.Log(data.ToString());

            if (ExistsLine(data))
                return;

            lineInstance = Object.Instantiate(inkPrefab.gameObject);
            lineInstance.name = $"{inkPrefix} ({inkNo++})";

            var inkInfo = GetData(data, 3);
            lineInstance.layer = (int)inkInfo.x;
            lineInstance.GetComponentInChildren<Collider>(true).gameObject.layer = (int)inkInfo.y;
            SetParentAndResetLocalTransform(lineInstance.transform, (int)inkInfo.z == 1 ? inkPoolSynced : inkPoolNotSynced);

            var line = lineInstance.GetComponent<LineRenderer>();

            line.positionCount = data.Length - GetFooterLength(data);
            line.SetPositions(data);

            CreateInkCollider(line);

            lineInstance.SetActive(true);

            justBeforeInk = lineInstance;
        }

        private void CreateInkCollider(LineRenderer lineRenderer)
        {
            lineRenderer.material = trailRenderer.material;

            var inkCollider = lineRenderer.GetComponentInChildren<Collider>(true);

            inkCollider.name = "InkCollider";
            SetParentAndResetLocalTransform(inkCollider.transform, lineRenderer.transform);
            
            // inkCollider.transform.SetParent(lineRenderer.transform);
            // inkCollider.transform.localPosition = Vector3.zero;
            // inkCollider.transform.localRotation = Quaternion.identity;
            // inkCollider.transform.localScale = Vector3.one;

            var mesh = new Mesh();
            var tmpWidthMultiplier = lineRenderer.widthMultiplier;
            lineRenderer.widthMultiplier = inkWidth;
            lineRenderer.BakeMesh(mesh);
            lineRenderer.widthMultiplier = tmpWidthMultiplier;

            if (inkCollider.GetComponent<MeshFilter>())
            {
                inkCollider.GetComponent<MeshFilter>().mesh = mesh;
            }

            // Calculate the center of the mesh
            Vector3 center = Vector3.zero;
            foreach (var vertex in mesh.vertices)
            {
                center += vertex;
            }
            center /= mesh.vertexCount;

            // Adjust the vertices to center the mesh
            Vector3[] adjustedVertices = new Vector3[mesh.vertexCount];
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                adjustedVertices[i] = mesh.vertices[i] - center;
            }
            mesh.vertices = adjustedVertices;

            // Recalculate bounds and normals
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            // Set the local position of the collider to the center
            inkCollider.transform.localPosition = center;
            
            inkCollider.GetComponent<MeshCollider>().sharedMesh = mesh;
            inkCollider.gameObject.SetActive(true);
            
            
            //var pickupInstance = ((VRCPickup)GetComponent(typeof(VRCPickup)));
            //pickupInstance.transform.position = inkCollider.transform.position;

        }

        private bool ExistsLine(Vector3[] data)
        {
            if (data.Length < GetFooterSize(MODE_DRAW))
                return true;

            var exists = false;

            const float eraserMiniRadius = 0.18e-4f;
            var middlePosition = GetData(data, 6);
            var count = Physics.OverlapSphereNonAlloc(middlePosition, eraserMiniRadius, results, 1 << inkColliderLayer, QueryTriggerInteraction.Ignore);
            for (var i = 0; i < count; i++)
            {
                var other = results[i];

                if (other
                 && other.transform.parent
                 && other.transform.parent.parent
                 && other.transform.parent.parent.parent
                 && other.transform.parent.parent.parent.parent == inkPoolRoot
                )
                {
                    var lineRenderer = other.GetComponentInParent<LineRenderer>();
                    if (lineRenderer
                     && lineRenderer.positionCount == (int)GetData(data, 4).x
                     && lineRenderer.GetPosition(0) == GetData(data, 5)
                     && lineRenderer.GetPosition(lineRenderer.positionCount / 2) == middlePosition
                     && lineRenderer.GetPosition(Mathf.Max(0, lineRenderer.positionCount - 1)) == GetData(data, 7)
                    )
                        exists |= true;
                }

                results[i] = null;
            }

            return exists;
        }

        #endregion

        #region Erase Line

        private void EraseInk(Vector3[] data)
        {
            if (data.Length < GetFooterSize(MODE_ERASE))
                return;

            const float eraserMiniRadius = 0.18e-4f;
            var middlePosition = GetData(data, 5);
            var count = Physics.OverlapSphereNonAlloc(middlePosition, eraserMiniRadius, results, 1 << inkColliderLayer, QueryTriggerInteraction.Ignore);
            for (var i = 0; i < count; i++)
            {
                var other = results[i];

                if (other
                 && other.transform.parent
                 && other.transform.parent.parent
                 && (canBeErasedWithOtherPointers
                    ? other.transform.parent.parent.parent && other.transform.parent.parent.parent.parent == inkPoolRoot
                    : other.transform.parent.parent.parent == inkPool
                    )
                )
                {
                    var lineRenderer = other.GetComponentInParent<LineRenderer>();
                    if (lineRenderer
                     && lineRenderer.positionCount == (int)GetData(data, 3).x
                     && lineRenderer.GetPosition(0) == GetData(data, 4)
                     && lineRenderer.GetPosition(lineRenderer.positionCount / 2) == middlePosition
                     && lineRenderer.GetPosition(Mathf.Max(0, lineRenderer.positionCount - 1)) == GetData(data, 6)
                    )
                        Destroy(other.transform.parent.gameObject);
                }

                results[i] = null;
            }
        }

        #endregion

        #region Utility

        private void SetParentAndResetLocalTransform(Transform child, Transform parent)
        {
            if (child)
            {
                child.SetParent(parent);
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
                child.localScale = Vector3.one;
            }
        }

        #endregion

        #region Log

        private void Log(object o) => Debug.Log($"{logPrefix}{o}", this);
        private void Warning(object o) => Debug.LogWarning($"{logPrefix}{o}", this);
        private void Error(object o) => Debug.LogError($"{logPrefix}{o}", this);

        private readonly Color logColor = new Color(0xf2, 0x7d, 0x4a, 0xff) / 0xff;
        private string ColorBeginTag(Color c) => $"<color=\"#{ToHtmlStringRGB(c)}\">";
        private const string ColorEndTag = "</color>";

        [FieldChangeCallback(nameof(logPrefix))]
        private string _logPrefix;
        private string logPrefix
            => string.IsNullOrEmpty(_logPrefix)
                ? (_logPrefix = $"[{ColorBeginTag(logColor)}{nameof(QvPen)}.{nameof(QvPen.Udon)}.{nameof(QvPen_Pen)}{ColorEndTag}] ") : _logPrefix;

        private string ToHtmlStringRGB(Color c)
        {
            c *= 0xff;
            return $"{Mathf.RoundToInt(c.r):x2}{Mathf.RoundToInt(c.g):x2}{Mathf.RoundToInt(c.b):x2}";
        }

        #endregion
    }
}