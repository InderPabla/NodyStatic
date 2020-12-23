using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum VisualFrameType
{
   CREATE , SHOWN, DESTROY, HIDDEN, NONE
}

public enum VisualEyeLine
{
    NO_LINE, ALL_LINE, TOUCH_LINE
}

public interface INodyRayDetail
{
   RaycastHit2D HitRay { get; set; }
}

public class NodyRayDetail : INodyRayDetail
{
    public Material Mat;
    public RaycastHit2D _Hit;
    public NodyStatic Nody;
    
    public NodyRayDetail(NodyStatic nody)
    {
        Nody = nody;
    }

    public virtual RaycastHit2D HitRay
    {
        get
        {
            return _Hit;
        }

        set
        {
            _Hit = value;
        }
    }

    public bool HasHitOccured
    {
        get
        {
            return _Hit.collider != null && _Hit.collider.gameObject!=null;
        }
    }

    public bool HasHitNody
    {
        get
        {
            return HasHitOccured && _Hit.collider.gameObject.layer == NodyData.NodyLayer;
        }
    }

    public bool HasHitFood
    {
        get
        {
            return HasHitOccured && _Hit.collider.gameObject.layer == NodyData.FoodLayer;
        }
    }

    public bool HasHitWall
    {
        get
        {
            return HasHitOccured && _Hit.collider.gameObject.layer == NodyData.BoundaryLayer;
        }
    }
}


public interface INodyCollDetail
{
    Collision2D HitColl { get; set; }
}

public enum NodyColDetailType
{
    NODE, SHELL, SPIKE
}

public class NodyCollDetail : INodyCollDetail
{
    public Material Mat;
    public Collision2D _Coll;
    public NodyColDetailType Type;
    public Collider2D ThisCollider;
   
    public NodyStatic Parent;

    public NodyCollDetail _OtherNodyColDetail;

    public NodyCollDetail(NodyStatic parent, Collider2D thisCollider, NodyColDetailType type)
    {
        Type = type;
        Parent = parent;
        ThisCollider = thisCollider;
    }

    public virtual Collision2D HitColl
    {
        get
        {
            return _Coll;
        }

        set
        {
            _Coll = value;
            _OtherNodyColDetail = null;

            if (HasHitNody)
            {
                NodyStatic OtherNodyHit = _Coll.collider.GetComponent<NodyStatic>();
                int OtherColliderId = _Coll.collider.GetInstanceID();
                _OtherNodyColDetail = OtherNodyHit.GetNodyCollDetailFromId(OtherColliderId);
            }
        }
    }

    public bool HasHitOccured
    {
        get
        {
            return _Coll!=null && _Coll.collider != null;
        }
    }

    public bool HasHitNody
    {
        get
        {
            return HasHitOccured && _Coll.collider.gameObject.layer == NodyData.NodyLayer;
        }
    }

    public bool HasHitFood
    {
        get
        {
            return HasHitOccured && _Coll.collider.gameObject.layer == NodyData.FoodLayer;
        }
    }

    public bool HasHitWall
    {
        get
        {
            return HasHitOccured && _Coll.collider.gameObject.layer == NodyData.BoundaryLayer;
        }
    }
}


public class NodyStaticEye : NodyRayDetail
{

    public LineRenderer EyeLine;
    public NodyEyeMeta EyeMeta;
    public bool HasHitSameSpecies;

    public NodyStaticEye(NodyStatic Nody, NodyEyeMeta eyeMeta) : base(Nody)
    {
        EyeMeta = eyeMeta;
    }

    //@override
    public override RaycastHit2D HitRay
    {

        set
        {
            _Hit = value;
            HasHitSameSpecies = false;

            if (HasHitOccured)
            {
                EyeMeta.CurrentEyeViewDistance01 = _Hit.distance / (NodyData.MaxViewDistance - NodyData.MinViewDistance);
                EyeMeta.HitEnd = _Hit.point;

                if (HasHitNody || HasHitFood)
                    EyeMeta.HitEnd = _Hit.transform.position;

                if (HasHitFood)
                {
                    HitRay.collider.gameObject.GetComponent<Food>().ResetTime();
                }
                else if(HasHitNody)
                {
                    NodyStaticMeta N = HitRay.collider.gameObject.GetComponent<NodyStatic>().Meta;


                    HasHitSameSpecies = Nody.Meta.IsSameSpecies(N);
                }
            }
            else
            {
                EyeMeta.CurrentEyeViewDistance01 = -1;
                EyeMeta.HitEnd = EyeMeta.HitStart + EyeMeta.WorldLookDirectionPerturbed * EyeMeta.ViewDistance;
            }
        }
    }

