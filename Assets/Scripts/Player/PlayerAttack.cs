namespace NetGame.Assets.Scripts
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using System.Collections;

    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] BoxCollider2D attackCol;
        private Player player;
        private float moveInput;
        private bool side;
        private InputAction horizontalAction;
        private InputAction verticalAction;
        private InputAction attackAction;

        private bool attacking;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            attacking = false;
            attackCol.enabled = false;
            horizontalAction = InputSystem.actions["Player/Horizontal"];
            verticalAction = InputSystem.actions["Player/Vertical"];
            attackAction = InputSystem.actions["Player/Attack"];
            player = GetComponent<Player>();
        }

        // Update is called once per frame
        void Update()
        {
            ComputeSide();

            if (attackAction.triggered)
            {
                Attack();
            }
        }

        public void Attack()
        {
            if (attacking && player.GetPlayerMovement().IsDefending())
            {
                return;
            }

            if (verticalAction.ReadValue<float>() > 0.1f || verticalAction.ReadValue<float>() < -0.1f)
            {
                return; //implementar ataque vertical dps
            }

            attacking = true;
            attackCol.enabled = true;
            StartCoroutine(HorizontalAttack());
        }

        protected void ComputeSide()
        {
            moveInput = horizontalAction.ReadValue<float>();
            if (moveInput > 0.1f)
            {
                side = true;
            }
            else if (moveInput < -0.1f)
            {
                side = false;
            }
        }

        private IEnumerator HorizontalAttack()
        {
            if (side)
            {
                attackCol.offset = Vector2.zero + new Vector2(attackCol.size.x / 2, 0f);
            }
            else
            {
                attackCol.offset = Vector2.zero - new Vector2(attackCol.size.x / 2, 0f);
            }
            yield return new WaitForSeconds(.4f);
            attackCol.offset = Vector2.zero;
            attackCol.enabled = false;
            attacking = false;
        }
    }
}