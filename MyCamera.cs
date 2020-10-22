using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCamera : MonoBehaviour
{
    public GameObject target;
    private GameObject player;
    GameObject balrog , Boss3;
    private Vector3 pos;
    Vector3 shakePos;
    int numberOfRefeat = 0;
    RaycastHit[] rayHit , prevRayHit;
    
    void Start()
    {
        player = GameObject.Find("Player");
        balrog = GameObject.Find("Balrog");
        DistanceSetting();
    }
    void Update()
    {
        if (GameObject.Find("Balrog") == true && !player.GetComponent<Character>().isDie)
        {
            if (balrog.GetComponent<Balrog>().isHowling || balrog.GetComponent<Balrog>().isTwoHandAttack || balrog.GetComponent<Balrog>().shakeCamera)
            {
                if (numberOfRefeat == 0)
                {
                    numberOfRefeat++;
                    StartCoroutine(Shake(1f, 30));
                }
                this.transform.position = target.transform.position + pos + shakePos;
            }
            else
            {
                this.transform.position = target.transform.position + pos;
            }
        }
        else this.transform.position = target.transform.position + pos;
    }
    public void DistanceSetting()
    {
        if (target == balrog)
        {
            pos = this.transform.position - player.transform.position;//임의로 준 카메라와 보스의 거리.
        }
        else
        {
            pos = this.transform.position - player.transform.position;
        }
    }
    public IEnumerator Shake(float duration, int repeat) //카메라 지진 효과 (발록)
    {
        float _delay = duration / repeat;
        int _count = 0;
        float _x = 0, _y = 0;
        float _maxX = 0.5f, _maxY = 0.5f;
        while (_count != repeat)
        {
            _count++;
            if (_count % 2 == 0)
            {
                _x = _maxX - ((_maxX / (repeat / duration)) * _count);
                _y = _maxY - ((_maxY / (repeat / duration)) * _count);
                shakePos = new Vector3(_x, 0, 0);
            }
            else
            {
                _x = _maxX - ((_maxX / (repeat / duration)) * _count);
                _y = _maxY - ((_maxY / (repeat / duration)) * _count);
                shakePos -= new Vector3(_x, 0, 0);
            }
            yield return new WaitForSeconds(_delay);
        }
        yield return new WaitForSeconds(3f);
        numberOfRefeat = 0;
    }
}