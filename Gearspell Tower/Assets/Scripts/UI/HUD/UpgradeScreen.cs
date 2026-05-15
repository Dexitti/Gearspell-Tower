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

    [SerializeField] private Button unlockSlotButton;
    [SerializeField] private TextMeshProUGUI unlockSlotCostText;

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
        if (unlockSlotButton != null) unlockSlotButton.onClick.AddListener(TryUnlockSlot);

        //if (healButton != null) healButton.onClick.AddListener(TryHeal);
        //if (regenButton != null) regenButton.onClick.AddListener(TryRegenBoost);
        //if (mineButton != null) mineButton.onClick.AddListener(TryPlaceMines);

        if (G.EventManager != null)
            G.EventManager.OnGearsChanged += OnGearsChanged;
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

        if (currentOffers == null || currentOffers.Count == 0)
            currentOffers = new List<UpgradeData>(offers);

        onCardSelected = callback;
        RefreshCards();

        UpdateUI();
        G.GameManager?.OpenUpgrade();
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
        G.GameManager?.CloseUpgrade();
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
        UpdateSlotBuyBtn();

        //UpdateGlobalButtons();
    }

    private void UpdateGearsText()
    {
        int gears = G.ResourceManager?.Gears ?? 0;
        gearsText.text = gears.ToString();
    }

    private void UpdateSlotBuyBtn()
    {
        //if (unlockSlotButton == null) return;

        //bool canUnlock = G.EquipmentManager != null && G.EquipmentManager.CanUnlockNextSlot(out int cost);

        //unlockSlotButton.gameObject.SetActive(canUnlock);

        //if (canUnlock)
        //{
        //    unlockSlotCostText.text = cost.ToString();
        //    bool canAfford = (G.ResourceManager?.Gears ?? 0) >= cost;
        //    unlockSlotButton.interactable = canAfford;
        //}
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
        UpdateSlotBuyBtn();
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
            UpdateSlotBuyBtn();
            UpdateGearsText();
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
