using System;
using UnityEngine;

public class FireDrillShot : MonoBehaviour
{
    [SerializeField] private float riseSpeed = 15f;
    [SerializeField] private float forwardSpeed = 3f;
    private float tiltRight = 1f;
    private float timer;
    [SerializeField] private float lifetime = 1f;

    private void Start()
    {
        timer = 0;
        if (gameObject.transform.rotation.z < 0f) tiltRight = -1f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        transform.position += new Vector3(-tiltRight * forwardSpeed * Time.deltaTime, riseSpeed * Time.deltaTime, 0);
        forwardSpeed += 0.1f;
        transform.Rotate(0f, 0f, tiltRight * Mathf.Atan2(forwardSpeed, riseSpeed) * Mathf.Rad2Deg * Time.deltaTime);
        if (timer/lifetime > 1f) Destroy(gameObject);
    }
}
