using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public Rigidbody rb;
    public float speed = 15;

    private bool isTravelling;
    private Vector3 travelDirection;
    private Vector3 nextCollisionPosition;

    public int minSwipeRecognition = 500;
    private Vector2 swipePosCurrentFrame;
    private Vector2 swipePosLastFrame;
    private Vector2 currentSwipe;

    private Color solveColor;

    private AudioSource playerAudio;
    public AudioClip hitSound;

    public ParticleSystem dirtParticle;

    private void Start()
    {
        solveColor = Random.ColorHSV(0.5f, 1);
        GetComponent<MeshRenderer>().material.color = solveColor;

        playerAudio = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        if (isTravelling)
        {
            rb.velocity = speed * travelDirection;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position - (Vector3.up / 2), 0.05f);
        int i = 0;
        while(i < hitColliders.Length)
        {
            GroundPiece ground = hitColliders[i].transform.GetComponent<GroundPiece>();
            if(ground && !ground.isColored)
            {
                ground.ChangeColor(solveColor);
            }
            i++;
        }

        if (nextCollisionPosition != Vector3.zero)
        {
            if(Vector3.Distance(transform.position, nextCollisionPosition) < 1)
            {
                isTravelling = false;
                travelDirection = Vector3.zero;
                nextCollisionPosition = Vector3.zero;
                playerAudio.PlayOneShot(hitSound, 1.0f);
                dirtParticle.Stop();
            }
        }

        if (isTravelling)
            return;

        if (Input.GetMouseButton(0))
        {
            swipePosCurrentFrame = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            if (swipePosLastFrame != Vector2.zero)
            {
                currentSwipe = swipePosCurrentFrame - swipePosLastFrame;
                var particleShape = dirtParticle.shape;

                if (currentSwipe.sqrMagnitude < minSwipeRecognition)
                {
                    return;
                }

                currentSwipe.Normalize();

                if(currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
                {
                    // Go UP/DOWN
                    SetDestination(currentSwipe.y > 0 ? Vector3.forward : Vector3.back);
                    dirtParticle.Play();
                }

                if (currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
                {
                    // Go Left/Right
                    SetDestination(currentSwipe.x > 0 ? Vector3.right : Vector3.left);                    
                    dirtParticle.Play();
                }
            }

            swipePosLastFrame = swipePosCurrentFrame;
        }

        if (Input.GetMouseButtonUp(0))
        {
            swipePosLastFrame = Vector2.zero;
            currentSwipe = Vector2.zero;
        }
    }

    private void SetDestination(Vector3 direction)
    {
        travelDirection = direction;

        RaycastHit hit;
        if(Physics.Raycast(transform.position, direction, out hit, 100f))
        {
            nextCollisionPosition = hit.point;
        }

        isTravelling = true;
    }
}
