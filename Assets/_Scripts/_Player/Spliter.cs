using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Spliter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private List<ParticleCollisionEvent> collisionEvent;
    private ParticleSystem splasher;
    private Color playerColor;
    public GameObject image;
    public bool scratch = false;
    public Player player;
    

    void Start()
    {
        if(player == null)
        {
            player = FindFirstObjectByType<Player>();
        }
        splasher = GetComponent<ParticleSystem>();
        collisionEvent = new List<ParticleCollisionEvent>();
        playerColor = player.GetComponent<SpriteRenderer>().color;
        var mainModule = splasher.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(playerColor);
    }

    // Update is called once per frame
    private void OnParticleCollision(GameObject other)
    {
        if (!other.CompareTag("Player"))
        {
            image.GetComponent<SpriteRenderer>().color = playerColor;

            int numCollisionParticle = splasher.GetCollisionEvents(other, collisionEvent);

            for (int i = 0; i < numCollisionParticle; i++)
            {
                Vector3 normal = collisionEvent[i].normal;

                Vector3 pos = collisionEvent[i].intersection - (normal * 0.5f);

                float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Deg2Rad;
                Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
                GameObject splash = Instantiate(image, pos, rotation);
                splash.transform.SetParent(other.transform, true);
                if (scratch)
                {
                    float random = Random.Range(1f, 2f);
                    splash.transform.localScale = new Vector3(
                        splash.transform.localScale.x * random,
                        splash.transform.localScale.y * random,
                        splash.transform.localScale.z
                    );
                }


            }
        }
        
    }
}
