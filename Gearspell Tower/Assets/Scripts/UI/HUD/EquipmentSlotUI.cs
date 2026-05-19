using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum EquipmentSlotState
{
    Locked,
    Empty,
    Equipped
}

public class EquipmentSlotUI : MonoBehaviour
{
    [SerializeField] private Image spriteImage;
    [SerializeField] private Button slotButton;
    [SerializeField] private TextMeshProUGUI buyText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private GameObject selectionBorder;

    [Header("Sprites")]
    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private Sprite lockedSlotSprite;

    private int slotIndex;
    private Action<int> onClick;
    private bool isInteractive;

    public int SlotIndex => slotIndex;

    public void Initialize(int index, Action<int> callback, bool interactive)
    {
        slotIndex = index;
        onClick = callback;
        isInteractive = interactive;

        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.interactable = interactive;
            if (interactive)
                slotButton.onClick.AddListener(() => onClick?.Invoke(slotIndex));
        }
    }

    public void SetState(EquipmentSlotState state, Sprite eqIcon = null, int unlockCost = -1)
    {
        switch (state)
        {
            case EquipmentSlotState.Locked:
                spriteImage.sprite = lockedSlotSprite;
                buyText.text = isInteractive ? G.LocalizationManager.GetText("Buy") : "";
                costText.text = isInteractive && unlockCost > 0 ? unlockCost.ToString() : "";
                break;

            case EquipmentSlotState.Empty:
                spriteImage.sprite = emptySlotSprite;
                buyText.text = "";
                costText.text = "";
                break;

            case EquipmentSlotState.Equipped:
                spriteImage.sprite = eqIcon ?? emptySlotSprite;
                buyText.text = "";
                costText.text = "";
                break;
        }

        if (slotButton != null)
            slotButton.interactable = isInteractive;
    }

    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
            selectionBorder.SetActive(selected);
    }
}