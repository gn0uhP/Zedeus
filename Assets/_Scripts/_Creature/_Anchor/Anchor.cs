using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Anchor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject dashLine;

    public AnimationCurve curve;
    public float animTime;
    public float jointRadius;



    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, jointRadius);
        dashLine.transform.localScale = new Vector3(jointRadius * 0.55f, jointRadius * 0.55f, dashLine.transform.position.z);

    }

}
