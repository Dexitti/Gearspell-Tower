using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UILocalizedText : MonoBehaviour
{
    [SerializeField] private string localizationKey;
    private TextMeshProUGUI textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        if (string.IsNullOrEmpty(localizationKey))
            localizationKey = gameObject.name;
    }

    private void Start()
    {
        if (G.LocalizationManager != null)
        {
            G.LocalizationManager.OnLanguageChanged += UpdateText;
            UpdateText(G.LocalizationManager.CurrentLanguage);
        }
    }

    private void OnDestroy()
    {
        if (G.LocalizationManager != null)
        {
            G.LocalizationManager.OnLanguageChanged -= UpdateText;
        }
    }

    private void UpdateText(LocalizationManager.Language lang)
    {
        if (textComponent != null && !string.IsNullOrEmpty(localizationKey))
        {
            textComponent.text = G.LocalizationManager.GetText(localizationKey);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();

        // Автоматически подставляем имя объекта
        if (string.IsNullOrEmpty(localizationKey))
            localizationKey = gameObject.name;
    }
#endif
}