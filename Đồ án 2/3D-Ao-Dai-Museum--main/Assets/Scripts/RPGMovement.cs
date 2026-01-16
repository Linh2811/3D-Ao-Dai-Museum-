using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RPGMovement : MonoBehaviour
{
    [Header("Cài đặt Camera")]
    public Transform cameraTransform;   // Kéo Main Camera vào đây (chứ không phải làm con nữa)
    public float rotateSpeed = 5.0f;    // Tốc độ xoay camera
    public float scrollSpeed = 5.0f;    // Tốc độ zoom (nếu cần)

    [Header("Khoảng cách Camera")]
    public float distanceFromPlayer = 5.0f; // Khoảng cách camera so với nhân vật
    public float heightFromPlayer = 2.0f;   // Chiều cao camera

    private NavMeshAgent agent;
    private float yaw = 0.0f;   // Góc xoay ngang
    private float pitch = 0.0f; // Góc xoay dọc

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Đảm bảo chuột luôn hiện ra
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Tắt tự xoay của Agent để tránh xung đột
        agent.updateRotation = true;

        // Khởi tạo góc quay hiện tại
        if (cameraTransform != null)
        {
            Vector3 angles = cameraTransform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
        }
    }

    void LateUpdate()
    {
        // 1. XỬ LÝ CLICK CHUỘT TRÁI ĐỂ DI CHUYỂN
        // Chỉ di chuyển nếu KHÔNG giữ chuột phải (để tránh bị lẫn thao tác)
        if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                agent.SetDestination(hit.point);
            }
        }

        // 2. XỬ LÝ GIỮ CHUỘT PHẢI ĐỂ XOAY CAMERA
        if (cameraTransform)
        {
            if (Input.GetMouseButton(1)) // Đang giữ chuột phải
            {
                // Ẩn chuột tạm thời để xoay cho mượt
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                yaw += Input.GetAxis("Mouse X") * rotateSpeed;
                pitch -= Input.GetAxis("Mouse Y") * rotateSpeed;

                // Giới hạn góc ngẩng lên/xuống
                pitch = Mathf.Clamp(pitch, -45f, 85f);
            }
            else
            {
                // Nhả chuột phải ra thì hiện lại chuột để click
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            // Tính toán vị trí Camera: Luôn bám theo nhân vật nhưng xoay quanh nó
            // Nếu bạn muốn camera góc nhìn thứ nhất (FPS), hãy set distanceFromPlayer = 0
            Vector3 targetPosition = transform.position + new Vector3(0, heightFromPlayer, 0);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            // Công thức tính vị trí camera lùi lại phía sau nhân vật
            Vector3 position = targetPosition - (rotation * Vector3.forward * distanceFromPlayer);

            cameraTransform.rotation = rotation;
            cameraTransform.position = position;

            // (Tùy chọn) Xoay nhân vật theo hướng camera khi di chuyển
            // transform.eulerAngles = new Vector3(0, yaw, 0); 
        }
    }
}