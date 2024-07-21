using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using JetBrains.Annotations;

public class TestPlayerController : NetworkBehaviour
{
    private Animator _animator;
    private bool _hasAnimator;
    private bool isWalking;

    private void Awake()
		{
			_hasAnimator = TryGetComponent(out _animator);
        }

    // Update is called once per frame
    void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);

        Move();
    }


    void Move()
    {
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);

        if(x != 0.0f || z != 0.0f)
        {
            isWalking = true;
            SetPlayerWalking(isWalking);
        }
        else
        {
            isWalking = false;
            SetPlayerWalking(isWalking);
        }
    }

    public void SetPlayerWalking(bool isWalking)
    {

        if (_hasAnimator && isWalking)
		{
			_animator.SetTrigger("isWalk");
		}
    }
}
