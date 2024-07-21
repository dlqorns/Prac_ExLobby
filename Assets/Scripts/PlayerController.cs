using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode.Components;
using JetBrains.Annotations;

public class PlayerController : NetworkBehaviour
{
    public GameObject CameraHolder;
    private NetworkAnimator _netAnimator;
    private bool _hasNetAnimator;

    private void Awake()
		{
			_hasNetAnimator = TryGetComponent(out _netAnimator);
        }

    // Update is called once per frame
    void Update()
    {
        if (!IsLocalPlayer)
        {
            CameraHolder.gameObject.SetActive(false);
            return;
        }

        _hasNetAnimator = TryGetComponent(out _netAnimator);

        Move();
    }


    void Move()
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);

        if (_hasNetAnimator)
		{
            if (!IsLocalPlayer)
            {
                return;
            }
			_netAnimator.Animator.SetBool("isWalk", x != 0.0f || z != 0.0f);
		}
    }
}
