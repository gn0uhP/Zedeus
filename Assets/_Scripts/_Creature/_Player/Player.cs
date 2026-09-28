using System.Collections;
using System.Diagnostics.Contracts;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header ("General")]
    private Rigidbody2D rb;
    private Vector3 spawnPos;
    private bool freeze = false;
    private SpriteRenderer spriteRenderer;

    [Header ("GroundChecker")]
    public bool isGrounded = false;
    public LayerMask groundLayer;
    public Vector2 boxCenterOffset;
    public Vector2 boxSize;

    [Header("Movement")]
    private float jumpTime;
    private float fallTime;
    private bool isJumping = false;
    public bool isFacingRight = true;
    public float jumpForce;
    public float speed;
    public bool canWallJump = false;
    public AnimationCurve gravityCurve;

    [Header("Hook")]
    private HingeJoint2D hJoint;
    private LineRenderer lineRenderer;
    private int lastBestPosJoint = -1;
    private int lastBestPosSelected = -1;
    private int bestPos;
    private float lastDistance;
    private float recentDistance;
    private Vector3 jointPos;
    private bool sticked = false;
    public GameObject anchor;
    public float ropeGravity = 2f;
    public float airGravity = 1.5f;
    public float factorX = 1.2f;
    public float factorY = 1f;

    [Header("LightingDash")]
    private Vector3 dashDirection;
    private int bestBall;
    private int lastBall = -1;
    public GameObject lightBall;
    public bool dashed;

    [Header("Particle")]
    public ParticleSystem particle;
    public ParticleSystem hemisphereParticle;
    private ParticleSystem bloodFront;
    private ParticleSystem bloodBack;
    private Vector3 pos;
    private Quaternion rotationFront;
    private Quaternion rotationBack;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hJoint = GetComponent<HingeJoint2D>();
        lineRenderer = GetComponent<LineRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        hJoint.enabled = false;
        OnInit();
    }

    // Update is called once per frame
    void Update()
    {

        if (freeze)
        {
            Debug.Log("you die");
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            return;
        }


        //Jump
        isGrounded = CheckGround();

        if (Input.GetKey(KeyCode.R))
        {
            LevelReset();
        }
        if (isGrounded)
        {
            fallTime = 0f;
            rb.gravityScale = 1.5f;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartJumping();
            }
            if(!isJumping)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        if (isJumping && !sticked)
        {
            OnJumping();
        }

        if(Input.GetKeyUp(KeyCode.Space) && !sticked && isJumping)
        {
            if(rb.linearVelocityY > 0.1f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * 0.5f);
            }
            isJumping = false;
            rb.gravityScale = airGravity;
            
        }
        if(rb.linearVelocityY < 0.1 && !canWallJump)
        {
            fallTime += Time.deltaTime;
            Falling();
        }
        
        

        //hook
        bestPos = -1;

            lastDistance = float.MaxValue;

        for(int i = 0; i < anchor.transform.childCount; i++)
        {
            recentDistance = Vector2.Distance(transform.position, anchor.transform.GetChild(i).position);
            if (recentDistance < lastDistance && recentDistance < anchor.transform.GetChild(i).GetComponent<Anchor>().jointRadius)
            {
                lastDistance = recentDistance;
                bestPos = i;
            }

        }



        dashDirection = Vector3.zero;
        bestBall = -1;
        for(int i = 0; i < lightBall.transform.childCount; i++)
        {
            recentDistance = Vector2.Distance(transform.position, lightBall.transform.GetChild(i).transform.position);
            if(recentDistance < lightBall.transform.GetChild(i).GetComponent<LightBall>().jointRadius)
            {
                dashDirection = (lightBall.transform.GetChild(i).transform.position - transform.position).normalized;
                bestBall = i;
                
            }
        }

        CheckInput();



        if (sticked)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, anchor.transform.GetChild(lastBestPosSelected).transform.position);

        }

        
        lastBestPosSelected = bestPos;
        lastBall = bestBall;

    }
    
    private void OnInit()
    {
        spawnPos = transform.position;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

    }
    private void LevelReset()
    {
        if (!isFacingRight)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        spriteRenderer.enabled = true;
        transform.position = spawnPos;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 1.5f;
        freeze = false;
    }

    private void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && bestPos != -1 && !sticked)
        {
            
            lineRenderer.enabled = true;
            hJoint.enabled = true;
            rb.gravityScale = ropeGravity;
            hJoint.connectedBody = anchor.transform.GetChild(bestPos).transform.GetChild(0).GetComponent<Rigidbody2D>();
            
            jointPos = anchor.transform.GetChild(bestPos).transform.position;

            lastBestPosJoint = bestPos;
            rb.angularVelocity = 0f;
            sticked = !sticked;
        }
        if (Input.GetKeyUp(KeyCode.Space) && sticked)
        {
            hJoint.enabled = false;
            lineRenderer.enabled = false;
            rb.linearVelocity = new Vector2(rb.linearVelocityX * factorX, rb.linearVelocityY * factorY);
            rb.gravityScale = airGravity;

            rb.AddTorque(-rb.linearVelocity.magnitude);
            sticked = !sticked;

        }

        if (Input.GetKeyDown(KeyCode.Space) && dashDirection != Vector3.zero && bestBall != -1 && dashed == false) 
        {
            rb.linearVelocity = dashDirection * lightBall.transform.GetChild(bestBall).GetComponent<LightBall>().dashForce;
            dashed = true;
            StartCoroutine(Dashing());
        }
    }
    private bool CheckGround()
    {
        Vector2 boxCenter = (Vector2)transform.position + boxCenterOffset;
        Collider2D hit = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayer);
        return hit != null;
    }

    private void Falling()
    {
        if(fallTime <= 1f)
        {
            rb.gravityScale = gravityCurve.Evaluate(fallTime);
        }
        else
        {
            rb.gravityScale = gravityCurve.Evaluate(1f);
        }

    }

    private void StartJumping()
    {
        isJumping = true;
        rb.linearVelocity = new Vector2(0.8f, 1) * jumpForce;
        jumpTime = 0f;
    }

    private void OnJumping()
    {
        jumpTime += Time.fixedDeltaTime;
        if(jumpTime <= 1f)
        {
            rb.gravityScale = gravityCurve.Evaluate(jumpTime);
            
        }
        else
        {
            isJumping = false;
            rb.gravityScale = airGravity;
        }

    }

    private void StartParticle()
    {
        bloodFront.Play();
        bloodBack.Play();
    }

    IEnumerator Dashing()
    {
        float time = 0f;
        float dashTime = 0.5f;
        while (time <=  dashTime)
        {
            time += Time.deltaTime;
            rb.gravityScale = 0f;
            yield return null;
        }

        if(time > dashTime)
        {
            dashed = false;
            rb.gravityScale = airGravity;
        }
    }



    private void OnDrawGizmos()
    {
        Vector2 boxCenter = (Vector2)transform.position + boxCenterOffset;
        Gizmos.DrawWireCube(boxCenter, boxSize);
        Gizmos.color = Color.green;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "CheckPoint")
        {
            spawnPos = transform.position;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Spike") || collision.gameObject.CompareTag("Border"))
        {
            
            freeze = true;
            spriteRenderer.enabled = false;
            Invoke(nameof(LevelReset), 1f);
            ContactPoint2D contact = collision.contacts[0];
            Vector2 normal = contact.normal;
            pos = contact.point;
            float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
            rotationFront = Quaternion.Euler(0, 0, angle - 90f);
            rotationBack = Quaternion.Euler(0, 0, angle - 180f);
            bloodFront = Instantiate(particle, pos, rotationFront);
            bloodBack = Instantiate(hemisphereParticle, pos, rotationBack);
            Invoke(nameof(StartParticle), 0f);
        }

    }

}
