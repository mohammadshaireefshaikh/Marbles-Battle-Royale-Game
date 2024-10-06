using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class WeaponHandler : NetworkBehaviour
{
    [Header("Prefabs")]
    public GrenadeHandler grenadePrefab;
    public RocketHandler rocketPrefab;

    [Header("Effects")]
    public ParticleSystem fireParticleSystem;
    public ParticleSystem fireParticleSystemRemotePlayer;

    [Header("Aim")]
    public Transform aimPoint;

    [Header("Collision")]
    public LayerMask collisionLayers;

    [Networked]
    public bool isFiring { get; set; }

    ChangeDetector changeDetector;

    float lastTimeFired = 0;

    float aiFireRate = 1.5f;

    float maxHitDistance = 200;

    //Timing
    TickTimer grenadeFireDelay = TickTimer.None;
    TickTimer rocketFireDelay = TickTimer.None;

    //Other components
    HPHandler hpHandler;
    NetworkPlayer networkPlayer;
    NetworkObject networkObject;

    private void Awake()
    {
        hpHandler = GetComponent<HPHandler>();
        networkPlayer = GetBehaviour<NetworkPlayer>();
        networkObject = GetComponent<NetworkObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        if (hpHandler.isDead)
            return;

        //Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            if (networkInputData.isFireButtonPressed)
                Fire(networkInputData.aimForwardVector, networkInputData.cameraPosition);

           /* if (networkInputData.isGrenadeFireButtonPressed)
                FireGrenade(networkInputData.aimForwardVector);
           */
            if (networkInputData.isRocketLauncherFireButtonPressed)
                FireRocket(networkInputData.aimForwardVector, networkInputData.cameraPosition);
        }

        if (networkPlayer.isBot && Object.HasStateAuthority)
        {
            Fire(transform.forward + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)), transform.position);
        }
    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(isFiring):
                    var boolReader = GetPropertyReader<bool>(nameof(isFiring));
                    var (previousBool, currentBool) = boolReader.Read(previousBuffer, currentBuffer);
                    OnFireChanged(previousBool, currentBool);
                    break;
            }
        }
    }

    void Fire(Vector3 aimForwardVector, Vector3 cameraPosition)
    {
        //Limit fire rate
        if (Time.time - lastTimeFired < 0.15f)
            return;

        //Limit fire for bots even more
        if (networkPlayer.isBot && Time.time - lastTimeFired < aiFireRate)
            return;


        StartCoroutine(FireEffectCO());

        HPHandler hitHPHandler = CalculateFireDirection(aimForwardVector, cameraPosition, out Vector3 fireDirection);

        if(hitHPHandler != null && Object.HasStateAuthority)
            hitHPHandler.OnTakeDamage(networkPlayer.nickName.ToString(), 1); ;

        lastTimeFired = Time.time;

        //Make the AI fire a bit more randomly 
        aiFireRate = Random.Range(0.1f, 1.5f);
    }

    HPHandler CalculateFireDirection(Vector3 aimForwardVector, Vector3 cameraPosition, out Vector3 fireDirection)
    {
        LagCompensatedHit hitinfo = new LagCompensatedHit();

        fireDirection = aimForwardVector;
        float hitDistance = maxHitDistance;

        //Do a raycast from the 3rd person camera
        if (networkPlayer.is3rdPersonCamera)
        {
            Runner.LagCompensation.Raycast(cameraPosition, fireDirection, hitDistance, Object.InputAuthority, out hitinfo, collisionLayers, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX);

            //Check against other players
            if (hitinfo.Hitbox != null)
            {
                fireDirection = (hitinfo.Point - aimPoint.position).normalized;
                hitDistance = hitinfo.Distance;

                Debug.DrawRay(cameraPosition, aimForwardVector * hitDistance, new Color(0.4f, 0, 0), 1);
            }
            //Check aginst PhysX colliders if we didn't hit a player
            else if (hitinfo.Collider != null)
            {
                fireDirection = (hitinfo.Point - aimPoint.position).normalized;
                hitDistance = hitinfo.Distance;

                Debug.DrawRay(cameraPosition, aimForwardVector * hitDistance, new Color(0, 0.4f, 0), 1);
            }
            else
            {
                Debug.DrawRay(cameraPosition, fireDirection * hitDistance, Color.gray, 1);

                fireDirection = ((cameraPosition + fireDirection * hitDistance) - aimPoint.position).normalized;
            }
        }

        //Reset hit distance
        hitDistance = maxHitDistance;

        //Check if we hit anything with the fire
        Runner.LagCompensation.Raycast(aimPoint.position, fireDirection, maxHitDistance, Object.InputAuthority, out hitinfo, collisionLayers, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX);

        //Check against other players
        if (hitinfo.Hitbox != null)
        {
            hitDistance = hitinfo.Distance;
            HPHandler hitHPHandler = null;

            if (Object.HasStateAuthority)
            {
                hitHPHandler = hitinfo.Hitbox.transform.root.GetComponent<HPHandler>();
                Debug.DrawRay(aimPoint.position, fireDirection * hitDistance, Color.red, 1);

                return hitHPHandler;
            }
        }
        //Check aginst PhysX colliders if we didn't hit a player
        else if (hitinfo.Collider != null)
        {
            hitDistance = hitinfo.Distance;

            Debug.DrawRay(aimPoint.position, fireDirection * hitDistance, Color.green, 1);
        }
        else Debug.DrawRay(aimPoint.position, fireDirection * hitDistance, Color.black, 1);

        return null;
    }

    /*void FireGrenade(Vector3 aimForwardVector)
    {
        //Check that we have not recently fired a grenade. 
        if (grenadeFireDelay.ExpiredOrNotRunning(Runner))
        {
            Runner.Spawn(grenadePrefab, aimPoint.position + aimForwardVector * 1.5f, Quaternion.LookRotation(aimForwardVector), Object.InputAuthority, (runner, spawnedGrenade) =>
            {
                spawnedGrenade.GetComponent<GrenadeHandler>().Throw(aimForwardVector * 15, Object.InputAuthority, networkPlayer.nickName.ToString());
            });

            //Start a new timer to avoid grenade spamming
            grenadeFireDelay = TickTimer.CreateFromSeconds(Runner, 1.0f);
        }
    }*/

    void FireRocket(Vector3 aimForwardVector, Vector3 cameraPosition)
    {
        //Check that we have not recently fired a grenade. 
        if (rocketFireDelay.ExpiredOrNotRunning(Runner))
        {
            CalculateFireDirection(aimForwardVector, cameraPosition, out Vector3 fireDirection);

            Runner.Spawn(rocketPrefab, aimPoint.position + fireDirection * 1.5f, Quaternion.LookRotation(fireDirection), Object.InputAuthority, (runner, spawnedRocket) =>
            {
                spawnedRocket.GetComponent<RocketHandler>().Fire(Object.InputAuthority, networkObject,  networkPlayer.nickName.ToString());
            });

            //Start a new timer to avoid grenade spamming
            rocketFireDelay = TickTimer.CreateFromSeconds(Runner, 3.0f);
        }
    }

    IEnumerator FireEffectCO()
    {
        isFiring = true;

        if (networkPlayer.is3rdPersonCamera)
            fireParticleSystemRemotePlayer.Play();
        else fireParticleSystem.Play();


        yield return new WaitForSeconds(0.09f);

        isFiring = false;
    }


    void OnFireChanged(bool previous, bool current)
    {
        if (current && !previous)
            OnFireRemote();

    }

    void OnFireRemote()
    {
        if (!Object.HasInputAuthority)
            fireParticleSystemRemotePlayer.Play();
    }

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }
}
