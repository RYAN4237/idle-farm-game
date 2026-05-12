using UnityEngine;

[CreateAssetMenu(fileName = "UnlockNodeData", menuName = "Farm/Unlock Node Data")]
public class UnlockNodeData : ScriptableObject
{
    public string NodeId;
    public string DisplayName;
    public float PointCost;
    public float MultiplierGranted;
    public string[] PrerequisiteNodeIds = new string[0];
}
