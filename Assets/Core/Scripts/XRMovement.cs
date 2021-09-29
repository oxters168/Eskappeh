using UnityEngine;
using UnityHelpers;
using UnityEngine.InputSystem;

public class XRMovement : MonoBehaviour
{
    public float speed = 2;
    public float turnStep = 15;
    public float deadzone = 0.1f;

    [Space(10)]
    public Transform hmdTracker;

    [Space(10)]
    public InputAction leftStickInput;
    public InputAction rightStickInput;

    private Vector2 leftStick;
    private Vector2 prevLeftStick;

    private Vector2 rightStick;
    private Vector2 prevRightStick;

    [Space(10)]
    public bool debugValues;

    void OnEnable()
    {
        leftStickInput.Enable();
        rightStickInput.Enable();
    }
    void OnDisable()
    {
        leftStickInput.Disable();
        rightStickInput.Disable();
    }

    void Update()
    {
        ReadInput();
        
        if (debugValues)
            DebugValues();

        ApplyInput();
    }

    private void ApplyInput()
    {
        bool lookRight = rightStick.x > deadzone;
        bool lookLeft = rightStick.x < -deadzone;
        bool onLookRight = lookRight && prevRightStick.x < deadzone;
        bool onLookLeft = lookLeft && prevRightStick.x > -deadzone;

        bool moveRight = leftStick.x > deadzone;
        bool moveLeft = leftStick.x < -deadzone;
        bool moveForward = leftStick.y > deadzone;
        bool moveBack = leftStick.y < -deadzone;
        bool onMoveRight = moveRight && prevLeftStick.x < deadzone;
        bool onMoveLeft = moveLeft && prevLeftStick.x > -deadzone;
        bool onMoveForward = moveForward && prevLeftStick.y < deadzone;
        bool onMoveBack = moveBack && prevLeftStick.y > -deadzone;

        if (onLookRight)
            transform.eulerAngles += Vector3.up * turnStep;
        if (onLookLeft)
            transform.eulerAngles -= Vector3.up * turnStep;

        if (moveLeft || moveRight || moveForward || moveBack)
        {
            Vector3 forward = hmdTracker.forward.Planar(Vector3.up);
            Vector3 right = Quaternion.Euler(0, 90, 0) * forward; //Cheaper than cross
            float xValue = ((leftStick.x - deadzone) / (1 - deadzone));
            float yValue = ((leftStick.y - deadzone) / (1 - deadzone));

            if (moveRight || moveLeft)
                transform.position += right * speed * Time.deltaTime * xValue;
            if (moveForward || moveBack)
                transform.position += forward * speed * Time.deltaTime * yValue;
        }
    }
    private void ReadInput()
    {
        prevLeftStick = leftStick;
        prevRightStick = rightStick;

        leftStick = leftStickInput.ReadValue<Vector2>();
        rightStick = rightStickInput.ReadValue<Vector2>();
    }
    private void DebugValues()
    {
        DebugPanel.Log("RightStick", rightStick);
        DebugPanel.Log("LeftStick", leftStick);
    }
}
