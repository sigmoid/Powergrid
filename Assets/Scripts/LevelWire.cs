using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelWire : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;

    public Color ActiveColor;
    public Color InactiveColor;

    public UnityEvent OnActivate;
    public UnityEvent OnDeactivate;

    public bool IsActive;

    // Start is called before the first frame update
    void Start()
    {
        if (IsActive)
            Activate();
        else
            Deactivate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Activate()
    {
        IsActive = true;
        SpriteRenderer.color = ActiveColor;
        OnActivate.Invoke();
    }

    public void Deactivate()
    {
        IsActive = false;
        SpriteRenderer.color = InactiveColor;
        OnDeactivate.Invoke();
    }
}
