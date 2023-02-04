using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Lock : MonoBehaviour
{
    public Transform PointA;
    public Transform PointB;
    public LineRenderer LineRenderer;

    public Color ActiveColor;
    public Color InactiveColor;

    public bool IsLocked = true;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        LineRenderer.SetPosition(0, PointA.position);
        LineRenderer.SetPosition(1, PointB.position);
    }

    public void Activate()
    {
        IsLocked = false;
        LineRenderer.startColor = ActiveColor;
        LineRenderer.endColor = ActiveColor;
    }

    public void Deactivate()
    {
        IsLocked = true;
        LineRenderer.startColor = InactiveColor;
        LineRenderer.endColor = InactiveColor;
    }
}
