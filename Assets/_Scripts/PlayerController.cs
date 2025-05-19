using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _speed = 6f;
    [SerializeField] int _health = 10;
    [SerializeField] int _damage = 1;

    public Transform targetPoint;
    private Vector3 _targetPosition;
    private bool _isMoving = false;
    private Transform _currentTargetPoint;
    private NavMeshAgent _navMeshAgent;
    private Outline enemyOutline;
    [SerializeField] private GameObject lockedEnemy;
    public float enemyStoppingDistance = 4.0f; // Distance to stop in front of the enemy
    public float attackCooldown = 1.0f; // Cooldown time between attacks
    private float lastAttackTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _speed;
        _targetPosition = transform.position;
        lastAttackTime = -attackCooldown; // Initialize to allow immediate attack
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseInput();
        HandleMovement();
        HandleEnemyOutline();
        HandleAttack();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButton(0)) // Check if the mouse button is held down
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                if (hitInfo.collider.gameObject.layer == 3) // Ground layer
                {
                    HandleGroundClick(hitInfo);
                }
                else if (hitInfo.collider.gameObject.layer == 6) // Enemy layer
                {
                    HandleEnemyClick(hitInfo);
                }
            }
        }
    }

    private void HandleGroundClick(RaycastHit hitInfo)
    {
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(hitInfo.point, out navHit, 1.0f, NavMesh.AllAreas))
        {
            _targetPosition = navHit.position;

            if (_currentTargetPoint != null)
            {
                Destroy(_currentTargetPoint.gameObject);
            }

            _currentTargetPoint = Instantiate(targetPoint, _targetPosition, Quaternion.identity);
            _navMeshAgent.stoppingDistance = 0; // Reset stopping distance for ground movement
            _navMeshAgent.SetDestination(_targetPosition);
            _isMoving = true;

            // Unlock the enemy if moving to a new ground position
            if (lockedEnemy != null)
            {
                Outline outline = lockedEnemy.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = false;
                }
                lockedEnemy = null;
            }
        }
    }

    private void HandleEnemyClick(RaycastHit hitInfo)
    {
        // Cast a secondary ray from the hit point downwards to find the ground position
        RaycastHit groundHitInfo;
        if (Physics.Raycast(hitInfo.point, Vector3.down, out groundHitInfo, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(groundHitInfo.point, out navHit, 1.0f, NavMesh.AllAreas))
            {
                // Unlock the previously locked enemy
                if (lockedEnemy != null)
                {
                    Outline outline = lockedEnemy.GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = false;
                    }
                }

                // Lock onto the new enemy
                lockedEnemy = hitInfo.collider.gameObject;
                enemyOutline = lockedEnemy.GetComponent<Outline>();
                if (enemyOutline != null)
                {
                    enemyOutline.enabled = true;
                    enemyOutline.OutlineColor = Color.red;
                }

                _targetPosition = navHit.position;
                _navMeshAgent.stoppingDistance = enemyStoppingDistance; // Set stopping distance for enemy
                _navMeshAgent.SetDestination(_targetPosition);
                _isMoving = true;
            }
        }
        else
        {
            if (enemyOutline != null)
            {
                enemyOutline.enabled = false;
            }
        }
    }

    private void HandleMovement()
    {
        if (_isMoving && !_navMeshAgent.pathPending)
        {
            if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
            {
                if (!_navMeshAgent.hasPath || _navMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    _isMoving = false;
                    if (_currentTargetPoint != null)
                    {
                        Destroy(_currentTargetPoint.gameObject);
                    }
                }
            }
        }
    }

    private void HandleEnemyOutline()
    {
        Ray constantRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit constantRayhitInfo;
        if (Physics.Raycast(constantRay, out constantRayhitInfo))
        {
            if (constantRayhitInfo.collider.gameObject.layer == 6)
            {
                enemyOutline = constantRayhitInfo.collider.gameObject.GetComponent<Outline>();
                if (enemyOutline != null)
                {
                    enemyOutline.enabled = true;
                    enemyOutline.OutlineColor = Color.white;
                }
            }
            else
            {
                if (enemyOutline != null)
                {
                    enemyOutline.enabled = false;
                }
            }
        }
    }

    private void HandleAttack()
    {
        if (lockedEnemy != null && Time.time >= lastAttackTime + attackCooldown)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, lockedEnemy.transform.position);

            if (distanceToEnemy <= enemyStoppingDistance)
            {
                MeleeAttack();
                lastAttackTime = Time.time; // Update the last attack time
            }
        }
    }

    private void MeleeAttack()
    {
        Debug.Log("Melee attacking enemy");
        // Implement melee attack logic here, e.g., reduce enemy's health
        EnemyController enemyController = lockedEnemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.TakeDamage(_damage);
        }
    }
}
