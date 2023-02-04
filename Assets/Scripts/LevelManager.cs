using Powergrid;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;


public class ConnectionLineSegment
{
    public PowerSlot StartSlot;
    public PowerSlot EndSlot;
    public GameObject GameObject;
    public Vector3 StartPosition;
    public Vector3 EndPosition;

    public bool IsActive;

    public List<Lock> ConnectedLocks;
    public List<ConnectionLineSegment> CompetingLineSegments;
}

public class LevelManager : MonoBehaviour
{
    public GameObject LinePrefab;
    public Transform LineTransform;
    public float LinePadding;
    public UnityEvent OnLevelSolved;
    public UnityEvent OnLevelUnSolved;
    public GameObject InactiveMask;

    public bool IsActive;

    private List<PowerSlot> _slots;
    private Dictionary<PowerSlot, List<ConnectionLineSegment>> _lines;

    private const int MAX_DEPTH = 3;
    private bool _isSolved;
    private bool _lastIsSolved;

    // Start is called before the first frame update
    void Start()
    {
        GetSlots();
        GenerateLines();
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
                line.GameObject.SetActive(false);
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
            for (int i = 0; i < lineGroup.Count; i++)
            {
                var line = lineGroup[i];
                if (line.GameObject.activeInHierarchy)
                {
                    line.IsActive = false;
                    if (line.CompetingLineSegments != null)
                    {
                        foreach (var otherLine in line.CompetingLineSegments)
                        {
                            otherLine.GameObject.SetActive(true);
                        }
                    }
                    line.GameObject.SendMessage("Deactivate");
                }
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
            for (int i = 0; i < _lines[node].Count; i++)
            {
                var line = _lines[node][i];
                line.GameObject.SetActive(true);
                line.EndSlot.gameObject.SetActive(true);

                bool hasLock = false;
                if (line.ConnectedLocks != null)
                {
                    foreach (var lck in line.ConnectedLocks)
                    {
                        if (lck.IsLocked)
                            hasLock = true;
                    }
                }

                bool hasConflict = false;
                if (line.CompetingLineSegments != null)
                {
                    foreach (var competitor in line.CompetingLineSegments)
                    {
                        if (competitor.IsActive)
                            hasConflict = true;
                    }
                }

                if (!hasConflict && !hasLock)
                {
                    line.IsActive = true;
                    line.GameObject.SendMessage("Activate");
                    line.EndSlot.gameObject.SendMessage("Activate");
                    line.EndSlot.AddPowerFrom(node);
                    Distribute(_lines[node][i].EndSlot, power - 1, seen);
                }
            }
        }

        foreach (var childNode in node.OutgoingConnections)
        {
            childNode.gameObject.SetActive(true);

            childNode.gameObject.SendMessage("Activate");
            Distribute(childNode, power - 1, seen);
        }
    }

    public bool CanAddPower(PowerSlot slot, int power)
    {
        if (!IsActive) return false;
        if (slot == null) return false;
        if (!_slots.Contains(slot)) return false;

        if (HasOverlaps(slot, power))
            return false;

        if (CanAnchorFind(slot))
            return true;

        return false;
    }

    public bool CanRemovePower(PowerSlot slot)
    {
        if (!IsActive) return false;
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
                if (Search(slot, destination, slot.GetPower() + 1, new HashSet<PowerSlot>()))
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


        foreach (var connect in _lines[start])
        {
            // Ignore locked lines
            bool isLocked = false;
            if (connect.ConnectedLocks?.Count > 0)
            {
                foreach (var lck in connect.ConnectedLocks)
                {
                    if (lck.IsLocked)
                        isLocked = true;
                }
            }

            // Ignore any lines that can't be crossed due to an overlap
            bool hasConflict = false;
            if (connect.CompetingLineSegments != null)
            {
                foreach (var line in connect.CompetingLineSegments)
                {
                    if (line.IsActive)
                    {
                        hasConflict = true;
                        break;
                    }
                }
            }

            if (!hasConflict && ! isLocked && Search(connect.EndSlot, destination, Mathf.Max(maxDepth - 1, currentPower), seenSlots))
                return true;
        }

        return false;
    }

    public bool HasOverlaps(PowerSlot slot, int power, HashSet<PowerSlot> seenSlots = null)
    {
        if (power <= 0)
            return false;

        if (seenSlots == null)
            seenSlots = new HashSet<PowerSlot>();

        if (seenSlots.Contains(slot))
            return false;
        seenSlots.Add(slot);
        var currentPower = power;


        foreach (var connect in _lines[slot])
        {
            // Find any lines that can't be crossed due to an overlap
            if (connect.CompetingLineSegments != null)
            {
                foreach (var line in connect.CompetingLineSegments)
                {
                    if (line.IsActive)
                    {
                        return true;
                    }
                }
            }

            if (HasOverlaps(connect.EndSlot, currentPower - 1, seenSlots))
                return true;
        }

        return false;
    }

    #endregion

    #region Line Generation

    public void GenerateLines()
    {
        ClearExistingLines();
        var lines = CreateLines();
        PopulateCompetingLines(lines);
        PopulateLocks(lines);
    }

    private void ClearExistingLines()
    {
        foreach (var item in LineTransform.GetComponentsInChildren<Transform>())
        {
            if (item != LineTransform)
                GameObject.DestroyImmediate(item.gameObject);
        }

        _lines = new Dictionary<PowerSlot, List<ConnectionLineSegment>>();
    }

