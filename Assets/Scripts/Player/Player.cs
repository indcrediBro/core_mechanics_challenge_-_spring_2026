using System;
using IncredibleAttributes;
using UnityEngine;
using PlayerAbility;

[Serializable]
public class PlayerContext
{
    public Player player;
    public Rigidbody rigidbody;
    public Transform camera;
    public Transform groundCheck;

    public float yaw;
}

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerContext playerContext;
    [SerializeField, Expandable] private PlayerAbilityBase[] abilities;

    private void Awake()
    {
        foreach (PlayerAbilityBase ability in abilities)
        {
            ability.OnAwake(playerContext);
        }
    }

    private void Start()
    {
        foreach (PlayerAbilityBase ability in abilities)
        {
            ability.OnStart(playerContext);
        }
    }

    private void Update()
    {
        foreach (PlayerAbilityBase ability in abilities)
        {
            ability.OnUpdate(playerContext);
        }
    }

    private void FixedUpdate()
    {
        foreach (PlayerAbilityBase ability in abilities)
        {
            ability.OnFixedUpdate(playerContext);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        foreach (PlayerAbilityBase ability in abilities)
        {
            ability.OnTriggerColliderEnter(other, playerContext);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        foreach (PlayerAbilityBase ability in abilities)
        {
            ability.OnCollisionColliderEnter(other, playerContext);
        }
    }
}
