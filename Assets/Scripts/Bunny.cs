﻿using UnityEngine;
using System.Collections;
using System.Linq;
using Assets.Scripts;

public class Bunny : MonoBehaviour
{

    //arrow sound found here
    //https://www.freesound.org/people/Erdie/sounds/65734/

    public Transform ArrowSpawnPosition;
    public GameObject ArrowPrefab;
    public float ShootWaitTime = 2f;
    private float LastShootTime = 0f;
    GameObject targetedEnemy;
    private float InitialArrowForce = 500f;

    // Use this for initialization
    void Start()
    {
        State = BunnyState.Inactive;
        ArrowSpawnPosition = transform.FindChild("ArrowSpawnPosition");
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.FinalRound &&
            GameManager.Instance.Enemies.Where(x => x != null).Count() == 0)
            State = BunnyState.Inactive;

        if (State == BunnyState.Searching)
        {
            if (GameManager.Instance.Enemies.Where(x => x != null).Count() == 0) return;

            //aggregate method proposed here
            //http://unitygems.com/linq-1-time-linq/
            targetedEnemy = GameManager.Instance.Enemies.Where(x => x != null)
           .Aggregate((current, next) => Vector2.Distance(current.transform.position, transform.position)
               < Vector2.Distance(next.transform.position, transform.position)
              ? current : next);

            if (targetedEnemy != null && targetedEnemy.activeSelf
                && Vector3.Distance(transform.position, targetedEnemy.transform.position)
                < Constants.MinDistanceForBunnyToShoot)
            {
                State = BunnyState.Targeting;
            }

        }
        else if (State == BunnyState.Targeting)
        {
            if (targetedEnemy != null 
                && Vector3.Distance(transform.position, targetedEnemy.transform.position)
                    < Constants.MinDistanceForBunnyToShoot)
            {
                LookAndShoot();
            }
            else
            {
                State = BunnyState.Searching;
            }
        }
    }

    private void LookAndShoot()
    {
        //look at the enemy
        Quaternion diffRotation = Quaternion.LookRotation
            (transform.position - targetedEnemy.transform.position, Vector3.forward);
        transform.rotation = Quaternion.RotateTowards
            (transform.rotation, diffRotation, Time.deltaTime * 2000);
        transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);

        //make sure we're almost looking at the enemy before start shooting
        Vector2 direction = targetedEnemy.transform.position - transform.position;
        float axisDif = Vector2.Angle(transform.up, direction);

        if (axisDif <= 20f)
        {
            if (Time.time - LastShootTime > ShootWaitTime)
            {
                Shoot(direction);
                LastShootTime = Time.time;
            }

        }
    }


    private void Shoot(Vector2 dir)
    {
        if (targetedEnemy != null && targetedEnemy.activeSelf
            && Vector3.Distance(transform.position, targetedEnemy.transform.position)
                    < Constants.MinDistanceForBunnyToShoot)
        {
            GameObject go = ObjectPoolerManager.Instance.ArrowPooler.GetPooledObject();
            go.transform.position = ArrowSpawnPosition.position;
            go.transform.rotation = transform.rotation;
            go.SetActive(true);
                //Instantiate(ArrowPrefab, ArrowSpawnPosition.position, transform.rotation) as GameObject;
            go.GetComponent<Rigidbody2D>().AddForce(dir * InitialArrowForce);
            AudioManager.Instance.PlayArrowSound();
        }
        else
        {
            State = BunnyState.Searching;
        }


    }
    private BunnyState State;


    public void Activate()
    {
        State = BunnyState.Searching;
    }
}
