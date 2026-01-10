using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject infoPanel;        // panel root
    public Text titleText;
    public Text descriptionText;
    public CanvasGroup infoCanvasGroup;

    public GameObject viewer360Panel;
    public Transform viewerModelRoot;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Fade")]
    public float fadeDuration = 0.3f;

    InteractableItem currentItem;

    void Awake()
    {
        Instance = this;
        if (infoPanel != null) infoPanel.SetActive(false);
        if (viewer360Panel != null) viewer360Panel.SetActive(false);
    }

    // called by InteractableItem
    public void OpenInfoPanel(InteractableItem item)
    {
        currentItem = item;
        titleText.text = item.GetTitle();
        descriptionText.text = item.GetDescription();
        StartCoroutine(FadeIn(infoCanvasGroup));
        infoPanel.SetActive(true);
    }

    public void CloseInfoPanel()
    {
        StartCoroutine(FadeOut(infoCanvasGroup, () => infoPanel.SetActive(false)));
        if (audioSource.isPlaying) audioSource.Stop();
    }

    public void PlayAudio(InteractableItem item)
    {
        if (item.GetAudio() == null) return;
        if (audioSource.isPlaying) audioSource.Stop();
        audioSource.clip = item.GetAudio();
        audioSource.Play();
    }

    public void Open360Viewer(InteractableItem item)
    {
        viewer360Panel.SetActive(true);
        // Clear old children
        foreach (Transform t in viewerModelRoot) Destroy(t.gameObject);
        // instantiate a copy of the exhibit (or a model prefab) and parent to viewerModelRoot
        GameObject copy = Instantiate(item.gameObject);
        // remove InteractableItem & colliders if desired
        foreach (var comp in copy.GetComponents<InteractableItem>()) Destroy(comp);
        foreach (var col in copy.GetComponents<Collider>()) Destroy(col);
        copy.transform.SetParent(viewerModelRoot, false);
        copy.transform.localPosition = Vector3.zero;
        copy.transform.localRotation = Quaternion.identity;
        // scale adjust if needed
    }

    public void Close360Viewer()
    {
        viewer360Panel.SetActive(false);
    }

    IEnumerator FadeIn(CanvasGroup cg)
    {
        float t = 0;
        cg.alpha = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1;
    }

    IEnumerator FadeOut(CanvasGroup cg, System.Action onDone = null)
    {
        float t = 0;
        float start = cg.alpha;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(1 - t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0;
        onDone?.Invoke();
    }

    // Example API for guided tour
    public void StartGuidedTourTo(Transform target)
    {
        // you can call your GuidedTourManager or PlayerMover here
        Debug.Log("Start guided tour to: " + target.name);
    }
}
