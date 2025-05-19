using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] float _speed = 5f;
    [SerializeField] int _maxHealth = 3;
    [SerializeField] int _health;
    [SerializeField] public int damage = 1;
    [SerializeField] float followRange = 10f;
    [SerializeField] float attackRange = 2f;
    [SerializeField] float strollRadius = 20f;
    [SerializeField] float strollInterval = 5f;
    [SerializeField] float attackCooldown = 2f; // Cooldown time between attacks

    private NavMeshAgent _navMeshAgent;
    private Transform player;
    private float lastStrollTime;
    private float lastAttackTime;

    void Start()
    {
        _health = _maxHealth;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _speed;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        lastStrollTime = Time.time;
        lastAttackTime = -attackCooldown; // Initialize to allow immediate attack
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(DelayedAttack());
                lastAttackTime = Time.time; // Update the last attack time
            }
        }
        else if (distanceToPlayer <= followRange)
        {
            FollowPlayer();
        }
        else
        {
            Stroll();
        }
    }

    void FollowPlayer()
    {
        _navMeshAgent.stoppingDistance = attackRange;
        _navMeshAgent.SetDestination(player.position);
    }

    void AttackPlayer()
    {
        Debug.Log("Attacking player");
        // Implement attack logic here, e.g., reduce player's health
    }

    IEnumerator DelayedAttack()
    {
        yield return new WaitForSeconds(2f);
        AttackPlayer();
    }

    void Stroll()
    {
        if (Time.time - lastStrollTime > strollInterval)
        {
            Vector3 randomDirection = Random.insideUnitSphere * strollRadius;
            randomDirection += transform.position;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(randomDirection, out navHit, strollRadius, NavMesh.AllAreas))
            {
                _navMeshAgent.SetDestination(navHit.position);
                lastStrollTime = Time.time;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, followRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, strollRadius);
    }

    public void TakeDamage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void Heal(int health)
    {
        _health += health;
        if (_health > _maxHealth)
        {
            _health = _maxHealth;
        }
    }
}
