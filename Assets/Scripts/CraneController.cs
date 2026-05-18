using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필요

public class CraneController : MonoBehaviour
{
    // 크레인 조작 상태를 정의
    private enum CraneState { Idle, MovingX, ReadyZ, MovingZ, Drop, ReturnHome }
    private CraneState currentState = CraneState.Idle;
    private bool isDropSequenceRunning = false; // 새로운 플래그 변수
    private bool isTouchingGoods = false; // 추가: 굿즈 충돌 플래그

    // 움직임 속도 및 시작 위치
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float dropSpeed = 1f; // 상하 운동 속도에 활용
    [SerializeField] private float homeReturnSpeed = 2f;

    // Y축 하강 및 상승 관련 변수 (인스펙터에서 설정 필요)
    [SerializeField] private float dropYPosition = -3f; // 집게가 내려갈 최종 로컬 Y 좌표
    private Vector3 startLocalPosition; // 초기 로컬 위치 (X, Y, Z 모두 저장)
    private float initialYPosition;    // 초기 로컬 Y 좌표

    // 크레인이 움직이는 방향 (X축은 음수, Z축은 양수)
    private readonly Vector3 moveDirectionX = new Vector3(-1, 0, 0); // X축 (음수 방향)
    private readonly Vector3 moveDirectionZ = new Vector3(0, 0, 1);  // Z축 (양수 방향)

    // 딜레이/대기 시간 변수 (시간 기반 이동 변수는 삭제됨)
    [Header("Drop Sequence Settings")]
    [SerializeField] private float waitTimeAtBottom = 1.5f; // 바닥에서 대기 시간 (집게 작동 시간)
    [SerializeField] private float delayBeforeDrop = 1f;   // Z축 이동 후 하강 전 딜레이
    [SerializeField] private float delayBeforeReturn = 1f; // 상승 후 복귀 전 딜레이

    // 크레인 이동 범위 제한 변수
    [Header("Movement Limits")]
    [SerializeField] private float minX = -4.4f;
    [SerializeField] private float maxX = 3f;
    [SerializeField] private float minZ = -3f;
    [SerializeField] private float maxZ = 1f;

    private ClawController clawController;
    private CraneCoinManager coinManager;

    public AudioClip craneMoveSFX; // 크레인 이동 루프 소리

    void Start()
    {
        // 핵심 수정 1: 모든 시작 위치를 Local Position으로 저장
        startLocalPosition = transform.localPosition;
        initialYPosition = startLocalPosition.y;

        // ClawController 컴포넌트 찾기 및 할당
        clawController = GetComponent<ClawController>();

        if (clawController == null)
        {
            Debug.LogError("ClawController 인스턴스를 찾을 수 없습니다! ClawController가 CraneController와 동일 오브젝트에 붙어 있는지 확인하세요.");
        }

        // 씬에서 CraneCoinManager 컴포넌트 찾기
        coinManager = FindFirstObjectByType<CraneCoinManager>();
        if (coinManager == null) Debug.LogError("CraneCoinManager를 씬에서 찾을 수 없습니다.");
    }

