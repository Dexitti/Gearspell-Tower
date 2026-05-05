using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class GameLoopManager : MonoBehaviour
{
    public Action<int> OnWaveStarting;
    public Action<int> OnWaveCompleted;
    public Action<DialogData> OnDialogShow;

    [SerializeField] private EquipmentManager equipmentManager;
    [SerializeField] private UpgradeSystem upgradeSystem;
    [SerializeField] private float timeBetweenWaves = 3f;
    [SerializeField] private WaveData[] waves;

    private int currentWaveIndex = 0;
    private Coroutine gameLoopCoroutine;

    public void SetCurrentWave(int wave) => currentWaveIndex = wave - 1;
    public int GetCurrentWaveNumber() => currentWaveIndex + 1;
    public int GetTotalWaves() => waves.Length;

    private void Awake()
    {
        G.GameLoopManager = this;
        waves = Resources.LoadAll<WaveData>("Data/WavesData");
    }

    private void Start()
    {
        gameLoopCoroutine = StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
            WaveData wave = GetCurrentWave();
            if (wave == null) yield break;

            // Диалог перед волной
            var dialogsBeforeWave = wave.waveDialogs.Where(d => d.appearance == -1);
            if (dialogsBeforeWave.Any())
            {
                foreach (var dialog in dialogsBeforeWave)
                {
                    OnDialogShow?.Invoke(dialog);
                    yield return new WaitForSeconds(dialog.duration);
                }
            }

            // Запуск волны
            OnWaveStarting?.Invoke(currentWaveIndex + 1);
            G.EventManager?.TriggerWaveStarted(currentWaveIndex + 1);

            G.SpawnManager?.StartWave(wave);
            Debug.Log($"{wave.waveName} началась");

            // Ждём пока все враги будут побеждены
            yield return new WaitWhile(() => G.SpawnManager != null && G.SpawnManager.HasActiveEnemies);
            // Когда осталось мало врагов (пусть 15%) и это не последняя волна, надо показывать индикатор состава следующей волны
            yield return new WaitForSeconds(0.5f);

            // Волна завершена
            G.EventManager?.TriggerWaveCompleted(currentWaveIndex + 1);
            G.ResourceManager?.AddGears(wave.gearsReward);

            // Разблокировка снаряжения
            if (wave.equipmentUnlocks != null && wave.equipmentUnlocks.Length > 0)
            {
                foreach (var eq in wave.equipmentUnlocks)
                {
                    G.SaveManager?.UnlockEquipment(eq.equipmentName);
                    Debug.Log($"[GameLoop] Unlocked: {eq.equipmentName}");
                }

                // Оповещаем UI (можно показать уведомление)
                G.EventManager?.TriggerEquipmentUnlocked(wave.equipmentUnlocks);
            }

            // Диалог после волны
            var dialogsAfterWave = wave.waveDialogs.Where(d => d.appearance == 1);
            if (dialogsAfterWave.Any())
            {
                foreach (var dialog in dialogsAfterWave)
                {
                    OnDialogShow?.Invoke(dialog);
                    yield return new WaitForSeconds(dialog.duration);
                }
            }

            // Предлагаем выбор предметов (пока нет)
            //if (wave.upgradeChoices != null && wave.upgradeChoices.Length > 0)
            //{
            //    yield return upgradeSystem.OfferChoices(wave.upgradeChoices);
            //}

            currentWaveIndex++;
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private WaveData GetCurrentWave()
    {
        int index = currentWaveIndex;
        if (index >= waves.Length)
        {
            index = waves.Length - 1;
        }

        return waves[index];
    }

    public void SkipWave()
    {
        //G.SpawnManager?.KillAllEnemies();
    }
}
