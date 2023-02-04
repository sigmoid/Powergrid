using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    public Lock ConnectedLock;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPowered()
    {
        ConnectedLock.Activate();
    }

    public void OnUnpowered()
    {
        ConnectedLock.Deactivate();
    }
}
