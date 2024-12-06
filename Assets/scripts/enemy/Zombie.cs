using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace enemy
{
public class Zombie : enemy
{
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Player player;
    private bool isAttacking = false;
    private bool canAttack = true;
    public float attackCooldown = 1.5f;
    public float turnSpeed = 10f;
    public float attackInaccuracy = 0.05f;

    private Vector2 currentAttackDirection;


    void Awake()
    {
        player = FindFirstObjectByType<Player>();
    }

    public override void Movement()
    {
        if(!isAttacking)
        {
        base.Movement();

        Vector2 direction = (playerTransform.position - transform.position).normalized;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > attackRange)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, minimumDistance, ~enemyLayer);

            Debug.DrawRay(transform.position, direction * attackRange, Color.red);

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, speed * Time.deltaTime);
            }
            else
            {
                Debug.Log("Zombie nie widzi gracza.");
            }
        }
        else
        {
            Debug.Log("Zombie jest w zasięgu ataku, nie porusza się.");
        }
        }
    }

    public override void Attack()
    {
        base.Attack();
        if(isAttacking || !canAttack)
        {
            return;
        }

        isAttacking = true;
        canAttack = false;

        Debug.Log("Zombie rozpoczyna atak");

        currentAttackDirection = (playerTransform.position - transform.position).normalized;
        StartCoroutine(PerformAttack());
    }

    IEnumerator PerformAttack()
    {
        float attackTime = attackSpeed;

        while(attackTime > 0)
        {
            attackTime -= Time.deltaTime;

            Vector2 targetDirection = (playerTransform.position - transform.position).normalized;

            targetDirection += new Vector2(
                Random.Range(-attackInaccuracy, attackInaccuracy),
                Random.Range(-attackInaccuracy, attackInaccuracy)
            ).normalized * 0.1f;

            currentAttackDirection = Vector2.Lerp(currentAttackDirection, targetDirection, Time.deltaTime * turnSpeed).normalized;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, currentAttackDirection, attackRange, ~enemyLayer);
            Debug.DrawRay(transform.position, currentAttackDirection * attackRange, Color.green);

            if(hit.collider != null && hit.collider.CompareTag("Player"))
            {
                Debug.Log("Zombie trafia gracza");
            }

            yield return null;
        }

        RaycastHit2D finalHit = Physics2D.Raycast(transform.position, currentAttackDirection, attackRange, ~enemyLayer);

        if (finalHit.collider != null && finalHit.collider.GetComponent<Player>() == player)
        {
            player.health -= damage;
            Debug.Log("Zombie zadał obrażenia graczowi po zakończeniu ataku!");
        }

        Debug.Log("Zombie zakończył atak.");

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        canAttack = true;
    }

    private void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if(distanceToPlayer <= attackRange && canAttack && !isAttacking)
        {
            Attack();
        }
        
        Movement();

    }
}
}