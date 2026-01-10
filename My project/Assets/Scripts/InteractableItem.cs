using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class InteractableItem : MonoBehaviour
{
    [Header("Metadata")]
    public string itemTitle;
    [TextArea] public string itemDescription;
    public AudioClip audioGuide;

    [Header("Icon Prefabs (set in inspector)")]
    public GameObject iconPrefab;        // the generic icon prefab (UI Button)
    public Sprite speakerSprite;
    public Sprite eyeSprite;
    public Sprite viewer360Sprite;
    public Sprite exploreSprite;

    [Header("Icon settings")]
    public Vector3 iconOffset = Vector3.up * 1.6f;
    public float iconSpacing = 40f; // pixel spacing between icons

    // runtime handles
    GameObject speakerIcon, eyeIcon, viewerIcon, exploreIcon;
    Canvas parentCanvas;
    bool iconsCreated = false;
    Material originalMat;
    Renderer rend;

    void Start()
    {
        parentCanvas = FindObjectOfType<Canvas>();
        rend = GetComponent<Renderer>();
        if (rend != null) originalMat = rend.material;
    }

    // call to spawn icons (create once when player is near or on hover)
    public void CreateIconsIfNeeded()
    {
        if (iconsCreated) return;
        if (iconPrefab == null || parentCanvas == null) return;

        // helper to make a world icon instance
        System.Func<Sprite, GameObject> makeIcon = (sprite) => {
            GameObject go = Instantiate(iconPrefab, parentCanvas.transform);
            Image img = go.GetComponentInChildren<Image>();
            if (img != null && sprite != null) img.sprite = sprite;
            WorldIcon wi = go.GetComponent<WorldIcon>();
            wi.target = this.transform;
            wi.worldOffset = iconOffset;
            return go;
        };

        if (speakerSprite) speakerIcon = makeIcon(speakerSprite);
        if (eyeSprite) eyeIcon = makeIcon(eyeSprite);
        if (viewer360Sprite) viewerIcon = makeIcon(viewer360Sprite);
        if (exploreSprite) exploreIcon = makeIcon(exploreSprite);

        // position them horizontally by offsetting their RectTransform anchors
        ArrangeIcons();

        // wire up button events
        WireIconButtons();

        iconsCreated = true;
    }

    void ArrangeIcons()
    {
        // center icons around target screen point; offset by iconSpacing
        GameObject[] icons = new GameObject[] { speakerIcon, eyeIcon, viewerIcon, exploreIcon };
        int count = 0;
        foreach (var ic in icons) if (ic != null) count++;
        if (count == 0) return;

        int i = 0;
        for (int idx = 0; idx < icons.Length; idx++)
        {
            var ic = icons[idx];
            if (ic == null) continue;
            RectTransform rt = ic.GetComponent<RectTransform>();
            float x = (i - (count - 1) / 2f) * iconSpacing;
            // store offset in child script (WorldIcon) -> we keep worldOffset same but use screen offset via localPosition
            rt.anchoredPosition += new Vector2(x, 0);
            i++;
        }
    }

    void WireIconButtons()
    {
        if (speakerIcon)
        {
            Button b = speakerIcon.GetComponentInChildren<Button>();
            b.onClick.AddListener(() => { UIManager.Instance.PlayAudio(this); });
        }
        if (eyeIcon)
        {
            Button b = eyeIcon.GetComponentInChildren<Button>();
            b.onClick.AddListener(() => { UIManager.Instance.OpenInfoPanel(this); });
        }
        if (viewerIcon)
        {
            Button b = viewerIcon.GetComponentInChildren<Button>();
            b.onClick.AddListener(() => { UIManager.Instance.Open360Viewer(this); });
        }
        if (exploreIcon)
        {
            Button b = exploreIcon.GetComponentInChildren<Button>();
            b.onClick.AddListener(() => { UIManager.Instance.StartGuidedTourTo(transform); });
        }
    }

    // call when player hovers near / looks at object
    public void HighlightOn()
    {
        if (rend != null)
        {
            // simple emission highlight
            if (rend.material.HasProperty("_EmissionColor"))
            {
                rend.material.EnableKeyword("_EMISSION");
                rend.material.SetColor("_EmissionColor", Color.yellow * 0.5f);
            }
            else
            {
                // fallback tint
                rend.material.color = Color.Lerp(rend.material.color, Color.yellow, 0.4f);
            }
        }
        CreateIconsIfNeeded();
    }

    public void HighlightOff()
    {
        if (rend != null && originalMat != null)
        {
            rend.material = originalMat;
        }
        // optionally hide icons when not highlighted:
        if (speakerIcon) speakerIcon.SetActive(false);
        if (eyeIcon) eyeIcon.SetActive(false);
        if (viewerIcon) viewerIcon.SetActive(false);
        if (exploreIcon) exploreIcon.SetActive(false);
    }

    // optional: called by UIManager to get metadata
    public string GetTitle() => itemTitle;
    public string GetDescription() => itemDescription;
    public AudioClip GetAudio() => audioGuide;
}
