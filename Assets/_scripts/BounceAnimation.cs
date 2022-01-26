using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class BounceAnimation : MonoBehaviour {
    [SerializeField] private float bounceSpeed = 5;
    [SerializeField] private float bounceAmplitude = 0.6f;
    [SerializeField] private float rotationSpeed = 50;

    private float startingHeight;
    private float timeOffset;

    private void Start() {
        startingHeight = transform.localPosition.y;
        timeOffset = Random.value * Mathf.PI * 2;
    }

    private void Update() {
        var finalHeight = startingHeight + Mathf.Sin(Time.time * bounceSpeed + timeOffset) * bounceAmplitude;
        var position = transform.localPosition;
        position.y = finalHeight;
        transform.localPosition = position;

        transform.Rotate(rotationSpeed * Time.deltaTime * Vector3.up);
    }
}
