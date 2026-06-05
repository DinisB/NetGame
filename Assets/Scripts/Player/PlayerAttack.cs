namespace NetGame.Assets.Scripts
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using System.Collections;
    using Photon.Pun;

    public class PlayerAttack : MonoBehaviourPun, IPunObservable
    {
        [SerializeField] BoxCollider2D attackCol;
        private Player player;
        private float moveInput;
        private bool side;
        private InputAction horizontalAction;
        private InputAction verticalAction;
        private InputAction attackAction;

        private bool attacking;
        private PlayerVisuals playerVisuals;
        private PlayerMovement playerMovement;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            attacking = false;
            attackCol.enabled = false;
            player = GetComponent<Player>();
            playerVisuals = player.GetPlayerVisuals();
            playerMovement = player.GetPlayerMovement();

            if (!photonView.IsMine) return;

            horizontalAction = InputSystem.actions["Player/Horizontal"];
            verticalAction = InputSystem.actions["Player/Vertical"];
            attackAction = InputSystem.actions["Player/Attack"];
        }

        void Update()
        {
            ComputeSide();
            if (!photonView.IsMine) return;

            if (attackAction.triggered)
                Attack();
        }

        public void Attack()
        {
            if (attacking && player.GetPlayerMovement().IsDefending())
            {
                return;
            }

            playerVisuals.GetAnimator().SetBool("Attack", true);
            playerMovement.SetAttack(true);

            if (verticalAction.ReadValue<float>() > 0.1f || verticalAction.ReadValue<float>() < -0.1f)
            {
                attacking = true;
                attackCol.enabled = true;
                StartCoroutine(VerticalAttack(verticalAction.ReadValue<float>()));
                return;
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
            playerVisuals.GetAnimator().SetBool("Attack", false);
            playerMovement.SetAttack(false);
        }

        private IEnumerator VerticalAttack(float verticalInput)
        {
            if (verticalInput > 0.1f)
            {
                attackCol.offset = Vector2.zero + new Vector2(0f, attackCol.size.y / 2);
            }
            else
            {
                attackCol.offset = Vector2.zero - new Vector2(0f, attackCol.size.y / 2);
            }
            yield return new WaitForSeconds(.4f);
            attackCol.offset = Vector2.zero;
            attackCol.enabled = false;
            attacking = false;
            playerVisuals.GetAnimator().SetBool("Attack", false);
            playerMovement.SetAttack(false);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(attacking);
                stream.SendNext(attackCol.enabled);
                stream.SendNext(attackCol.offset);
            }
            else
            {
                attacking = (bool)stream.ReceiveNext();
                attackCol.enabled = (bool)stream.ReceiveNext();
                attackCol.offset = (Vector2)stream.ReceiveNext();
            }
        }
    }
}