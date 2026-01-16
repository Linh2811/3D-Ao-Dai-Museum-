using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpScript : MonoBehaviour
{
    [Header("Cài đặt chung")]
    public GameObject player;
    public Transform holdPos;
    public float pickUpRange = 5f;
    public float rotationSensitivity = 10f;

    [Header("Khoảng cách an toàn để vật rắn lại (m)")]
    public float safeDistance = 2.0f; // Player phải đi xa 2m thì vật mới cứng lại

    [Header("Kéo Script Di Chuyển vào đây")]
    public MonoBehaviour playerController;

    private GameObject heldObj;
    private Rigidbody heldObjRb;
    private int holdLayerIndex;
    private int defaultLayerIndex = 0;

    private Vector3 originPos;
    private Quaternion originRot;

    // Biến để quản lý Coroutine (để tránh lỗi nếu nhặt lại quá nhanh)
    private Coroutine collisionCoroutine;

    void Start()
    {
        holdLayerIndex = LayerMask.NameToLayer("holdLayer");
        if (holdLayerIndex == -1) holdLayerIndex = 0;
    }

    void Update()
    {
        if (heldObj == null)
        {
            // --- CHƯA CẦM GÌ ---
            if (Input.GetMouseButtonDown(0))
            {
                // Bắn tia bỏ qua layer Player
                int playerLayerMask = 1 << LayerMask.NameToLayer("Player");
                int mask = ~playerLayerMask;

                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, pickUpRange, mask))
                {
                    if (hit.transform.CompareTag("canPickUp"))
                    {
                        // Nếu đang chạy dở tiến trình đợi cứng lại thì hủy nó đi
                        if (collisionCoroutine != null) StopCoroutine(collisionCoroutine);
                        PickUpObject(hit.transform.gameObject);
                    }
                }
            }
        }
        else
        {
            // --- ĐANG CẦM VẬT ---
            if (Input.GetMouseButton(0))
            {
                RotateObject();
                if (playerController != null) playerController.enabled = false;
            }
            else
            {
                if (playerController != null) playerController.enabled = true;
            }

            if (Input.GetMouseButtonDown(1))
            {
                DropObject();
            }
        }
    }

    void LateUpdate()
    {
        if (heldObj != null)
        {
            MoveObject();
        }
    }

    void PickUpObject(GameObject pickUpObj)
    {
        if (pickUpObj.GetComponent<Rigidbody>())
        {
            heldObj = pickUpObj;
            heldObjRb = pickUpObj.GetComponent<Rigidbody>();

            // Lưu vị trí cũ
            originPos = pickUpObj.transform.position;
            originRot = pickUpObj.transform.rotation;

            heldObjRb.isKinematic = true;
            heldObjRb.useGravity = false;
            heldObjRb.transform.parent = holdPos.transform;

            // Đưa sang holdLayer để không đụng vào người
            SetLayerRecursively(heldObj, holdLayerIndex);
        }
    }

    void DropObject()
    {
        // 1. Khóa cứng vật lý
        heldObjRb.isKinematic = true;
        heldObjRb.velocity = Vector3.zero;
        heldObjRb.angularVelocity = Vector3.zero;

        // 2. Tách cha con & Trả về vị trí cũ
        heldObj.transform.parent = null;
        heldObj.transform.position = originPos;
        heldObj.transform.rotation = originRot;

        // 3. Reset các biến điều khiển
        if (playerController != null) playerController.enabled = true;

        // 4. QUAN TRỌNG: Không set về Layer 0 ngay!
        // Gọi Coroutine đợi Player đi ra chỗ khác rồi mới set
        collisionCoroutine = StartCoroutine(RestoreCollisionWhenSafe(heldObj));

        heldObj = null;
    }

    // --- LOGIC MỚI: Đợi Player đi xa mới bật va chạm ---
    IEnumerator RestoreCollisionWhenSafe(GameObject objToRestore)
    {
        // Vòng lặp: Kiểm tra khoảng cách mỗi khung hình
        while (objToRestore != null && Vector3.Distance(player.transform.position, objToRestore.transform.position) < safeDistance)
        {
            // Nếu Player vẫn đứng quá gần (< 2m), thì chờ frame tiếp theo
            // Lúc này vật vẫn ở 'holdLayer' nên Player đi xuyên qua được (không bị đẩy)
            yield return null;
        }

        // Khi Player đã đi xa rồi, hoặc vật bị hủy
        if (objToRestore != null)
        {
            // Trả vật về Layer mặc định (Lúc này nó mới thành vật rắn)
            SetLayerRecursively(objToRestore, 0);
            // Nếu muốn bật trọng lực lại thì bỏ comment dòng dưới
            // objToRestore.GetComponent<Rigidbody>().useGravity = true;
        }
    }

    void MoveObject()
    {
        heldObj.transform.position = holdPos.transform.position;
    }

    void RotateObject()
    {
        float XaxisRotation = Input.GetAxis("Mouse X") * rotationSensitivity;
        float YaxisRotation = Input.GetAxis("Mouse Y") * rotationSensitivity;

        heldObj.transform.Rotate(Vector3.up, -XaxisRotation, Space.World);
        heldObj.transform.Rotate(Vector3.right, YaxisRotation, Space.World);
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child != null) SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}