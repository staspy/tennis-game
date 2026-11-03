using UnityEngine;

[System.Serializable]
public class Shot
{
    public float upForce;
    public float hitForce;
}

public class ShotManager : MonoBehaviour
{
    public Shot topSpin;
    public Shot flat;
}