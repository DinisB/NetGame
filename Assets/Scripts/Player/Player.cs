namespace NetGame.Assets.Scripts
{
    using UnityEngine;

    public class Player : MonoBehaviour
    {
        [SerializeField]
        private Team team;
        [SerializeField] private int balls;
        private static readonly int maxBalls = 200;

        [SerializeField] private CharacterData characterData;
        [SerializeField] private PlayerVisuals playerVisuals;
        [SerializeField] private BoxCollider2D triggerCol;
        [SerializeField] private PlayerMovement playerMovement;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            balls = 100;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public CharacterData GetCharacterData()
        {
            return characterData;
        }

        public PlayerVisuals GetPlayerVisuals()
        {
            return playerVisuals;
        }

        public BoxCollider2D GetTriggerCol()
        {
            return triggerCol;
        }

        public PlayerMovement GetPlayerMovement()
        {
            return playerMovement;
        }

        public Team GetTeam()
        {
            return team;
        }

        public void SetTeam(Team team)
        {
            this.team = team;
        }

        public void AddBall()
        {
            balls = Mathf.Clamp(balls + 1, 0, maxBalls);
        }

        public void RemoveBall()
        {
            if (balls > 0)
            {
                balls = Mathf.Clamp(balls - 1, 0, maxBalls);
            }
        }

        public int GetBalls()
        {
            return balls;
        }
    }
}