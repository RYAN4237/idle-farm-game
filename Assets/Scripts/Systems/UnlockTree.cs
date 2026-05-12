using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[DefaultExecutionOrder(-100)]
public class UnlockTree : MonoBehaviour
{
    public static UnlockTree Instance { get; private set; }

    [SerializeField] private UnlockNodeData[] _nodes = new UnlockNodeData[0];

    private Dictionary<string, UnlockNodeData> _nodeMap = new();
    private HashSet<string> _unlockedIds = new();

    public event Action<string> OnNodeUnlocked;
    public event Action OnUnlockTreeStateChanged;
    public event Action OnUnlockTreeRestored;

    public enum NodeState { Locked, Available, Unlocked }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _nodeMap.Clear();
        foreach (var node in _nodes)
        {
            if (node == null) continue;
            _nodeMap[node.NodeId] = node;
        }

        if (_nodeMap.Count == 0)
            Debug.LogWarning("[UnlockTree] Node array is empty — no nodes to unlock.");
    }

    public NodeState ComputeNodeState(string nodeId)
    {
        if (!_nodeMap.TryGetValue(nodeId, out var node)) return NodeState.Locked;
        if (_unlockedIds.Contains(nodeId)) return NodeState.Unlocked;

        foreach (var prereqId in node.PrerequisiteNodeIds)
        {
            if (!_unlockedIds.Contains(prereqId))
                return NodeState.Locked;
        }

        float balance = ResourceSystem.Instance != null ? ResourceSystem.Instance.FocusPoints : 0f;
        return balance >= node.PointCost ? NodeState.Available : NodeState.Locked;
    }

    public bool TryUnlockNode(string nodeId)
    {
        if (ComputeNodeState(nodeId) != NodeState.Available) return false;

        var node = _nodeMap[nodeId];
        var rs = ResourceSystem.Instance;
        if (rs == null || !rs.SpendFocusPoints(node.PointCost)) return false;

        _unlockedIds.Add(nodeId);
        OnNodeUnlocked?.Invoke(nodeId);
        OnUnlockTreeStateChanged?.Invoke();
        return true;
    }

    public bool IsNodeUnlocked(string nodeId) => _unlockedIds.Contains(nodeId);

    public IReadOnlyList<UnlockNodeData> GetAllNodes() => _nodes;

    public IEnumerable<UnlockNodeData> GetUnlockedNodes()
    {
        return _unlockedIds
            .Where(id => _nodeMap.ContainsKey(id))
            .Select(id => _nodeMap[id]);
    }

    public float ComputeGlobalMultiplier()
    {
        float sum = 1f;
        foreach (var id in _unlockedIds)
        {
            if (_nodeMap.TryGetValue(id, out var node))
                sum += node.MultiplierGranted;
        }
        return sum;
    }

    // Save/Load
    public string[] GetUnlockedNodeIds() => _unlockedIds.ToArray();

    public void ApplySaveData(string[] unlockedNodeIds)
    {
        _unlockedIds.Clear();
        if (unlockedNodeIds == null) return;

        foreach (var nodeId in unlockedNodeIds)
        {
            if (_nodeMap.ContainsKey(nodeId))
                _unlockedIds.Add(nodeId);
            else
                Debug.LogWarning($"[UnlockTree] Unknown nodeId '{nodeId}' skipped during load.");
        }

        OnUnlockTreeRestored?.Invoke();
        OnUnlockTreeStateChanged?.Invoke();
    }

    void OnDestroy()
    {
        OnNodeUnlocked = null;
        OnUnlockTreeStateChanged = null;
        OnUnlockTreeRestored = null;
        if (Instance == this) Instance = null;
    }
}
