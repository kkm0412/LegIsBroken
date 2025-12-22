using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    /// <summary>
    /// 이 클래스는 다양한 상호작용 상태에 있는 컨트롤러의 Interactor와
    /// 해당 Interactor가 사용하는 입력 액션을 중재하는 데 사용됩니다.
    /// </summary>
    /// <remarks>
    /// 텔레포트 레이 입력이 활성화되면, 원거리 조작에 사용되는 Ray Interactor는 비활성화되고
    /// 텔레포트에 사용되는 Ray Interactor가 활성화됩니다. Ray Interactor가 선택 중이고
    /// 부착 트랜스폼 조작을 허용하도록 구성된 경우, 모든 로코모션 입력 액션(텔레포트 레이, 이동, 회전 제어)은
    /// Ray Interactor가 사용하는 조작 입력과의 충돌을 방지하기 위해 비활성화됩니다.
    /// <br />
    /// 일반적인 계층 구조에는 Interactor 간의 중재를 위한 XR Interaction Group 컴포넌트도 포함됩니다.
    /// Interaction Group은 Direct Interactor와 Ray Interactor가 동시에 상호작용할 수 없도록 보장하며,
    /// Direct Interactor가 Ray Interactor보다 우선순위를 가집니다.
    /// </remarks>
    [AddComponentMenu("XR/컨트롤러 입력 액션 관리자 (도움말)")]
    public class ControllerInputActionManagerHelp : MonoBehaviour
    {
        [Space]
        [Header("Interactors")]

        [SerializeField]
        [Tooltip("원거리/레이 조작에 사용되는 Ray Interactor입니다. Near-Far Interactor와 함께 사용하지 마십시오.")]
        XRRayInteractor m_RayInteractor;

        [SerializeField]
        [Tooltip("원거리/레이 조작에 사용되는 Near-Far Interactor입니다. Ray Interactor와 함께 사용하지 마십시오.")]
        NearFarInteractor m_NearFarInteractor;

        [SerializeField]
        [Tooltip("텔레포트에 사용되는 Interactor입니다.")]
        XRRayInteractor m_TeleportInteractor;

        [Space]
        [Header("컨트롤러 액션 (Controller Actions)")]

        [SerializeField]
        [Tooltip("이 컨트롤러의 텔레포트 조준 모드를 시작하는 액션에 대한 참조입니다.")]
        [FormerlySerializedAs("m_TeleportModeActivate")]
        InputActionReference m_TeleportMode;

        [SerializeField]
        [Tooltip("이 컨트롤러의 텔레포트 조준 모드를 취소하는 액션에 대한 참조입니다.")]
        InputActionReference m_TeleportModeCancel;

        [SerializeField]
        [Tooltip("이 컨트롤러로 XR Origin을 연속적으로 회전시키는 액션에 대한 참조입니다.")]
        InputActionReference m_Turn;

        [SerializeField]
        [Tooltip("이 컨트롤러로 XR Origin을 스냅 회전시키는 액션에 대한 참조입니다.")]
        InputActionReference m_SnapTurn;

        [SerializeField]
        [Tooltip("이 컨트롤러로 XR Origin을 이동시키는 액션에 대한 참조입니다.")]
        InputActionReference m_Move;

        [SerializeField]
        [Tooltip("이 컨트롤러로 UI를 스크롤하는 액션에 대한 참조입니다.")]
        InputActionReference m_UIScroll;

        [Space]
        [Header("로코모션 설정 (Locomotion Settings)")]

        [SerializeField]
        [Tooltip("true이면 연속 이동이 활성화됩니다. false이면 텔레포트가 활성화됩니다.")]
        bool m_SmoothMotionEnabled;

        [SerializeField]
        [Tooltip("true이면 연속 회전이 활성화됩니다. false이면 스냅 회전이 활성화됩니다. 참고: 연속 이동이 활성화되고 연속 이동 공급자에서 스트레이프(strafe)가 활성화된 경우, 회전은 스트레이프를 위해 재정의됩니다.")]
        bool m_SmoothTurnEnabled;

        [SerializeField]
        [Tooltip("Near-Far 인터랙터 사용 시, true이면 근거리 상호작용 중에 텔레포트가 활성화됩니다. false이면 근거리 상호작용 중에 텔레포트가 비활성화됩니다.")]
        bool m_NearFarEnableTeleportDuringNearInteraction = true;

        [Space]
        [Header("UI 설정 (UI Settings)")]

        [SerializeField]
        [Tooltip("true이면 UI 스크롤이 활성화됩니다. UI를 가리킬 때 로코모션이 비활성화되어 스크롤할 수 있게 됩니다.")]
        bool m_UIScrollingEnabled = true;

        [Space]
        [Header("중재 이벤트 (Mediation Events)")]

        [SerializeField]
        [Tooltip("활성 Ray Interactor가 상호작용과 텔레포트 사이에서 변경될 때 발생하는 이벤트입니다.")]
        UnityEvent<IXRRayProvider> m_RayInteractorChanged;

        public bool smoothMotionEnabled
        {
            get => m_SmoothMotionEnabled;
            set
            {
                m_SmoothMotionEnabled = value;
                UpdateLocomotionActions();
            }
        }

        public bool smoothTurnEnabled
        {
            get => m_SmoothTurnEnabled;
            set
            {
                m_SmoothTurnEnabled = value;
                UpdateLocomotionActions();
            }
        }

        public bool uiScrollingEnabled
        {
            get => m_UIScrollingEnabled;
            set
            {
                m_UIScrollingEnabled = value;
                UpdateUIActions();
            }
        }

        bool m_StartCalled;
        bool m_PostponedDeactivateTeleport;
        bool m_PostponedNearRegionLocomotion;
        bool m_HoveringScrollableUI;

        readonly HashSet<InputAction> m_LocomotionUsers = new HashSet<InputAction>();
        readonly BindingsGroup m_BindingsGroup = new BindingsGroup();

        /// <summary>
        /// Interactor 이벤트들을 설정합니다.
        /// </summary>
        void SetupInteractorEvents()
        {
            if (m_NearFarInteractor != null)
            {
                m_NearFarInteractor.uiHoverEntered.AddListener(OnUIHoverEntered);
                m_NearFarInteractor.uiHoverExited.AddListener(OnUIHoverExited);
                m_BindingsGroup.AddBinding(m_NearFarInteractor.selectionRegion.Subscribe(OnNearFarSelectionRegionChanged));
            }

            if (m_RayInteractor != null)
            {
                m_RayInteractor.selectEntered.AddListener(OnRaySelectEntered);
                m_RayInteractor.selectExited.AddListener(OnRaySelectExited);
                m_RayInteractor.uiHoverEntered.AddListener(OnUIHoverEntered);
                m_RayInteractor.uiHoverExited.AddListener(OnUIHoverExited);
            }

            var teleportModeAction = GetInputAction(m_TeleportMode);
            if (teleportModeAction != null)
            {
                teleportModeAction.performed += OnStartTeleport;
                teleportModeAction.performed += OnStartLocomotion;
                teleportModeAction.canceled += OnCancelTeleport;
                teleportModeAction.canceled += OnStopLocomotion;
            }

            var teleportModeCancelAction = GetInputAction(m_TeleportModeCancel);
            if (teleportModeCancelAction != null)
            {
                teleportModeCancelAction.performed += OnCancelTeleport;
            }

            var moveAction = GetInputAction(m_Move);
            if (moveAction != null)
            {
                moveAction.started += OnStartLocomotion;
                moveAction.canceled += OnStopLocomotion;
            }

            var turnAction = GetInputAction(m_Turn);
            if (turnAction != null)
            {
                turnAction.started += OnStartLocomotion;
                turnAction.canceled += OnStopLocomotion;
            }

            var snapTurnAction = GetInputAction(m_SnapTurn);
            if (snapTurnAction != null)
            {
                snapTurnAction.started += OnStartLocomotion;
                snapTurnAction.canceled += OnStopLocomotion;
            }
        }

        /// <summary>
        /// Interactor 이벤트들을 해제합니다.
        /// </summary>
        void TeardownInteractorEvents()
        {
            m_BindingsGroup.Clear();

            if (m_NearFarInteractor != null)
            {
                m_NearFarInteractor.uiHoverEntered.RemoveListener(OnUIHoverEntered);
                m_NearFarInteractor.uiHoverExited.RemoveListener(OnUIHoverExited);
            }

            if (m_RayInteractor != null)
            {
                m_RayInteractor.selectEntered.RemoveListener(OnRaySelectEntered);
                m_RayInteractor.selectExited.RemoveListener(OnRaySelectExited);
                m_RayInteractor.uiHoverEntered.RemoveListener(OnUIHoverEntered);
                m_RayInteractor.uiHoverExited.RemoveListener(OnUIHoverExited);
            }

            var teleportModeAction = GetInputAction(m_TeleportMode);
            if (teleportModeAction != null)
            {
                teleportModeAction.performed -= OnStartTeleport;
                teleportModeAction.performed -= OnStartLocomotion;
                teleportModeAction.canceled -= OnCancelTeleport;
                teleportModeAction.canceled -= OnStopLocomotion;
            }

            var teleportModeCancelAction = GetInputAction(m_TeleportModeCancel);
            if (teleportModeCancelAction != null)
            {
                teleportModeCancelAction.performed -= OnCancelTeleport;
            }

            var moveAction = GetInputAction(m_Move);
            if (moveAction != null)
            {
                moveAction.started -= OnStartLocomotion;
                moveAction.canceled -= OnStopLocomotion;
            }

            var turnAction = GetInputAction(m_Turn);
            if (turnAction != null)
            {
                turnAction.started -= OnStartLocomotion;
                turnAction.canceled -= OnStopLocomotion;
            }

            var snapTurnAction = GetInputAction(m_SnapTurn);
            if (snapTurnAction != null)
            {
                snapTurnAction.started -= OnStartLocomotion;
                snapTurnAction.canceled -= OnStopLocomotion;
            }
        }

        /// <summary>
        /// 텔레포트 시작 시 호출되는 함수입니다.
        /// </summary>
        /// <param name="context">Input 액션 콜백 컨텍스트</param>
        void OnStartTeleport(InputAction.CallbackContext context)
        {
            m_PostponedDeactivateTeleport = false;

            if (m_TeleportInteractor != null)
                m_TeleportInteractor.gameObject.SetActive(true);

            if (m_RayInteractor != null)
                m_RayInteractor.gameObject.SetActive(false);

            if (m_NearFarInteractor != null && m_NearFarInteractor.selectionRegion.Value != NearFarInteractor.Region.Near)
                m_NearFarInteractor.gameObject.SetActive(false);

            m_RayInteractorChanged?.Invoke(m_TeleportInteractor);
        }

        /// <summary>
        /// 텔레포트 취소 시 호출되는 함수입니다.
        /// </summary>
        /// <param name="context">Input 액션 콜백 컨텍스트</param>
        void OnCancelTeleport(InputAction.CallbackContext context)
        {
            // 이 콜백에서 텔레포트 인터랙터를 비활성화하지 마십시오.
            // 텔레포트 인터랙터가 필요한 경우 텔레포트를 완료할 기회를 갖도록
            // 이 콜백에서 텔레포트 인터랙터를 끄는 것을 지연시킵니다.
            // OnAfterInteractionEvents가 해당 GameObject를 비활성화하는 것을 처리합니다.
            m_PostponedDeactivateTeleport = true;

            if (m_RayInteractor != null)
                m_RayInteractor.gameObject.SetActive(true);

            if (m_NearFarInteractor != null)
                m_NearFarInteractor.gameObject.SetActive(true);

            m_RayInteractorChanged?.Invoke(m_RayInteractor);
        }

        /// <summary>
        /// 로코모션 시작 시 호출되는 함수입니다.
        /// </summary>
        /// <param name="context">Input 액션 콜백 컨텍스트</param>
        void OnStartLocomotion(InputAction.CallbackContext context)
        {
            m_LocomotionUsers.Add(context.action);
        }

        /// <summary>
        /// 로코모션 정지 시 호출되는 함수입니다.
        /// </summary>
        /// <param name="context">Input 액션 콜백 컨텍스트</param>
        void OnStopLocomotion(InputAction.CallbackContext context)
        {
            m_LocomotionUsers.Remove(context.action);

             // 로코모션이 정지되고 스크롤 가능한 UI 위에 Hover 중인 경우
            // 모든 로코모션 액션을 비활성화하고 UI 액션을 업데이트합니다.
            if (m_LocomotionUsers.Count == 0 && m_HoveringScrollableUI)


            {
                DisableAllLocomotionActions();
                UpdateUIActions();
            }
        }

        void OnNearFarSelectionRegionChanged(NearFarInteractor.Region selectionRegion)
        {
            m_PostponedNearRegionLocomotion = false;

            if (selectionRegion == NearFarInteractor.Region.None)
            {
                UpdateLocomotionActions();
                return;
            }

            var manipulateAttachTransform = false;
            var attachController = m_NearFarInteractor.interactionAttachController as InteractionAttachController;
            if (attachController != null)
            {
                manipulateAttachTransform = attachController.useManipulationInput &&
                    (attachController.manipulationInput.inputSourceMode == XRInputValueReader.InputSourceMode.InputActionReference && attachController.manipulationInput.inputActionReference != null) ||
                    (attachController.manipulationInput.inputSourceMode != XRInputValueReader.InputSourceMode.InputActionReference && attachController.manipulationInput.inputSourceMode != XRInputValueReader.InputSourceMode.Unused);
            }

            if (selectionRegion == NearFarInteractor.Region.Far)
            {
                if (manipulateAttachTransform)
                    DisableAllLocomotionActions();
                else
                    DisableTeleportActions();
            }
            else if (selectionRegion == NearFarInteractor.Region.Near)
            {
                // 사용자가 썸스틱을 뒤로 당겨서 근거리 영역에 들어갔는지 확인합니다.
                // 그렇다면, 사용자가 썸스틱을 놓을 때까지 로코모션 활성화를 연기하여
                // 영역 변경 시 즉각적인 스냅 회전이 트리거되는 것을 방지합니다.
                var hasStickInput = manipulateAttachTransform && HasStickInput(attachController);
                if (hasStickInput)
                {
                    m_PostponedNearRegionLocomotion = true;
                    DisableAllLocomotionActions();
                }
                else
                {
                    UpdateLocomotionActions();
                    if (!m_NearFarEnableTeleportDuringNearInteraction)
                        DisableTeleportActions();
                }
            }
        }

        void OnRaySelectEntered(SelectEnterEventArgs args)
        {
            if (m_RayInteractor.manipulateAttachTransform)
            {
                // 로코모션 및 회전 액션 비활성화
                DisableAllLocomotionActions();
            }
        }

        void OnRaySelectExited(SelectExitEventArgs args)
        {
            if (m_RayInteractor.manipulateAttachTransform)
            {
                // 로코모션 및 회전 액션 재활성화
                UpdateLocomotionActions();
            }
        }

        void OnUIHoverEntered(UIHoverEventArgs args)
        {
            m_HoveringScrollableUI = m_UIScrollingEnabled && args.deviceModel.isScrollable;
            UpdateUIActions();

            // 로코모션이 발생 중이면 대기
            if (m_HoveringScrollableUI && m_LocomotionUsers.Count == 0)
            {
                // 로코모션 및 회전 액션 비활성화
                DisableAllLocomotionActions();
            }
        }

        void OnUIHoverExited(UIHoverEventArgs args)
        {
            m_HoveringScrollableUI = false;
            UpdateUIActions();

            // 로코모션 및 회전 액션 재활성화
            UpdateLocomotionActions();
        }

        protected void OnEnable()
        {
            if (m_RayInteractor != null && m_NearFarInteractor != null)
            {
                Debug.LogWarning("Ray Interactor와 Near-Far Interactor가 모두 할당되었습니다. 둘 중 하나만 할당해야 합니다. Ray Interactor를 지웁니다.", this);
                m_RayInteractor = null;
            }

            if (m_TeleportInteractor != null)
                m_TeleportInteractor.gameObject.SetActive(false);

            // 이 컴포넌트가 다시 활성화될 때 액션을 새로 고칠 수 있도록 허용합니다.
            // Start에서 액션을 활성화/비활성화하기 위해 기다리는 이유는 Start의 주석을 참조하십시오.
            if (m_StartCalled)
            {
                UpdateLocomotionActions();
                UpdateUIActions();
            }

            SetupInteractorEvents();
        }

        protected void OnDisable()
        {
            TeardownInteractorEvents();
        }

        protected void Start()
        {
            m_StartCalled = true;

            // 로코모션 및 회전 액션의 활성화 상태가 올바르게 설정되었는지 확인합니다.
            // InputActionManager가 OnEnable에서 모든 입력 액션을 활성화한 후에 수행되도록 Start에서 호출됩니다.
            UpdateLocomotionActions();
            UpdateUIActions();
        }

        protected void Update()
        {
            // 이 동작은 기본 실행 순서를 가지므로 XRInteractionManager 이후에 실행됩니다.
            // 따라서 선택 이벤트는 이 프레임에서 이미 완료되었습니다. 이는 텔레포트 Interactor가
            // 선택 상호작용 이벤트를 처리하고 필요한 경우 텔레포트할 기회를 가졌음을 의미합니다.
            if (m_PostponedDeactivateTeleport)
            {
                if (m_TeleportInteractor != null)
                    m_TeleportInteractor.gameObject.SetActive(false);

                m_PostponedDeactivateTeleport = false;
            }

            // 스틱 입력으로 인해 근거리 영역에 진입한 경우,
            // 스틱을 놓을 때까지 기다린 후 로코모션을 활성화합니다.
            if (m_PostponedNearRegionLocomotion)
            {
                var hasStickInput = false;
                if (m_NearFarInteractor != null &&
                    m_NearFarInteractor.interactionAttachController is InteractionAttachController attachController
                    && attachController != null)
                {
                    hasStickInput = HasStickInput(attachController);
                }

                if (!hasStickInput)
                {
                    m_PostponedNearRegionLocomotion = false;

                    UpdateLocomotionActions();
                    if (!m_NearFarEnableTeleportDuringNearInteraction)
                        DisableTeleportActions();
                }
            }
        }

        /// <summary>
        /// 로코모션 액션들의 활성화 상태를 업데이트합니다.
        /// </summary>
        void UpdateLocomotionActions()
        {
            // 이동이 활성화/비활성화될 때 텔레포트 및 회전 비활성화/활성화
            SetEnabled(m_Move, m_SmoothMotionEnabled);
            SetEnabled(m_TeleportMode, !m_SmoothMotionEnabled);
            SetEnabled(m_TeleportModeCancel, !m_SmoothMotionEnabled);

            // 연속 이동 사용 시 회전 기능 비활성화
            SetEnabled(m_Turn, !m_SmoothMotionEnabled && m_SmoothTurnEnabled);
            SetEnabled(m_SnapTurn, !m_SmoothMotionEnabled && !m_SmoothTurnEnabled);
        }

        /// <summary>
        /// 텔레포트 액션들을 비활성화합니다.
        /// </summary>
        void DisableTeleportActions()
        {
            DisableAction(m_TeleportMode);
            DisableAction(m_TeleportModeCancel);
        }

        /// <summary>
        /// 이동 및 회전 액션들을 비활성화합니다.
        /// </summary>
        void DisableMoveAndTurnActions()
        {
            DisableAction(m_Move);
            DisableAction(m_Turn);
            DisableAction(m_SnapTurn);
        }

        
        /// <summary>
        /// 모든 로코모션 액션들을 비활성화합니다.
        /// </summary>
        void DisableAllLocomotionActions()
        {
            DisableTeleportActions();
            DisableMoveAndTurnActions();
        }

        /// <summary>
        /// UI 액션들의 활성화 상태를 업데이트합니다.
        /// </summary>
        void UpdateUIActions()
        {
            SetEnabled(m_UIScroll, m_UIScrollingEnabled && m_HoveringScrollableUI && m_LocomotionUsers.Count == 0);
        }

        /// <summary>
        /// 스틱 입력을 확인합니다.
        /// </summary>
        /// <param name="attachController">InteractionAttachController 컴포넌트</param>
        /// <returns>스틱 입력이 있는지 여부</returns>
        static bool HasStickInput(InteractionAttachController attachController)
        {
            // 기본 0.5 누름 임계값의 75%
            const float sqrStickReleaseThreshold = 0.375f * 0.375f;

            // InteractionAttachController에 스틱 입력이 있고, 스틱 입력의 제곱 크기가 임계값보다 큰지 확인합니다.
            return attachController.manipulationInput.TryReadValue(out var stickInput) &&
                stickInput.sqrMagnitude > sqrStickReleaseThreshold;
        }

        /// <summary>
        /// InputActionReference를 활성화 또는 비활성화합니다.
        /// </summary>
        /// <param name="actionReference">InputActionReference</param>
        /// <param name="enabled">활성화 여부</param>
        static void SetEnabled(InputActionReference actionReference, bool enabled)
        {
            // 활성화 여부에 따라 액션을 활성화 또는 비활성화합니다.
            if (enabled)
                EnableAction(actionReference);
            else
                DisableAction(actionReference);
        }

        static void EnableAction(InputActionReference actionReference)
        {
            var action = GetInputAction(actionReference);
            action?.Enable();
        }

        static void DisableAction(InputActionReference actionReference)
        {
            var action = GetInputAction(actionReference);
            action?.Disable();
        }

        static InputAction GetInputAction(InputActionReference actionReference)
        /// <summary>
        /// InputActionReference에서 InputAction을 가져옵니다.
        /// </summary>
        {
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
            return actionReference != null ? actionReference.action : null;
#pragma warning restore IDE0031
        }
    }
}