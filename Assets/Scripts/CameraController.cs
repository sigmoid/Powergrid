using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float MovementSpeed = 10.0f;

    public float ZoomSpeed = 1.0f;
    public float MaxZoom = 10.0f;
    public float MinZoom = 1.0f;

    public Camera Camera;

    private float _currentZoom = 0;

    // Start is called before the first frame update
    void Start()
    {
        _currentZoom = Camera.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 frameMovement = new Vector3();

        frameMovement.x = Input.GetAxis("Horizontal");
        frameMovement.y = Input.GetAxis("Vertical");

        transform.position += frameMovement * Time.deltaTime * MovementSpeed;

        float zoom = Input.GetAxis("Mouse ScrollWheel");

        _currentZoom += zoom * Time.deltaTime * ZoomSpeed;
        _currentZoom = Mathf.Clamp(_currentZoom, MinZoom, MaxZoom);

        Camera.orthographicSize = _currentZoom;
    }
}
