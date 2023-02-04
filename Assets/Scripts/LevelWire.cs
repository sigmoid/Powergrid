using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelWire : MonoBehaviour
{
    public List<SpriteRenderer> SpriteRenderers;

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
        foreach(var SpriteRenderer in SpriteRenderers)
            SpriteRenderer.color = ActiveColor;
        OnActivate.Invoke();
    }

    public void Deactivate()
    {
        IsActive = false;
		foreach (var SpriteRenderer in SpriteRenderers)
			SpriteRenderer.color = InactiveColor;
		OnDeactivate.Invoke();
    }
}
