using System;
using IncredibleAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class Floor : MonoBehaviour
{
    [SerializeField]
    private bool shouldRotateRandomly = false;


    [SerializeField, Dropdown("_directions"), ShowIf("shouldRotateRandomly")]
    private Vector3 direction;

    [SerializeField, MinMaxSlider(-5.0f, 5.0f), ShowIf("shouldRotateRandomly")]
    private Vector2 rotateSpeedRange;
    private float defaultRotationSpeed;
    private float rotateSpeed;

    private DropdownList<Vector3> _directions = new()
    {
        { "Up", Vector3.up },
        { "Down", Vector3.down },
        { "Left", Vector3.left },
        { "Right", Vector3.right },
        { "Forward", Vector3.forward },
        { "Back", Vector3.back }
    };

    private void OnEnable()
    {
        int r =  Random.Range(0, 4);
        if(r == 0){ shouldRotateRandomly = true; }
    }

    private void Update()
    {
        if (shouldRotateRandomly)
        {
            transform.RotateAround(transform.position, direction, rotateSpeed * Time.deltaTime);
        }
    }

    public void EnableRotation(bool _randomizeSpeed = false)
    {
        if (_randomizeSpeed)
        {
            rotateSpeed = Random.Range(rotateSpeedRange.x, rotateSpeedRange.y);
        }
        shouldRotateRandomly = true;
    }
}
