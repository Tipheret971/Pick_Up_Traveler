using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Coroutine과 List 사용을 위해 필요

public class ClawController : MonoBehaviour
{
    [Header("Claw Parts & Speed")]
    [SerializeField] private Rigidbody leftClawRb;
    [SerializeField] private Rigidbody rightClawRb;
    [SerializeField] private float clawForce = 20f;     // 집게를 닫는 힘
    [SerializeField] private float clawSpeed = 100f;    // 집게가 회전하는 속도

    [Header("Claw Angle Limits")]
    // HingeJoint limits는 상대 각도를 사용합니다.
    // 두 팔 모두 초기 각도 90도에서 45도(닫힘)로 이동해야 하므로,
    // Joint의 'zero'가 90도에 설정된다고 가정하고 상대 각도를 설정합니다.
    [SerializeField] private float jointClosedAngle = -45f; // 닫힘 각도 (상대적)
    [SerializeField] private float jointOpenAngle = 45f;    // 열림 각도 (상대적)
    [SerializeField] private float initialAngleTolerance = 1f; // 초기화 오차 허용 범위

    // 초기 회전 각도 저장을 위한 변수 (절대 각도)
    private const float initialZAngle = 90f;

    // 집게 파트를 연결할 Joint 변수
    private HingeJoint leftJoint;
    private HingeJoint rightJoint;

    void Start()
    {
        // Rigidbody 확인
        if (leftClawRb == null || rightClawRb == null)
        {
            Debug.LogError("Claw Rigidbodies must be assigned in the Inspector.");
            return;
        }

        // 1. Left Joint 설정
        leftJoint = leftClawRb.gameObject.AddComponent<HingeJoint>();
        leftJoint.connectedBody = GetComponent<Rigidbody>();
        leftJoint.anchor = leftClawRb.transform.InverseTransformPoint(transform.position);
        leftJoint.axis = Vector3.forward; // Z축 사용
        leftJoint.useMotor = true;

        // 1-1. 각도 제한 설정 적용
        ApplyAngleLimits(leftJoint);
        // HingeJoint의 Reference Angle을 90도로 설정 (Inspector에서 직접 설정해야 정확함)
        // 코드에서는 제어하기 어려우므로, Joint가 90도를 기준으로 작동한다고 가정합니다.

        // 2. Right Joint 설정
        rightJoint = rightClawRb.gameObject.AddComponent<HingeJoint>();
        rightJoint.connectedBody = GetComponent<Rigidbody>();
        rightJoint.anchor = rightClawRb.transform.InverseTransformPoint(transform.position);
        rightJoint.axis = Vector3.forward; // Z축 사용
        rightJoint.useMotor = true;

        // 2-1. 각도 제한 설정 적용
        ApplyAngleLimits(rightJoint);

        SetClawMotor(0, 0); // 초기 모터 정지
    }

    // --- 집게 제어 함수 ---

    // 집게를 닫는 함수
    public void CloseClaw()
    {
        // 두 팔 모두 음의 방향으로 회전해야 45도(닫힘)로 이동한다고 가정
        SetClawMotor(-clawSpeed, -clawSpeed, clawForce);
        Debug.Log("Claw: Closing started.");
    }

    // 집게를 여는 함수
    public void OpenClaw()
    {
        // 열림 각도(45도 이상)로 회전 시작 (양의 속도)
        SetClawMotor(clawSpeed, clawSpeed, clawForce);
        Debug.Log("Claw: Opening started.");

        // 초기화 코루틴 시작
        StartCoroutine(ResetClawToInitialAngle());
    }

    // 집게가 열린 후 초기 각도로 정확히 복귀시키는 코루틴
    private IEnumerator ResetClawToInitialAngle()
    {
        // 1. 열림 각도에 도달할 때까지 잠시 대기
        yield return new WaitForSeconds(0.5f);

        // 2. 초기화 시도 루프
        float currentAngleDifference = initialAngleTolerance + 1f;

        while (currentAngleDifference > initialAngleTolerance)
        {
            // 모터를 정지시켜 물리 시뮬레이션에 맡깁니다.
            SetClawMotor(0, 0, 0);

            // 현재 Z축 각도와 목표 각도(90도)의 차이 계산
            // Z축 회전값을 가져옵니다.
            float leftCurrentAngle = leftClawRb.transform.localEulerAngles.z;
            float rightCurrentAngle = rightClawRb.transform.localEulerAngles.z;

            // 초기 각도(90도)와의 차이를 계산하여 360도를 넘는 오차를 정확히 반영
            float leftDiff = Mathf.Abs(Mathf.DeltaAngle(leftCurrentAngle, initialZAngle));
            float rightDiff = Mathf.Abs(Mathf.DeltaAngle(rightCurrentAngle, initialZAngle));

            currentAngleDifference = Mathf.Max(leftDiff, rightDiff);

            // 각도에 도달하면 루프 종료
            if (currentAngleDifference <= initialAngleTolerance)
            {
                Debug.Log("Claw: Successfully reset to initial angle (90 deg).");
                // 최종적으로 각도를 90도로 고정 (불필요한 물리적 힘 제거)
                leftClawRb.transform.localEulerAngles = new Vector3(0, leftClawRb.transform.localEulerAngles.y, initialZAngle);
                rightClawRb.transform.localEulerAngles = new Vector3(0, rightClawRb.transform.localEulerAngles.y, initialZAngle);
                yield break;
            }

            // 각도에 도달하지 못했다면, 초기 각도를 향해 미세하게 모터를 작동시킵니다.
            // 각도 차이의 부호를 사용하여 모터 방향을 결정
            float leftAngleToTarget = Mathf.DeltaAngle(leftCurrentAngle, initialZAngle);
            float rightAngleToTarget = Mathf.DeltaAngle(rightCurrentAngle, initialZAngle);

            float leftResetSpeed = Mathf.Sign(leftAngleToTarget) * (clawSpeed / 4f);
            float rightResetSpeed = Mathf.Sign(rightAngleToTarget) * (clawSpeed / 4f);

            SetClawMotor(leftResetSpeed, rightResetSpeed, clawForce / 4f); // 낮은 힘으로 복귀 구동

            yield return null;
        }
    }

    // Joint에 각도 제한을 적용하는 유틸리티 함수
    private void ApplyAngleLimits(HingeJoint joint)
    {
        JointLimits limits = joint.limits;

        // HingeJoint의 limits는 min이 max보다 작아야 합니다.
        limits.min = jointClosedAngle;
        limits.max = jointOpenAngle;
        limits.bounciness = 0f;

        joint.useLimits = true;
        joint.limits = limits;
    }

    // JointMotor 설정 유틸리티 함수
    private void SetClawMotor(float leftTargetSpeed, float rightTargetSpeed, float force = 0)
    {
        // 모터 설정
        JointMotor leftMotor = leftJoint.motor;
        leftMotor.force = (force > 0) ? force : clawForce;
        leftMotor.targetVelocity = leftTargetSpeed;
        leftJoint.motor = leftMotor;

        JointMotor rightMotor = rightJoint.motor;
        rightMotor.force = (force > 0) ? force : clawForce;
        rightMotor.targetVelocity = rightTargetSpeed;
        rightJoint.motor = rightMotor;
    }
}