using System;
using GameData;
using IncredibleAttributes;
using UnityEngine;
using PlayerAbility;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerContextData playerContext;
    [SerializeField, Expandable, ] private PlayerAbilityBase[] abilities;

    public UnityEvent onBounce;
    public UnityEvent onDeath;
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
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        foreach (PlayerAbilityBase ability in abilities)
        {
            ability.OnUpdate(playerContext);
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        foreach (PlayerAbilityBase ability in abilities)
        {
            ability.OnFixedUpdate(playerContext);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        foreach (PlayerAbilityBase ability in abilities)
        {
            ability.OnTriggerColliderEnter(other, playerContext);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        foreach (PlayerAbilityBase ability in abilities)
        {
            ability.OnCollisionColliderEnter(other, playerContext);
        }
    }
}
