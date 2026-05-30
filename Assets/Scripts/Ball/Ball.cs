namespace NetGame.Assets.Scripts
{
    using UnityEngine;
    using Photon.Pun;

    public class Ball : MonoBehaviourPun, IPunObservable
    {
        [SerializeField]
        private Team team;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private float side;
        private Vector2 _netPosition;
        private Vector2 _netVelocity;
        private Sprite redBallSprite;
        private Sprite blueBallSprite;

        [SerializeField]
        private float smoothSpeed = 15f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            object[] data = photonView.InstantiationData;
            
            SetTeam((Team)(int)data[0]);
            Side((float)data[1]);
            Launch();
            TeamColor();


            if (!photonView.IsMine)
                rb.bodyType = RigidbodyType2D.Kinematic;
        }

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            redBallSprite = Resources.Load<Sprite>("Sprites/Balls/ballred");
            blueBallSprite = Resources.Load<Sprite>("Sprites/Balls/ballblu");
        }

        void FixedUpdate()
        {
            if (!photonView.IsMine)
                rb.MovePosition(
                Vector2.Lerp(
                    rb.position,
                    _netPosition,
                    smoothSpeed * Time.fixedDeltaTime
                )
            );
        }

        public void Launch()
        { rb.AddForce(new Vector2(side * 8, Random.Range(8f, 25f)), ForceMode2D.Impulse); }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(team);
                stream.SendNext(rb.position);
                stream.SendNext(rb.linearVelocity);
            }
            else
            {
                team = (Team)stream.ReceiveNext();
                TeamColor();
                _netPosition = (Vector2)stream.ReceiveNext();
                _netVelocity = (Vector2)stream.ReceiveNext();
                double lag = PhotonNetwork.Time - info.SentServerTime;
                _netPosition += _netVelocity * (float)lag;
            }
        }

        protected void TeamColor()
        {
            if (team == Team.Red)
            {
                spriteRenderer.sprite = redBallSprite;
            }
            else if (team == Team.Blue)
            {
                spriteRenderer.sprite = blueBallSprite;
            }
        }

        public void Side(float side)
        {
            if (side > 0)
            {
                this.side = -1;
            }
            else
            {
                this.side = 1;
            }
        }

        public void SetTeam(Team team)
        {
            this.team = team;
            TeamColor();
        }

        public Team GetTeam()
        {
            return team;
        }
    }
}