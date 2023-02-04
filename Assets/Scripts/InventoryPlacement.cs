using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryPlacement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        newPos.z = 10;
        transform.position = newPos;
    }
}
