namespace NetGame.Assets.Scripts
{
    using UnityEngine;
    using Photon.Pun;

    public class PlayerVisuals : MonoBehaviourPun, IPunObservable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void FlipSprite(bool facingRight)
        {
            spriteRenderer.flipX = !facingRight;
        }

        public SpriteRenderer GetSpriteRenderer()
        {
            return spriteRenderer;
        }

        public Animator GetAnimator()
        {
            return animator;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(animator.GetBool("Attack"));
                stream.SendNext(animator.GetBool("Hurt"));
                stream.SendNext(animator.GetBool("Walk"));
                stream.SendNext(animator.GetBool("Jump"));

            }
            else
            {
                animator.SetBool("Attack", (bool)stream.ReceiveNext());
                animator.SetBool("Hurt", (bool)stream.ReceiveNext());
                animator.SetBool("Walk", (bool)stream.ReceiveNext());
                animator.SetBool("Jump", (bool)stream.ReceiveNext());
            }
        }
    }
}