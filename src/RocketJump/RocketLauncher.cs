using Photon.Pun;
using pworld.Scripts.Extensions;
using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.VirtualTexturing;
using System.Linq.Expressions;
using BepInEx.Logging;

namespace RocketJump
{
    public class RocketLauncher : ItemComponent
    {
        private static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("RocketLauncher");
        public ParticleSystem gunshotVFX;

        public ParticleSystem fumesVFX;

        public Transform spawnPoint;

        public GameObject hideOnFire;

        public GameObject rocketPrefab;

        public float screenshakeIntensity = 30f;

        public int startAmmo = 3;

        public SFX_Instance[] shotSound;

        public SFX_Instance[] emptySound;
        public float maxDist = 250f;
        public float projectileSpeed = 100f;
        public float blastForce = 100;

        private bool initialized = false;
        private bool initializing = false;
        private float baseDrag;

        public bool Initialized => initialized;


        private int Ammo
        {
            get
            {
                return GetData(DataEntryKey.ItemUses | DataEntryKey.InstanceID, GetNew).Value;
            }
            set
            {
                GetData(DataEntryKey.ItemUses | DataEntryKey.InstanceID, GetNew).Value = value;
                item.SetUseRemainingPercentage((float)value / (float)startAmmo);
            }
        }

        public bool HasAmmo => Ammo >= 1;

        private IntItemData GetNew() => new IntItemData { Value = startAmmo };

        public override void OnInstanceDataSet() => StartCoroutine(InitCoroutine());

        public void Start() => OnInstanceDataSet();


        internal void MaybeFire()
        {
            try
            {
                logger.LogInfo("Maybe firing");
                if (!CanFire()) return;
                logger.LogInfo("Firing");
                if (gunshotVFX != null) gunshotVFX.Play();
                for (int j = 0; j < shotSound.Length; j++)
                {
                    if (shotSound[j] != null) shotSound[j].Play(base.transform.position);
                }
                hideOnFire.SetActive(HasAmmo);
                Ammo--;
                photonView.RPC("Sync_Rpc", RpcTarget.AllBuffered, HasAmmo);


                GamefeelHandler.instance.AddPerlinShakeProximity(transform.position, screenshakeIntensity, 0.3f);
                Vector3 impactPoint = GetImpactPoint();
                Vector3 origin = transform.position;
                Vector3 inverseImpactVector = origin - impactPoint;
                //GameObject rocket = PhotonNetwork.Instantiate(rocketPrefab.name, spawnPoint.position, spawnPoint.rotation, 0);  
                //float flightTime = Vector3.Distance(spawnPoint.position, impactPoint) / projectileSpeed;
                Character.localCharacter.refs.movement.drag = 0.99f;
                Character.localCharacter.AddForce(inverseImpactVector * blastForce);
                Character.localCharacter.data.isGrounded = false;
                Character.localCharacter.data.isJumping = true;
                Character.localCharacter.data.sinceJump = 0f;
                Character.localCharacter.StartCoroutine(AdjustDrag());
                //rocket.GetComponent<RocketProjectile>().photonView.RPC("OnFired", RpcTarget.AllBuffered, impactPoint, flightTime);
            } catch (Exception e)
            {
                logger.LogError(e);
            }
        }

        private IEnumerator AdjustDrag()
        {
            yield return null;
            Character yoinked = Character.localCharacter;
            while (!yoinked.data.isGrounded)
            {
                yield return null;
            }
            yoinked.refs.movement.drag = baseDrag;
        }

        internal bool CanFire()
        {
            if (initializing || !initialized) return false;
            if (!HasAmmo)
            {
                fumesVFX.Play();
                for (int i = 0; i < emptySound.Length; i++)
                {
                    emptySound[i].Play(base.transform.position);
                }
                return false;
            }
            return true;
        }

        private Vector3 GetImpactPoint()
        {
            Ray camRay = Camera.main.ForwardRay();
            if (!camRay.Raycast(out RaycastHit terrainHit, HelperFunctions.LayerType.TerrainMap.ToLayerMask(), 0f))
            {
                return Camera.main.transform.position + (camRay.direction.normalized * maxDist);
            }
            if (Vector3.Distance(terrainHit.point, Camera.main.transform.position) > maxDist)
            {
                return Camera.main.transform.position + (camRay.direction.normalized * maxDist);
            }
            return terrainHit.point;
        }

        [PunRPC]
        private void Sync_Rpc(bool show)
        {
            hideOnFire.SetActive(show);
        }



        private IEnumerator InitCoroutine()
        {
            logger.LogInfo("Starting wait for valid character");
            while (!Character.localCharacter)
            {
                yield return null;
            }

            if (initializing || initialized) yield break;
            initializing = true;
            logger.LogInfo("Initializing");
            for (int i = 0, iMax = transform.childCount; i < iMax; i++)
            {
                GameObject childObject = transform.GetChild(i).gameObject;
                switch (childObject.name)
                {
                    case "HideOnFire": hideOnFire = gameObject; break;
                    case "Launcher":
                        foreach (ParticleSystem particleSystem in childObject.GetComponentsInChildren<ParticleSystem>())
                        {
                            if (particleSystem.name == "VFX_Fumes") fumesVFX = particleSystem;
                            if (particleSystem.name == "VFX_Gunshot") gunshotVFX = particleSystem;
                        }
                        break;
                }
            }

            baseDrag = Character.localCharacter.refs.movement.drag;
            hideOnFire.SetActive(HasAmmo);
            item.SetUseRemainingPercentage((float)Ammo / (float)startAmmo);
            logger.LogInfo("Initialized");

            initializing = false;
            initialized = true;
        }
    }
}
