namespace NetGame.Assets.Scripts
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using Photon.Pun;

    public class PlayerMovement : MonoBehaviourPun, IPunObservable
    {
        /// <summary>
        /// Calcula o movimento e depois envia ao outro cliente.
        /// </summary>
        [SerializeField] private bool iAmASandBag = false;

        private PlayerVisuals playerVisuals;
        private CharacterData characterData;
        private InputAction horizontalAction;
        private InputAction verticalAction;
        private InputAction jumpAction;
        private InputAction quickRollAction;
        private Rigidbody2D rb;
        private BoxCollider2D boxCollider;
        private BoxCollider2D triggerCol;
        private float speed;
        private bool canMove;
        private float moveInput;
        private bool grounded;
        private bool defending;

        [SerializeField]
        protected Transform groundCheck;
        [SerializeField, Range(0.1f, 5.0f)]
        protected float groundCheckRadius = 2.0f;
        [SerializeField]
        protected LayerMask groundCheckLayers;
        [SerializeField]
        protected LayerMask platformLayerMask;
        private PhotonView _photonView;
        private bool canRoll;
        private bool isRolling;

        private Vector2 _netPosition;
        private Vector2 _netVelocity;

        [SerializeField]
        private float smoothSpeed = 15f;
        private bool canGround;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            characterData = GetComponent<Player>().GetCharacterData();
            playerVisuals = GetComponent<Player>().GetPlayerVisuals();
            canMove = true;
            canRoll = true;
            isRolling = false;
            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            triggerCol = GetComponent<Player>().GetTriggerCol();
            speed = characterData.moveSpeed;
            _photonView = GetComponent<PhotonView>();

            if (!_photonView.IsMine)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                return;
            }

            horizontalAction = InputSystem.actions["Player/Horizontal"];
            verticalAction = InputSystem.actions["Player/Vertical"];
            jumpAction = InputSystem.actions["Player/Jump"];
            quickRollAction = InputSystem.actions["Player/Quick Roll"];
        }

        public PhotonView GetPhotonView()
        {
            return _photonView;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            ComputeGrounded();

            if (!_photonView.IsMine || iAmASandBag)
            {
                SyncRemotePosition();
                return;
            }
        }

        void Update()
        {
            if (!_photonView.IsMine || iAmASandBag)
            {
                return;
            }
            moveInput = horizontalAction.ReadValue<float>();

            HandleMovement();
            HandleJump();

            if (quickRollAction.triggered && (moveInput > 0.1f || moveInput < -0.1f) && grounded)
                QuickRoll();

            Defense();
        }

        private void SyncRemotePosition()
        {
            rb.MovePosition(
            Vector2.Lerp(
                rb.position,
                _netPosition,
                smoothSpeed * Time.fixedDeltaTime
            )
        );
        }

        protected void Defense()
        {
            if (quickRollAction.IsPressed() && (moveInput < 0.1f || moveInput > -0.1f) && !isRolling && grounded)
            {
                StartCoroutine(DefenseTimer());
            }

            if (defending)
            {
                canMove = false;
            }
            else { canMove = true; }
        }

        protected void HandleMovement()
        {
            if (rb.linearVelocity.x > 0.1f|| rb.linearVelocity.x < -0.1f)
            {
                playerVisuals.GetAnimator().SetBool("Walk", true);
            }
            else
            {
                playerVisuals.GetAnimator().SetBool("Walk", false);
            }

            if (!canMove || isRolling) return;

            if (_photonView.IsMine)
            {
                rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocityY);
            }

            if (moveInput > 0)
            {
                playerVisuals.FlipSprite(true);
            }
            else if (moveInput < 0)
            {
                playerVisuals.FlipSprite(false);
            }
        }

        protected void HandleJump()
        {
            if (!canMove || !grounded) return;
            if (rb.linearVelocityY > 0.1f) return;


            if (jumpAction.triggered)
            {
                playerVisuals.GetAnimator().SetBool("Jump", true);
                StartCoroutine(ResetJumpAnimation());

                if (verticalAction.ReadValue<float>() < -0.5f)
                {
                    StartCoroutine(FallFromFloor());
                    return;
                }
                rb.AddForce(Vector2.up * characterData.jumpForce, ForceMode2D.Impulse);
            }
        }

        private IEnumerator ResetJumpAnimation()
        {
            canGround = false;
            yield return new WaitForSeconds(0.25f);
            canGround = true;
        }

        public void HurtJump(float direction)
        {
            rb.AddForce(new Vector2(-direction * 2, characterData.jumpForce / 2), ForceMode2D.Impulse);
        }

        protected void ComputeGrounded()
        {
            if (groundCheck == null) return;

            Collider2D collider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundCheckLayers);

            if (collider != null)
            {
                grounded = true;
                if (canGround)
                {
                    playerVisuals.GetAnimator().SetBool("Jump", false);
                }
            }
            else
            {
                grounded = false;
            }
        }

        void OnDrawGizmos()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        IEnumerator FallFromFloor()
        {
            Collider2D collider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundCheckLayers);
            if ((platformLayerMask.value & (1 << collider.gameObject.layer)) == 0) yield break;

            canMove = false;
            boxCollider.enabled = false;
            yield return new WaitForSeconds(0.35f);
            boxCollider.enabled = true;
            canMove = true;
        }

        IEnumerator DefenseTimer()
        {
            if (defending) yield break;

            defending = true;
            playerVisuals.GetAnimator().SetTrigger("Attack");
            yield return new WaitForSeconds(0.5f);
            playerVisuals.GetAnimator().ResetTrigger("Attack");
            defending = false;
        }

        protected void QuickRoll()
        {
            if (!canRoll) return;
            canRoll = false;
            isRolling = true;
            rb.AddForce(new Vector2(moveInput * speed * 3, 0), ForceMode2D.Impulse);
            StartCoroutine(ResetRoll());
        }

        private IEnumerator ResetRoll()
        {
            triggerCol.enabled = false;
            yield return new WaitForSeconds(0.3f);
            isRolling = false;
            triggerCol.enabled = true;
            yield return new WaitForSeconds(0.2f);
            canRoll = true;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(rb.position);
                stream.SendNext(rb.linearVelocity);
                stream.SendNext(playerVisuals.GetSpriteRenderer().flipX);

                stream.SendNext(grounded);
                stream.SendNext(defending);
                stream.SendNext(isRolling);

                stream.SendNext(gameObject.activeSelf);
            }
            else
            {
                _netPosition = (Vector2)stream.ReceiveNext();
                _netVelocity = (Vector2)stream.ReceiveNext();

                double lag = PhotonNetwork.Time - info.SentServerTime;
                _netPosition += _netVelocity * (float)lag;

                playerVisuals.GetSpriteRenderer().flipX = (bool)stream.ReceiveNext();
                grounded = (bool)stream.ReceiveNext();
                defending = (bool)stream.ReceiveNext();
                isRolling = (bool)stream.ReceiveNext();

                gameObject.SetActive((bool)stream.ReceiveNext());
            }
        }

        public bool IsDefending()
        {
            return defending;
        }

        public bool IsRolling()
        {
            return isRolling;
        }

        public void SetCanMove(bool canMove)
        {
            this.canMove = canMove;
        }

        public bool CheckIfIAmASandBag()
        {
            return iAmASandBag;
        }
    }
}