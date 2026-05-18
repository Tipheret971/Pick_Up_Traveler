using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float minAngle = -90f; // 좌측 시점 (예: 왼쪽 끝)
    [SerializeField] private float maxAngle = 90f;  // 우측 시점 (예: 오른쪽 끝)

    // --- 줌 설정 (Scale 기반) ---
    [Header("Zoom (Scale) Settings")]
    [SerializeField] private float zoomScaleSpeed = 8f; // 스케일 변화 속도
    [SerializeField] private float minScaleXZ = 5.5f;     // 최소 스케일 (가장 가까이 줌 인)
    [SerializeField] private float maxScaleXZ = 17f;     // 최대 스케일 (가장 멀리 줌 아웃)

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // A, D 키 입력

        if (horizontalInput != 0)
        {
            // 현재 Y축 회전 각도 가져오기
            float currentY = transform.localEulerAngles.y;

            // 0~360 범위를 -180~180 범위로 변환 (Clamp를 위해)
            // (Unity는 0~360을 사용하므로 변환 과정이 필요함)
            float angle = Mathf.DeltaAngle(0, currentY);

            // 회전량 계산
            float rotationAmount = horizontalInput * -1 * rotationSpeed * Time.deltaTime;

            // 새 각도 계산 및 Clamp 적용
            float newAngle = Mathf.Clamp(angle + rotationAmount, minAngle, maxAngle);

            // 로컬 회전 적용
            transform.localRotation = Quaternion.Euler(0, newAngle, 0);
        }

        // --- 2. 앞뒤 이동/줌 (W/S) 로직 (Scale 기반) ---
        float verticalInput = Input.GetAxis("Vertical"); // W, S 키 입력

        if (verticalInput != 0)
        {
            // 현재 로컬 스케일 값을 가져옵니다. (X, Y, Z가 동일하다고 가정)
            float currentScale = transform.localScale.x;

            // W를 누를 때 줌 인 (스케일 감소), S를 누를 때 줌 아웃 (스케일 증가)
            // W는 양수(verticalInput = 1), 줌 인은 감소이므로, -1을 곱하여 방향을 반전시킵니다.
            float scaleChangeAmount = verticalInput * -1 * zoomScaleSpeed * Time.deltaTime;

            // 새로운 스케일 계산 및 제한 적용
            float newScale = currentScale + scaleChangeAmount;

            // minScaleXZ와 maxScaleXZ 사이로 제한
            newScale = Mathf.Clamp(newScale, minScaleXZ, maxScaleXZ);

            // X, Z 스케일만 업데이트하고 Y 스케일은 유지합니다.
            Vector3 newScaleVector = new Vector3(newScale, transform.localScale.y, newScale);

            transform.localScale = newScaleVector;
        }
    }
}