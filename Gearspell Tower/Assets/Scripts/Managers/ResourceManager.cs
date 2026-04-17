using UnityEngine;

//Singleton
public class ResourceManager : MonoBehaviour
{
    [SerializeField] private int startingGears = 0;
    [SerializeField] private int startingUpgradePoints = 0;

    private int _gears;
    public int Gears
    {
        get => _gears;
        set
        {
            _gears = value;
            G.EventManager?.TriggerGearsChanged(_gears);
        }
    }

    private void Start()
    {
        ResetResources();
        if (G.EventManager != null)
        {
            G.EventManager.OnEnemyKilled += OnEnemyKilled;
        }
    }

    private void OnDestroy()
    {
        if (G.EventManager != null)
        {
            G.EventManager.OnEnemyKilled -= OnEnemyKilled;
        }
    }

    private void OnEnemyKilled(Creature enemy)
    {
        // TODO: Получать награду из данных врага
        AddGears(10);
    }

    public void ResetResources()
    {
        Gears = startingGears;
    }

    public void SetGears(int amount)
    {
        Gears = amount;
    }

    public void AddGears(int amount) => Gears += amount;

    public bool SpendGears(int amount)
    {
        if (Gears >= amount)
        {
            Gears -= amount;
            return true;
        }
        return false;
    }
}