using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ThreeStageBoss : MonoBehaviour
{
    enum Action
    {
        Start,
        Running,
        End,
    }
    enum BASIC_ANIM
    {
        Idle,
        Walk,
        Idle2,
        HeadImpact,
        Death,
    }
    enum PHASE_1
    {
        OneHand_Slash,
        OneHand_ThreeHit,
        DiagonalSlash,
        Kick,
    }
    enum PHASE_2
    {
        TwoHand_Slash,
        OneHand_ThreeHit,
        DiagonalSlash,
        Kick,
        Two_Hand_TwoWave,
        PowerUp,
        CastingAttack,
    }

    // <애니메이션 변수>
    bool isAnim = false;
    Animator animator;
    public AnimationClip[] basicAnimClips, Phase1AnimClips, Phase2AnimClips, Phase3AnimClips;
    List<AnimationClip[]> curAnimClips = new List<AnimationClip[]>();
    List<List<string>> animNames = new List<List<string>>();
    float effectStartNormalizedTime;
    int numOfAnimPlay;
    int checkSkill;
    string curAnimName;
    float curAnimSpeed;
    // </애니메이션 변수>

    //<세뇌 관련>
    public bool isBW = false;
    public List<GameObject> BW_InstObject = new List<GameObject>();
    public GameObject escapeCircle, obstacle_BW, camera_BW;
    RawImage fog;
    //</세뇌 관련>

    //<즉사>
    private bool isPhase3 = false;
    public GameObject[] eyeEffects = new GameObject[2];
    public GameObject instantDeath;
    //</즉사>

    // <UI>
    public GameObject UI;
    private Slider hpBar;
    // </UI>

    // <SOUND>
    enum SOUND
    {
        TwoWave_1,
        TwoWave_2,
        ThreeAttack_1,
        ThreeAttack_2,
        ThreeAttack_3,
        CastingAttack,
        EscapeZoonOpen,
        InstantDeath
    }
    public AudioClip[] sounds;
    private AudioSource myAudio;
    // </SOUND>
    
    private float prevGapAngle;
    private GameObject attackCheck;
    private Rigidbody rigid;
    private NavMeshAgent navi;
    private float playerAndBossDist, playerAndBossAngle;
    public bool isAttack, isMove, isTurn, isDeath;
    public int curPhase, prevPhase;
    public int power;
    public int speed;
    public float hp, curHp;
    public GameObject player;
    public RaycastHit rayHit;
    public GameObject map;
    public int gainGold;

    void Awake()
    {
        // <Init>
        animator = GetComponent<Animator>();
        animNames.Add(new List<string> { "OneHand_Slash", "OneHand_ThreeHit", "DiagonalSlash", "Kick" });
        animNames.Add(new List<string> { "TwoHand_Slash", "OneHand_ThreeHit", "DiagonalSlash", "Kick", "TwoHand_TwoWave", "CastingAttack", "Brain" });
        curAnimName = "Idle";
        curAnimSpeed = 1;
        PlayAnim("Walk");
        rigid = GetComponent<Rigidbody>();
        navi = gameObject.GetComponent<NavMeshAgent>();
        effectStartNormalizedTime = 0.5f;
        numOfAnimPlay = checkSkill = 0;
        curPhase = prevPhase = power = 0;
        playerAndBossDist = playerAndBossAngle = 0;
        isAttack = false; isDeath = false; isTurn = true;
        curAnimClips.Add(Phase1AnimClips); curAnimClips.Add(Phase2AnimClips); curAnimClips.Add(Phase3AnimClips);
        UI.GetComponent<Transform>().GetChild(2).gameObject.SetActive(true);
        hpBar = UI.GetComponent<Transform>().GetChild(2).GetComponent<Slider>();
        myAudio = transform.GetComponent<AudioSource>();
        hp = curHp = hpBar.value = 2000;
        // </Init>

        // <Coroutine>
        StartCoroutine(Attack());
        StartCoroutine(CheckPhase());
        StartCoroutine(SetAnim());
        // </Coroutine>
    }

    // <세뇌 관련 함수>
    void Brainwashing()
    {
        isBW = true;
        fog = GameObject.Find("Fog").GetComponent<RawImage>();
        fog.enabled = true;
        Debuff_BW();
        CreateEscapeCircle_BW();
        StartCoroutine(End_BW(10f));
    }
    void Debuff_BW()
    {
        camera_BW.GetComponent<Camera>().depth = 0;
    }
    void CreateEscapeCircle_BW()
    {
        Rect _rect = new Rect();
        _rect.Set(map.GetComponent<Transform>().position.x - map.GetComponent<Transform>().lossyScale.x / 2, map.GetComponent<Transform>().position.z - map.GetComponent<Transform>().lossyScale.z / 2,
            map.GetComponent<Transform>().lossyScale.x, map.GetComponent<Transform>().lossyScale.z);
        float randX = Random.Range(_rect.xMin, _rect.xMax);
        float randY = Random.Range(_rect.yMin, _rect.yMax);
        BW_InstObject.Add(Instantiate(escapeCircle, new Vector3(randX, 0.1f, randY), Quaternion.identity));
        SoundPlay("탈출");
        CreateObstacle_BW(randX, randY, _rect);
    }
    void CreateObstacle_BW(float randX, float randY, Rect mapRect)
    {
        float _x = mapRect.xMin, _z = mapRect.yMin;
        int _count = 0;
        while (_x < mapRect.xMax)
        {
            while (_z < mapRect.yMax)
            {
                BW_InstObject.Add(Instantiate(obstacle_BW, new Vector3(_x, obstacle_BW.transform.position.y, _z), Quaternion.identity));
                _z += 4;
            }
            _x += 4; _count++;
            if (_count % 2 != 0) _z = mapRect.yMin + 2;
            else _z = mapRect.yMin;
        }
    }
    IEnumerator End_BW(float time)
    {
        yield return new WaitForSeconds(time);
        if (isBW)
            player.GetComponent<Character>().SetHP(0, player.GetComponent<Character>().GetHP() - 1);

    }
    // </세뇌 관련 함수>

    //<기본 설정 함수>
    void State()
    {
        playerAndBossDist = PlayerAndBossDist();
        playerAndBossAngle = PlayerAndBossAngle();
        Death();
    }
    public void SetHP(float _hp)
    {
        curHp -= _hp;
        hpBar.value -= _hp;
    }
    public float GetHP()
    {
        return curHp;
    }
    public int GetPower()
    {
        return power;
    }
    void Death()
    {
        if (curHp < 0 && !isDeath)
        {
            isDeath = true;
            animator.SetTrigger("Death");
            GameObject.Find("GameMgr").GetComponent<GameMgr>().SetPlayerGold(gainGold);
        }
    }
    IEnumerator CheckPhase()
    {
        while (true)
        {
            if (hp * 0.4 < curHp && curHp <= hp * 0.9 && curPhase == 0)
            {
                curPhase = 1;
                curAnimSpeed = 1.3f;
                animator.speed = curAnimSpeed;
            }
            else if (curHp <= hp * 0.4 && !isPhase3)
            {
                isPhase3 = true;
                eyeEffects[0].transform.GetComponent<ParticleSystem>().Play();
                eyeEffects[1].transform.GetComponent<ParticleSystem>().Play();
                curAnimSpeed = 1.5f;
                animator.speed = curAnimSpeed;
                Brainwashing();
                SoundPlay("3페이즈시작");
            }
            else if (isDeath) break;
            yield return new WaitForSeconds(1f);
        }
    }
    void LookAtPlayer()
    {
        Vector3 _dir = player.transform.position - transform.position;
        Quaternion _target = Quaternion.LookRotation(_dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, _target, 3 * Time.deltaTime);
    }

    void SetNavi()
    {
        navi.destination = player.transform.position;
    }

    bool MoveCheckHelper()
    {
        if (!StoppingDist())
        {
            StopAnim("Idle");
            PlayAnim("Walk");
            isAttack = false;
            isTurn = true;
            return true;
        }
        else return false;
    }
    bool StoppingDist()
    {
        if (Vector3.Distance(transform.position, player.transform.position) <= navi.stoppingDistance + 0.1f)
        {
            navi.speed = 0;
            return true;
        }
        else
        {
            navi.speed = 5;
            return false;
        }
    }
    float BossAngle()
    {
        return 360 - transform.eulerAngles.y;
    }
    float PlayerAndBossDist()
    {
        return Vector2.Distance(new Vector2(player.GetComponent<Transform>().position.x, player.GetComponent<Transform>().position.z), new Vector2(transform.position.x, transform.position.z));
    }
    float PlayerAndBossAngle()
    {
        Vector2 _pos = new Vector2(player.GetComponent<Transform>().position.x, player.GetComponent<Transform>().position.z) - new Vector2(transform.position.x, transform.position.z);
        float _angle = Mathf.Atan2(_pos.y, _pos.x) * Mathf.Rad2Deg;
        if (_angle >= 0)
            return _angle;
        else
            return 360 + _angle;
    }
    //</기본 설정 함수>

    //<애니메이션-이펙트 함수>
    void PlayAnim(string name) { animator.SetBool(name, true); }
    void StopAnim(string name) { animator.SetBool(name, false); }

    Action animAction = Action.Start;
    Action effectAction = Action.Start;

    IEnumerator SetAnim()
    {
        while (true)
        {
            if (animAction == Action.Start)
            {
                animAction = Action.Running;
                while (animAction == Action.Running)
                {
                    if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.99f)
                    {
                        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Basic") && effectAction == Action.Start && curAnimName != "Brain")
                        {
                            effectAction = Action.Running;
                            StartCoroutine(AnimEffectPlayTime());
                        }
                    }
                    else
                    {
                    }
                    yield return new WaitForFixedUpdate();
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }
    //애니메이션 이름에 따른 설정.
    void SetAnimDetail()
    {
        switch (curAnimName)
        {
            case "OneHand_Slash":
                SetAnimDetailHelper(20);
                effectStartNormalizedTime = 0.5f;
                break;
            case "OneHand_ThreeHit":
                if (curPhase == 0)
                    SetAnimDetailHelper(30);
                else
                    SetAnimDetailHelper(40);
                effectStartNormalizedTime = 0.2f;
                break;
            case "DiagonalSlash":
                if (curPhase == 0)
                    SetAnimDetailHelper(30);
                else
                    SetAnimDetailHelper(40);
                effectStartNormalizedTime = 0.2f;
                break;
            case "Kick":
                SetAnimDetailHelper(10);
                effectStartNormalizedTime = 0.5f;
                break;
            case "TwoHand_Slash":
                SetAnimDetailHelper(30);
                effectStartNormalizedTime = 0.5f;
                break;
            case "TwoHand_TwoWave":
                SetAnimDetailHelper(50);
                effectStartNormalizedTime = 0.3f;
                break;
            case "PowerUp":
                SetAnimDetailHelper(0);
                effectStartNormalizedTime = 0.3f;
                break;
            case "CastingAttack":
                SetAnimDetailHelper(100);
                effectStartNormalizedTime = 0.8f;
                break;
            default:
                break;
        }
    }
    void SetAnimDetailHelper(int _power)
    {
        if (curPhase == 0)
        {
            power = _power * 1;
        }
        else if (curPhase == 1)
        {
            power = _power;
        }
        if (isPhase3)
        {
            power += 10;
        }
    }
    void PlayEffect(GameObject effect)
    {
        if (curAnimName == "OneHand_ThreeHit")
        {
            float[] delay = { 0.5f, 0.9f, 0.5f };
            float[] startAngle = { 0, 0, 70 };
            float[] finalAngle = { 180, 180, 110 };
            float[] dist = { 6, 6, 6 };
            StartCoroutine(PlayEffectEx(effect, delay, startAngle, finalAngle, dist));
        }
        else if (curAnimName == "TwoHand_TwoWave")
        {
            float[] delay = { 0.5f };
            float[] startAngle = { 0, 0 };
            float[] finalAngle = { 180, 180 };
            float[] dist = { 15, 15 };
            StartCoroutine(PlayEffectEx(effect, delay, startAngle, finalAngle, dist));
        }
        else if (curAnimName == "CastingAttack")
        {
            float delay = curAnimClips[1][5].length;
            float startAngle = 90;
            float finalAngle = 90;
            int dist = 30;
            StartCoroutine(PlayEffectCastingAttack(effect, delay, startAngle, finalAngle, dist));
        }
        else if (curAnimName == "OneHand_Slash")
        {
            AttackSoundPlay(0);
            AttackCheck(0, 120, 6);
            PlayEffectBasic(effect);
        }
        else if (curAnimName == "TwoHand_Slash")
        {
            AttackSoundPlay(0);
            AttackCheck(0, 120, 8);
            PlayEffectBasic(effect);
        }
        else if (curAnimName == "DiagonalSlash")
        {
            AttackSoundPlay(0);
            AttackCheck(0, 100, 6);
            PlayEffectBasic(effect);
        }
        else if (curAnimName == "Kick")
        {
            AttackCheck(70, 110, 10);
            PlayEffectBasic(effect);
        }
    }
    void PlayEffectBasic(GameObject effect)
    {
        for (int i = 0; i < effect.GetComponent<Transform>().childCount; i++)
        {
            effect.transform.GetChild(i).GetComponent<ParticleSystem>().Play();
        }
    }
    IEnumerator PlayEffectEx(GameObject effect, float[] delay, float[] _startAngle, float[] _finalAngle, float[] _dist)
    {
        for (int i = 0; i < effect.GetComponent<Transform>().childCount; i++)
        {
            AttackSoundPlay(i);
            GameObject chileEffect = effect.GetComponent<Transform>().GetChild(i).gameObject;
            for (int ceIdx = 0; ceIdx < chileEffect.GetComponent<Transform>().childCount; ceIdx++)
            {
                chileEffect.transform.GetChild(ceIdx).GetComponent<ParticleSystem>().Play();
            }
            AttackCheck(_startAngle[i], _finalAngle[i], _dist[i]);
            if (i == effect.GetComponent<Transform>().childCount - 1) break;
            yield return new WaitForSeconds(delay[i] / curAnimSpeed);
        }
    }
    IEnumerator PlayEffectCastingAttack(GameObject effect, float delay, float _startAngle, float _finalAngle, int _dist)
    {
        for (int i = 0; i < effect.GetComponent<Transform>().childCount; i++)
        {
            GameObject chileEffect = effect.GetComponent<Transform>().GetChild(i).gameObject;
            for (int ceIdx = 0; ceIdx < chileEffect.GetComponent<Transform>().childCount; ceIdx++)
            {
                chileEffect.transform.GetChild(ceIdx).GetComponent<ParticleSystem>().Play();
            }
            if (i == effect.GetComponent<Transform>().childCount - 1)
            {
                AttackCheckCastingAttack(_startAngle, _finalAngle, _dist);
                myAudio.PlayOneShot(sounds[(int)SOUND.CastingAttack]);
                break;
            }
            yield return new WaitForSeconds(delay / curAnimSpeed);
        }
    }
    IEnumerator AnimEffectPlayTime()
    {
        SetAnimDetail();
        while (true)
        {
            if (effectStartNormalizedTime <= animator.GetCurrentAnimatorStateInfo(0).normalizedTime && effectAction == Action.Running)
            {
                PlayEffect(transform.Find(curAnimName).gameObject);
                effectAction = Action.End;
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f && effectAction == Action.End)
            {
                if (curAnimName == "CastingAttack") yield return new WaitForSeconds((curAnimClips[1][5].length / curAnimSpeed) + (2f / curAnimSpeed));
                effectAction = Action.Start;
                animAction = Action.Start;
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }
    // </애니메이션-이펙트 함수>

    // <공격 함수>
    IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.5f); //시작시 딜레이타임
        while (true)
        {
            if (player.GetComponent<Character>().isDie) break;
            if (curPhase == 3)
            {
                Brainwashing();
            }
            if (StoppingDist() && numOfAnimPlay == 0)
            {
                StopAnim("Walk");
                PlayAnim("Idle");
                for (int i = 0; i < 3; i++)
                {
                    if (player.GetComponent<Character>().isDie) break;
                    isAttack = true;
                    isTurn = false;
                    if (curPhase != prevPhase)
                    {
                        prevPhase = curPhase;
                        yield return new WaitForSeconds(basicAnimClips[(int)BASIC_ANIM.Walk].length / curAnimSpeed);
                        PlayAnim("Idle2");
                        yield return new WaitForSeconds(basicAnimClips[(int)BASIC_ANIM.Idle2].length / 2 / curAnimSpeed);
                        StopAnim("Idle2");
                        isTurn = true;
                        yield return new WaitForSeconds(basicAnimClips[(int)BASIC_ANIM.Idle2].length / 2 / curAnimSpeed);
                        isTurn = false;
                    }
                    int _rand = Random.Range(1, animNames[curPhase].Count);
                    if (numOfAnimPlay == 1) _rand = checkSkill;
                    ++numOfAnimPlay;
                    curAnimName = animNames[curPhase][_rand];
                    if (curAnimName == "Brain")
                    {
                        Brainwashing();
                        animNames[curPhase].Remove("Brain");
                    }
                    if (MoveCheckHelper())
                    {
                        yield return new WaitForSeconds(basicAnimClips[(int)BASIC_ANIM.Walk].length / curAnimSpeed);
                        break;
                    }
                    yield return new WaitForSeconds(basicAnimClips[(int)BASIC_ANIM.Idle].length / curAnimSpeed);
                    StopAnim("Idle");
                    PlayAnim(curAnimName);
                    if (curAnimName == "CastingAttack") yield return new WaitForSeconds((curAnimClips[curPhase][_rand].length / curAnimSpeed) + (curAnimClips[curPhase][_rand].length / curAnimSpeed) + (2f / curAnimSpeed));
                    else if (curAnimName == "Kick") yield return new WaitForSeconds(curAnimClips[curPhase][_rand].length / curAnimSpeed);
                    else yield return new WaitForSeconds(curAnimClips[curPhase][_rand].length / curAnimSpeed);
                    StopAnim(curAnimName);
                    PlayAnim("Idle");
                    yield return new WaitForSeconds(0.5f);
                    isTurn = true;
                    yield return new WaitForSeconds(basicAnimClips[(int)BASIC_ANIM.Idle].length / 2 / curAnimSpeed);
                    if (MoveCheckHelper())
                    {
                        yield return new WaitForSeconds(basicAnimClips[(int)BASIC_ANIM.Walk].length / curAnimSpeed);
                        break;
                    }
                }
                numOfAnimPlay = 0;
            }
            yield return null;
        }
    }
    void AttackCheck(float _startAngle, float _finalAngle, float _dist)
    {
        float startAddBossAngle = _startAngle + BossAngle();
        float finalAddBossAngle = _finalAngle + BossAngle();
        float startAddAngle = 0;
        float finalAddAngle = 0;
        if (startAddBossAngle >= 360)
        {
            startAddAngle = startAddBossAngle;
            startAddBossAngle = startAddBossAngle - 360;
        }
        if (finalAddBossAngle >= 360)
        {
            finalAddAngle = finalAddBossAngle;
            finalAddBossAngle = finalAddBossAngle - 360;
        }

        if (startAddBossAngle <= finalAddBossAngle)
        {
            if (startAddBossAngle <= playerAndBossAngle && playerAndBossAngle <= finalAddBossAngle)
            {
                if (playerAndBossDist <= _dist)
                {
                    if (curAnimName == "Kick")
                    {
                        player.GetComponent<Character>().Stun();
                    }
                    player.GetComponent<Character>().SetHP(0, power);
                    if (isPhase3)
                    {
                        if (!player.GetComponent<Character>().isDie)
                        {
                            instantDeath.GetComponent<ParticleSystem>().Play();
                            StartCoroutine(InstantDeath());
                        }
                    }
                }
            }
        }
        else
        {
            if (startAddBossAngle <= playerAndBossAngle || playerAndBossAngle <= finalAddBossAngle)
            {
                if (playerAndBossDist <= _dist)
                {

                    if (curAnimName == "Kick")
                    {
                        player.GetComponent<Character>().Stun();
                    }
                    player.GetComponent<Character>().SetHP(0, power);
                    if (isPhase3)
                    {
                        if (!player.GetComponent<Character>().isDie)
                        {
                            instantDeath.GetComponent<ParticleSystem>().Play();
                            StartCoroutine(InstantDeath());
                        }
                    }
                }
            }
        }
    }
    IEnumerator InstantDeath()
    {
        yield return new WaitForSeconds(3f);
        player.GetComponent<Character>().SetHP(0, 1000);
    }
    void AttackCheckCastingAttack(float _startAngle, float _finalAngle, int _dist)
    {
        float startAddBossAngle = _startAngle + BossAngle();
        float finalAddBossAngle = _finalAngle + BossAngle();

        if (startAddBossAngle >= 360)
            startAddBossAngle = startAddBossAngle - 360;
        if (finalAddBossAngle >= 360)
            finalAddBossAngle = finalAddBossAngle - 360;

        float curDist = 0;
        for (int i = 0; i < _dist; i++)
        {
            curDist += 1;
            startAddBossAngle -= 1;
            finalAddBossAngle += 1;
            if (startAddBossAngle <= playerAndBossAngle && playerAndBossAngle <= finalAddBossAngle)
                if (playerAndBossDist <= curDist)
                {
                    player.GetComponent<Character>().SetHP(0, power);
                    break;
                }
        }
    }
    // </공격 함수>

    //<사운드 함수>
    void SoundPlay(string name)
    {
        if (name == "탈출")
            myAudio.PlayOneShot(sounds[(int)SOUND.EscapeZoonOpen]);
        else if (name == "3페이즈시작")
            myAudio.PlayOneShot(sounds[(int)SOUND.InstantDeath]);
    }
    void AttackSoundPlay(int repeat)
    {
        if (curAnimName == "TwoHand_TwoWave")
            myAudio.PlayOneShot(sounds[(int)SOUND.TwoWave_1 + repeat]);
        else if (curAnimName == "OneHand_ThreeHit")
            myAudio.PlayOneShot(sounds[(int)SOUND.ThreeAttack_1 + repeat]);
        else if (curAnimName == "DiagonalSlash")
            myAudio.PlayOneShot(sounds[(int)SOUND.ThreeAttack_2 + repeat]);
        else
            myAudio.PlayOneShot(sounds[(int)SOUND.TwoWave_1 + repeat]);
    }
    //</사운드 함수>

    void FixedUpdate()
    {
        if (isTurn && !isDeath)
        {
            LookAtPlayer();
        }
        if (!isAttack && !isDeath)
        {
            SetNavi();
        }
        StoppingDist();
        State();
    }
}
