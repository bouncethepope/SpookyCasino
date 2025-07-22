// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;
using PixelCrushers.DialogueSystem.UnityGUI;
using UnityEngine.Serialization;

namespace PixelCrushers.DialogueSystem
{
    [AddComponentMenu("")]
    public class Selector : MonoBehaviour, IEventSystemUser
    {
        [System.Serializable]
        public class Reticle
        {
            public Texture2D inRange;
            public Texture2D outOfRange;
            public float width = 64f;
            public float height = 64f;
        }

        public enum SelectAt { CenterOfScreen, MousePosition, CustomPosition }
        public enum DistanceFrom { Camera, GameObject, ActorTransform }
        public enum Dimension { In2D, In3D }

        private static LayerMask DefaultLayer = 1;

        [Tooltip("How to target. This is where the raycast points to.")]
        public SelectAt selectAt = SelectAt.CenterOfScreen;

        [Tooltip("Layer mask to use when targeting objects; objects on others layers are ignored.")]
        public LayerMask layerMask = DefaultLayer;

        [Tooltip("How to compute range to targeted object.")]
        public DistanceFrom distanceFrom = DistanceFrom.Camera;

        [Tooltip("Don't target objects farther than this; targets may still be unusable if beyond their usable range.")]
        public float maxSelectionDistance = 30f;

        public Dimension runRaycasts = Dimension.In3D;

        [Tooltip("Check all objects within raycast range for usables, even passing through obstacles.")]
        public bool raycastAll = false;

        [Tooltip("When Select At is set to Mouse Position, allow selection even when mouse is over a UI object.")]
        public bool selectBehindUIObjects = false;

        public bool useDefaultGUI = true;

        [Tooltip("GUI skin to use for the target's information (name and use message).")]
        public GUISkin guiSkin;

        [Tooltip("Name of the GUI style in the skin.")]
        public string guiStyleName = "label";

        public TextAnchor alignment = TextAnchor.UpperCenter;
        public TextStyle textStyle = TextStyle.Shadow;
        public Color textStyleColor = Color.black;

        [Tooltip("Color of the information labels when target is in range.")]
        public Color inRangeColor = Color.yellow;

        [Tooltip("Color of the information labels when target is out of range.")]
        public Color outOfRangeColor = Color.gray;

        public Reticle reticle;
        public KeyCode useKey = KeyCode.Space;
        public string useButton = "Fire2";

        [Tooltip("Default use message; can be overridden in the target's Usable component")]
        [SerializeField]
        [FormerlySerializedAs("defaultUseMessage")]
        private string m_defaultUseMessage = "(spacebar to interact)";
        public virtual string defaultUseMessage { get => m_defaultUseMessage; set => m_defaultUseMessage = value; }

        [Tooltip("Tick to also broadcast to the usable object's children")]
        public bool broadcastToChildren = true;

        [Tooltip("Actor transform to send with OnUse; defaults to this transform")]
        public Transform actorTransform = null;

        [Tooltip("If set, show this alert message if attempt to use something beyond its usable range")]
        public string tooFarMessage = string.Empty;

        public UsableUnityEvent onSelectedUsable = new UsableUnityEvent();
        public UsableUnityEvent onDeselectedUsable = new UsableUnityEvent();
        public UnityEvent tooFarEvent = new UnityEvent();

        [Tooltip("Tick to draw gizmos in Scene view")]
        public bool debug = false;

        [Tooltip("Show raycast line during runtime for debugging.")]
        public bool showRuntimeRaycast = false;

        public Vector3 CustomPosition { get; set; }

        public Usable CurrentUsable { get => usable; set => SetCurrentUsable(value); }
        public float CurrentDistance => distance;
        public GUIStyle GuiStyle { get { SetGuiStyle(); return guiStyle; } }

        private UnityEngine.EventSystems.EventSystem m_eventSystem = null;
        public UnityEngine.EventSystems.EventSystem eventSystem
        {
            get => m_eventSystem ?? UnityEngine.EventSystems.EventSystem.current;
            set => m_eventSystem = value;
        }

        public event SelectedUsableObjectDelegate SelectedUsableObject = null;
        public event DeselectedUsableObjectDelegate DeselectedUsableObject = null;
        public event System.Action Enabled = null;
        public event System.Action Disabled = null;

        protected GameObject selection = null;
        protected Usable usable = null;
        protected GameObject clickedDownOn = null;
        protected string heading = string.Empty;
        protected string useMessage = string.Empty;
        protected float distance = 0;
        protected GUIStyle guiStyle = null;
        protected float guiStyleLineHeight = 16f;

        protected Ray lastRay = new Ray();
        protected RaycastHit lastHit = new RaycastHit();
        protected RaycastHit[] lastHits = null;
        protected int numLastHits = 0;
        protected const int MaxHits = 100;
#if USE_PHYSICS2D || !UNITY_2018_1_OR_NEWER
        protected RaycastHit2D[] lastHits2D = null;
#endif
        protected bool hasReportedInvalidCamera = false;
        protected bool hasCheckedDefaultInputManager = false;
        protected bool isUsingDefaultInputManager = true;

