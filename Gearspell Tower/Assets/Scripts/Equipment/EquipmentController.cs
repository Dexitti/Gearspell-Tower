using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public abstract class EquipmentController : MonoBehaviour
{
    [SerializeField] public EquipmentData data;
    protected GameObject decorationInstance;
    protected Transform towerTransform;
    protected Vector3 detectionOrigin;

    private List<string> appliedUpgradeIds = new();

    protected int currentDamage;
    protected float currentSize;
    protected float currentAttackCooldown;
    protected float currentRange;
    protected int currentProjectileCount;

    public EquipmentData Data => data;
    public int Stage { get; set; } = 0;
    public bool HasAnyUpgrade => appliedUpgradeIds.Count > 0;
    public int ForkChoice { get; set; } = -1;
    public bool HasActiveAbility { get; set; } = false;
    public List<string> GetAppliedUpgradeIds() => appliedUpgradeIds;

    protected virtual void Awake()
    {
        ResetToBase();
    }

    private void ResetToBase()
    {
        currentDamage = Mathf.RoundToInt(data.damage);
        currentAttackCooldown = data.attackCooldown;
        currentRange = data.range;
        currentProjectileCount = data.projectileCount;
        currentSize = data.size;
    }

    protected virtual void OnEnable()
    {
        towerTransform = G.Tower.transform;
        detectionOrigin = towerTransform.position + new Vector3(0, -1.2f, 0);
        GameObject decoration = data.decorationPrefab;
        if (decoration == null) return;

        decorationInstance = Instantiate(decoration, towerTransform);
        
        switch (decoration.name)
        {
            case "Mortars and Parapet":
                Transform roof = towerTransform.Find("Roof");
                if (roof != null)
                    roof.gameObject.SetActive(false);

                Transform shield = towerTransform.Find("Shield(Clone)");
                Transform shieldBorder = shield?.Find("ShieldBorder");
                SpriteRenderer borderRenderer = shieldBorder?.GetComponent<SpriteRenderer>();
                Sprite parapetSprite = Resources.Load<Sprite>($"Arts/Projectiles/Parapet_Shield");

                if (borderRenderer != null && parapetSprite != null)
                    borderRenderer.sprite = parapetSprite;
                break;

            case "Shield":
                Transform shieldBorderTransform = decorationInstance.transform.Find("ShieldBorder");
                SpriteRenderer shieldBorderSprite = shieldBorderTransform?.GetComponent<SpriteRenderer>();
                string spriteName = towerTransform.Find("Mortars and Parapet(Clone)") != null ? "Parapet_Shield" : "Roof_Shield";
                Sprite borderSprite = Resources.Load<Sprite>($"Arts/Projectiles/{spriteName}");

                if (shieldBorderSprite != null && borderSprite != null)
                    shieldBorderSprite.sprite = borderSprite;
                break;

            case "Antenna":
                if (towerTransform.Find("Mortars and Parapet(Clone)") != null)
                {
                    GameObject column = data.projectilesPrefabs[1];
                    if (column != null && column.name == "Antenna column")
                        Instantiate(column, towerTransform);
                }
                break;
        }
    }

    protected virtual void OnDisable()
    {
        if (decorationInstance != null)
            Destroy(decorationInstance);
    }

    private void Start()
    {
        StartCoroutine(AttackManager());
    }

    IEnumerator AttackManager()
    {
        while (true) {
            List<GameObject> enemies = GameObject.FindGameObjectsWithTag("Enemy").ToList();
            enemies.AddRange(GameObject.FindGameObjectsWithTag("FlyingEnemy"));
            if (enemies.Count > 0)
            {
                yield return StartCoroutine(Attack());
                yield return new WaitForSeconds(currentAttackCooldown);
            }
            else yield return new WaitForSeconds(0.1f);
        }
    }

    protected abstract IEnumerator Attack();

    public void AddUpgradeId(string id)
    {
        if (!appliedUpgradeIds.Contains(id))
            appliedUpgradeIds.Add(id);
    }

    /// <summary>
    /// Паттерн расчета урона Modifiers
    /// Сначала Reset - сбрасываем все параметры к базовым
    /// Затем Apply каждый модификатор
    /// </summary>
    public void RefreshStats()
    {
        ResetToBase();
        foreach (var id in appliedUpgradeIds)
            ApplyEffect(id);
    }

    protected abstract void ApplyEffect(string upgradeId);
    protected abstract void ActivateAbility();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellowNice;
        Vector3 prevPoint = detectionOrigin + new Vector3(currentRange, 0, 0);
        int segments = 32;
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * 360f / segments;
            Vector3 point = detectionOrigin + IsometricExtension.IsoVector(angle, currentRange);
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}
