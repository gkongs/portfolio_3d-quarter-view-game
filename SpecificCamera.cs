using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//특별한 카메라 워킹이 필요할 때 사용하는 스크립트
public class SpecificCamera : MonoBehaviour
{
    private Vector3 curreuntTarget, finalTarget;
    private GameObject player;
    public Vector3 pos, startPos;
    float elapsedTime;
    float checkTime;
    int count = 0;
    float plusMinus = 0; // 0 == plus
    public bool isInteract = false;
    Vector3 startTarget;
    GameMgr GameMgr;
    private string playerViewingDirection;
    private Vector3 p0, p1, p2, p3;
    private GameObject characterSettingUI;
    void Start()
    {
        characterSettingUI = GameObject.Find("StatWindow");
        player = GameObject.Find("Player");
        GameMgr = GameObject.Find("GameMgr").GetComponent<GameMgr>();
        pos = this.transform.position - player.transform.position;
        for (int i = 0; i < characterSettingUI.GetComponent<Transform>().childCount; i++)
        {
            if (characterSettingUI.GetComponent<Transform>().GetChild(i).name == "case")
            {
                for (int j = 0; j < characterSettingUI.GetComponent<Transform>().GetChild(i).childCount; j++)
                {
                    characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).GetComponent<RawImage>().enabled = false;
                }
            }
            else if (characterSettingUI.GetComponent<Transform>().GetChild(i).name == "Text")
            {
                for (int j = 0; j < characterSettingUI.GetComponent<Transform>().GetChild(i).childCount; j++)
                {
                    characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).GetComponent<Text>().text = "";
                }
            }
            else
            {
                characterSettingUI.GetComponent<Transform>().GetChild(i).GetComponent<RawImage>().enabled = false;
            }
        }
        characterSettingUI.GetComponent<RawImage>().enabled = false;
        characterSettingUI.GetComponent<Transform>().GetChild(characterSettingUI.GetComponent<Transform>().childCount - 1).gameObject.SetActive(false);
    }
    void Update()
    {
        if (!isInteract)
        {
            this.transform.position = player.transform.position + pos; //curreuntTarget
            curreuntTarget = this.transform.position;
            if (count != 0)
            {
                for (int i = 0; i < characterSettingUI.GetComponent<Transform>().childCount; i++)
                {
                    if (characterSettingUI.GetComponent<Transform>().GetChild(i).name == "case")
                    {
                        for (int j = 0; j < characterSettingUI.GetComponent<Transform>().GetChild(i).childCount; j++)
                        {
                            characterSettingUI.GetComponent<Transform>().GetChild(i).transform.GetChild(j).GetComponent<RawImage>().enabled = false;
                        }
                    }
                    else if (characterSettingUI.GetComponent<Transform>().GetChild(i).name == "Text")
                    {
                        for (int j = 0; j < characterSettingUI.GetComponent<Transform>().GetChild(i).childCount; j++)
                        {
                            if (characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).name == "save")
                                characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).GetComponent<Text>().text = "";
                            else if (characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).name == "cost")
                            {
                                characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).GetComponent<Text>().text = "";
                            }
                            else if (characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).name == "gold")
                            {
                                characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).GetComponent<Text>().text = "";
                            }
                            else
                                characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).GetComponent<Text>().text = "";
                        }
                    }
                    else
                    {
                        characterSettingUI.GetComponent<Transform>().GetChild(i).GetComponent<RawImage>().enabled = false;
                    }
                }
                characterSettingUI.GetComponent<RawImage>().enabled = false;
                characterSettingUI.GetComponent<Transform>().GetChild(characterSettingUI.GetComponent<Transform>().childCount - 1).gameObject.SetActive(false);
            }
            count = 0;
        }
        else
        {
            Move();
        }
    }
    // 상점 UI로 이동 
    private void Move()
    {
        if (count == 0)
        {
            startTarget = curreuntTarget;
            player.GetComponent<Transform>().rotation = Quaternion.Euler(0, -45, 0);
            playerViewingDirection = player.GetComponent<Character>().viewingDirection;
            if (playerViewingDirection == "f")
            {
                finalTarget = new Vector3(player.transform.position.x - 4, player.transform.position.y, player.transform.position.z - 5);
            }
            elapsedTime += Time.deltaTime;
            if (((startTarget.z + 10) / 20) < 0)
                plusMinus = 1;
            if (elapsedTime > checkTime + 0.1f)
            {
                count++;
                if (playerViewingDirection == "f")
                {
                    if (count <= 5)
                    {
                        if (plusMinus == 0)
                            curreuntTarget = new Vector3(curreuntTarget.x + 2.0f, curreuntTarget.y - ((startTarget.y - 5) / 10),
                                   curreuntTarget.z + ((startTarget.z + 10) / 10));
                        else
                            curreuntTarget = new Vector3(curreuntTarget.x + 2.0f, curreuntTarget.y - ((startTarget.y - 5) / 10),
                                curreuntTarget.z - ((startTarget.z + 10) / 10));
                    }
                    else if (count > 5 && count <= 10)
                    {
                        if (plusMinus == 0)
                            curreuntTarget = new Vector3(curreuntTarget.x - 2.5f, curreuntTarget.y - ((startTarget.y - 5) / 10),
                                  curreuntTarget.z + ((startTarget.z + 10) / 10));
                        else
                            curreuntTarget = new Vector3(curreuntTarget.x - 2.5f, curreuntTarget.y - ((startTarget.y - 5) / 10),
                                  curreuntTarget.z - ((startTarget.z + 10) / 10));

                    }
                    if (count == 20)
                    {
                        characterSettingUI.GetComponent<RawImage>().enabled = true;
                        for (int i = 0; i < characterSettingUI.GetComponent<Transform>().childCount; i++)
                        {
                            if (characterSettingUI.GetComponent<Transform>().GetChild(i).name == "case")
                            {
                                for (int j = 0; j < characterSettingUI.GetComponent<Transform>().GetChild(i).childCount; j++)
                                {
                                    characterSettingUI.GetComponent<Transform>().GetChild(i).transform.GetChild(j).GetComponent<RawImage>().enabled = true;
                                }
                            }
                            else if (characterSettingUI.GetComponent<Transform>().GetChild(i).name == "Text")
                            {
                                for (int j = 0; j < characterSettingUI.GetComponent<Transform>().GetChild(i).childCount; j++)
                                {
                                    if (characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).name == "save")
                                        characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).GetComponent<Text>().text = "S A V E";
                                    else if (characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).name == "cost")
                                    {
                                        characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).GetComponent<Text>().text = "" + GameMgr.GetCostValue(0);
                                    }
                                    else if (characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).name == "gold")
                                    {
                                        characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).GetComponent<Text>().text = "" + GameObject.Find("GameMgr").GetComponent<GameMgr>().GetPlayerGold();
                                    }
                                    else
                                        characterSettingUI.GetComponent<Transform>().GetChild(i).GetChild(j).GetComponent<Text>().text = "" + GameObject.Find("GameMgr").GetComponent<GameMgr>().GetAddValue(j);
                                }
                            }
                            else
                            {
                                characterSettingUI.GetComponent<Transform>().GetChild(i).GetComponent<RawImage>().enabled = true;
                            }
                        }
                        characterSettingUI.GetComponent<RawImage>().enabled = true;
                        characterSettingUI.GetComponent<RawImage>().color = new Color(194, 194, 194, 1);
                        characterSettingUI.GetComponent<Transform>().GetChild(characterSettingUI.GetComponent<Transform>().childCount - 1).gameObject.SetActive(true);
                    }
                    checkTime = elapsedTime;
                }
            }
            this.transform.LookAt(finalTarget);
            this.transform.position = Vector3.Lerp(this.transform.position, curreuntTarget, 0.02f);
        }
    }
}

