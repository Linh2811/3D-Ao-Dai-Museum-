using UnityEngine;

public class ClickVisualizer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Kéo Prefab vòng tròn vào đây")]
    public GameObject clickIndicatorPrefab;

    [Tooltip("Thời gian tồn tại của vòng tròn (giây)")]
    public float duration = 1f;

    [Tooltip("Nâng nhẹ vòng tròn lên khỏi mặt đất để tránh bị lỗi hình ảnh")]
    public float heightOffset = 0.05f;

    // --- Phần quan trọng: Đăng ký nhận sự kiện ---

    // Hàm này chạy khi đối tượng chứa script được Bật (Enable)
    private void OnEnable()
    {
        // Đăng ký lắng nghe sự kiện OnGroundTouch từ script CharacterMovement
        CharacterMovement.OnGroundTouch += SpawnIndicator;
    }

    // Hàm này chạy khi đối tượng chứa script bị Tắt (Disable) hoặc bị hủy
    private void OnDisable()
    {
        // Hủy đăng ký sự kiện (Rất quan trọng để tránh lỗi bộ nhớ)
        CharacterMovement.OnGroundTouch -= SpawnIndicator;
    }

    // --- Hàm xử lý ---

    // Hàm này sẽ tự động được gọi khi sự kiện OnGroundTouch xảy ra
    // Nó nhận vào tham số position là vị trí mà bạn đã click chuột
    private void SpawnIndicator(Vector3 position)
    {
        if (clickIndicatorPrefab == null) return;

        // Tính toán vị trí sinh ra (nâng lên một chút so với mặt đất)
        Vector3 spawnPos = position + new Vector3(0, heightOffset, 0);

        // Sinh ra prefab vòng tròn tại vị trí đã tính, giữ nguyên góc xoay mặc định
        GameObject newIndicator = Instantiate(clickIndicatorPrefab, spawnPos, Quaternion.identity);

        // Ra lệnh tự hủy đối tượng vừa sinh ra sau khoảng thời gian 'duration'
        Destroy(newIndicator, duration);
    }
}