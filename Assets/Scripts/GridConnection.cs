using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridConnection : MonoBehaviour
{
    public LineRenderer Renderer;
    public Color ActiveColor;
    public Color InactiveColor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Activate()
    {
        Renderer.startColor = ActiveColor;
        Renderer.endColor = ActiveColor;
    }

	public void Deactivate() 
    {
        Renderer.startColor = InactiveColor;
        Renderer.endColor = InactiveColor;
	}
}
