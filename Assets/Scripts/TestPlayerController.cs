using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class TestPlayerController : MonoBehaviour
{
    private Animator _animator;
    private bool _hasAnimator;
    private bool isWalking;

    private bool isNewWeapon;

    //private Queue<GameObject> weaponsQueue = new Queue<GameObject>();
    //private int maxWeapons = 3; // weapons 배열의 크기

    bool _spaceDown;

    GameObject nearObject;



    private void Awake()
		{
			_hasAnimator = TryGetComponent(out _animator);
        }

    // Update is called once per frame
    void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);

        Move();
        GetInput();
        //Debug.Log("무기 개수: " + weaponsQueue.Count);
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
            Debug.Log("멈출게~");
        }
    }

    void GetInput()
    {
        _spaceDown = Input.GetKeyDown(KeyCode.Space);
    }

    public void SetPlayerWalking(bool isWalking)
    {

        if (_hasAnimator && isWalking)
		{
			_animator.SetTrigger("isWalk");
		}
    }

    void OnTriggerStay(Collider other) {
        if(other.tag == "Weapon")
        {
            nearObject = other.gameObject;
            Debug.Log("아이템에 닿았습니다.");

            if(_spaceDown && nearObject != null)
            {
                //AcquireWeapon(nearObject);
                Destroy(nearObject);
                Debug.Log("아이템을 획득하였습니다.");
                isNewWeapon = true;
                SetGetNewWeapon(isNewWeapon);
            }

            else
            {
                isNewWeapon = false;
                SetGetNewWeapon(isNewWeapon);
            }
        }
    }

    public void SetGetNewWeapon(bool isNewWeapon)
    {
        if(_hasAnimator && isNewWeapon)
        {
            _animator.SetTrigger("isGetNewWeapon");
            Debug.Log("Knife는 놓을게~");
        }
    }


    void OnTriggerExit(Collider other) {
        if(other.tag == "Weapon")
        {
            nearObject = null;
        }
    }

    /*void AcquireWeapon(GameObject weapon)
    {
        weaponsQueue.Enqueue(weapon);

        if(weaponsQueue.Count > maxWeapons)
        {
            GameObject oldestWeapon = weaponsQueue.Dequeue();
        }
    }*/
}
