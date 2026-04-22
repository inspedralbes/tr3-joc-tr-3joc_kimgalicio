using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class CoopCamera : MonoBehaviour
{
    [Header("Tracking Settings")]
    public float smoothTime = 0.3f;
    public float minOrthographicSize = 5f;
    public float maxOrthographicSize = 12f;
    public float zoomLimiter = 50f;
    public Vector3 offset = new Vector3(0, 0, -10f);

    private Camera _camera;
    private Vector3 _velocity;
    private GameManager _gameManager;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _gameManager = FindFirstObjectByType<GameManager>();
    }

    private void LateUpdate()
    {
        if (_gameManager == null || _gameManager.Entities == null || _gameManager.Entities.Length == 0)
            return;

        List<GameObject> activeEntities = new List<GameObject>();
        foreach (var entity in _gameManager.Entities)
        {
            if (entity != null && entity.activeInHierarchy)
            {
                // Solo seguir si no es espectador (si el GameManager tiene esa info)
                if (_gameManager.GameState != null && !_gameManager.GameState.Spectators.Contains(entity.name))
                {
                    activeEntities.Add(entity);
                }
            }
        }

        if (activeEntities.Count == 0)
            return;

        Move(activeEntities);
        Zoom(activeEntities);
    }

    private void Move(List<GameObject> targets)
    {
        Vector3 centerPoint = GetCenterPoint(targets);
        Vector3 newPosition = centerPoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref _velocity, smoothTime);
    }

    private void Zoom(List<GameObject> targets)
    {
        float newZoom = Mathf.Lerp(minOrthographicSize, maxOrthographicSize, GetGreatestDistance(targets) / zoomLimiter);
        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, newZoom, Time.deltaTime);
    }

    private float GetGreatestDistance(List<GameObject> targets)
    {
        var bounds = new Bounds(targets[0].transform.position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].transform.position);
        }

        return bounds.size.x > bounds.size.y ? bounds.size.x : bounds.size.y;
    }

    private Vector3 GetCenterPoint(List<GameObject> targets)
    {
        if (targets.Count == 1)
        {
            return targets[0].transform.position;
        }

        var bounds = new Bounds(targets[0].transform.position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].transform.position);
        }

        return bounds.center;
    }
}
