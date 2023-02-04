using Powergrid;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class LevelManager : MonoBehaviour
{
    public GameObject LinePrefab;
    public Transform LineTransform;
    public float LinePadding;
    public UnityEvent OnLevelSolved;
    public UnityEvent OnLevelUnSolved;

    private List<PowerSlot> _slots;
    private Dictionary<PowerSlot, List<GameObject>> _lines;

    private const int MAX_DEPTH = 3;
    private bool _isSolved;
    private bool _lastIsSolved;

    // Start is called before the first frame update
    void Start()
    {
        GetSlots();
        BuildLineDictionary();
        ClearGrid();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGrid();

        if (_isSolved && !_lastIsSolved)
            OnLevelSolved.Invoke();
        else if (!_isSolved && _lastIsSolved)
            OnLevelUnSolved.Invoke();
        _lastIsSolved = _isSolved;
    }

    private void GetSlots()
    {
        _slots = GetComponentsInChildren<PowerSlot>().ToList();
    }


    #region Grid Logic

    private void ClearGrid()
    {
        foreach (var slot in _slots)
        {
            if (!slot.IsStart && !slot.IsGoal)
                slot.gameObject.SetActive(false);
        }

        foreach (var lineGroup in _lines.Values)
        {
            foreach (var line in lineGroup)
            {
                line.gameObject.SetActive(false);
            }
        }
    }

    private void ResetGrid()
    {
        foreach (var slot in _slots)
        {
            if (slot.gameObject.activeInHierarchy)
                slot.gameObject.SendMessage("Deactivate");
        }

        foreach (var lineGroup in _lines.Values)
        {
            foreach (var line in lineGroup)
            {
                if (line.activeInHierarchy)
                    line.SendMessage("Deactivate");
            }
        }
    }

    private void UpdateGrid()
    {
        ResetGrid();
        _isSolved = true;

        foreach (var node in _slots)
        {
            if (node.GetPower() > 0)
            {
                Distribute(node, node.GetPower(), new HashSet<PowerSlot>());
            }
            if (node.IsGoal && node.GetPower() == 0)
            {
                _isSolved = false;
            }
        }
    }

    private void Distribute(PowerSlot node, int power, HashSet<PowerSlot> seen)
    {
        if (seen.Contains(node) || power <= 0)
            return;

        seen.Add(node);

        if (_lines.ContainsKey(node))
        {
            foreach (var line in _lines[node])
            {
                line.gameObject.SetActive(true);
                line.gameObject.SendMessage("Activate");
            }
        }

        foreach (var childNode in node.OutgoingConnections)
        {
            childNode.gameObject.SetActive(true);
            childNode.gameObject.SendMessage("Activate");
            Distribute(childNode, power - 1, seen);
        }
    }

    public bool CanAddPower(PowerSlot slot)
    {
        if (slot == null) return false;
        if (!_slots.Contains(slot)) return false;

        if (slot.IsAnchor)
            return true;

        if (CanAnchorFind(slot))
            return true;

        return false;
    }

    public bool CanRemovePower(PowerSlot slot)
    {
        if (slot == null) return false;
        if (!_slots.Contains(slot)) return false;

        slot.Removed = true;

        foreach (var node in _slots)
        {
            if (node == slot)
                continue;

            if (node.GetPower() > 0 && !CanAnchorFind(node))
            {
                slot.Removed = false;
                return false;
            }
        }

        slot.Removed = false;
        return true;
    }

    public bool CanAnchorFind(PowerSlot destination)
    {
        if (destination.IsAnchor && !destination.Removed)
            return true;

		foreach (var slot in _slots)
		{
			if (slot == destination)
				continue;

			if (slot.IsAnchor && !slot.Removed)
			{
				if (Search(slot, destination, slot.GetPower()+1, new HashSet<PowerSlot>()))
				{
					return true;
				}
			}
		}

		return false;
	}

    public bool Search(PowerSlot start, PowerSlot destination, int maxDepth, HashSet<PowerSlot> seenSlots)
    {
        if (maxDepth <= 0)
            return false;

        if (start == destination)
            return true;

        if (seenSlots.Contains(start))
            return false;

        seenSlots.Add(start);

        var currentPower = start.Removed ? 0 : start.GetPower();


        foreach (var connect in start.OutgoingConnections)
        {
            if (Search(connect, destination, Mathf.Max(maxDepth - 1, currentPower), seenSlots))
                return true;
        }

        return false;
    }

	#endregion

	#region Line Generation

	public void GenerateLines()
    {
        ClearExistingLines();
		CreateLines();
	}

    private void ClearExistingLines()
    {
        foreach (var item in LineTransform.GetComponentsInChildren<Transform>())
        {
            if(item != LineTransform)
                GameObject.DestroyImmediate(item.gameObject);
        }
       
        _lines = new Dictionary<PowerSlot, List<GameObject>>();
	}

    private void CreateLines()
    {
		_slots = GetComponentsInChildren<PowerSlot>().ToList();

		foreach (var slot in _slots)
		{
            _lines[slot] = new List<GameObject>();

            foreach (var connect in slot.OutgoingConnections)
            {
                _lines[slot].Add(CreateLine(slot, connect));
            }
		}
	}

    private GameObject CreateLine(PowerSlot pointA, PowerSlot pointB)
    {
        Vector3 midPoint = Vector3.Lerp(pointA.transform.position, pointB.transform.position, 0.5f);
        GameObject res = Instantiate(LinePrefab,LineTransform);
        res.name = "line," + pointA.name + "," + pointB.name;

        var lineRenderer = res.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            Vector3 startingPos = pointA.transform.position;
            Vector3 startingDir = pointB.transform.position - pointA.transform.position;
            startingDir.Normalize();

            lineRenderer.SetPosition(0, startingPos + startingDir * LinePadding);

			startingPos = pointB.transform.position;
			startingDir = pointA.transform.position - pointB.transform.position;
			startingDir.Normalize();

			lineRenderer.SetPosition(1, startingPos + startingDir * LinePadding);
        }

        return res;
    }

    private void BuildLineDictionary()
    {
        _lines = new Dictionary<PowerSlot, List<GameObject>>();

		foreach (var item in LineTransform.GetComponentsInChildren<Transform>())
		{
            if (item == LineTransform)
                continue;

            var names = item.gameObject.name.Split(',');
            
            if (names.Length < 2)
                continue;

            var startGameObject = GameObject.Find(names[1]).GetComponent<PowerSlot>();

            if (startGameObject == null)
                return;

            if (!_lines.ContainsKey(startGameObject))
                _lines[startGameObject] = new List<GameObject>();

            _lines[startGameObject].Add(item.gameObject);
		}
	}

	#endregion
}


