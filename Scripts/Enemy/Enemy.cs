using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{
    private GameObject player;
    public Transform magicAttackPoint;
    private Detection detection;

    [Header("State")]
    public int animState;
    public EnemyBaseState currentState;

    [Header("Patrol")]
    public bool canPatrol;
    public GameObject[] wayPointObject;
    public List<Vector3> wayPoints = new List<Vector3>();
    public int index;



    [Header("Enemy Statistics")]
    public float enemyHealth;
    public List<Transform> attackList = new List<Transform>();
    public float attackRange;
    public float attackRate;
    public float minDamage;
    public float maxDamage;
    public float patrolMovementSpeed;
    public float attackMovementSpeed;
    public float patrolAcceleration;
    public float attackAcceleration;
    public enum EnemyType
    {
        Paladin,
        Witch,
    }
    public  EnemyType enemyType;
    private float nextAttack = 0;
    private int attackNumber;

    [Header("Art")]
    public GameObject deadEffect;
    public Transform targetPoint;
    public PatrolState patrolState = new PatrolState();
    public AttackState attackState = new AttackState();
    public bool isDead;
    public Animator animator;
    public NavMeshAgent agent;
    public GameObject sphereMagicAttack;
    public GameObject lightningMagicAttack;

    [Header("UI")]
    public Slider slider;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        detection = GetComponentInChildren<Detection>();
        index = 0;
        TransitionToState(patrolState);
        isDead = false;
        slider.minValue = 0;
        slider.maxValue = enemyHealth;
        slider.value = enemyHealth;
        player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (isDead) return;
        currentState.OnUpdate(this);
        animator.SetInteger("State", animState);
        slider.transform.LookAt(Camera.main.transform);
    }

    public void LoadPath(GameObject go)
    {
        wayPoints.Clear();
        
        foreach (Transform T in go.transform)
        {
            wayPoints.Add(T.position);
        }
    }


    public void MoveToTarget()
    {
        if (attackList.Count == 0)
        {
            agent.SetDestination(wayPoints[index]);
        }
        else if (targetPoint != null) 
        {
            agent.SetDestination(targetPoint.position);
        }
    }


    public void TransitionToState(EnemyBaseState state)
    {
        currentState = state;
        currentState.EnemyState(this);
    }

    public void Health(float damage)
    {
        if (isDead) return;

        enemyHealth -= damage;
        slider.value = enemyHealth;

        if (slider.value <= 0)
        {
            isDead = true;

            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();

            animator.SetBool("Die", true);

            Destroy(Instantiate(deadEffect, transform.position, Quaternion.identity), 3f);

            slider.gameObject.SetActive(false);
        }
    }

    public void AttackAction()
    {
        if (isDead) return;
        if ((Vector3.Distance(transform.position, targetPoint.position) < attackRange))
        {
            if (Time.time > nextAttack)
            {
                if (enemyType == EnemyType.Paladin)
                {
                    attackNumber = Random.Range(0, 2);
                }

                if (enemyType == EnemyType.Witch)
                {
                    if (Vector3.Distance(transform.position, targetPoint.position) < 23)
                    {
                        attackNumber = 1;
                    }
                    else attackNumber = 0;
                }

                animator.SetTrigger("Attack");
                nextAttack = Time.time + attackRate;
                animator.SetInteger("AttackNumber", attackNumber);
            }
            else
            {
                transform.LookAt(player.transform);
            }
        }

        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Attack0")|| animator.GetCurrentAnimatorStateInfo(1).IsName("Attack1"))
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }
    }

    public void MagicAttack()
    {
        Instantiate(sphereMagicAttack, magicAttackPoint.transform.position, magicAttackPoint.transform.rotation);
    }

    public void LightningAttack()
    {
        Instantiate(lightningMagicAttack, magicAttackPoint.transform.position, magicAttackPoint.transform.rotation);
    }
}