    public void UpdateEye()
    {
        if (HasHitNody)
        {
            EyeMeta.HitEnd = _Hit.transform.position;
        }
    }
}

public class NodyStaticNode : NodyCollDetail
{

    public NodyNodeStaticMeta NodeMeta;

    public NodyStaticNode(NodyStatic Parent, Collider2D thisCollider, NodyNodeStaticMeta nodeMeta) : base(Parent, thisCollider, NodyColDetailType.NODE)
    {
        NodeMeta = nodeMeta;
    }
}


public class NodyStaticShell : NodyCollDetail
{

    public NodyShellMeta ShellMeta;

    public NodyStaticShell(NodyStatic Parent, Collider2D thisCollider, NodyShellMeta shellMeta) : base(Parent, thisCollider, NodyColDetailType.SHELL)
    {
        ShellMeta = shellMeta;
    }
}

public class NodyStaticSpike : NodyCollDetail
{

    public NodySpikeMeta SpikeMeta;

    public NodyStaticSpike(NodyStatic Parent, Collider2D thisCollider, NodySpikeMeta spikeMeta):base(Parent, thisCollider, NodyColDetailType.SPIKE)
    {
        SpikeMeta = spikeMeta;
    }
}

public class NodyStatic : MonoBehaviour
{
    private NodyStaticMeta _Meta;

    private Text HealthText;

    private Rigidbody2D Rig;
    private LineRenderer MainBodyLine;

    private NodyStaticEye[] EyeArr;
    private NodyStaticNode[] NodeArr;
    private NodyStaticShell[] ShellArr;
    private NodyStaticSpike[] SpikeArr;

    private VisualFrameType FrameType = VisualFrameType.NONE;
    private VisualEyeLine EyeLineType = VisualEyeLine.NO_LINE;
    private bool HideTexts = false;

    private Dictionary<int, NodyCollDetail> ColliderIdToNodeDict;

