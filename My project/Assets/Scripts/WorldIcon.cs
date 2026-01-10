using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class WorldIcon : MonoBehaviour
{
    public Transform target;         // world target to follow
    public Vector3 worldOffset = Vector3.up * 1.6f; // offset above exhibit
    RectTransform rt;
    Camera mainCam;
    public bool hideIfBehind = true; // hide icon if behind camera

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            gameObject.SetActive(false);
            return;
        }
        Vector3 worldPos = target.position + worldOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

        if (hideIfBehind && screenPos.z < 0)
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
            return;
        }

        if (!gameObject.activeSelf) gameObject.SetActive(true);
        rt.position = screenPos;
    }

    // call to set text/icon dynamically
    public void SetLabel(string text)
    {
        Text t = GetComponentInChildren<Text>();
        if (t != null) t.text = text;
    }
}
