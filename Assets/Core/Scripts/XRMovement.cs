using UnityEngine;
using UnityHelpers;
using UnityEngine.InputSystem;
using Shapes;

public class XRMovement : MonoBehaviour
{
    public Transform movableRig;
    public Transform hmdTracker;

    [Space(10)]
    public float speed = 3;
    public float turnStep = 15;
    public float deadzone = 0.1f;
    public LayerMask obstacleMask = ~0;
    public float radius = 0.3f;
    public float rayAngle = 5;
    public float maxHeight = 1.9f;
    public float minHeight = 0.1f;

    [Space(10)]
    public InputAction leftStickInput;
    public InputAction rightStickInput;

    #region Values
    private Vector2 leftStick;
    private Vector2 prevLeftStick;

    private Vector2 rightStick;
    private Vector2 prevRightStick;

    private bool lookRight;
    private bool lookLeft;
    private bool moveUp;
    private bool moveDown;
    private bool onLookRight;
    private bool onLookLeft;
    private bool onMoveUp;
    private bool onMoveDown;

    private bool moveRight;
    private bool moveLeft;
    private bool moveForward;
    private bool moveBack;
    private bool onMoveRight;
    private bool onMoveLeft;
    private bool onMoveForward;
    private bool onMoveBack;
    #endregion

    [Header("Debug")]
    public bool debugValues;
    public Disc radiusDiscDebug;
    public Line movementLineDebug;

    private Vector3 forward;
    private Vector3 right;
    private float leftStickXValue;
    private float leftStickYValue;

    private float rightDist;
    private float forwardDist;

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
        RefineInput();
        ApplyInput();
        BodyCollision();

        if (debugValues)
            DebugValues();
    }
    
    private void BodyCollision()
    {
        Vector3 shortestDir = Vector3.zero;
        float shortestDistance = float.MaxValue;
        bool wallHit = false;

        //Find the shortest raycast
        Vector3 startPoint = hmdTracker.position.Multiply(new Vector3(1, 0.5f, 1));
        int rays = Mathf.RoundToInt(360 / rayAngle);
        for (int i = 0; i < rays; i++)
        {
            Vector3 currentDir = Quaternion.Euler(0, rayAngle * i, 0) * Vector3.forward;
            RaycastHit hitInfo;
            bool rayHit = Physics.Raycast(startPoint, currentDir, out hitInfo, radius, obstacleMask);
            wallHit = wallHit || rayHit;
            if (rayHit && hitInfo.distance < shortestDistance)
            {
                shortestDir = currentDir;
                shortestDistance = hitInfo.distance;
            }
            if (debugValues)
                Debug.DrawRay(startPoint, currentDir * radius, rayHit ? Color.green : Color.red);
        }

        //Move back based on shortest raycast output
        if (wallHit)
            movableRig.position -= shortestDir * (radius - shortestDistance);
    }
    private void ApplyInput()
    {
        if (onLookRight)
            movableRig.eulerAngles += Vector3.up * turnStep;
        if (onLookLeft)
            movableRig.eulerAngles -= Vector3.up * turnStep;

        forward = hmdTracker.forward.Planar(Vector3.up);
        right = Quaternion.Euler(0, 90, 0) * forward; //Cheaper than cross
        if (moveLeft || moveRight)
            leftStickXValue = Mathf.Sign(leftStick.x) * ((Mathf.Abs(leftStick.x) - deadzone) / (1 - deadzone));
        else
            leftStickXValue = 0;
        if (moveForward || moveBack)
            leftStickYValue = Mathf.Sign(leftStick.y) * ((Mathf.Abs(leftStick.y) - deadzone) / (1 - deadzone));
        else
            leftStickYValue = 0;
        rightDist = speed * Time.deltaTime * leftStickXValue;
        forwardDist = speed * Time.deltaTime * leftStickYValue;
        if (moveRight || moveLeft)
            movableRig.position += right * rightDist;
        if (moveForward || moveBack)
            movableRig.position += forward * forwardDist;
        
        if (moveUp || moveDown)
        {
            float rightStickYValue = Mathf.Sign(rightStick.y) * ((Mathf.Abs(rightStick.y) - deadzone) / (1 - deadzone));
            float upOffset = speed * Time.deltaTime * rightStickYValue;
            float maxOffset = maxHeight - hmdTracker.position.y;
            float minOffset = minHeight - hmdTracker.position.y;
            float correctedUpOffset = Mathf.Clamp(upOffset, minOffset, maxOffset);
            if (Mathf.Sign(upOffset) != Mathf.Sign(correctedUpOffset))
                correctedUpOffset = 0;
            movableRig.position += Vector3.up * correctedUpOffset;
        }
    }
    private void ReadInput()
    {
        prevLeftStick = leftStick;
        prevRightStick = rightStick;

        leftStick = leftStickInput.ReadValue<Vector2>();
        rightStick = rightStickInput.ReadValue<Vector2>();
    }
    private void RefineInput()
    {
        lookRight = rightStick.x > deadzone;
        lookLeft = rightStick.x < -deadzone;
        moveUp = rightStick.y > deadzone;
        moveDown = rightStick.y < -deadzone;
        onLookRight = lookRight && prevRightStick.x < deadzone;
        onLookLeft = lookLeft && prevRightStick.x > -deadzone;
        onMoveUp = moveUp && prevRightStick.y < deadzone;
        onMoveDown = moveDown && prevRightStick.y > -deadzone;

        moveRight = leftStick.x > deadzone;
        moveLeft = leftStick.x < -deadzone;
        moveForward = leftStick.y > deadzone;
        moveBack = leftStick.y < -deadzone;
        onMoveRight = moveRight && prevLeftStick.x < deadzone;
        onMoveLeft = moveLeft && prevLeftStick.x > -deadzone;
        onMoveForward = moveForward && prevLeftStick.y < deadzone;
        onMoveBack = moveBack && prevLeftStick.y > -deadzone;
    }
    private void DebugValues()
    {
        DebugPanel.Log("RightStick", rightStick, 10);
        DebugPanel.Log("LeftStick", leftStick, 10);
        DebugPanel.Log("EyeHeight", hmdTracker.position.y, 10);

        radiusDiscDebug.Radius = radius;
        radiusDiscDebug.transform.position = hmdTracker.position.Multiply(new Vector3(1, 0.5f, 1));

        Vector3 end = ((right * leftStickXValue) + (forward * leftStickYValue)).normalized * radius;
        movementLineDebug.transform.forward = Vector3.forward;
        movementLineDebug.End = end;
        movementLineDebug.transform.position = hmdTracker.position.Multiply(new Vector3(1, 0.5f, 1));
    }
}
