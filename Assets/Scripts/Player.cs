using UnityEngine;
using Fusion;
using Fusion.Addons.KCC;
using System;

public enum AbilityMode : byte
{
    BreakBlock,
    Cage,
    Shove
}

public class Player : NetworkBehaviour
{
    [SerializeField] private MeshRenderer[] modelParts;
    [SerializeField] private LayerMask lagCompLayers;
    [SerializeField] private KCC kcc;
    [SerializeField] private Transform camTarget;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip shoveSound;
    [SerializeField] private Cage cagePrefab;
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private Vector3 jumpImpulse = new(0f, 10f, 0f);
    [SerializeField] private float doubleJumpMultiplier = 0.75f;

    [SerializeField] private float breakBlockCD = 1.25f;
    [SerializeField] private float cageCD = 10f;
    [SerializeField] private float shoveCD = 2f;

    [SerializeField] private float doubleJumpCD = 5f;

    [SerializeField] private float grappleCD = 2F;
    [SerializeField] private float grappleStrength = 12f;
    [SerializeField] private float shoveStrength = 20f;
    [field: SerializeField] public float AbilityRange { get; private set; } = 25f;

    public float BreakCDFactor => (BreakBlockCD.RemainingTime(Runner) ?? 0f) / breakBlockCD;
    public float CageCDFactor => (CageCD.RemainingTime(Runner) ?? 0f) / cageCD;
    public float ShoveCDFactor => (ShoveCD.RemainingTime(Runner) ?? 0f) / shoveCD;

    public float GrappleCDFactor => (GrappleCD.RemainingTime(Runner) ?? 0f) / grappleCD;

    public float DoubleJumpCDFactor => (DoubleJumpCD.RemainingTime(Runner) ?? 0f / doubleJumpCD);

    public double Score => Math.Round(transform.position.y, 1);

    public bool isReady; //Server is the only one who cares about this
    public AbilityMode SelectedAbility { get; private set; }

    [Networked] public string Name { get; private set; }

    [Networked] public bool IsCaged { get; set; }

    [Networked] TickTimer BreakBlockCD { get; set; }
    [Networked] TickTimer CageCD { get; set; }
    [Networked] TickTimer ShoveCD { get; set; }

    [Networked] TickTimer GrappleCD { get; set; }
    [Networked] TickTimer DoubleJumpCD { get; set; }
    [Networked] private NetworkButtons PreviousButtons { get; set; }

    [Networked, OnChangedRender(nameof(Jumped))] private int JumpSync { get; set; }

    [Networked, OnChangedRender(nameof(Shoved))] private int ShoveSync { get; set; }

    private InputManager inputmanager;
    private Vector2 baseLookRotation;

    public Transform PlayerHandPos;
    public LineRendererController LineController;

    //Call when object has been properly initialized
    public override void Spawned()
    {

        //HasInputAuthority means the player himself which is the one playing infont the pc
        if (HasInputAuthority)
        {
            foreach(MeshRenderer renderer in modelParts)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            inputmanager = Runner.GetComponent<InputManager>();
            inputmanager.LocalPlayer = this;

            Name = PlayerPrefs.GetString("Photon.Menu.Username");
            RPC_PlayerName(Name);
            CameraFollow.Singleton.SetTarget(camTarget, this);
            UIManager.Singleton.LocalPlayer = this;
            kcc.Settings.ForcePredictedLookRotation = true;
        }
        else
        {
           
        }

        //Spawn VFX
        Vector3 spawnPosition;

        SFX sfx = SFXManager.Instance.OutSFX("PlayerSpawn");

        spawnPosition = transform.position;
        GameObject vfxInstance = Instantiate(PlayerVFX.Instance.OutVfx("PlayerSpawn"), spawnPosition, Quaternion.identity);

        source.PlayOneShot(sfx.clip, sfx.volume);

        Destroy(vfxInstance, 2f);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (HasInputAuthority)
        {
            CameraFollow.Singleton.SetTarget(null, this);
            UIManager.Singleton.LocalPlayer = null;
        }
    }