    private List<ConnectionLineSegment> CreateLines()
    {
        _slots = GetComponentsInChildren<PowerSlot>().ToList();
        var res = new List<ConnectionLineSegment>();

        foreach (var slot in _slots)
        {
            _lines[slot] = new List<ConnectionLineSegment>();

            foreach (var connect in slot.OutgoingConnections)
            {
                var line = CreateLine(slot, connect);
                _lines[slot].Add(line);
                res.Add(line);
            }
        }
        return res;
    }

    private ConnectionLineSegment CreateLine(PowerSlot pointA, PowerSlot pointB)
    {
        ConnectionLineSegment res = new ConnectionLineSegment();
        res.StartSlot = pointA;
        res.EndSlot = pointB;

        Vector3 midPoint = Vector3.Lerp(pointA.transform.position, pointB.transform.position, 0.5f);
        res.GameObject = Instantiate(LinePrefab, LineTransform);
        res.GameObject.transform.position = midPoint;
        Vector3 dir = pointB.transform.position - pointA.transform.position;
        dir.Normalize();
        float theta = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        res.GameObject.transform.rotation = Quaternion.AngleAxis(theta ,Vector3.forward);
        res.GameObject.name = "line," + pointA.name + "," + pointB.name;

        var lineRenderer = res.GameObject.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            Vector3 startingPos = pointA.transform.position;
            Vector3 startingDir = pointB.transform.position - pointA.transform.position;
            startingDir.Normalize();

            lineRenderer.SetPosition(0, startingPos + startingDir * LinePadding);
            res.StartPosition = startingPos + startingDir * LinePadding;

            startingPos = pointB.transform.position;
            startingDir = pointA.transform.position - pointB.transform.position;
            startingDir.Normalize();

            lineRenderer.SetPosition(1, startingPos + startingDir * LinePadding);
            res.EndPosition = startingPos + startingDir * LinePadding;
        }

        return res;
    }

    public void PopulateCompetingLines(List<ConnectionLineSegment> lines)
    {
        for (int i = 0; i < lines.Count - 1; i++)
        {
            for (int j = i + 1; j < lines.Count; j++)
            {
                if (lines[i].StartPosition == lines[j].EndPosition && lines[j].StartPosition == lines[i].StartPosition)
                    continue;

                if (DoLinesIntersect(lines[i].StartPosition, lines[i].EndPosition, lines[j].StartPosition, lines[j].EndPosition))
                {
                    if (lines[i].CompetingLineSegments == null)
                        lines[i].CompetingLineSegments = new List<ConnectionLineSegment>();
                    if (lines[j].CompetingLineSegments == null)
                        lines[j].CompetingLineSegments = new List<ConnectionLineSegment>();

                    lines[i].CompetingLineSegments.Add(lines[j]);
                    lines[j].CompetingLineSegments.Add(lines[i]);
                }
            }
        }
    }

    public void PopulateLocks(List<ConnectionLineSegment> lines)
    {
        var locks = GetComponentsInChildren<Lock>();
		for (int i = 0; i < lines.Count - 1; i++)
		{
			for (int j = 0; j < locks.Length; j++)
			{
				if (DoLinesIntersect(lines[i].StartPosition, lines[i].EndPosition, locks[j].PointA.position, locks[j].PointB.position))
				{
					if (lines[i].ConnectedLocks == null)
						lines[i].ConnectedLocks = new List<Lock>();

                    lines[i].ConnectedLocks.Add(locks[j]);
				}
			}
		}
	}

    //private void BuildLineDictionary()
    //{
    //    _lines = new Dictionary<PowerSlot, List<GameObject>>();

    //    foreach (var item in LineTransform.GetComponentsInChildren<Transform>())
    //    {
    //        if (item == LineTransform)
    //            continue;

    //        var names = item.gameObject.name.Split(',');

    //        if (names.Length < 2)
    //            continue;

    //        var startGameObject = GameObject.Find(names[1]).GetComponent<PowerSlot>();

    //        if (startGameObject == null)
    //            return;

    //        if (!_lines.ContainsKey(startGameObject))
    //            _lines[startGameObject] = new List<GameObject>();

    //        _lines[startGameObject].Add(item.gameObject);
    //    }
    //}

    #endregion

    #region Activate / Deactivate

    public void Activate()
    {
        IsActive = true;
        InactiveMask.SetActive(false);
    }

    public void Deactivate()
    {
        IsActive = false;
        InactiveMask.SetActive(true);
    }

	#endregion

	#region Util

	/// <summary>
	/// Taken from https://forum.unity.com/threads/line-intersection.17384/
	/// </summary>
	/// <param name="p1"></param>
	/// <param name="p2"></param>
	/// <param name="p3"></param>
	/// <param name="p4"></param>
	/// <returns></returns>
	bool DoLinesIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
	{

		Vector2 a = p2 - p1;
		Vector2 b = p3 - p4;
		Vector2 c = p1 - p3;

		float alphaNumerator = b.y * c.x - b.x * c.y;
		float alphaDenominator = a.y * b.x - a.x * b.y;
		float betaNumerator = a.x * c.y - a.y * c.x;
		float betaDenominator = a.y * b.x - a.x * b.y;

		bool doIntersect = true;

		if (alphaDenominator == 0 || betaDenominator == 0)
		{
			doIntersect = false;
		}
		else
		{

			if (alphaDenominator > 0)
			{
				if (alphaNumerator < 0 || alphaNumerator > alphaDenominator)
				{
					doIntersect = false;

				}
			}
			else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator)
			{
				doIntersect = false;
			}

			if (doIntersect && betaDenominator > 0) {
				if (betaNumerator < 0 || betaNumerator > betaDenominator)
				{
					doIntersect = false;
				}
			} else if (betaNumerator > 0 || betaNumerator < betaDenominator)
			{
				doIntersect = false;
			}
		}

		return doIntersect;
	}

	#endregion
}


