using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCard : MonoBehaviour
{
    Action<UpgradeCard> onClick;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button button;
    [SerializeField] private GameObject selectedBorder;
    [SerializeField] private GameObject unavailableOverlay;

    private UpgradeData data;
    public UpgradeData Data => data;

    public void Initialize(UpgradeData upgradeData, Action<UpgradeCard> callback)
    {
        data = upgradeData;
        onClick = callback;

        if (nameText != null) nameText.text = data.upgradeName;
        if (iconImage != null) iconImage.sprite = data.icon;
        if (descriptionText != null) descriptionText.text = data.description;
        if (costText != null) costText.text = data.cost.ToString();

        if (button != null)
            button.onClick.AddListener(OnClicked);

        UpdateAvailability();
    }

    public void UpdateAvailability()
    {
        if (data == null) return;

        bool canAfford = G.ResourceManager != null && G.ResourceManager.Gears >= data.cost;

        if (button != null)
            button.interactable = canAfford;

        if (costText != null)
            costText.color = canAfford ? Color.white : Color.red;
    }

    private void OnClicked()
    {
        onClick?.Invoke(this);
    }

    public void SetSelected(bool selected)
    {
        if (selectedBorder != null)
            selectedBorder.SetActive(selected);
    }

    public void Shake()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        Vector3 originalPos = transform.localPosition;
        float duration = 0.3f;
        float magnitude = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = originalPos.x + UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = originalPos.y + UnityEngine.Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClicked);
    }
}
