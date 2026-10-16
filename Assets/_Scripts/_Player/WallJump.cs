using UnityEngine;

public class WallJump : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Rigidbody2D rb;
    public Player player;


    private void Start()
    {
       rb = player.GetComponent<Rigidbody2D>();    
    }

    private void Update()
    {
        if (player.canWallJump)
        {
            if(rb.linearVelocityY < 0.1f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, -2f);

            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!player.isGrounded)
                {
                    rb.linearVelocity = new Vector2(player.isFacingRight ? -1 : 1, 1.3f) * player.jumpForce;
                    player.transform.rotation = Quaternion.Euler(new Vector3(0, !player.isFacingRight ? 0 : 180, 0));
                    player.isFacingRight = !player.isFacingRight;
                }
                else
                {
                    rb.linearVelocity = new Vector2(0f, player.jumpForce);
                }
            }
        }
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Wall")
        {
            player.canWallJump = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        player.canWallJump = false;
    }
}
