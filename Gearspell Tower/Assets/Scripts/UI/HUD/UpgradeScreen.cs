using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UpgradeScreen : MonoBehaviour
{
    private Action<UpgradeData> onCardSelected;

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
    [SerializeField] private TextMeshProUGUI healCostText;
    [SerializeField] private Button regenButton;
    [SerializeField] private TextMeshProUGUI regenCostText;
    [SerializeField] private TextMeshProUGUI regenTimerText;
    [SerializeField] private Button mineButton;
    [SerializeField] private TextMeshProUGUI mineCostText;

    public bool IsOpen => panel != null && panel.activeSelf;

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

        //if (healButton != null) healButton.onClick.AddListener(TryHeal);
        //if (regenButton != null) regenButton.onClick.AddListener(TryRegenBoost);
        //if (mineButton != null) mineButton.onClick.AddListener(TryPlaceMines);

        if (G.EventManager == null) return;
        G.EventManager.OnGearsChanged += OnGearsChanged;
        G.EventManager.OnSlotUnlocked += _ => upgradeInventoryPanel?.Refresh();
        
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

    public void Open(List<UpgradeData> offers, Action<UpgradeData> callback)
    {
        if (IsOpen) return;

        currentOffers = new List<UpgradeData>(offers);
        onCardSelected = callback;
        RefreshCards();

        UpdateUI();

        upgradeInventoryPanel.Initialize(OnUpgradeSlotClicked, true);
        upgradeInventoryPanel.Refresh();
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

    private void UpdateUI()
    {
        UpdateGearsText();
        upgradeInventoryPanel.Refresh();
        //UpdateGlobalButtons();
    }

    private void UpdateGearsText()
    {
        int gears = G.ResourceManager?.Gears ?? 0;
        gearsText.text = gears.ToString();
    }

    private void OnUpgradeSlotClicked(int slotIndex)
    {
        int unlockedSlots = G.EquipmentManager?.UnlockedSlots ?? 1;
        if (slotIndex >= unlockedSlots)
            TryUnlockSlot();
        upgradeInventoryPanel?.Refresh();
    }

    //private void UpdateGlobalButtons()
    //{
    //    var sys = G.UpgradeSystem;
    //    if (sys == null) return;

    //    int gears = G.ResourceManager?.Gears ?? 0;

    //    // Heal
    //    healCostText.text = sys.HealCost.ToString();
    //    healButton.interactable = gears >= sys.HealCost;

    //    // Regen
    //    regenCostText.text = sys.RegenBoostCost.ToString();
    //    bool regenActive = sys.IsRegenBoostActive();
    //    regenButton.interactable = !regenActive && gears >= sys.RegenBoostCost;

    //    // Mines
    //    mineCostText.text = sys.MineCost.ToString();
    //    mineButton.interactable = gears >= sys.MineCost;
    //}

    private void OnGearsChanged(int amount)
    {
        UpdateGearsText();
        //UpdateGlobalButtons();

        foreach (var card in activeCards)
        {
            if (card.gameObject.activeSelf)
                card.UpdateAvailability();
        }
    }

    private void TryUnlockSlot()
    {
        if (G.EquipmentManager != null && G.EquipmentManager.TryUnlockNextSlot())
        {
            UpdateGearsText();
            upgradeInventoryPanel.Refresh();
        }
    }

    private void OnCardClicked(UpgradeCard card)
    {
        if (G.ResourceManager.Gears >= card.Data.cost)
        {
            onCardSelected?.Invoke(card.Data);

            int index = currentOffers.IndexOf(card.Data);
            if (index >= 0)
                currentOffers.RemoveAt(index);

            UpgradeData newCard = GenerateReplacementCard();
            if (newCard != null)
                currentOffers.Add(newCard);

            RefreshCards();
            UpdateUI();
        }
        else
            card.Shake();
    }

    private UpgradeData GenerateReplacementCard()
    {
        var availablePool = G.UpgradeSystem.GetAvailableUpgrades();
        if (availablePool == null) return null;

        // Исключаем уже показанные карты
        var remaining = availablePool.Where(u => !currentOffers.Contains(u)).ToList();
        if (remaining.Count == 0) return null;

        return remaining[UnityEngine.Random.Range(0, remaining.Count)];
    }

    //private void TryHeal()
    //{
    //    G.UpgradeSystem?.TryHealTower();
    //    UpdateGlobalButtons();
    //}

    //private void TryRegenBoost()
    //{
    //    G.UpgradeSystem?.TryBoostRegen();
    //    UpdateGlobalButtons();
    //}

    //private void TryPlaceMines()
    //{
    //    G.UpgradeSystem?.TryPlaceTraps();
    //    UpdateGlobalButtons();
    //}
}
