using UnityEngine;

public class JointDistanceLock : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Player player;
    private float distance;
    public bool playerSticked = false;

    private void Start()
    {
        player = FindFirstObjectByType<Player>();

    }

    // Update is called once per frame
    private void Update()
    {



        if (playerSticked)
        {
            
            Vector3 dir = (player.transform.position - transform.position).normalized;

            player.transform.position = transform.position + dir * distance;

        }
    }

    public void DistanceCal()
    {
        distance = Vector3.Distance(transform.position, player.transform.position);
    }
}
