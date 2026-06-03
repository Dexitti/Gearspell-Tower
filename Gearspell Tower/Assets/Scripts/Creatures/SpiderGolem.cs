using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SpiderGolem : WalkingCreature
{
    [SerializeField] private float invincibleDuration = 1.6f;
    [SerializeField] private float invincibleCooldown = 5f;
    private bool isInvincible = false;
    private float nextInvincibleTime;

    protected override void Update()
    {
        base.Update();

        if (!isInvincible && Time.time >= nextInvincibleTime)
        {
            StartCoroutine(ActivateInvincibility());
            nextInvincibleTime = Time.time + invincibleCooldown;
        }
    }

    private IEnumerator ActivateInvincibility()
    {
        isInvincible = true;
        sprite.color = new Color(0.8f, 0.8f, 0.8f, 0.2f);

        health.OnBeforeTakeDamage += OnBeforeTakeDamage;
        yield return new WaitForSeconds(invincibleDuration);
        health.OnBeforeTakeDamage -= OnBeforeTakeDamage;

        G.AudioManager.PlaySFX("Protocol appear", 0.4f);
        sprite.color = Color.white;
        isInvincible = false;
    }

    private void OnBeforeTakeDamage(int damage, Action<int> modifyDamage)
    {
        if (isInvincible)
        {
            modifyDamage(0);

            StartCoroutine(FlashBlock());
        }
    }

    private IEnumerator FlashBlock()
    {
        sprite.color = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        yield return new WaitForSeconds(0.1f);
        if (isInvincible)
            sprite.color = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        else
            sprite.color = Color.white;
    }
}