using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameMgr : MonoBehaviour
{
    public GameObject player;
    public int playerHP, playerSP ,playerAttackPower, playerHPStat, playerSPStat, playerAttackPowerStat;
    public int gold;
    public int addHP,addSP,addAttackPower, costHP, costSP, costAttackPower;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        playerHP = playerSP = playerHPStat = playerSPStat = 100;
        playerAttackPower = playerAttackPowerStat = 10;
        gold = 200;
        addHP = addSP = addAttackPower = 0;
        costHP = costSP = costAttackPower = 10;
        player.GetComponent<Character>().attackPower = playerAttackPower;
    }

    public int GetPlayerGold()
    {
        return gold;
    }
    public void SetPlayerGold(int gold)
    {
        this.gold += gold;
    }
    public int GetAddValue(int index)
    {
        if (index == 0)
            return addHP;
        else if (index == 1)
            return addSP;
        else
            return addAttackPower;
    }
    public int GetCostValue(int index)
    {
        if (index == 0)
            return costHP;
        else if (index == 1)
            return costSP;
        else
            return costAttackPower;
    }
    public void SetAddValue(int index, int value)
    {
        if (index == 0) this.addHP = value;
        else if (index == 1) this.addSP = value;
        else this.addAttackPower = value;
    }
    public void SetCostValue(int index, int cost)
    {
        if (index == 0) this.costHP = cost;
        else if (index == 1) this.costSP = cost;
        else  this.costAttackPower = cost;
    }
    public void SetPlayerHP(int HP)
    {
        playerHP = HP;
        player.GetComponent<Character>().HP = playerHP;
    }
    public void SetPlayerSP(int SP)
    {
        playerSP = SP;
        player.GetComponent<Character>().SP = playerSP;
    }
    public void SetPlayerAttackPower(int attackPower)
    {
        playerAttackPower = attackPower;
        player.GetComponent<Character>().attackPower = playerAttackPower;
    }
    public int GetPlayerHP() { return playerHP; }
    public int GetPlayerSP() { return playerSP; }
    public void SetPlayerStat(int addHP, int addSP , int addAttackPower)
    {
        playerHPStat += addHP;
        playerSPStat += addSP;
        playerAttackPowerStat += addAttackPower;
    }
    public int GetPlayerHPStat() { return playerHPStat; }
    public int GetPlayerSPStat() { return playerSPStat; }
    public int GetPlayerAttackPowerStat() { return playerAttackPowerStat; }
    public void ReflectStat()
    {
        SetPlayerHP(GetPlayerHPStat());
        SetPlayerSP(GetPlayerSPStat());
        SetPlayerAttackPower(GetPlayerAttackPowerStat());
    }
    // Update is called once per frame
    void Update()
    {
        player.GetComponent<Character>().attackPower = playerAttackPower;
        if (Input.GetKeyDown(KeyCode.Escape))
            if(SceneManager.GetActiveScene().name == "Intro")
            SceneManager.LoadScene("Title");
        else if (Input.GetKeyDown(KeyCode.M))
        {
            SceneManager.LoadScene("Stage_3");
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            SceneManager.LoadScene("Stage_2");
        }
    }
}
