using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    Rigidbody2D rb;
    Vector2 movementVector;
    Animator animator;
    PlayerInput playerInputActions;
    private bool isAttacking = false;
    private float attackSpeedUI;

    [SerializeField] GameObject attackSpherePrefab;
    private bool isHoldingAttack = false;
    float damageRadius = 0.75f;
    [SerializeField] public float health = 10;
    [SerializeField] private float damage = 10;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float range = 1f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private bool canFire = false;
    [SerializeField] private bool canPoison = false;



    void Awake()
    {
      
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        playerInputActions = new PlayerInput();
        playerInputActions.Player.Move.performed += MovePerformed;
        playerInputActions.Player.Move.canceled += OnMoveCanceled;

        playerInputActions.Player.Fire.started += OnAttackStarted;
        playerInputActions.Player.Fire.canceled += OnAttackCanceled;

        playerInputActions.Player.Enable();

    }
    private void Start()
    {
        attackSpeedUI =  attackSpeed;

        Stats.Instance.UpdateStats(damage, speed, attackSpeedUI);

    }

    void OnDestroy()
    {
        playerInputActions.Player.Move.performed -= MovePerformed;
        playerInputActions.Player.Move.canceled -= OnMoveCanceled;

        playerInputActions.Player.Fire.started -= OnAttackStarted;
        playerInputActions.Player.Fire.canceled -= OnAttackCanceled;

        playerInputActions.Player.Disable();
    }

    public void MovePerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        movementVector.x = input.x;
        movementVector.y = input.y;
        movementVector *= speed;
        rb.velocity = movementVector;

        animator.SetFloat("LastXinput", movementVector.x);
        animator.SetFloat("LastYinput", movementVector.y);
        animator.SetFloat("Xinput", movementVector.x);
        animator.SetFloat("Yinput", movementVector.y);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        movementVector = Vector2.zero;
        rb.velocity = movementVector;

        animator.SetFloat("Xinput", 0);
        animator.SetFloat("Yinput", 0);
    }

    void FixedUpdate()
    {
        rb.velocity = movementVector;
    }

    private void OnAttackStarted(InputAction.CallbackContext context)
    {
        isHoldingAttack = true;
        StartCoroutine(AttackWhileHolding());
    }

    private void OnAttackCanceled(InputAction.CallbackContext context)
    {
        isHoldingAttack = false;
    }

    private IEnumerator AttackWhileHolding()
    {
        while (isHoldingAttack)
        {
            if (!isAttacking)
            {
                PerformAttack();
                yield return new WaitForSeconds(0.4f);
            }
            yield return null;
        }
    }

    private void PerformAttack()
    {
        isAttacking = true;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.z = 0;
        Vector2 attackDirection = (mousePosition - transform.position).normalized;
        Vector3 spherePosition = transform.position + (Vector3)attackDirection * 1.3f ;

        GameObject sphere = Instantiate(attackSpherePrefab, spherePosition, Quaternion.identity);       
        sphere.transform.localScale = Vector3.one * 1.5f * range;
        Destroy(sphere, 0.3f);
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(spherePosition, damageRadius * range);
        foreach (Collider2D collider in hitObjects)
        {
            IDamageable target = collider.GetComponent<IDamageable>();
            if (target != null)
            {

                target.TakeDamage(damage);
                if (canFire)
                {
                    target.ApplyDamageOverTime(DamageOverTime.Fire, 5f, 1f); 
                }

                if (canPoison)
                {
                    target.ApplyDamageOverTime(DamageOverTime.Poison, 5f, 1f); 
                }
            }
        }

        animator.SetFloat("AttackXinput", attackDirection.x);
        animator.SetFloat("AttackYinput", attackDirection.y);
        animator.SetBool("IsAttacking", true);

        StartCoroutine(AttackCooldown());
    }


    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("IsAttacking", false);
        yield return new WaitForSeconds(attackSpeed-0.5f);

        isAttacking = false;
    }
    public void ApplyItemStats(ItemData itemData)
    {
        if (itemData == null) return;
        attackSpeedUI= attackSpeedUI + (itemData.attackSpeed* -1);
        health += itemData.health;
        damage += itemData.damage;
        speed += itemData.speed;
        attackSpeed += itemData.attackSpeed;
        range += itemData.range;
        canFire = itemData.canFire;
        canPoison = itemData.canPoison;

        damage = Mathf.Clamp(damage, 0.1f, 5f);
        speed = Mathf.Clamp(speed, 0.1f, 5f);
        attackSpeed = Mathf.Clamp(attackSpeed, 0.1f, 3f);
        range = Mathf.Clamp(range, 0.1f, 3f);



        Stats.Instance.UpdateStats(damage, speed, attackSpeedUI);

    }
}
