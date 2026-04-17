using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    private List<UpgradeData> currentOffers;
    private List<UpgradeCard> activeCards = new();

    [Header("Global Abilities")]
    [SerializeField] private Button healButton;
    [SerializeField] private TextMeshProUGUI healCostText;
    [SerializeField] private Button regenButton;
    [SerializeField] private TextMeshProUGUI regenCostText;
    [SerializeField] private TextMeshProUGUI regenTimerText;
    [SerializeField] private Button mineButton;
    [SerializeField] private TextMeshProUGUI mineCostText;


    private void Awake()
    {
        panel.SetActive(false);
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
        foreach (var card in activeCards)
            Destroy(card.gameObject);
        activeCards.Clear();

        foreach (var offer in offers)
        {
            var cardObj = Instantiate(cardPrefab, cardsContainer);
            var card = cardObj.GetComponent<UpgradeCard>();
            card.Initialize(offer, OnCardClicked);
            activeCards.Add(card);
        }

        onCardSelected = callback;

        UpdateUI();
        panel.SetActive(true);
        G.GameManager?.PauseGame();
    }

    public void Close()
    {
        panel.SetActive(false);
        G.GameManager?.ResumeGame();
    }

    private void UpdateUI()
    {
        UpdateGearsText();
        UpdateSlotBuyBtn();

        for (int i = 0; i < currentOffers.Count; i++)
        {
            activeCards[i].Initialize(currentOffers[i], OnCardClicked);
        }

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
            onCardSelected?.Invoke(card.Data);
            // Заблокировать выбор карт
        else
            card.Shake();
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