        protected virtual void Reset()
        {
#if UNITY_EDITOR
            if (gameObject.GetComponent<SelectorUseStandardUIElements>() == null)
            {
                if (UnityEditor.EditorUtility.DisplayDialog("Use Unity UI for Selector?", "Add a 'Selector Use Standard UI Elements' component to allow the Selector to use Unity UI?", "Add", "Don't Add"))
                {
                    gameObject.AddComponent<SelectorUseStandardUIElements>();
                }
            }
#endif
        }

        protected virtual void OnEnable() => Enabled?.Invoke();
        protected virtual void OnDisable() => Disabled?.Invoke();

        public virtual void Start()
        {
            if (Camera.main == null)
                Debug.LogError("Dialogue System: Missing a camera tagged 'MainCamera'.", this);
        }

        protected virtual void Update()
        {
            if (!enabled || Time.timeScale <= 0 || Camera.main == null) return;

#if !USE_NEW_INPUT
            if ((selectAt == SelectAt.MousePosition) && !selectBehindUIObjects && eventSystem?.IsPointerOverGameObject() == true)
                return;
#endif

            switch (runRaycasts)
            {
                case Dimension.In2D: Run2DRaycast(); break;
                default:
                case Dimension.In3D: Run3DRaycast(); break;
            }

            if (IsUseButtonDown()) UseCurrentSelection();
        }