    private int Animaiton_HealthBlinkerCount = 0;
    private bool Animation_HealthShowTrue = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_Meta.IsDead) return;

        if (FrameType == VisualFrameType.SHOWN)
        {
            UpdateView();

            if (!ShouldShowNody())
            {
                FrameType = VisualFrameType.DESTROY;
            }
        }
        else if (FrameType == VisualFrameType.CREATE)
        {
            CreateVisualAssets();
            FrameType = VisualFrameType.SHOWN;
        }
        else if (FrameType == VisualFrameType.DESTROY)
        {
            DestroyVisualAssets();
            FrameType = VisualFrameType.HIDDEN;
        }
        else if (FrameType == VisualFrameType.HIDDEN)
        {
            if (ShouldShowNody())
            {
                FrameType = VisualFrameType.CREATE;
            }
        }
    }

    void FixedUpdate()
    {
        PreFireNetworks();
        FireNetworks();
        PostFireNetworks();
        PostFixedUpdate();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        NodyCollDetail Node = ColliderIdToNodeDict[collision.otherCollider.GetInstanceID()];

        if (collision.collider.gameObject.layer==NodyData.FoodLayer)
        {
            if (_Meta.Type == NodyType.PRED) return;
            
            //if(Node.Type != NodyColDetailType.SPIKE)
            //{
                if (collision.collider.gameObject && collision.collider.gameObject.activeSelf)
                {
                    NodyData._RemoveFood(collision.collider.gameObject);
                    _Meta.UpdateAteFood();
                }
            //}
        }
        else
        {
           
            Node.HitColl = collision;

            
        }
    }

    public NodyCollDetail GetNodyCollDetailFromId(int ColliderId)
    {
        return ColliderIdToNodeDict[ColliderId];
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        NodyCollDetail Node = GetNodyCollDetailFromId(collision.otherCollider.GetInstanceID());
        Node.HitColl = null;
    }

    private bool ShouldShowNody()
    {
        return Time.timeScale < NodyData.HideAssetsAtTimeScale && IsNodyOnScreen;
    }

    private bool IsNodyNearScreenCenter
    {
       get
       {
            Vector2 Pos = transform.position;
            Vector2 PosToViewport = Camera.main.WorldToViewportPoint(Pos);
            
            return Mathf.Abs(PosToViewport.x-0.5f)<=0.2f && Mathf.Abs(PosToViewport.y - 0.5f)<=0.2f;
       }
    }

    private bool IsNodyOnScreen
    {
        get
        {
            float Min = -NodyData.HideNodyOutsideViewportPadding;
            float Max = 1f + NodyData.HideNodyOutsideViewportPadding;
            Vector2 Pos = transform.position;
            Vector2 PosToViewport = Camera.main.WorldToViewportPoint(Pos);
            return PosToViewport.x >= Min
                && PosToViewport.x <= Max
                && PosToViewport.y >= Min
                && PosToViewport.y <= Max;
        }
    }

    private void PostFixedUpdate()
    {
        _Meta.Input_IsTouchingWall = false;
        _Meta.Input_IsAccessoryTouching = false;

        for (int i = 0; i < NodeArr.Length; i++)
        {
            if (NodeArr[i].HasHitWall)
            {
                _Meta.Input_IsTouchingWall = true;
                break;
            }
        }

      
        for (int i = 0; i < ShellArr.Length; i++)
        {
            NodyStaticShell S = ShellArr[i];

            if (S.HasHitOccured)
            {
                _Meta.Input_IsAccessoryTouching = true;
            }

            if (S.HasHitWall)
            {
                if (S.ShellMeta.IsDead)
                {
                    _Meta.Input_IsTouchingWall = true;
                }
                else
                {
                    //S.ShellMeta.ApplyDamage(1f);
                }

            }
        }

        bool ShouldSameSpeciesAffectEachOther = NodyData.ShouldSameSpeciesAffectEachOther;

        for (int i = 0; i < SpikeArr.Length; i++) {
            NodyStaticSpike S = SpikeArr[i];

            if (S.HasHitOccured)
            {
                _Meta.Input_IsAccessoryTouching = true;
            }

            if (S.HasHitWall)
            {
                _Meta.Input_IsTouchingWall = true;
            }
            else if(S._OtherNodyColDetail!=null)
            {
                if(S._OtherNodyColDetail.Type != NodyColDetailType.SHELL)
                {
                    if (!_Meta.IsSameSpecies(S._OtherNodyColDetail.Parent.Meta))
                    {
                        _Meta.UpdateGainedSpikeDamage(S._OtherNodyColDetail.Parent.Meta);
                    }
                    else
                    {
                        if(ShouldSameSpeciesAffectEachOther)
                        {
                            _Meta.UpdateGainedSpikeDamage(S._OtherNodyColDetail.Parent.Meta);
                        }
                        //_Meta.UpdateGainedSpikeDamage(S._OtherNodyColDetail.Parent.Meta);

                        //Meta.UpdateLostSpeciesAttackPeneltyDamage();

                        //S._OtherNodyColDetail.Parent.Meta.UpdateGainedSpikeDamage(_Meta);
                        //_Meta.UpdateGainedSpikeDamage(S._OtherNodyColDetail.Parent.Meta);
                    }
                }
                else
                {
                    if(S._OtherNodyColDetail!=null)
                    {
                        NodyShellMeta OtherShellMeta = ((NodyStaticShell)S._OtherNodyColDetail).ShellMeta;
                        OtherShellMeta.ApplyDamage(S._OtherNodyColDetail.Parent._Meta.Calc_BodyMass);
                        if (OtherShellMeta.IsDead)
                        {
                            S._OtherNodyColDetail.ThisCollider.enabled = false;
                        }
                    }
                }
            }
        }

        _Meta.ApplyEnergyCostForTick();

        if (_Meta.IsDead)
        {
            if(FrameType!=VisualFrameType.HIDDEN)
            {
                FrameType = VisualFrameType.HIDDEN;
                DestroyVisualAssets();
            }

            Destroy();
        }
        else
        {
            if(_Meta.ShouldGiveBirth)
            {
                int Count = NodyData.RandInt(1, NodyData.MaxBirthChildCount);
                for (int i = 0; i < Count; i++)
                {
                    NodyStaticMeta ChildMeta = _Meta.CreateChild();
                    ChildMeta.Health = NodyData.InitialHealth / (float)Count;
                    NodyData._CreateNody(ChildMeta, transform.position);
                }
                _Meta.UpdateGiveBirth(NodyData.EnergyLoss_Brith);
            }
        }
    }

    private void PreFireNetworks()
    {
        _Meta.Input_AngularTurnNeg11 = UtilityMath.DegreeToBetweenNeg1To1(transform.eulerAngles.z);
    }


    private void PostFireNetworks()
    {

        bool NoEyeMemory = false;

        for (int i = 0; i < _Meta.AllEyeNodesList.Count; i++)
        {
            NodyEyeMeta EyeMeta = _Meta.AllEyeNodesList[i];
            NodyStaticEye Eye = EyeArr[i];

            EyeMeta.UpdateHitStart(transform.position, transform.eulerAngles.z);

            if(NoEyeMemory)
            {
                RaycastHit2D Hit = Physics2D.Raycast(EyeMeta.HitStart, EyeMeta.WorldLookDirectionPerturbed, EyeMeta.ViewDistance);
                Eye.HitRay = Hit;
            }
            else
            {
                if (EyeMeta.EyeTickCount == 0 || (!Eye.HasHitOccured || Eye.HasHitWall))
                {
                    RaycastHit2D Hit = Physics2D.Raycast(EyeMeta.HitStart, EyeMeta.WorldLookDirectionPerturbed, EyeMeta.ViewDistance);
                    Eye.HitRay = Hit;

                    /*bool ChangeHitRay = Hit.collider != null 
                                    || (!Eye.HasHitNody && !Eye.HasHitFood) 
                                    || ((Eye.HasHitNody || Eye.HasHitFood) && Vector3.Distance(Eye.HitRay.transform.position,transform.position)>EyeMeta.ViewDistance*1.1f + 0.5f);

                    if (ChangeHitRay)
                    {
                        Eye.HitRay = Hit;
                    }*/

                    EyeMeta.EyeTickCount = 0;
                }
                EyeMeta.EyeTickCount++;

                if (EyeMeta.EyeTickCount >= NodyData.MaxEyeTickCount)
                {
                    EyeMeta.EyeTickCount = 0;
                }
            }

            Eye.UpdateEye();
        }

        int OutputIndex = 0;

        _Meta.Output_ForwardVelo01 = _Meta._Outputs[OutputIndex];
        OutputIndex++;
        _Meta.Output_AngularVeloNeg11 = _Meta._Outputs[OutputIndex];
        OutputIndex++;
        _Meta.Output_AngularVeloBrakeNeg11 = _Meta._Outputs[OutputIndex];
        OutputIndex++;
        _Meta.Output_SpeedDeltaTime = _Meta._Outputs[OutputIndex];
        OutputIndex++;
        _Meta.Output_CreateLife = _Meta._Outputs[OutputIndex];
        OutputIndex++;

        float DeltaTime = _Meta.Output_SpeedDeltaTime < 0f ? Time.fixedDeltaTime : _Meta.Output_SpeedDeltaTime;

        if (_Meta.Output_ForwardVelo01 >= 0f)
        {
            Vector2 NewVelo = (Vector2)transform.right * _Meta.Calc_MaxForwardVelo * _Meta.Output_ForwardVelo01;
            Rig.velocity = Vector2.Lerp(Rig.velocity, NewVelo, DeltaTime);
        }
        else
        {
           // Rig.velocity = Rig.velocity.magnitude * (Vector2)transform.right * (1f - NodyData.ForwardDrag);
        }

        if (_Meta.Output_AngularVeloBrakeNeg11 >= 0f)
        {
            float NewAngularVelo = _Meta.Calc_MaxAngularVelo * Meta.Output_AngularVeloNeg11;
            Rig.angularVelocity = Mathf.Lerp(Rig.angularVelocity, NewAngularVelo, DeltaTime);
        }
        else
        {
           // Rig.angularVelocity *= (1f - NodyData.AngularDrag);
        }


        /*for (int i = 0; i < _Meta.AllThrustNodesList.Count; i++)
        {
            NodyThrustMeta ThrustMeta = _Meta.AllThrustNodesList[i];
            float ThurstDirectionDegree = transform.eulerAngles.z + ThrustMeta.InitialUnitVectorDegree;
            Vector2 ForceDirection = Quaternion.Euler(0, 0, ThurstDirectionDegree) * Vector2.right;
            float ForceScalar = _Meta._Outputs[OutputIndex];
            //ForceScalar = ForceScalar < 0 ? 0 : ForceScalar;
            Vector2 Force = ForceDirection * ForceScalar;
            //Rig.AddForce(ThrustMeta.ThrustPower * -Force);
            Rig.AddForceAtPosition(ThrustMeta.ThrustPower * -Force, ThrustMeta.WorldspaceLocation(transform.position, transform.eulerAngles.z));
            OutputIndex++;
        }
        OutputIndex++;*/
    }

    private void DestroyVisualAssets()
    {
        Transform[] T = transform.GetComponentsInChildren<Transform>();
        for(int i =0; i <T.Length;i++)
        {
            if(T[i]!=transform)
            Destroy(T[i].gameObject);
        }

        if(HealthText)
            Destroy(HealthText.gameObject);
    }

    private void CreateVisualAssets()
    {
        int EyeIndex = 0;
        int ShellIndex = 0;
        int SpikeIndex = 0;

        for (int i = 0; i < Meta.AllNodesList.Count; i++)
        {
            NodyNodeStaticMeta NodeMeta = Meta.AllNodesList[i];

            GameObject BodyNode = new GameObject();
            BodyNode.transform.position = NodeMeta.WorldspaceLocation(transform.position, transform.eulerAngles.z);//(Vector3)NodeMeta.InitialSpawnOffset + transform.position;

            MeshRenderer BodyRen = BodyNode.AddComponent<MeshRenderer>();
            BodyRen.material = new Material(Shader.Find(NodyData.MainBodyMaterial.shader.name));
            BodyRen.material.CopyPropertiesFromMaterial(NodyData.MainBodyMaterial);
            BodyRen.material.SetColor("_BaseColor", Meta.NodyColor);
            BodyNode.AddComponent<MeshFilter>().sharedMesh = MeshGenerator.CircleMesh(4, NodeMeta.BaseSize);

            BodyNode.transform.parent = transform;
            BodyNode.name = "Node-" + i;

            NodeArr[i].Mat = BodyRen.material;

            if (NodeMeta.IsMasterNode)
            {
                MainBodyLine = BodyNode.AddComponent<LineRenderer>();
                MainBodyLine.positionCount = 2;
                MainBodyLine.startWidth = NodeMeta.BaseSize;//NodyData.ForwardLineWidth;
                MainBodyLine.endWidth = 0f;//NodyData.ForwardLineWidth;
                MainBodyLine.sharedMaterial = NodyData.MainBodyLineMaterial;
            }

            for (int j = 0; j < NodeMeta.EyeList.Count; j++)
            {
                NodyEyeMeta EyeMeta = NodeMeta.EyeList[j];

                GameObject EyeNode = new GameObject();

                Vector3 EyeNodePos = EyeMeta.WorldspaceLocation(transform.position,transform.eulerAngles.z);//;(Vector3)EyeMeta.InitialSpawnOffset + transform.position;
                EyeNodePos.z = NodyData.EyePosZIndex;
                EyeNode.transform.position = EyeNodePos;

                MeshRenderer EyeRen = EyeNode.AddComponent<MeshRenderer>();
                EyeRen.material = new Material(Shader.Find(NodyData.MainBodyMaterial.shader.name));
                EyeRen.material.CopyPropertiesFromMaterial(NodyData.MainBodyMaterial);
                EyeRen.material.SetColor("_BaseColor", Color.red);
                EyeNode.AddComponent<MeshFilter>().sharedMesh = MeshGenerator.CircleMesh(2, EyeMeta.BaseSize);
                EyeNode.transform.parent = transform;
                EyeNode.name = "Eye-" + i + "-" + j;
                EyeArr[EyeIndex].Mat = EyeRen.material;

                LineRenderer EyeLineRen = EyeNode.AddComponent<LineRenderer>();
                EyeLineRen.positionCount = 2;
                EyeLineRen.startWidth = NodyData.EyeLineWidth;
                EyeLineRen.endWidth = NodyData.EyeLineWidth;
                EyeLineRen.material = new Material(Shader.Find(NodyData.MainBodyLineMaterial.shader.name));// NodyData.MainBodyLineMaterial;
                EyeLineRen.material.CopyPropertiesFromMaterial(NodyData.MainBodyLineMaterial);
                EyeArr[EyeIndex].EyeLine = EyeLineRen;

                EyeIndex++;
            }

            /**
             * Generating Shell Game Objects to be displayed
             * TODO: Updating to use sprite renderer
             */
            for (int j = 0; j < NodeMeta.ShellList.Count; j++)
            {
                NodyShellMeta ShellMeta = NodeMeta.ShellList[j];

                GameObject ShellNode = new GameObject();

                Vector3 ShellNodePos = ShellMeta.WorldspaceLocation(transform.position, transform.eulerAngles.z);
                ShellNodePos.z = NodyData.ShellPosZIndex;
                ShellNode.transform.position = ShellNodePos;

                SpriteRenderer sprite = ShellNode.AddComponent<SpriteRenderer>();
                sprite.sprite = Resources.Load<Sprite>("Shell");
                ShellNode.transform.parent = transform;
                ShellNode.name = "Shell-" + i + "-" + j;
                ShellNode.transform.localScale = new Vector3(NodyData.ShellSize*0.6f, NodyData.ShellSize * 0.6f, 1f);
                ShellArr[ShellIndex].Mat = sprite.material;

                ShellIndex++;
            }

            for (int j = 0; j < NodeMeta.SpikeList.Count; j++)
            {
                NodySpikeMeta SpikeMeta = NodeMeta.SpikeList[j];

                GameObject SpikeNode = new GameObject();

                Vector3 SpikeNodePos = SpikeMeta.WorldspaceLocation(transform.position, transform.eulerAngles.z);
                SpikeNodePos.z = NodyData.SpikePosZIndex;
                SpikeNode.transform.position = SpikeNodePos;

                SpriteRenderer sprite = SpikeNode.AddComponent<SpriteRenderer>();
                sprite.sprite = Resources.Load<Sprite>("Spike");
                SpikeNode.transform.parent = transform;
                SpikeNode.name = "Spike-" + i + "-" + j;
                SpikeNode.transform.localScale = new Vector3(NodyData.SpikeSize * .75f*2f, NodyData.SpikeSize * .75f*2f, 1f);
                SpikeArr[SpikeIndex].Mat = sprite.material;

                SpikeNode.transform.eulerAngles = new Vector3(0,0, SpikeMeta.InitialUnitVectorDegree+transform.eulerAngles.z);
                //SpikeNode.transform.position += SpikeNode.transform.right * 0.3f;

                SpikeIndex++;
            }
        }


        GameObject HealthTextObj = new GameObject();
        HealthTextObj.name = _Meta.Name+"-Health";
        HealthTextObj.transform.parent = NodyData._Canvas.transform;
        HealthText = HealthTextObj.AddComponent<Text>();
        HealthText.font = NodyData._TextFont;
        HealthText.fontSize = 12;
        HealthText.fontStyle = FontStyle.Bold;
        HealthText.color = Color.white;

        UpdateView();
    }

    private void UpdateView()
    {
        UpdateMainBodyLine();
        UpdateEyeLines();
        UpdateNodes();
        UpdateShells();
        UpdateSpikes();
        UpdateText();
    }

    private void UpdateMainBodyLine()
    {
        MainBodyLine.SetPosition(0, transform.position);
        MainBodyLine.SetPosition(1, transform.position+transform.right* _Meta.AllNodesList[0].BaseSize);
    }

    private void UpdateShells()
    {
        for (int i = 0; i < ShellArr.Length; i++)
        {
            if(ShellArr[i].ShellMeta.IsDead)
            {
                ShellArr[i].Mat.color = Color.black;
            }
            else 
            {
                if (ShellArr[i].HasHitOccured)
                {
                    ShellArr[i].Mat.color = Color.gray;
                }
                else
                {
                    float Ratio = ShellArr[i].ShellMeta.ShellHealth / NodyData.MaxShellHealth;
                    if(Ratio<0.95f)
                    {
                        float H, S, V;
                        Color.RGBToHSV(_Meta.ShellColor, out H, out S, out V);
                        ShellArr[i].Mat.color = Color.HSVToRGB(H, Ratio, V);
                    }
                    else
                    {      
                        ShellArr[i].Mat.color = Color.white;
                    }
                }
            }
        }
    }

    
    private void UpdateSpikes()
    {
        for (int i = 0; i < SpikeArr.Length; i++)
        {
            if (SpikeArr[i].HasHitOccured)
            {
                //SpikeArr[i].Mat.SetColor("_BaseColor", Color.gray);
                SpikeArr[i].Mat.color = Color.gray;
            }
            else
            {
                //SpikeArr[i].Mat.SetColor("_BaseColor", _Meta.SpikeColor);
                SpikeArr[i].Mat.color = Color.white;
            }
        }
    }

    private void UpdateEyeLines()
    {
        bool _IsNodyNearScreenCenter = IsNodyNearScreenCenter;
        for (int i = 0; i < _Meta.AllEyeNodesList.Count;i++)
        {
            NodyEyeMeta EyeMeta = _Meta.AllEyeNodesList[i];
            LineRenderer EyeLineRen = EyeArr[i].EyeLine;

            Vector3 Start = EyeLineRen.transform.position;
            Vector3 End = EyeMeta.HitEnd;
            Start.z = NodyData.EyeLineZIndex;
            End.z = NodyData.EyeLineZIndex;
            EyeLineRen.SetPosition(0, Start);

            if (EyeLineType==VisualEyeLine.NO_LINE || (EyeLineType==VisualEyeLine.TOUCH_LINE && !EyeArr[i].HasHitOccured)) {
                EyeLineRen.SetPosition(1, Start);
            }
            else
            {
                EyeLineRen.SetPosition(1, End);
            }
            

            if (EyeMeta.CurrentEyeViewDistance01 == -1)
                EyeLineRen.material.SetColor("_BaseColor", Color.red);
            else
                EyeLineRen.material.SetColor("_BaseColor", Color.green);

            float SV = 0f;
            if (EyeMeta.CurrentEyeViewDistance01 != -1)
                SV = 1f - EyeMeta.CurrentEyeViewDistance01;

            Material EyeMat = EyeArr[i].Mat;
            float H = 1f;
            if (EyeArr[i].HasHitFood) H = 0.25f;
            else if (EyeArr[i].HasHitWall) H = 0.473f;
            EyeMat.SetColor("_BaseColor",Color.HSVToRGB(H, SV, 1f));
        }
    }

    private void UpdateNodes()
    {
        float S = _Meta.Health / NodyData.InitialHealth;
        S = S > 1f ? 1f : S;

        float Hue, Sa, Va;
        Color.RGBToHSV(_Meta.NodyColor,out Hue,out Sa,out Va);
        Hue += 0.5f;
        if (Hue > 1f) Hue = Hue-1f;

        if (_Meta.Health<NodyData.Animation_MinHealthBeforeBlinker)
        {
            if (Animaiton_HealthBlinkerCount >= NodyData.Animation_MaxLowHealthBlinkerCount)
                Animaiton_HealthBlinkerCount = 0;
            else
                Animaiton_HealthBlinkerCount++;

            if (Animaiton_HealthBlinkerCount == 0)
                Animation_HealthShowTrue = !Animation_HealthShowTrue;

            if (!Animation_HealthShowTrue)
            {
                S = 1f;
                Hue = 1f;
            }
        }

        MainBodyLine.material.SetColor("_BaseColor", Color.HSVToRGB(Hue, S, 1f));

        for(int i = 0; i <NodeArr.Length;i++)
        {
            if (NodeArr[i].HasHitOccured)
                NodeArr[i].Mat.SetColor("_BaseColor", Color.gray);
            else
                NodeArr[i].Mat.SetColor("_BaseColor", _Meta.NodyColor);
        }
    }

    private string FormatToTwoDecimal(float f)
    {
        return string.Format((f % 1) == 0 ? "{0:0}" : "{0:0.00}", f);
    }

    private string FormatToOneDecimal(float f)
    {
        return string.Format((f % 1) == 0 ? "{0:0}" : "{0:0.0}", f);
    }

    private void UpdateText()
    {
        if (HideTexts)
        {
            HealthText.text = "";
            return;
        }

        Vector3 ScreenPoint = Camera.main.WorldToScreenPoint(transform.position);
        HealthText.text = "H:" + (int)_Meta.Health+", TL:"+(int)_Meta.TimeLived +"\nN:"+_Meta.Name+", SID:"+_Meta.SpeciesId+"\nCC:"+_Meta.ChildCount;
        HealthText.rectTransform.position = ScreenPoint;
    }

    
    public void FireNetworks()
    {
        int InputIndex = 0;

        _Meta._Inputs[InputIndex] = _Meta.Health/NodyData.InitialHealth;
        InputIndex++;

        Meta._Inputs[InputIndex] = _Meta.Input_IsAccessoryTouching ? 1f : 0f;
        InputIndex++;

        for (int i = 0; i < _Meta.AllEyeNodesList.Count; i++)
        {
            NodyStaticEye Eye = EyeArr[i];

            _Meta._Inputs[InputIndex] = Eye.EyeMeta.CurrentEyeViewDistance01 == -1 ? -1 : Mathf.Max(Mathf.Min(1f - Eye.EyeMeta.CurrentEyeViewDistance01,1f),0f);
            InputIndex++;

            if (Eye.HasHitNody)
            {
                _Meta._Inputs[InputIndex] = Eye.HasHitSameSpecies?2f:1f;
                _Meta._Inputs[InputIndex + 1] = 0f;
                _Meta._Inputs[InputIndex + 2] = 0f;
            }
            else if (Eye.HasHitFood)
            {
                _Meta._Inputs[InputIndex] = 0f;
                _Meta._Inputs[InputIndex + 1] = 1f;
                _Meta._Inputs[InputIndex + 2] = 0f;
            }
            else if (Eye.HasHitWall)
            {
                _Meta._Inputs[InputIndex] = 0f;
                _Meta._Inputs[InputIndex + 1] = 0f;
                _Meta._Inputs[InputIndex + 2] = 1f;
            }
            else
            {
                _Meta._Inputs[InputIndex] = 0f;
                _Meta._Inputs[InputIndex + 1] = 0f;
                _Meta._Inputs[InputIndex + 2] = 0f;
            }

            InputIndex++;
            InputIndex++;
            InputIndex++;
        }

        for (int i = 0; i < _Meta.AllNodesList.Count; i++)
        {
            NodyStaticNode Node = NodeArr[i];

            _Meta._Inputs[InputIndex] = Node.HasHitOccured?1f:0f;
            InputIndex++;
 
            /*if(Node.HasHitOccured)
            {
                Vector2 Pos = Node.HasHitWall ? Node.HitColl.GetContact(0).point : (Vector2)Node.HitColl.collider.transform.position;
                Meta._Inputs[InputIndex] = UtilityMath.DegreeToBetweenNeg1To1FromTwoPoints(transform.position, Pos, -transform.eulerAngles.z);
            }
            else
            {
                Meta._Inputs[InputIndex] = 0f;
            }
            InputIndex++; */
        }

        _Meta._Inputs[InputIndex] = NodyData.BiasValue;
        InputIndex++;

        for (int i = 0; i < NodyData.MemoryNeurons; i++)
        {
            _Meta._Inputs[InputIndex] = _Meta._Outputs[i + _Meta._OutputMemoryNeuronStartIndex];
            InputIndex++;
        }

        _Meta.FireNetworks();
    }

    public NodyStaticMeta Meta
    {
        get
        {
            return _Meta;
        }

        set
        {
            _Meta = value;
        }
    }

    public void Destroy()
    {
        _Meta.Destroy();
        NodyData._RemoveNody(this);
        
    }

    public void SetVisualEyeLineType(VisualEyeLine eyeLineType)
    {
        EyeLineType = eyeLineType;
    }

    public void SetHideTexts(bool hideTexts)
    {
        HideTexts = hideTexts;
    }

    public static NodyStatic Init(NodyStaticMeta Meta, VisualEyeLine EyeLineType, bool HideTexts)
    {
        NodyStatic NS = new GameObject().AddComponent<NodyStatic>();
        
        NS.gameObject.layer = NodyData.NodyLayer;
        NS.name = Meta.Name;
        
        NS.Rig = NS.gameObject.AddComponent<Rigidbody2D>();
        NS.Rig.gravityScale = 0f;
        NS.Rig.collisionDetectionMode = NodyData.CollisionType;
        NS.Rig.sharedMaterial = NodyData.SlipperyPhysicsMat;
        NS.Rig.drag = 1f;
        NS.Rig.angularDrag = 1f;
        NS.Rig.mass = Meta.Calc_BodyMass * NodyData.NodyMassScalar;

        NS.ColliderIdToNodeDict = new Dictionary<int, NodyCollDetail>();

        if (Meta.MainBodySpawnPosition == Vector2.zero) throw new System.Exception("Cannot spawn a Vector2(0,0)");

        NS.transform.position = Meta.MainBodySpawnPosition;
        NS.transform.eulerAngles = new Vector3(0, 0, Meta.MainBodyRotationDegree);

        NS.EyeArr = new NodyStaticEye[Meta.AllEyeNodesList.Count];
        NS.NodeArr = new NodyStaticNode[Meta.AllNodesList.Count];
        NS.ShellArr = new NodyStaticShell[Meta.AllShellNodesList.Count];
        NS.SpikeArr = new NodyStaticSpike[Meta.AllSpikeNodesList.Count];

        for (int i = 0; i < NS.EyeArr.Length; i++)
            NS.EyeArr[i] = new NodyStaticEye(NS, Meta.AllEyeNodesList[i]);

        for (int i = 0; i < NS.NodeArr.Length; i++)
        {
            NodyNodeStaticMeta NodeMeta = Meta.AllNodesList[i];
            CircleCollider2D Col2d = NS.gameObject.AddComponent<CircleCollider2D>();
            Col2d.radius = NodeMeta.BaseSize;
            Col2d.offset = NodeMeta.InitialSpawnOffset;
            NS.NodeArr[i] = new NodyStaticNode(NS, Col2d, Meta.AllNodesList[i]);
            NS.ColliderIdToNodeDict.Add(Col2d.GetInstanceID(), NS.NodeArr[i]);
        }

        for (int i = 0; i < NS.ShellArr.Length; i++)
        {
            NodyShellMeta ShellMeta = Meta.AllShellNodesList[i];
            CircleCollider2D Col2d = NS.gameObject.AddComponent<CircleCollider2D>();
            Col2d.radius = ShellMeta.BaseSize;
            Col2d.offset = ShellMeta.InitialSpawnOffset;
            NS.ShellArr[i] = new NodyStaticShell(NS, Col2d, ShellMeta);
            NS.ColliderIdToNodeDict.Add(Col2d.GetInstanceID(), NS.ShellArr[i]);
            
        }

        for (int i = 0; i < NS.SpikeArr.Length; i++)
        {
            NodySpikeMeta SpikeMeta = Meta.AllSpikeNodesList[i];
            CircleCollider2D Col2d = NS.gameObject.AddComponent<CircleCollider2D>();
            Col2d.radius = SpikeMeta.BaseSize;
            Col2d.offset = SpikeMeta.InitialSpawnOffset;
            NS.SpikeArr[i] = new NodyStaticSpike(NS, Col2d, SpikeMeta);
            NS.ColliderIdToNodeDict.Add(Col2d.GetInstanceID(), NS.SpikeArr[i]);
            
        }

        NS.SetVisualEyeLineType(EyeLineType);
        NS.SetHideTexts(HideTexts);
        NS._Meta = Meta;

        NS.FrameType = VisualFrameType.HIDDEN;

        
        return NS;
    }
}