    void Update()
    {
        // 1. 스페이스바를 누를 때 발생하는 상태 전이
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentState == CraneState.Idle)
            {
                // 핵심 수정: 게임을 시작할 수 있는지 CraneCoinManager에게 확인
                if (coinManager != null && coinManager.CanPlayAndDeductCoin())
                {
                    // 코인 차감 성공 시에만 다음 상태로 전이
                    currentState = CraneState.MovingX;
                }
                // else: 코인 부족 등의 메시지는 CraneCoinManager에서 처리됩니다.
            }
            else if (currentState == CraneState.ReadyZ)
            {
                currentState = CraneState.MovingZ;
            }
        }

        // 2. 스페이스바를 뗄 때 발생하는 상태 전이 (GetKeyUp)
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (currentState == CraneState.MovingX)
            {
                currentState = CraneState.ReadyZ;
            }
            else if (currentState == CraneState.MovingZ)
            {
                currentState = CraneState.Drop;
            }
        }

        // 3. 현재 상태에 따른 동작 실행 (Switch 문)
        switch (currentState)
        {
            case CraneState.Idle:
                SoundManager.Instance.PlayLoopSFX(null);
                break;
            case CraneState.ReadyZ:
                SoundManager.Instance.PlayLoopSFX(null);
                break;
            case CraneState.MovingX:
                MoveCrane(moveDirectionX);
                SoundManager.Instance.PlayLoopSFX(craneMoveSFX, 0.7f);
                break;
            case CraneState.MovingZ:
                MoveCrane(moveDirectionZ);
                SoundManager.Instance.PlayLoopSFX(craneMoveSFX, 0.7f);
                break;
            case CraneState.Drop:
                SoundManager.Instance.PlayLoopSFX(null);
                HandleDropSequence();
                break;
            case CraneState.ReturnHome:
                HandleReturnHome();
                SoundManager.Instance.PlayLoopSFX(craneMoveSFX, 0.7f);
                break;
        }
    }

    // ================= 동작 실행 로직 ===================

    // X, Z 이동 처리
    private void MoveCrane(Vector3 direction)
    {
        // 1. 현재 위치를 로컬 좌표를 기준으로 이동 
        transform.Translate(direction * moveSpeed * Time.deltaTime);

        // 2. 이동 제한을 로컬 좌표를 기준으로 적용
        Vector3 currentLocalPosition = transform.localPosition;

        // X축 제한
        currentLocalPosition.x = Mathf.Clamp(currentLocalPosition.x, minX, maxX);

        // Z축 제한
        currentLocalPosition.z = Mathf.Clamp(currentLocalPosition.z, minZ, maxZ);

        // 3. 로컬 좌표를 다시 적용
        transform.localPosition = currentLocalPosition;
    }

    // 하강 및 상승 처리 (코루틴 시작)
    // 하강 및 상승 처리 (코루틴 시작)
    private void HandleDropSequence()
    {
        // Drop 시퀀스가 실행 중이 아니라면 (false)
        if (!isDropSequenceRunning)
        {
            isDropSequenceRunning = true; // 플래그를 true로 설정
            StartCoroutine(DropSequenceCoroutine());
        }
        // 이 상태에서는 아무것도 하지 않고 코루틴이 끝날 때까지 기다립니다.
    }

    // 속도 기반 Drop 시퀀스 코루틴 (MoveTowards 활용)
    IEnumerator DropSequenceCoroutine()
    {
        // 0. 하강 전 딜레이
        yield return new WaitForSeconds(delayBeforeDrop);

        // 1. 하강 (Down) - 속도 기반 이동
        isTouchingGoods = false; // 하강 시작 시 플래그 초기화

        // 핵심 수정 : 목표 Y 좌표를 현재 로컬 X, Z와 설정된 로컬 dropYPosition으로 설정
        Vector3 targetY_Down = new Vector3(transform.localPosition.x, dropYPosition, transform.localPosition.z);

        // 루프 조건 수정: 목표에 근접하거나 (AND) 굿즈에 닿지 않았을 때까지
        while (Vector3.Distance(transform.localPosition, targetY_Down) > 0.01f && !isTouchingGoods)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                targetY_Down,
                dropSpeed * Time.deltaTime);
            yield return null;
        }
        // 루프가 끝난 이유가 목표 지점 도달이 아닌 경우에만 위치를 덮어쓰지 않습니다.
        if (!isTouchingGoods)
        {
            // 굿즈에 닿지 않고 최종 바닥에 도달한 경우에만 위치 보정
            transform.localPosition = targetY_Down;
        }

        // 2. 바닥에서 집게 닫기 시도 및 대기

        yield return new WaitForSeconds(waitTimeAtBottom); // 집게 작동 이전 대기

        // 2-1. 집게 닫기 (물리적 Joint 연결)
        if (clawController != null)
        {
            clawController.CloseClaw();
        }

        // 2-2. 대기 시간 (집게가 닫힌 채 유지)
        yield return new WaitForSeconds(waitTimeAtBottom); // 집게 작동 이후 대기

        // 3. 상승 (Up) - 속도 기반 이동
        // 핵심 수정 : 목표 Y 좌표를 초기 로컬 위치(initialYPosition)로 설정
        Vector3 targetY_Up = new Vector3(transform.localPosition.x, initialYPosition, transform.localPosition.z);

        while (Vector3.Distance(transform.localPosition, targetY_Up) > 0.01f) // Local Position 사용
        {
            transform.localPosition = Vector3.MoveTowards( // Local Position 사용
                transform.localPosition,
                targetY_Up,
                dropSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localPosition = targetY_Up; // 정확한 위치 보정

        // 4. 복귀 전 딜레이
        yield return new WaitForSeconds(delayBeforeReturn);

        // 5. 복귀 시작
        currentState = CraneState.ReturnHome;
        isDropSequenceRunning = false; // 코루틴 완료 후 플래그 해제
    }

    // 시작 위치 복귀 처리
    private void HandleReturnHome()
    {
        // 로컬 위치를 사용하여 Smooth하게 이동
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition, // Local Position 사용
            startLocalPosition,      // Local Position 목표 설정
            homeReturnSpeed * Time.deltaTime
        );

        // 도착하면 Idle 상태로 전환
        if (transform.localPosition == startLocalPosition) // Local Position 비교
        {
            // 복귀 완료 후 집게 열기 (Joint 파괴 및 초기화)
            if (clawController != null)
            {
                clawController.OpenClaw();
            }

            // 초기화 완료 후 Idle 상태로 전환
            currentState = CraneState.Idle;
        }
    }

    // 추가: Trigger Sensor에서 호출되는 메서드
    public void NotifyHitGoods()
    {
        // 현재 Drop 상태이고 코루틴이 실행 중일 때만 플래그를 설정
        if (currentState == CraneState.Drop && isDropSequenceRunning)
        {
            isTouchingGoods = true;
        }
    }
}