        public virtual void UseCurrentSelection()
        {
            if (usable != null && usable.enabled && usable.gameObject.activeInHierarchy)
            {
                clickedDownOn = null;
                if (distance <= usable.maxUseDistance)
                {
                    usable.OnUseUsable();
                    var fromTransform = actorTransform != null ? actorTransform : transform;
                    if (broadcastToChildren)
                        usable.gameObject.BroadcastMessage("OnUse", fromTransform, SendMessageOptions.DontRequireReceiver);
                    else
                        usable.gameObject.SendMessage("OnUse", fromTransform, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    if (!string.IsNullOrEmpty(tooFarMessage)) DialogueManager.ShowAlert(tooFarMessage);
                    tooFarEvent.Invoke();
                }
            }
        }

        protected virtual void Run2DRaycast()
        {
#if USE_PHYSICS2D || !UNITY_2018_1_OR_NEWER
            var mainCamera = Camera.main;
            Vector3 screenPos = mainCamera.ScreenToWorldPoint(GetSelectionPoint());

            if (showRuntimeRaycast)
                Debug.DrawRay(screenPos, Vector3.forward * maxSelectionDistance, Color.cyan);

            RaycastHit2D hit = Physics2D.Raycast(screenPos, Vector2.zero, maxSelectionDistance, layerMask);
            if (hit.collider != null)
            {
                distance = Vector3.Distance(transform.position, hit.collider.transform.position);
                Usable hitUsable = hit.collider.GetComponent<Usable>();
                if (hitUsable != null && hitUsable.enabled)
                {
                    SetCurrentUsable(hitUsable);
                }
                else
                {
                    DeselectTarget();
                }
            }
            else
            {
                DeselectTarget();
            }
#endif
        }

        protected virtual void Run3DRaycast()
        {
            Ray ray = Camera.main.ScreenPointToRay(GetSelectionPoint());
            lastRay = ray;

            if (showRuntimeRaycast)
                Debug.DrawRay(ray.origin, ray.direction * maxSelectionDistance, Color.cyan);

            if (raycastAll)
            {
                if (lastHits == null) lastHits = new RaycastHit[MaxHits];
                numLastHits = Physics.RaycastNonAlloc(ray, lastHits, maxSelectionDistance, layerMask);
                for (int i = 0; i < numLastHits; i++)
                {
                    var hit = lastHits[i];
                    distance = Vector3.Distance(transform.position, hit.collider.transform.position);
                    Usable hitUsable = hit.collider.GetComponent<Usable>();
                    if (hitUsable != null && hitUsable.enabled)
                    {
                        SetCurrentUsable(hitUsable);
                        return;
                    }
                }
                DeselectTarget();
            }
            else
            {
                if (Physics.Raycast(ray, out RaycastHit hit, maxSelectionDistance, layerMask))
                {
                    lastHit = hit;
                    distance = Vector3.Distance(transform.position, hit.collider.transform.position);
                    Usable hitUsable = hit.collider.GetComponent<Usable>();
                    if (hitUsable != null && hitUsable.enabled)
                    {
                        SetCurrentUsable(hitUsable);
                    }
                    else
                    {
                        DeselectTarget();
                    }
                }
                else
                {
                    DeselectTarget();
                }
            }
        }

        public virtual void SetCurrentUsable(Usable usable)
        {
            if (usable == this.usable) return;
            if (this.usable != null) OnDeselectedUsableObject(this.usable);
            this.usable = usable;
            if (usable != null)
            {
                usable.disabled += OnUsableDisabled;
                selection = usable.gameObject;
                heading = string.Empty;
                useMessage = string.Empty;
                OnSelectedUsableObject(usable);
            }
            else
            {
                DeselectTarget();
            }
        }

        protected void OnSelectedUsableObject(Usable usable)
        {
            SelectedUsableObject?.Invoke(usable);
            onSelectedUsable.Invoke(usable);
            usable?.OnSelectUsable();
        }

        protected void OnDeselectedUsableObject(Usable usable)
        {
            DeselectedUsableObject?.Invoke(usable);
            onDeselectedUsable.Invoke(usable);
            usable?.OnDeselectUsable();
        }

        protected virtual void DeselectTarget()
        {
            if (usable != null) usable.disabled -= OnUsableDisabled;
            OnDeselectedUsableObject(usable);
            usable = null;
            selection = null;
            heading = string.Empty;
            useMessage = string.Empty;
        }

        protected virtual void OnUsableDisabled(Usable u)
        {
            if (u == usable) DeselectTarget();
        }

        protected virtual bool IsUseButtonDown()
        {
            if (DialogueManager.IsDialogueSystemInputDisabled()) return false;

            if (!string.IsNullOrEmpty(useButton) && DialogueManager.getInputButtonDown(useButton))
                clickedDownOn = selection;

            if ((useKey != KeyCode.None) && InputDeviceManager.IsKeyDown(useKey)) return true;
            if (!string.IsNullOrEmpty(useButton))
            {
                if (DialogueManager.instance != null &&
                    DialogueManager.getInputButtonDown == DialogueManager.instance.StandardGetInputButtonDown && IsUsingDefaultInputManager())
                {
                    return InputDeviceManager.IsButtonUp(useButton) && (selection == clickedDownOn);
                }
                return DialogueManager.GetInputButtonDown(useButton);
            }

            return false;
        }

        protected virtual bool IsUsingDefaultInputManager()
        {
            if (!hasCheckedDefaultInputManager)
            {
                hasCheckedDefaultInputManager = true;
                var rewiredType = RuntimeTypeUtility.GetTypeFromName("PixelCrushers.RewiredSupport.InputDeviceManagerRewired");
                isUsingDefaultInputManager = (rewiredType == null);
            }
            return isUsingDefaultInputManager;
        }

        protected virtual Vector3 GetSelectionPoint()
        {
            return selectAt switch
            {
                SelectAt.MousePosition => InputDeviceManager.GetMousePosition(),
                SelectAt.CustomPosition => CustomPosition,
                _ => new Vector3(Screen.width / 2, Screen.height / 2)
            };
        }

        public virtual void OnGUI()
        {
            if (!enabled || !useDefaultGUI) return;
            if (guiStyle == null && (Event.current.type == EventType.Repaint || usable != null))
                SetGuiStyle();

            if (usable != null)
            {
                bool inUseRange = distance <= usable.maxUseDistance;
                guiStyle.normal.textColor = inUseRange ? inRangeColor : outOfRangeColor;
                if (string.IsNullOrEmpty(heading))
                {
                    heading = usable.GetName();
                    useMessage = DialogueManager.GetLocalizedText(string.IsNullOrEmpty(usable.overrideUseMessage) ? defaultUseMessage : usable.overrideUseMessage);
                }
                UnityGUITools.DrawText(new Rect(0, 0, Screen.width, Screen.height), heading, guiStyle, textStyle, textStyleColor);
                UnityGUITools.DrawText(new Rect(0, guiStyleLineHeight, Screen.width, Screen.height), useMessage, guiStyle, textStyle, textStyleColor);
                Texture2D tex = inUseRange ? reticle.inRange : reticle.outOfRange;
                if (tex != null)
                    GUI.Label(new Rect(0.5f * (Screen.width - reticle.width), 0.5f * (Screen.height - reticle.height), reticle.width, reticle.height), tex);
            }
        }

        protected void SetGuiStyle()
        {
            guiSkin = UnityGUITools.GetValidGUISkin(guiSkin);
            GUI.skin = guiSkin;
            guiStyle = new GUIStyle(string.IsNullOrEmpty(guiStyleName) ? GUI.skin.label : GUI.skin.FindStyle(guiStyleName) ?? GUI.skin.label);
            guiStyle.alignment = alignment;
            guiStyleLineHeight = guiStyle.CalcSize(new GUIContent("Ay")).y;
        }

        public virtual void OnDrawGizmos()
        {
            if (!debug) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(lastRay.origin, lastRay.origin + lastRay.direction * maxSelectionDistance);
            if (raycastAll && lastHits != null)
            {
                for (int i = 0; i < numLastHits; i++)
                {
                    var hit = lastHits[i];
                    bool isUsable = hit.collider.GetComponent<Usable>()?.enabled == true;
                    Gizmos.color = isUsable ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(hit.point, 0.2f);
                }
            }
            else if (lastHit.collider != null)
            {
                bool isUsable = lastHit.collider.GetComponent<Usable>()?.enabled == true;
                Gizmos.color = isUsable ? Color.green : Color.red;
                Gizmos.DrawWireSphere(lastHit.point, 0.2f);
            }
        }
    }
}