    //Fusion's version of Unity FixedUpdate
    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetInput input))
        {
            SelectedAbility = input.AbilityMode;
            CheckJump(input);
            kcc.AddLookRotation(input.LookDelta * lookSensitivity, -maxPitch, maxPitch);
            UpdateCamTarget();
            Vector3 lookDirection = camTarget.forward;

            if (input.Buttons.WasPressed(PreviousButtons, InputButton.Grapple))
            {
                TryGrapple(lookDirection);
            }

            SetInputDirection(input);
            CheckAbilities(input,lookDirection);
            PreviousButtons = input.Buttons;
            baseLookRotation = kcc.GetLookRotation();
        }
    }

    public override void Render()
    {
        if (kcc.Settings.ForcePredictedLookRotation)
        {
            Vector2 predictedLookRotation = baseLookRotation + inputmanager.AccumulatedMouseDelta * lookSensitivity;
            kcc.SetLookRotation(predictedLookRotation);
        }

        UpdateCamTarget();
    }

    private void CheckJump(NetInput input)
    {
        if (input.Buttons.WasPressed(PreviousButtons, InputButton.Jump))
        {
            if (kcc.FixedData.IsGrounded)
            {
                kcc.Jump(jumpImpulse);
                JumpSync++;
            }
            else if (DoubleJumpCD.ExpiredOrNotRunning(Runner))
            {
                kcc.Jump(jumpImpulse * doubleJumpMultiplier);
                DoubleJumpCD = TickTimer.CreateFromSeconds(Runner, doubleJumpCD);
                kcc.Jump(jumpImpulse);
            }
        }
    }

    private void SetInputDirection(NetInput input)
    {
        Vector3 worldDirection = kcc.FixedData.TransformRotation * input.Direction.X0Y();
        kcc.SetInputDirection(worldDirection);
    }

    private void UpdateCamTarget()
    {
        camTarget.localRotation = Quaternion.Euler(kcc.GetLookRotation().x, 0f, 0f);
    }

    private void CheckAbilities(NetInput input, Vector3 lookDirection)
    {
        if (!HasStateAuthority || !input.Buttons.WasPressed(PreviousButtons, InputButton.UseAbility))
            return;

        switch (input.AbilityMode)
        {
            case AbilityMode.BreakBlock:
                TryBreakBlock(lookDirection);
                break;
            case AbilityMode.Cage:
                TryCage(lookDirection);
                break;
            case AbilityMode.Shove:
                TryShove(lookDirection);
                break;
            default:
                break;
        }
    }
     
    [Rpc(RpcSources.InputAuthority, RpcTargets.InputAuthority | RpcTargets.StateAuthority)]
    public void RPC_SetReady()
    {
        isReady = true;
        if (HasInputAuthority)
        {
            UIManager.Singleton.DidSetReady();
        }
    }

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        kcc.SetPosition(position);
        kcc.SetLookRotation(rotation);   
    }

    public void RestCooldowns()
    {
        BreakBlockCD = TickTimer.None;
        CageCD = TickTimer.None;
        ShoveCD = TickTimer.None;
        GrappleCD = TickTimer.None;
        DoubleJumpCD = TickTimer.None;
    }

    private void TryBreakBlock(Vector3 lookDirection)
    {
        if(BreakBlockCD.ExpiredOrNotRunning(Runner) && Physics.Raycast(camTarget.position, lookDirection, out RaycastHit hitInfo, AbilityRange))
        {
            if(hitInfo.collider.TryGetComponent(out Block block))
            {
                BreakBlockCD = TickTimer.CreateFromSeconds(Runner, breakBlockCD);
                block.Disable();
            }
        }
    }

    private void TryCage(Vector3 lookDirection)
    {
        if(CageCD.ExpiredOrNotRunning(Runner) && Runner.LagCompensation.Raycast(camTarget.position, lookDirection, AbilityRange, Object.InputAuthority, out LagCompensatedHit hit, lagCompLayers, HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority))
        {
            if(hit.Hitbox != null && hit.Hitbox.TryGetComponent(out Player other))
            {
                CageCD = TickTimer.CreateFromSeconds(Runner, cageCD);
                other.Cage();
            }
        }
    }
    
    private void TryShove(Vector3 lookDirection)
    {
        if (ShoveCD.ExpiredOrNotRunning(Runner) && Runner.LagCompensation.Raycast(camTarget.position, lookDirection, AbilityRange, Object.InputAuthority, out LagCompensatedHit hit, lagCompLayers, HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority))
        {
            if (hit.Hitbox != null && hit.Hitbox.TryGetComponent(out Player other))
            {
                ShoveCD = TickTimer.CreateFromSeconds(Runner, shoveCD);
                other.Shove(lookDirection, shoveStrength);

                if (Runner.IsForward)
                {
                    // If we are the Host (StateAuthority), we tell everyone (All) to play the VFX
                    if (Object.HasStateAuthority)
                    {
                        RPC_SpawnVFX(hit.Point, "PlayerSmash");
                    }
                }
            }
        }
    }

    private void TryGrapple(Vector3 lookDirection)
    {
        if(GrappleCD.ExpiredOrNotRunning(Runner) && Physics.Raycast(camTarget.position, lookDirection, out RaycastHit hitInfo, AbilityRange))
        {
            if(hitInfo.collider.TryGetComponent(out Block _))
            {
                GrappleCD = TickTimer.CreateFromSeconds(Runner, grappleCD);
                Vector3 grappleVector = Vector3.Normalize(hitInfo.point - transform.position);
                if(grappleVector.y > 0f)
                {
                    grappleVector = Vector3.Normalize(grappleVector + Vector3.up);
                }

                //Debug.Log($"Player : {Name} , HandsPos : {PlayerHandPos.position}, hitInfo :{hitInfo.transform.position}");
                if (Object.HasStateAuthority)
                    LineController.TriggerLineRendererRPC(PlayerHandPos.position, hitInfo.transform.position);

                kcc.Jump(grappleVector * grappleStrength);
            }
        }
    }

    // RpcSources.StateAuthority = Only the Host is allowed to trigger this
    // RpcTargets.All = This code will run on The Host AND all Clients
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SpawnVFX(Vector3 hitPoint, string vfxname)
    {
        // This runs on everyone's computer individually
        GameObject vfx = Instantiate(PlayerVFX.Instance.OutVfx(vfxname), hitPoint, Quaternion.identity);

        // Cleanup purely local object
        Destroy(vfx, 1.5f);
    }

    private void Cage()
    {
        Runner.Spawn(cagePrefab, transform.position, Quaternion.identity, Object.InputAuthority).Init(this);
        IsCaged = true;
    }

    private void Shove(Vector3 direction, float strength)
    {
        kcc.AddExternalImpulse(direction * strength);
        ShoveSync++;
    }

    private void Jumped()
    {
        source.Play();
    }

    private void Shoved()
    {
        source.PlayOneShot(shoveSound);
    }

    //The host loading all the name in the lobby and tell everyone everyone's name
    //so that whether they join late, dc or reconnected will still get the name, since the host have saving all the user name
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PlayerName(string name)
    {
        Name = name;
    }

}
