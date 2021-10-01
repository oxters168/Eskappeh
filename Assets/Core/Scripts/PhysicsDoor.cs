using UnityEngine;
using UnityHelpers;

public class PhysicsDoor : MonoBehaviour
{
    public HingeJoint doorHinge;
    public HingeJoint handle1Hinge;
    public HingeJoint handle2Hinge;

    [Space(10)]
    public bool isOpen;

    [Header("Door")]
    public bool flipDoor;
    public float maxDoorAngle = 90;

    [Header("Handle 1")]
    public bool flipHandle1;
    public float maxHandle1Angle = 30;

    [Header("Handle 2")]
    public bool flipHandle2;
    public float maxHandle2Angle = 30;

    private bool prevFlipDoor;
    private float prevMaxDoorAngle = float.MaxValue;
    private bool prevFlipHandle1;
    private float prevMaxHandle1Angle = float.MaxValue;
    private bool prevFlipHandle2;
    private float prevMaxHandle2Angle = float.MaxValue;

    void Update()
    {
        float doorAngle = FixAngle(doorHinge.connectedBody.transform.localEulerAngles.y);
        float handle1Angle = FixAngle(handle1Hinge.connectedBody.transform.localEulerAngles.x);
        float handle2Angle = FixAngle(handle2Hinge.connectedBody.transform.localEulerAngles.x);
        bool handle1Open = Mathf.Abs(handle1Angle) > (maxHandle1Angle * 0.8f);
        bool handle2Open = Mathf.Abs(handle2Angle) > (maxHandle2Angle * 0.8f);
        isOpen = handle1Open || handle2Open || Mathf.Abs(doorAngle) > 5;
        DebugPanel.Log("Door", isOpen + " " + MathHelpers.SetDecimalPlaces(doorAngle, 2) + " " + MathHelpers.SetDecimalPlaces(handle1Angle, 2) + " " + MathHelpers.SetDecimalPlaces(handle2Angle, 2) + " " + handle1Open + " " + handle2Open);

        if (isOpen)
            UpdateHinge(doorHinge, maxDoorAngle, prevMaxDoorAngle, flipDoor, prevFlipDoor);
        else
            UpdateHinge(doorHinge, 0, prevMaxDoorAngle, flipDoor, prevFlipDoor);

        UpdateHinge(handle1Hinge, maxHandle1Angle, prevMaxHandle1Angle, flipHandle1, prevFlipHandle1);
        UpdateHinge(handle2Hinge, maxHandle2Angle, prevMaxHandle2Angle, flipHandle2, prevFlipHandle2);
    }

    private static float FixAngle(float angle)
    {
        float adjusted = angle;
        if (Mathf.Abs(adjusted) > 90)
            adjusted = 90 - (adjusted % 90);
        return adjusted;
    }
    private static void UpdateHinge(HingeJoint hingeJoint, float angle, float prevAngle, bool flip, bool prevFlip)
    {
        if (flip != prevFlip || !Mathf.Approximately(angle, prevAngle))
        {
            hingeJoint.useLimits = true;
            JointLimits limits = new JointLimits();
            if (flip)
            {
                limits.min = -angle;
                limits.max = 0;
            }
            else
            {
                limits.min = 0;
                limits.max = angle;
            }
            hingeJoint.limits = limits;
        }
    }
}
