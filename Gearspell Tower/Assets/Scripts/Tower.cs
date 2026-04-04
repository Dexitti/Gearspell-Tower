using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float detectionRadius = 5f; //?
    private HealthComponent healthComponent;
    [SerializeField] private int regeneration = 1;

    public Vector3 Position => transform.position;
    
    private List<Creature> detectedEnemies = new List<Creature>(); // может быть обнаружение врагов вынести сюда и просто вызывать!

    // Модификаторы из систем
    //private float globalDamageMultiplier = 1f;
    //private float globalAttackSpeedMultiplier = 1f;
    //private float globalActiveCooldownReduction = 0f;
    //private float globalProjSize = 1f;
    //private float globalConstructCooldownReduction = 0f;

    private void Awake()
    {
        // Получаем ссылки на системные менеджеры-сингтоны
        //gameManager = GameManager.Instance;
        //uiManager = UIManager.Instance;

        healthComponent = GetComponent<HealthComponent>();
    }

    private void Start()
    {
        StartCoroutine(Regenerate());
    }

    IEnumerator Regenerate()
    {
        while (true)
        {
            healthComponent.Heal(regeneration);
            yield return new WaitForSeconds(1f);
        }
    }


    private void OnDestroy()
    {
        // Отписываемся от событий
        // if (UpgradeSystem.Instance != null)
        // {
        //     UpgradeSystem.Instance.OnGlobalUpgradeChanged -= UpdateGlobalModifiers;
        // }
    }
}
