using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Gyroscope = UnityEngine.InputSystem.Gyroscope;
using AttitudeSensor = UnityEngine.InputSystem.AttitudeSensor;
using AndroidGravitySensor = UnityEngine.InputSystem.Android.AndroidGravitySensor;
using GravitySensor = UnityEngine.InputSystem.GravitySensor;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MonoBehaviour inputReceiver;
    private IInputReceiver handler;

    public TMP_Text text;
    public TMP_Text text2;
    public TMP_Text text3;

    [Header("Input Actions")]
    private string tiltV2ActionName = "Tilt_v2";
    private string jumpActionName = "Jump";
    private string moveActionName = "Move";
    private string calibrateActionName = "Calibrate";

    private InputAction tiltV2Action;
    private InputAction jumpAction;
    private InputAction moveAction;
    private InputAction calibrateAction;

    [Header("Calibration")]
    public float deadZone = 0.02f;
    public float maxTilt = 0.7f;        // clamp [-1,1] gravity
    public float smoothTime = 0.1f;

    private bool calibrated;
    private Vector3 lastGravity = Vector3.zero;
    private Quaternion calibrationRotation = Quaternion.identity;

    private Vector3 smoothTilt;
    private Vector3 tiltVelocity;

    private void Awake()
    {
        CacheHandler();
        CacheActions();
    }

    private void OnEnable()
    {
        EnsureSensorsEnabled();
        EnableActionsAndHookEvents();
    }

    private void OnDisable()
    {
        UnhookEventsAndDisableActions();
    }

    private void Update()
    {
#if UNITY_EDITOR
        try
        {
            if (UnityEditor.EditorApplication.isRemoteConnected)
            {
                EnsureSensorsEnabled();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
#endif
    }

    private void CacheHandler()
    {
        if (inputReceiver is IInputReceiver receiver)
        {
            handler = receiver;
            return;
        }

        handler = null;
        Debug.LogError("Assigned object does not implement IInputReceiver!");
    }

    private void CacheActions()
    {
        tiltV2Action = InputSystem.actions.FindAction(tiltV2ActionName);
        jumpAction = InputSystem.actions.FindAction(jumpActionName);
        moveAction = InputSystem.actions.FindAction(moveActionName);
        calibrateAction = InputSystem.actions.FindAction(calibrateActionName);

        Debug.Log($"{tiltV2ActionName} action found: {tiltV2Action != null}");
        Debug.Log($"{jumpActionName} action found: {jumpAction != null}");
        Debug.Log($"{moveActionName} action found: {moveAction != null}");
        Debug.Log($"{calibrateActionName} action found: {calibrateAction != null}");
    }

    private void EnableActionsAndHookEvents()
    {
        if (tiltV2Action != null)
        {
            tiltV2Action.performed += Tilt_v2;
            tiltV2Action.Enable();
        }

        if (jumpAction != null)
        {
            jumpAction.performed += OnJump;
            jumpAction.Enable();
        }

        if (moveAction != null)
        {
            moveAction.performed += OnMove;
            moveAction.Enable();
        }

        if (calibrateAction != null)
        {
            calibrateAction.performed += OnCalibrate;
            calibrateAction.Enable();
        }
    }

    private void UnhookEventsAndDisableActions()
    {
        if (tiltV2Action != null)
        {
            tiltV2Action.performed -= Tilt_v2;
            tiltV2Action.Disable();
        }

        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
            jumpAction.Disable();
        }

        if (moveAction != null)
        {
            moveAction.performed -= OnMove;
            moveAction.Disable();
        }

        if (calibrateAction != null)
        {
            calibrateAction.performed -= OnCalibrate;
            calibrateAction.Disable();
        }
    }

    private static void TryEnableDevice(InputDevice device)
    {
        if (device != null)
        {
            InputSystem.EnableDevice(device);
        }
    }

    private void EnsureSensorsEnabled()
    {
        TryEnableDevice(Gyroscope.current);
        TryEnableDevice(AttitudeSensor.current);
        TryEnableDevice(AndroidGravitySensor.current);
        TryEnableDevice(GravitySensor.current);
        TryEnableDevice(LinearAccelerationSensor.current);
    }

    private void OnCalibrate(InputAction.CallbackContext _)
    {
        Calibrate();
    }

    private void Calibrate()
    {
        if (lastGravity == Vector3.zero)
        {
            Debug.LogError("No gravity data available for calibration.");
            return;
        }

        calibrationRotation = Quaternion.FromToRotation(lastGravity.normalized, Vector3.down);
        calibrated = true;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("Jump");
        if (text3 != null) text3.text = "Jump";
        handler?.Jump();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var move = context.ReadValue<Vector2>();
        // Debug.Log("Move");

        handler?.Move(new Vector3(move.x * 1.5f, 0f, move.y * 1.5f));
    }

    private float ApplyDeadZone(float value)
    {
        return Mathf.Abs(value) < deadZone ? 0f : value;
    }

    public void Tilt_v2(InputAction.CallbackContext context)
    {
        Vector3 gravity = context.ReadValue<Vector3>();
        lastGravity = gravity;

        if (!calibrated)
        {
            Calibrate();
            return;
        }

        // Apply calibration rotation
        Vector3 calibratedGravity = calibrationRotation * gravity;

        // Project onto ground plane
        Vector3 tilt = new Vector3(calibratedGravity.x, 0f, -calibratedGravity.z);

        tilt.x = ApplyDeadZone(tilt.x);
        tilt.z = ApplyDeadZone(tilt.z);

        // Clamp tilt strength
        tilt = Vector3.ClampMagnitude(tilt, maxTilt);

        // Smooth input
        smoothTilt = Vector3.SmoothDamp(smoothTilt, tilt, ref tiltVelocity, smoothTime);

        // Debug.Log($"Tilt_v2 input: {gravity} tilt: {tilt} smoothTilt: {smoothTilt}");

        handler?.Move(smoothTilt);
        if (text != null) text.text = "Tilt_v2" + smoothTilt;
    }
}




