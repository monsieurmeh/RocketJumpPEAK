using Photon.Pun;
using pworld.Scripts.Extensions;
using System;
using UnityEngine;

namespace RocketJump
{
    public class RocketLauncher : ItemComponent
    {
        public ParticleSystem gunshotVFX;

        public ParticleSystem fumesVFX;

        public Transform spawnPoint;

        public GameObject hideOnFire;

        public GameObject rocketPrefab;

        public float screenshakeIntensity = 30f;

        public int startAmmo = 1;

        public SFX_Instance[] shotSound;

        public SFX_Instance[] emptySound;
        public float maxDist = 250f;
        public float projectileSpeed = 100f;

        private int Ammo
        {
            get
            {
                return GetData(DataEntryKey.PetterItemUses, GetNew).Value;
            }
            set
            {
                GetData(DataEntryKey.PetterItemUses, GetNew).Value = value;
                item.SetUseRemainingPercentage((float)value / (float)startAmmo);
            }
        }

        public bool HasAmmo => Ammo >= 1;

        private IntItemData GetNew() => new IntItemData { Value = startAmmo };
        

        public override void Awake()
        {
            base.Awake();
            Item obj = item;
            obj.OnPrimaryFinishedCast = (Action)Delegate.Combine(obj.OnPrimaryFinishedCast, new Action(OnPrimaryFinishedCast));
        }

        private void OnDestroy()
        {
            Item obj = item;
            obj.OnPrimaryFinishedCast = (Action)Delegate.Remove(obj.OnPrimaryFinishedCast, new Action(OnPrimaryFinishedCast));
        }


        private void OnPrimaryFinishedCast()
        {
            if (!CanFire()) return;
            gunshotVFX.Play();
            for (int j = 0; j < shotSound.Length; j++)
            {
                shotSound[j].Play(base.transform.position);
            }
            GamefeelHandler.instance.AddPerlinShakeProximity(gunshotVFX.transform.position, screenshakeIntensity, 0.3f);
            hideOnFire.SetActive(HasAmmo);
            Ammo--;
            photonView.RPC("Sync_Rpc", RpcTarget.AllBuffered, HasAmmo);

            Vector3 impactPoint = GetImpactPoint();
            GameObject rocket = PhotonNetwork.Instantiate(rocketPrefab.name, spawnPoint.position, spawnPoint.rotation, 0);
            float flightTime = Vector3.Distance(spawnPoint.position, impactPoint) / projectileSpeed;
            //rocket.GetComponent<RocketProjectile>().photonView.RPC("OnFired", RpcTarget.AllBuffered, impactPoint, flightTime);
            // handle player bounce back
        }

        private bool CanFire()
        {
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
            if (!camRay.Raycast(out RaycastHit terrainHit, HelperFunctions.LayerType.TerrainMap.ToLayerMask() | HelperFunctions.LayerType.CharacterAndDefault.ToLayerMask(), 0f))
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

        public override void OnInstanceDataSet()
        {
            hideOnFire.SetActive(HasAmmo);
            item.SetUseRemainingPercentage((float)Ammo / (float)startAmmo);
        }
    }
}
