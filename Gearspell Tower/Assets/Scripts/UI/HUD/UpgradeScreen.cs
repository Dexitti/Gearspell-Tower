using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UpgradeScreen : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI gearsText;

    [SerializeField] private EquipmentInventoryPanel upgradeInventoryPanel;

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardsContainer;
    private List<UpgradeData> currentOffers = new();
    private List<UpgradeCard> activeCards = new();

    [Header("Global Abilities")]
    [SerializeField] private Button healButton;
    [SerializeField] private TextMeshProUGUI healText;
    [SerializeField] private Button regenButton;
    [SerializeField] private TextMeshProUGUI regenBoostText;
    [SerializeField] private Button mineButton;
    [SerializeField] private TextMeshProUGUI placeMinesText;

    public bool IsOpen => panel != null && panel.activeSelf;
    public event Action<UpgradeData> CardClicked;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
        foreach (Transform child in cardsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void Start()
    {
        if (backButton != null) backButton.onClick.AddListener(Close);

        if (G.EventManager == null) return;
        G.EventManager.OnGearsChanged += OnGearsChanged;
        G.EventManager.OnSlotUnlocked += _ => upgradeInventoryPanel?.Refresh();
        upgradeInventoryPanel.OnClick += OnUpgradeSlotClicked;

        if (healButton != null) healButton.onClick.AddListener(Heal);
        if (regenButton != null) regenButton.onClick.AddListener(RegenBoost);
        if (mineButton != null) mineButton.onClick.AddListener(PlaceMines);
    }

    private void Update()
    {
        if (panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    private void OnDestroy()
    {
        if (G.EventManager != null)
            G.EventManager.OnGearsChanged -= OnGearsChanged;
    }

    public void Open(List<UpgradeData> offers)
    {
        if (IsOpen) return;

        currentOffers = new List<UpgradeData>(offers);
        upgradeInventoryPanel.Initialize(true);
        UpdateUI();
        G.GameManager?.OpenUpgrade();
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
        G.GameManager?.CloseUpgrade();

        var hud = FindFirstObjectByType<HUDController>();
        hud?.GetComponent<EquipmentInventoryPanel>()?.Refresh();
    }

    private void UpdateUI()
    {
        UpdateGearsText();
        RefreshCards();
        upgradeInventoryPanel.Refresh();
        UpdateGlobalButtons();
    }

    private void RefreshCards()
    {
        foreach (var card in activeCards)
            Destroy(card.gameObject);
        activeCards.Clear();

        foreach (var offer in currentOffers)
        {
            if (offer == null) continue;
            var cardObj = Instantiate(cardPrefab, cardsContainer);
            var card = cardObj.GetComponent<UpgradeCard>();
            card.Initialize(offer, OnCardClicked);
            activeCards.Add(card);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(cardsContainer as RectTransform); // Выравнивает карты
    }

    private void UpdateGearsText()
    {
        int gears = G.ResourceManager?.Gears ?? 0;
        gearsText.text = gears.ToString();
    }

    private void OnGearsChanged(int amount)
    {
        UpdateGearsText();
        foreach (var card in activeCards)
        {
            if (card.gameObject.activeSelf)
                card.UpdateAvailability();
        }
        UpdateGlobalButtons();
    }

    private void OnUpgradeSlotClicked(int slotIndex)
    {
        if (slotIndex >= G.EquipmentManager.UnlockedSlots && G.EquipmentManager.TryUnlockNextSlot())
        {
            G.UpgradeSystem.InvalidateCache();
            AddOffers();
        }
    }

    private void AddOffers()
    {
        if (currentOffers.Count == 3) return;
        var availablePool = G.UpgradeSystem.GetAvailableUpgrades();
        if (availablePool == null) return;
        var additionalCards = availablePool
            .Where(u => !currentOffers.Contains(u))
            .Take(3 - currentOffers.Count)
            .ToList();
        if (additionalCards.Count > 0)
        {
            currentOffers.AddRange(additionalCards);
            UpdateUI();
        }

    }

    private void OnCardClicked(UpgradeCard card)
    {
        if (G.ResourceManager.Gears >= card.Data.cost)
        {
            CardClicked?.Invoke(card.Data);
            G.UpgradeSystem.InvalidateCache();
            //int index = currentOffers.IndexOf(card.Data);
            //if (index >= 0)
            //    currentOffers.RemoveAt(index);
            RefreshAllOffers();
        }
        else
            card.Shake(); // Не работает из-за выравнивания, надо чисто визуал, а не всю!
    }

    private void RefreshAllOffers()
    {
        var availablePool = G.UpgradeSystem.GetAvailableUpgrades();
        if (availablePool == null) return;
        currentOffers = availablePool.Take(Mathf.Min(3, availablePool.Count)).ToList();
        UpdateUI();
    }

    //private UpgradeData GenerateReplacementCard()
    //{
    //    var availablePool = G.UpgradeSystem.GetAvailableUpgrades();
    //    if (availablePool == null) return null;

    //    // Исключаем уже показанные карты
    //    var remaining = availablePool.Where(u => !currentOffers.Contains(u)).ToList();
    //    if (remaining.Count == 0) return null;

    //    return remaining.First();
    //}

    private void Heal()
    {
        G.UpgradeSystem?.TryHealTower();
        UpdateGlobalButtons();
    }

    private void RegenBoost()
    {
        G.UpgradeSystem?.TryBoostRegen();
        UpdateGlobalButtons();
    }

    private void PlaceMines()
    {
        G.UpgradeSystem?.TryPlaceTraps();
        UpdateGlobalButtons();
    }

    private void UpdateGlobalButtons()
    {
        var sys = G.UpgradeSystem;
        int gears = G.ResourceManager?.Gears ?? 0;

        healText.text = G.LocalizationManager.GetText("Heal", sys.HealPercent);
        healButton.interactable = gears >= sys.HealCost;

        regenBoostText.text = G.LocalizationManager.GetText("Regen", sys.RegenBoostDuration);
        regenButton.interactable = gears >= sys.RegenBoostCost;

        placeMinesText.text = G.LocalizationManager.GetText("PlaceMines", sys.MinesCount);
        mineButton.interactable = gears >= sys.MinesCost;
    }
}
