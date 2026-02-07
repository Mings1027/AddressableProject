using System;
using UnityEngine;

public class RotateObjects : MonoBehaviour
{
    [SerializeField] private float speed = 10;

    private void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime * speed);
    }
}