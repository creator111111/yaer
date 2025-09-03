using UnityEngine;

namespace DebugScene
{
    public class TestJump : MonoBehaviour
    {
        public float jumpForce;
        public float startForce;
        public float downForce;
        public float xVelocity;
        private Animator ani;
        private Rigidbody2D rg;

        // Start is called before the first frame update
        private void Start()
        {
            ani = GetComponent<Animator>();
            rg = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) ani.SetTrigger("Jump");

            if (Input.GetKey(KeyCode.D)) rg.velocity = new Vector2(xVelocity, rg.velocity.y);

            if (Input.GetKey(KeyCode.A)) rg.velocity = new Vector2(-xVelocity, rg.velocity.y);
        }

        private void StartJump()
        {
            rg.AddForce(Vector2.up * startForce);
        }

        private void Jump()
        {
            Debug.Log("jump");
            rg.AddForce(Vector2.up * jumpForce);
        }

        private void Down()
        {
            rg.AddForce(Vector2.right * downForce);
        }
    }
}