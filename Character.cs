using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Character : MonoBehaviour
{
    public int HP; public int SP;
    Slider hpBar;
    Slider spBar;

    private float dietextA = 1;

    public float walkSpeed;
    public float dashSpeed;
    public bool dashCoolTime = false;
    public bool isDash = false;
    public bool isDefense = false;
    public float dashElapsedTime;
    private float attackDuration;
    public int criticalPercentage; // criticalDadage도 있는지 물어보자.
    private int criticalCheckValue;
    public int attackPower;
    public GameObject damageText;
    private Vector3[] basicAttackRange;
    public string viewingDirection;
    public bool defending = false;
    private float elapsedTime = 0.0f; private int SEC = 60;
    public bool isAttack = false; bool isSkill = false;
    private int pressA = 0; int[] numberOfAnimFucRepeat = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public bool isKnockBack = false;
    public bool checkCiritical = false;

    private float damageTextYPos = 0;

    public float curAttackPower = 0;
    float moveDirX;
    float moveDirZ;
    Vector3 rotHorizon;


    public GameObject AttackTarget; //여러마리일 경우 바꾸어야 함!
    public Vector3 AttackTartgetPos;
    private GameObject[] enemy;
    private Rigidbody rigid;
    private Animator anim;
    private GameObject balrog;
    private GameObject weapon;
    private RaycastHit rayHit;
    public bool isInteract = false;
    public GameObject P_Attack1, P_Attack2;
    private GameObject AttackEffect, currentAttackEffect;
    public GameObject E_Attack;
    private GameObject specificCamera;
    private GameObject mainCamera;
    int numberOfP_Attack1, numberOfP_Attack2, effectCount = 0; //후에 배열이나 리스트 작업
    float fixedCameraPosX, fixedCameraPosZ;
    public bool isStun = false;
    public bool isDie = false;
    private string currentAttackAnimName;
    private bool[] skillCoolTime;
    private string currentAnimFucName;
    private GameObject currentSkill;
    private Animator StunAnim;
    public GameObject dieText;
    public GameObject castingEffect;
    public bool isStop = false;
    public AudioClip[] sound;
    public GameObject keyHelp;
    bool OnHelp = false;
    GameMgr GameMgr;
    Vector3 attackEffectPos;
    public int runSoundCount = 0, enemyHitSoundCount = 0;
    GameObject createEffect;
    List<GameObject> effectList = new List<GameObject>();
    enum Sound
    {
        Attack1,
        Attack2,
        Attack3,
        Dash,
        EnemyHit,
        Run,
        SwordAttack1,
        SwordAttack2,
        Sting,
        EmptyAttack1,
        EmptyAttack2,
        InteractiveStatue,
        Defense,
        Buff,
        Casting,
    }
    enum thisAnimOfNumber
    {
        Attack,
        Defense,
        Skill_Sting,
        Skill_Sting_Attack,
        Skill_CrescentCut,
        Skill_Howling,
        Skill_Kick,
        Skill_PowerKick,
        Skill_DropKick,
        Skill_TruningAttack,
    }
    void Awake()
    {
        GameMgr = GameObject.Find("GameMgr").GetComponent<GameMgr>();
        HP = GameMgr.GetPlayerHP();
        SP = GameMgr.GetPlayerSP();
        hpBar = GameObject.Find("HPBar").GetComponent<Slider>();
        spBar = GameObject.Find("SPBar").GetComponent<Slider>();
        hpBar.value = HP;
        spBar.value = SP;
        rigid = GetComponent<Rigidbody>();
        balrog = GameObject.Find("Balrog");
        weapon = GameObject.Find("WeaponCol");
        specificCamera = GameObject.Find("specificCamera");
        mainCamera = GameObject.Find("Camera");
        anim = GetComponent<Animator>();
        skillCoolTime = new bool[10];
        StunAnim = GameObject.Find("Stun").GetComponent<Animator>();
        AttackEffect = GameObject.Find("Attack");
        viewingDirection = "f";
        currentAttackEffect = AttackEffect.GetComponent<Transform>().GetChild(0).gameObject;
        attackEffectPos = AttackEffect.GetComponent<Transform>().position;
        StartCoroutine(SPHEAL());
    }
    // 공격을 받을 시.

    IEnumerator EnemyHitSoundPlay()
    {
        enemyHitSoundCount = 1;
        transform.GetComponent<AudioSource>().PlayOneShot(sound[(int)Sound.EnemyHit], 1);
        yield return new WaitForSeconds(sound[(int)Sound.EnemyHit].length);
        enemyHitSoundCount = 0;
    }
    public void SetHP(int increase, int decrease)
    {
        HP += increase;

        if (!isDefense)
            HP -= decrease;
        if (HP > 100)
        {
            HP = 100;
        }
        if (HP < hpBar.value)
            if (enemyHitSoundCount == 0)
                StartCoroutine(EnemyHitSoundPlay());
        hpBar.value = HP;
    }
    public int GetHP()
    {
        return HP;
    }
    public void SetSP(int increase, int decrease)
    {
        SP += increase;
        SP -= decrease;
        if (SP >= 100)
        {
            SP = 100;
        }
        else if (SP <= 0)
        {
            SP = 0;
        }
        spBar.value = SP;
    }
    public int GetSP()
    {
        return SP;
    }
    public void SetAttackPower()
    {
    }
    public float GetAttackPower()
    {
        return curAttackPower;
    }
    public float CheckCritical(float power)
    {
        if (criticalCheckValue <= criticalPercentage)
        {
            power = power * 2;
            checkCiritical = true;
            return power;
        }
        else
        {
            checkCiritical = false;
            return power;
        }
    }
    private void TimeFuc()
    {
        elapsedTime += Time.deltaTime;
    }
    Vector3 v1, v2;
    void Update()
    {
        attackPower = GameMgr.GetPlayerAttackPowerStat(); // 임시
        enemy = GameObject.FindGameObjectsWithTag("enemy");
        AttackEffect.GetComponent<Transform>().position = transform.position;
        TimeFuc();
        State();
        CurrentBehavior();
        InteractiveObjectHit();
        if (!isStun && !isInteract && !isDie && !isDash && !isKnockBack && !isStop)
        {
            Move();
            BasicBehavior();
            UseSkill();
        }
    }
    private void Move()
    {
        if (!anim.GetBool("Attack1") && !anim.GetBool("Attack2") && !anim.GetBool("Stun"))
        {
            if (!isAttack && !isSkill && !isDefense && !isDash && !isStop)
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)
                || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
                {
                    moveDirX = Input.GetAxisRaw("Horizontal");
                    moveDirZ = Input.GetAxisRaw("Vertical");

                    Vector3 _moveHorizontal = transform.right * moveDirX;
                    Vector3 _moveVertical = transform.forward * moveDirZ;
                    Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * walkSpeed;
                    Vector3 _velocityDash = (_moveHorizontal + _moveVertical).normalized * dashSpeed;

                    rotHorizon = moveDirZ * Vector3.forward + moveDirX * Vector3.right;
                    this.transform.rotation = Quaternion.LookRotation(rotHorizon);

                    this.transform.Translate(Vector3.forward * walkSpeed * Time.smoothDeltaTime);

                    PlayAnim("Run");
                    if (runSoundCount == 0)
                        StartCoroutine(RunningSound());
                }
                else
                    StopAnim("Run");
        }
    }
    IEnumerator RunningSound()
    {
        runSoundCount = 1;
        transform.GetComponent<AudioSource>().PlayOneShot(sound[(int)Sound.Run], 0.5f);
        yield return new WaitForSeconds(sound[(int)Sound.Run].length);
        runSoundCount = 0;
    }
    void CurrentBehavior()
    {
        if (isAttack)
            EndOfAnim(currentAttackAnimName);
        if (isSkill)
            EndOfAnim(currentAnimFucName);
        if (isDefense)
            EndOfAnim("Defense");
        if (HP < 0 && !isDie)
            Die();
    }
    public void PlayAnim(String name)
    {
        anim.SetBool(name, true);
    }
    public void StopAnim(String name)
    {
        anim.SetBool(name, false);
    }
    Vector3 effectPos;
    Quaternion effectRot;
    public void ShowParticle(GameObject effect)
    {
        effect.GetComponent<ParticleSystem>().Play();
        currentSkill = effect;

    }
    public void ShowAttackEffect(GameObject effect, int attackNumber)
    {
        for (int j = 0; j < effect.GetComponent<Transform>().GetChild(attackNumber).childCount; j++)
            for (int i = 0; i < effect.GetComponent<Transform>().GetChild(0).GetChild(j).childCount; i++)
            {
                effect.GetComponent<Transform>().GetChild(0).GetChild(j).GetChild(i).GetComponent<ParticleSystem>().Play();
            }
    }
    IEnumerator SPHEAL()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);
            if (!isDie && !isStop)
                SetSP(1, 0);
        }
    }
    private void State()
    {

        hpBar.value = HP;
        spBar.value = SP;
        // 방향 확인
        if (!isAttack)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                if (Input.GetKey(KeyCode.LeftArrow)) { viewingDirection = "fl"; currentAttackEffect = AttackEffect.GetComponent<Transform>().GetChild(7).gameObject; }
                else if (Input.GetKey(KeyCode.RightArrow)) { viewingDirection = "fr"; currentAttackEffect = AttackEffect.GetComponent<Transform>().GetChild(1).gameObject; }
                else
                {
                    viewingDirection = "f";
                    currentAttackEffect = AttackEffect.GetComponent<Transform>().GetChild(0).gameObject;
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                if (Input.GetKey(KeyCode.LeftArrow)) { viewingDirection = "bl"; currentAttackEffect = AttackEffect.GetComponent<Transform>().GetChild(5).gameObject; }
                else if (Input.GetKey(KeyCode.RightArrow)) { viewingDirection = "br"; currentAttackEffect = AttackEffect.GetComponent<Transform>().GetChild(3).gameObject; }
                else
                {
                    viewingDirection = "b"; currentAttackEffect = AttackEffect.GetComponent<Transform>().GetChild(4).gameObject;
                }
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (Input.GetKey(KeyCode.UpArrow)) { viewingDirection = "fl"; currentAttackEffect = AttackEffect.GetComponent<Transform>().GetChild(7).gameObject; }
                else if (Input.GetKey(KeyCode.DownArrow)) { viewingDirection = "bl"; currentAttackEffect = AttackEffect.GetComponent<Transform>().GetChild(5).gameObject; }
                else
                {
                    viewingDirection = "l"; currentAttackEffect = AttackEffect.GetComponent<Transform>().GetChild(6).gameObject;
                }
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                if (Input.GetKey(KeyCode.UpArrow)) { viewingDirection = "fr"; currentAttackEffect = AttackEffect.GetComponent<Transform>().GetChild(1).gameObject; }
                else if (Input.GetKey(KeyCode.DownArrow)) { viewingDirection = "br"; currentAttackEffect = AttackEffect.GetComponent<Transform>().GetChild(3).gameObject; }
                else
                {
                    viewingDirection = "r";
                    currentAttackEffect = AttackEffect.GetComponent<Transform>().GetChild(2).gameObject;
                }
            }
        }
        //공격 파티클 실행 횟수 제한.
        if (!isAttack) { numberOfP_Attack1 = 0; numberOfP_Attack2 = 0; }

    }
    private void DirRotSet()
    {
        moveDirX = Input.GetAxisRaw("Horizontal");
        moveDirZ = Input.GetAxisRaw("Vertical");
        rotHorizon = moveDirZ * Vector3.forward + moveDirX * Vector3.right;
        this.transform.rotation = Quaternion.LookRotation(rotHorizon);
    }
    private void BasicBehavior()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            if(!OnHelp)
            {
                OnHelp = true;
                keyHelp.gameObject.SetActive(true);
            }
            else
            {
                OnHelp = false;
                keyHelp.SetActive(false);
            }
        }
        //A,D,SPACEBAR
        //공격
        if (Input.GetKeyDown(KeyCode.A) && !isSkill && GetSP() >= 10)
        {
            if (!isAttack)
            {
                SetSP(0, 10);
                isAttack = true;
                currentAttackAnimName = "Attack1";
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space) && !dashCoolTime && !isSkill && !isInteract && !isDefense && GetSP() >= 30)
        {
            SetSP(0, 30);
            transform.GetComponent<AudioSource>().PlayOneShot(sound[(int)Sound.Dash], 0.5f);
            anim.SetTrigger("Dash");
            dashCoolTime = true;
            StartCoroutine(IsDash(0.3f));
            StartCoroutine(DashCoolTime(5f));
            ShowParticle(GameObject.Find("Dash"));
            if (Input.GetKey(KeyCode.UpArrow))
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    DirRotSet();
                    StartCoroutine(DashPower(0.01f, 15, Vector3.forward));
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    DirRotSet();
                    StartCoroutine(DashPower(0.01f, 15, Vector3.forward));
                }
                else
                {
                    DirRotSet();
                    StartCoroutine(DashPower(0.01f, 15, Vector3.forward));
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    DirRotSet();
                    StartCoroutine(DashPower(0.01f, 15, Vector3.forward));
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    DirRotSet();
                    StartCoroutine(DashPower(0.01f, 15, Vector3.forward));
                }
                else
                {
                    DirRotSet();
                    StartCoroutine(DashPower(0.01f, 15, Vector3.forward));
                }
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {

                this.transform.rotation = Quaternion.LookRotation(rotHorizon);
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    DirRotSet();
                    StartCoroutine(DashPower(0.01f, 15, Vector3.forward));
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    DirRotSet();
                    StartCoroutine(DashPower(0.01f, 15, Vector3.forward));
                }
                else
                {
                    DirRotSet();
                    StartCoroutine(DashPower(0.01f, 15, Vector3.forward));
                }
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    DirRotSet();
                    StartCoroutine(DashPower(0.01f, 15, Vector3.forward));
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    DirRotSet();
                    StartCoroutine(DashPower(0.01f, 15, Vector3.forward));
                }
                else
                {
                    DirRotSet();
                    StartCoroutine(DashPower(0.01f, 15, Vector3.forward));
                }
            }
            //스킬도중 사용시 스킬관련 변수 초기화

            if (isAttack)
                StopAnim(currentAttackAnimName);
            isAttack = false;
            isDefense = false;
            //attack 부분
            currentAttackAnimName = "Attack1";
            numberOfAnimFucRepeat[(int)thisAnimOfNumber.Attack] = 0;
            numberOfP_Attack1 = 0;
            numberOfP_Attack2 = 0;
        }
    }
    public GameObject erdas;
    public void Attack(bool comboAttack, int xDamage)
    {
        if (Physics.Raycast(new Vector3(this.transform.position.x, this.transform.position.y + 1, this.transform.position.z), this.transform.forward, out rayHit, 5))
        {
            if (rayHit.transform.tag == "enemy")
            {
                criticalCheckValue = UnityEngine.Random.Range(1, 101);
                if (SceneManager.GetActiveScene().name == "Stage_1" || SceneManager.GetActiveScene().name == "Stage_1_1")
                {
                    AttackTarget = enemy[0];
                    curAttackPower = CheckCritical(UnityEngine.Random.Range(attackPower - 5, attackPower)) * xDamage;
                    AttackTarget.gameObject.GetComponent<Balrog>().SetHP(curAttackPower);
                }
                else if (SceneManager.GetActiveScene().name == "Stage_2")
                {
                    if (erdas.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Groggy"))
                        curAttackPower = CheckCritical(UnityEngine.Random.Range(attackPower - 5, attackPower)) * xDamage;
                    else if (erdas.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Crouch") && rayHit.transform.name == "Erdas")
                        curAttackPower = 1;
                    else
                        curAttackPower = CheckCritical(UnityEngine.Random.Range(attackPower - 5, attackPower)) * xDamage;
                    if (rayHit.transform.name == "Erdas")
                    {
                        AttackTarget = enemy[0];
                        curAttackPower = CheckCritical(UnityEngine.Random.Range(attackPower - 5, attackPower)) * xDamage;
                        AttackTarget.gameObject.GetComponent<Erdas>().SetHP(curAttackPower);
                    }
                    else if (rayHit.transform.position == enemy[1].transform.position)
                    {
                        AttackTarget = enemy[1];
                        curAttackPower = CheckCritical(UnityEngine.Random.Range(attackPower - 5, attackPower)) * xDamage;
                        AttackTarget.gameObject.GetComponent<Eyes>().SetHP(curAttackPower);

                    }
                    else if (rayHit.transform.position == enemy[2].transform.position)
                    {
                        AttackTarget = enemy[2];
                        curAttackPower = CheckCritical(UnityEngine.Random.Range(attackPower - 5, attackPower)) * xDamage;
                        AttackTarget.gameObject.GetComponent<Eyes>().SetHP(curAttackPower);

                    }
                    else if (rayHit.transform.position == enemy[3].transform.position)
                    {
                        AttackTarget = enemy[3];
                        curAttackPower = CheckCritical(UnityEngine.Random.Range(attackPower - 5, attackPower)) * xDamage;
                        AttackTarget.gameObject.GetComponent<Eyes>().SetHP(curAttackPower);

                    }
                    else if (rayHit.transform.position == enemy[4].transform.position)
                    {
                        AttackTarget = enemy[4];
                        curAttackPower = CheckCritical(UnityEngine.Random.Range(attackPower - 5, attackPower)) * xDamage;
                        AttackTarget.gameObject.GetComponent<Eyes>().SetHP(curAttackPower);

                    }
                    else
                        AttackTarget = enemy[0];
                }
                else if (SceneManager.GetActiveScene().name == "Stage_3")
                {
                    AttackTarget = enemy[0];
                    curAttackPower = CheckCritical(UnityEngine.Random.Range(attackPower - 5, attackPower)) * xDamage;
                    AttackTarget.gameObject.GetComponent<ThreeStageBoss>().SetHP(curAttackPower);
                }
                transform.GetComponent<AudioSource>().PlayOneShot(sound[(int)Sound.SwordAttack2], 1);
                if (!comboAttack)
                    AttackTartgetPos = new Vector3(AttackTarget.transform.position.x + UnityEngine.Random.Range(-1.3f, -1.1f)/*우선 이렇게 x축 조금씩만 움직이게..*/,
                        AttackTarget.transform.position.y, AttackTarget.transform.position.z);
                else
                    AttackTartgetPos = new Vector3(AttackTarget.transform.position.x + UnityEngine.Random.Range(-1.3f, -1.1f)/*우선 이렇게 x축 조금씩만 움직이게..*/,
                        AttackTarget.transform.position.y + damageTextYPos, AttackTarget.transform.position.z);

                Instantiate(damageText, AttackTartgetPos, Quaternion.identity);
            }
        }
    }
    IEnumerator DashPower(float delay, int power, Vector3 dir)
    {
        int time = 0;
        while (time != 30)
        {
            yield return new WaitForSeconds(0.01f);
            this.transform.Translate(dir * power * 2 * Time.smoothDeltaTime);
            time++;
        }
    }
    IEnumerator IsDash(float duration)
    {
        isDash = true; PlayAnim("Run");
        yield return new WaitForSeconds(duration);
        isDash = false;
    }
    IEnumerator DashCoolTime(float coolTime)
    {
        yield return new WaitForSeconds(coolTime);
        dashCoolTime = false;
    }
    float waitStingAttackTime;
    int numOfStingAtttack = 0;
    private void UseSkill()
    {
        //Q,W,E,R
        if (!isSkill)
        {
            if (Input.GetKeyDown(KeyCode.Q) && !skillCoolTime[(int)thisAnimOfNumber.Skill_Sting] && GetSP() >= 40 &&
                anim.GetCurrentAnimatorStateInfo(0).normalizedTime>0.05f)
            {
                StartCoroutine(SkillCoolTime(thisAnimOfNumber.Skill_Sting, 5.0f));

                currentAnimFucName = "Skill_Sting";
                isSkill = true;
                SetSP(0, 40);
                waitStingAttackTime = elapsedTime;
                StartCoroutine(WaitForStingTime());
            }
            else if (Input.GetKeyDown(KeyCode.W) && !skillCoolTime[(int)thisAnimOfNumber.Defense] && GetSP() >= 20)
            {
                StartCoroutine(SkillCoolTime(thisAnimOfNumber.Defense, 7.0f));
                currentAnimFucName = "Defense";
                isDefense = true;
                isSkill = true;
                SetSP(0, 20);
            }
            else if (Input.GetKeyDown(KeyCode.E) && !skillCoolTime[(int)thisAnimOfNumber.Skill_PowerKick] && GetSP() >= 50)
            {
                StartCoroutine(SkillCoolTime(thisAnimOfNumber.Skill_PowerKick, 7.0f));
                currentAnimFucName = "Skill_PowerKick";
                isSkill = true;
                SetSP(0, 50);
            }
            else if (Input.GetKeyDown(KeyCode.R) && !skillCoolTime[(int)thisAnimOfNumber.Skill_Howling] && GetSP() >= 70)
            {
                StartCoroutine(SkillCoolTime(thisAnimOfNumber.Skill_Howling, 60f));
                currentAnimFucName = "Skill_Howling";
                isSkill = true;
                SetSP(0, 70);
            }
        }
    }
    IEnumerator WaitForStingTime()
    {
        while (true)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                currentAnimFucName = "Skill_Sting";
            }
            else
            {
                for (int i = 0; i < castingEffect.transform.childCount; i++)
                    castingEffect.transform.GetChild(i).GetComponent<ParticleSystem>().Stop();
                if (elapsedTime - waitStingAttackTime < 0.5f)
                    numOfStingAtttack = 1;
                else if (elapsedTime - waitStingAttackTime < 1.3f)
                    numOfStingAtttack = 3;
                else
                    numOfStingAtttack = 7;
                currentAnimFucName = "Skill_Sting_Attack";
                break;
            }
            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator SkillCoolTime(thisAnimOfNumber name, float time)
    {
        skillCoolTime[(int)name] = true;
        yield return new WaitForSeconds(time);
        skillCoolTime[(int)name] = false;
    }
    bool EndOfAnim(string name)
    {
        if (name == "Attack1")
        {
            if (numberOfAnimFucRepeat[(int)thisAnimOfNumber.Attack] == 0)
            {
                StopAnim("Run");
                PlayAnim(name);
                numberOfAnimFucRepeat[(int)thisAnimOfNumber.Attack] = 1;
            }
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(name))
            {
                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.99f / anim.GetCurrentAnimatorStateInfo(0).speed + 0.2f) //0.2f만큼 여유를 줌.
                {
                    if (numberOfP_Attack1 == 0)
                    {
                        numberOfP_Attack1++;
                        transform.GetComponent<AudioSource>().PlayOneShot(sound[(int)Sound.Attack1], 0.5f);
                        AttackEffect.GetComponent<Transform>().position = attackEffectPos + transform.position;
                        ShowAttackEffect(currentAttackEffect, 0);

                        Attack(false , 1);
                    }
                    if (Input.GetKeyDown(KeyCode.A) && GetSP() >= 20)
                    {
                        SetSP(0, 20);
                        StopAnim(name); numberOfP_Attack1 = 0;
                        currentAttackAnimName = "Attack2";
                        numberOfAnimFucRepeat[(int)thisAnimOfNumber.Attack] = 2;
                    }
                }
                else
                {
                    if (numberOfAnimFucRepeat[(int)thisAnimOfNumber.Attack] == 1)
                    {
                            StopAnim(name); numberOfP_Attack1 = 0;
                            numberOfAnimFucRepeat[(int)thisAnimOfNumber.Attack] = 2;
                            StartCoroutine(AttackDelay());
                    }
                }
            }
        }
        else if (name == "Attack2")
        {
            if (numberOfAnimFucRepeat[(int)thisAnimOfNumber.Attack] == 2)
            {
                PlayAnim(name);
                numberOfAnimFucRepeat[(int)thisAnimOfNumber.Attack] = 3;
            }
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(name))
            {
                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.99f / anim.GetCurrentAnimatorStateInfo(0).speed + 0.2f)
                {
                    if (numberOfP_Attack2 == 0)
                    {
                        numberOfP_Attack2++;
                        transform.GetComponent<AudioSource>().PlayOneShot(sound[(int)Sound.Attack2], 0.5f);
                        AttackEffect.GetComponent<Transform>().position = attackEffectPos + transform.position;
                        ShowAttackEffect(currentAttackEffect, 1);
                        Attack(false, 1);
                    }
                    if (Input.GetKeyDown(KeyCode.A) && GetSP() >= 20)
                    {
                        SetSP(0, 20);
                        numberOfAnimFucRepeat[(int)thisAnimOfNumber.Attack] = 0;
                        currentAttackAnimName = "Attack1";
                        PlayAnim("Attack1");
                        StopAnim(name); numberOfP_Attack2 = 0;
                    }
                }
                else
                {
                    if (numberOfAnimFucRepeat[(int)thisAnimOfNumber.Attack] == 3)
                    {
                        StopAnim(name); numberOfP_Attack2 = 0;
                        StartCoroutine(AttackDelay());
                    }
                }
            }
        }
        else if (name == "Defense")
        {
            if (numberOfAnimFucRepeat[(int)thisAnimOfNumber.Defense] == 0)
            {
                PlayAnim(name);
                numberOfAnimFucRepeat[(int)thisAnimOfNumber.Defense] = 1;
            }
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(name))
            {
                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.99f)
                {
                    if (effectCount == 0)
                    {
                        effectCount = 1;
                        ShowParticle(GameObject.Find("Defense"));
                    }
                }
                else
                {
                    if (numberOfAnimFucRepeat[(int)thisAnimOfNumber.Defense] == 1)
                    {
                        numberOfAnimFucRepeat[(int)thisAnimOfNumber.Defense] = 2;
                        StopAnim(name);
                        StartCoroutine(DefenseDelay(0.3f));
                        effectCount = 0;
                    }
                }
            }
        }
        else if (name == "Skill_Sting")
        {
            if (numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_Sting] == 0)
            {
                StopAnim("Run");
                PlayAnim(name);
                numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_Sting] = 1;
            }
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(name))
            {
                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.99f && anim.GetCurrentAnimatorStateInfo(0).IsName("Stun"))
                {
                    
                }
                else
                {
                    if (numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_Sting] == 1)
                    {
                        for (int i = 0; i < castingEffect.transform.childCount; i++)
                            castingEffect.transform.GetChild(i).GetComponent<ParticleSystem>().Play();

                        transform.GetComponent<AudioSource>().PlayOneShot(sound[(int)Sound.Casting], 0.5f);
                        StopAnim(name);
                        currentAnimFucName = "Skill_Sting_Attack";
                        numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_Sting] = 2;
                    }
                }
            }
        }
        else if (name == "Skill_Sting_Attack")
        {
            if (numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_Sting_Attack] == 0)
            {
                StopAnim("Skill_Sting");
                PlayAnim(name);
                numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_Sting_Attack] = 1;
            }
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(name))
            {
                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.9f)
                {
                    if (effectCount == 0)
                    {
                        effectCount = 1;
                        transform.GetComponent<AudioSource>().PlayOneShot(sound[(int)Sound.Attack3], 0.5f);
                        StartCoroutine(StingAttack(numOfStingAtttack));
                    }
                }
                else
                {
                    if (numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_Sting_Attack] == 1)
                    {
                        effectCount = 0;
                        StopAnim(name);
                        numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_Sting_Attack] = 2;
                        StartCoroutine(SkillDelay(thisAnimOfNumber.Skill_Sting, 0.1f));
                        StartCoroutine(SkillDelay(thisAnimOfNumber.Skill_Sting_Attack, 0.1f));
                    }
                }
            }
        }
        else if (name == "Skill_PowerKick")
        {
            if (numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_PowerKick] == 0)
            {
                StopAnim("Run");
                PlayAnim(name);
                numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_PowerKick] = 1;
            }
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(name))
            {
                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.9f)
                {
                    if (0.4f < anim.GetCurrentAnimatorStateInfo(0).normalizedTime)
                    {
                        if (effectCount == 0)
                        {
                            GameObject kick = transform.Find("Kick").gameObject;
                            effectCount = 1;
                            for(int i = 0; i < kick.transform.childCount; i++)
                            {
                                kick.transform.GetChild(i).GetComponent<ParticleSystem>().Play();
                            }
                            StartCoroutine(Kick());
                            transform.GetComponent<AudioSource>().PlayOneShot(sound[(int)Sound.Attack3], 0.5f);
                            Attack(false, 2);

                        }
                    }
                }
                else
                {
                    if (numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_PowerKick] == 1)
                    {
                        StopAnim(name);
                        numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_PowerKick] = 2;
                        StartCoroutine(SkillDelay(thisAnimOfNumber.Skill_PowerKick, 0.3f));
                        effectCount = 0;
                    }
                }
            }
        }
        else if (name == "Skill_Howling")
        {
            if (numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_Howling] == 0)
            {
                StopAnim("Run");
                PlayAnim(name);
                numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_Howling] = 1;
            }
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(name))
            {
                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.8f)
                {
                    if (effectCount == 0)
                    {
                        effectCount = 1;
                        transform.GetComponent<AudioSource>().PlayOneShot(sound[(int)Sound.Buff], 1);
                        ShowParticle(GameObject.Find("Howling_Buff"));
                        StartCoroutine(Buff(10f));
                    }
                }
                else
                {
                    if (numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_Howling] == 1)
                    {
                        StopAnim(name);
                        numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_Howling] = 2;
                        StartCoroutine(SkillDelay(thisAnimOfNumber.Skill_Howling, 0));
                        effectCount = 0;
                    }
                }
            }
        }
        return false;
    }
    IEnumerator Kick()
    {
        int _prevCriPercentage = criticalPercentage;
        criticalPercentage = 100;
        yield return new WaitForSeconds(1f);
        criticalPercentage = _prevCriPercentage;
    }
    IEnumerator Buff(float durBuff)
    {
        // 버프 효과
        attackPower += 10;
        yield return new WaitForSeconds(durBuff);
        // 효과 해제
        attackPower -= 10;
    }
    IEnumerator StingAttack(int repeat)
    {
        GameObject _effect = GameObject.Find("Sting");
        Vector3 _effectPos = transform.GetChild(18).position;
        _effect.transform.rotation = transform.rotation;
        float _X = 0, _Y = 0, _Z = -0;
        for (int i = 0; i < repeat; i++)
        {
            yield return new WaitForSeconds(0.05f);

            createEffect = Instantiate(_effect, new Vector3(_effectPos.x + _X, _effectPos.y + _Y, _effectPos.z + _Z), transform.rotation);
            effectList.Add(createEffect);
            createEffect.transform.parent = transform;
            for (int j = 0; j < _effect.GetComponent<Transform>().childCount; j++)
            {
                createEffect.GetComponent<Transform>().GetChild(j).GetComponent<ParticleSystem>().Play();
            }
            if (i % 2 == 1)
            {
                _X += 0.5f * i;
                _Z += 0.5f * i;
            }
            else
            {
                _X -= 0.5f * i;
                _Z -= 0.5f * i;
            }
            if (i == 3)
                _Y = 1.5f;
            if (i == 5)
                _Y = -1.5f;

            damageTextYPos += 0.4f;
            Attack(true, 1);
        }
        yield return new WaitForSeconds(0.1f);
        damageTextYPos = 0;
        currentAttackAnimName = "Attack1";
        StopAnim("Attack1");
        StopAnim("Attack2");
        numberOfAnimFucRepeat[(int)thisAnimOfNumber.Attack] = 0;
        numberOfP_Attack1 = 0;
        numberOfP_Attack2 = 0;
        for (int i = 0; i < effectList.Count; i++)
            Destroy(effectList[i]);
    }
    IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(0.1f);
        numberOfAnimFucRepeat[(int)thisAnimOfNumber.Attack] = 0;
        isAttack = false;
    }
    IEnumerator SkillDelay(thisAnimOfNumber name, float time)
    {
        yield return new WaitForSeconds(time);
        numberOfAnimFucRepeat[(int)name] = 0;
        isSkill = false;
    }
    IEnumerator DefenseDelay(float delay)
    {
        isDefense = false;
        yield return new WaitForSeconds(delay);
        numberOfAnimFucRepeat[(int)thisAnimOfNumber.Defense] = 0;
        isSkill = false;
    }
    public IEnumerator IsKnockback()
    {
        isKnockBack = true;
        yield return new WaitForSeconds(0.3f);
        isKnockBack = false;
    }
    IEnumerator StunDelay()
    {
        yield return new WaitForSeconds(1.5f);
        isStun = false;
        GameObject.Find("Stun").GetComponent<Transform>().GetChild(0).gameObject.GetComponent<ParticleSystem>().Stop();
        anim.ResetTrigger("Stun");
    }
    private void InteractiveObjectHit()
    {
        if (viewingDirection == "f") 
            if (Physics.Raycast(new Vector3(this.transform.position.x, this.transform.position.y + 1, this.transform.position.z), this.transform.forward, out rayHit, 5))
            {
                if (rayHit.transform.tag == "InteractiveObject")
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        if (!isInteract)
                        {
                            transform.GetComponent<AudioSource>().PlayOneShot(sound[(int)Sound.InteractiveStatue], 1);
                            specificCamera.GetComponent<SpecificCamera>().isInteract = true;
                            specificCamera.GetComponent<Camera>().depth = 1;
                            isInteract = true;
                            StopAnim("Run");
                            anim.Play("Idle");
                        }
                    }
                }
                else if (rayHit.transform.tag == "Potal_Lobby")
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        if (SceneManager.GetActiveScene().name == "Stage_1_1" || SceneManager.GetActiveScene().name == "Stage_3" || SceneManager.GetActiveScene().name == "Stage_2")
                            SceneManager.LoadScene("Lobby");
                    }
                }
                else if (rayHit.transform.tag == "Potal_stage1")
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        SceneManager.LoadScene("Stage_1");
                    }
                }
                else if (rayHit.transform.tag == "Potal_stage2")
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        SceneManager.LoadScene("Stage_2");
                    }
                }
            }
        if (viewingDirection == "l")
            if (Physics.Raycast(new Vector3(this.transform.position.x, this.transform.position.y + 1, this.transform.position.z), transform.forward, out rayHit, 5))
            {
                if(rayHit.transform.tag == "Potal_stage3")
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        SceneManager.LoadScene("Stage_3");
                    }
                }
            }
    }
    public void Stun()
    {
        anim.SetTrigger("Stun");
        StunAnim.SetTrigger("Stun");
        ShowParticle(GameObject.Find("Stun").GetComponent<Transform>().GetChild(0).gameObject);
        isStun = true;
        StartCoroutine(StunDelay());

        //    초기화
        if (isAttack)
            StopAnim(currentAttackAnimName);
        if (isSkill)
            StopAnim(currentAnimFucName);
        if (isDefense)
            StopAnim("Defense");
        isAttack = false;
        isSkill = false;
        isDefense = false;
        //attack 부분
        currentAttackAnimName = "Attack1";
        numberOfAnimFucRepeat[(int)thisAnimOfNumber.Attack] = 0;
        numberOfAnimFucRepeat[(int)thisAnimOfNumber.Skill_Sting] = 0;
        numberOfP_Attack1 = 0;
        numberOfP_Attack2 = 0;
        effectCount = 0;
        StartCoroutine(SkillDelay(thisAnimOfNumber.Skill_Sting, 0.1f));
        StartCoroutine(SkillDelay(thisAnimOfNumber.Skill_Sting_Attack, 0.1f));
    }
    public void Die()
    {
        anim.Play("Die");
        isDie = true;
        if (!isStop)
            StartCoroutine("DieText");
        //죽은다음행동
    }
    IEnumerator DieText()
    {
        isStop = true;
        yield return new WaitForSeconds(2f);

        dieText.SetActive(true);

        yield return new WaitForSeconds(2f);

        while (dietextA > 0) //Fade Out
        {
            dieText.GetComponent<RawImage>().color = new Color(dieText.GetComponent<RawImage>().color.r, dieText.GetComponent<RawImage>().color.g, dieText.GetComponent<RawImage>().color.b, dietextA);
            dieText.GetComponent<Transform>().GetChild(0).GetComponent<RawImage>().color = new Color(dieText.GetComponent<Transform>().GetChild(0).GetComponent<RawImage>().color.r
                , dieText.GetComponent<Transform>().GetChild(0).GetComponent<RawImage>().color.g, dieText.GetComponent<Transform>().GetChild(0).GetComponent<RawImage>().color.b, dietextA);
            dietextA -= 0.005f;
            yield return new WaitForSeconds(0.01f);
        }
        SceneManager.LoadScene("Lobby");
        GameObject.Find("PlayerCol").GetComponent<CapsuleCollider>().enabled = true;
    }
    public Vector3 Knockback() //방어시 넉백??? 같은 느낌이면 될 거 같다.
    {
        Vector3 _myPos = balrog.GetComponent<Transform>().localPosition;
        Vector3 _targetPos = transform.localPosition;
        Vector3 _angle = new Vector3(0, GetDegree(_myPos, _targetPos));
        Vector3 _value;
        float percentX, percentZ;
        if (0 <= _angle.y && _angle.y <= 90)
        {
            percentZ = _angle.y / 90;
            percentX = 1 - percentZ;
            _value = new Vector3(-1 * percentX, 0, -1 * percentZ).normalized;
            return _value;
        }
        else if (90 < _angle.y && _angle.y <= 180)
        {
            _angle.y = _angle.y - 90;
            percentX = _angle.y / 90;
            percentZ = 1 - percentX;
            _value = new Vector3(1 * percentX, 0, -1 * percentZ).normalized;
            return _value;
        }
        else if (180 < _angle.y && _angle.y <= 270)
        {
            _angle.y = _angle.y - 180;
            percentX = _angle.y / 90;
            percentZ = 1 - percentX;
            _value = new Vector3(1 * percentX, 0, 1 * percentZ).normalized;
            return _value;
        }
        else if (270 < _angle.y && _angle.y <= 360)
        {
            _angle.y = _angle.y - 270;
            percentZ = _angle.y / 90;
            percentX = 1 - percentZ;
            _value = new Vector3(-1 * percentX, 0, 1 * percentZ).normalized;
            return _value;
        }
        else
            return Vector3.zero;
    }
    float GetDegree(Vector3 _from, Vector3 _to)
    {
        float _angle = Mathf.Atan2(_to.z - _from.z, _to.x - _from.x) * 180 / Mathf.PI;
        if (_angle < 0)
            _angle += 360; // 0~360 까지표현.
        return _angle;
    }
    bool isFire = true;
    private void OnTriggerStay(Collider other)
    {
        try
        {
            if (other.tag == ("Fire") && isFire)
            {
                StartCoroutine(FireAttackDelay());
            }
            else if (other.name == "Meteor_hole")
            {
                transform.position = new Vector3(0, -30, 0); // 떨어지는 애니메이션 필요

                if (!isStop)
                    StartCoroutine("DieText");
            }
            else if (GameObject.Find("Laser") != null && other.transform.parent.name == "Laser")
            {
                StartCoroutine(HitLaser(other.transform.parent.GetComponent<EGA_Laser>().Damage()));
            }
        }
        catch (NullReferenceException ex)
        {
        }

    }
    IEnumerator FireAttackDelay()
    {
        isFire = false;
        SetHP(0, balrog.GetComponent<Balrog>().CircleAttack_Damage);
        yield return new WaitForSeconds(0.5f);
        isFire = true;
    }
    IEnumerator HitLaser(int damage)
    {
        if (!isDash)
            SetHP(0, damage);
        yield return new WaitForEndOfFrame();
    }
}
