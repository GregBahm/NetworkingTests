using System;
using UnityEngine;

[Serializable]
struct HandData
{
    public Vector3 WristPos { get; }
    public Quaternion[] JointRotations { get; }
}
