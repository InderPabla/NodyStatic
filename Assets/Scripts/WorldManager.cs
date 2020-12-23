using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using System.Linq;
using Unity.Jobs;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public struct RectNode
{
    public Color _Color;
    public Rect _Rect;
 
    public RectNode(Color color, Rect rect)
    {
        this._Color = color;
        this._Rect = rect;
    }
}

public class SpeciesCountData
{
    public int SpeciesId;
    public float Count;

    public SpeciesCountData(int speciesId, float count)
    {
        SpeciesId = speciesId;
        Count = count;
    }
}


public class WorldManager : MonoBehaviour
{
    public int Seed = 100;
    public bool UseStaticSeed = false;
    public Font TextFont;

    private bool LeftClickDown = false;
    
    private Vector2 WorldLeftClickLocation;
    private Vector2 LeftClickMousePosition;
    private Vector3 WorldCameraLocation;
    private Vector2 PreviousMousePosition;
    private VisualEyeLine EyeLineType = VisualEyeLine.NO_LINE;
    private bool HideTexts = false;

    private float TotalWorldTime = 0f;
    private int TotalFoodCount = 0;
    private int SpawnFoodCount = 0;
    private float SpawnNodyTime = 0f;
    private float FoodSpawnTime = 0f;
    
    private float SpeciesTime = 0f;
    private float CurrentFoodSpawnTime = 0;

    private bool ShowDataPanel;
    private NodyStaticMeta SelectedNodyMeta;

    private Texture2D TextureNodyBrain;
    private RectNode[] RectNodeArr;
    private float MinHeight = 110f;
    private float SpeciesDataMinHeight = 75f;

    public Color InitialColor;
    private float InitialColorH, InitialColorS, InitialColorV;
    

    private List<SpeciesCountData> SpeciesCountDataList;
    private List<NodyStatic> NodyList; 
    private Color[] Colors;

    public Boundary BoundaryPrefab;
    public bool IsPrintLogs = false;

    private int MovingBoundariesTimer = 0;
    private int MaxMovingBoundariesTimer = 5;

    void Start()
    {
        if(!UseStaticSeed)
        {
            Seed = UnityEngine.Random.Range(0,100000);
        }

        Canvas _Canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        NodyData.InitStaticData(Seed, RemoveNody, RemoveFood, CreateNodyAroundPosition, GenerateRandomSpawnPos, _Canvas, TextFont);
        SpawnFoodCount = NodyData.MaxFoodCount;
        SpeciesCountDataList = new List<SpeciesCountData>();
        NodyList = new List<NodyStatic>();
        Colors = new Color[1000];
        for(int i = 0; i < Colors.Length;i++)
        {
            Colors[i] = Color.HSVToRGB(UnityEngine.Random.Range(0f,1f), UnityEngine.Random.Range(0.5f, 1f), UnityEngine.Random.Range(0.5f, 1f));
        }

        Boundary.BoundariesInit(BoundaryPrefab);

        Time.fixedDeltaTime = NodyData.FixedDeltaTime;

        InitSpawn();
        InitialColor = Camera.main.backgroundColor;
        Color.RGBToHSV(InitialColor,out InitialColorH, out InitialColorS, out InitialColorV);


    }

    private void InitSpawn()
    {
        //New Session UUID for every new state (Making sure old folder is NOT overridden
        NodyData.SessionUUID = NodyData.UUID;

        if (NodyData.GameStateUUID != null)
        {
            if(NodyData.LoadSessionUUD ==null)
            {
                string gameStateSubString = NodyData.GameStateUUID + "_";
                string[] Files = Directory.GetDirectories(NodyData.CreatureListFolder);
                DirectoryInfo info = new DirectoryInfo(NodyData.CreatureListFolder);
                DirectoryInfo[] directories = info.EnumerateDirectories()
                                                .OrderBy(p => p.CreationTime)
                                                .ToList()
                                                .FindAll(v=>v.Name.Contains(gameStateSubString))
                                                .ToArray();
                NodyData.LoadSessionUUD = directories[directories.Length-1].Name.Replace(gameStateSubString,""); //Latest Load state id
            }

            LoadState();
        }
        else
        {
            NodyData.GameStateUUID = NodyData.UUID;

            for (int i = 0; i < NodyData.InitialSpanwCount; i++)
            {
                GenerateRandomNody();
            }

            while (TotalFoodCount < SpawnFoodCount)
            {
                GenerateRandomFood();
            }
        }
        
    }

    private Vector2 GenerateRandomSpawnPos()
    {
        int count = 0;
        while (count < 100)
        {
            Vector3 SpawnPos = new Vector3(UnityEngine.Random.Range(NodyData.MinSpawnBoundary, NodyData.MaxSpawnBoundary), UnityEngine.Random.Range(NodyData.MinSpawnBoundary, NodyData.MaxSpawnBoundary), 0f);

            Collider2D Hit = Physics2D.OverlapCircle(SpawnPos, 1f);
            if (Hit == null)
            {
                return SpawnPos;
            }

            count++;
        }

        return new Vector3(UnityEngine.Random.Range(NodyData.MinSpawnBoundary, NodyData.MaxSpawnBoundary), UnityEngine.Random.Range(NodyData.MinSpawnBoundary, NodyData.MaxSpawnBoundary), 0f);
    }

    private Vector2 GenerateRandomSpawnPos(Vector3 Position)
    {
        //return GenerateRandomSpawnPos();

        int count = 0;
        float Size = 7*3f;

        float MinX = Position.x - Size;
        float MaxX = Position.x + Size;
        float MinY = Position.y - Size;
        float MaxY = Position.y + Size;

        if(MinX < NodyData.MinSpawnBoundary)
        {
            MinX = NodyData.MinSpawnBoundary;
            MaxX = NodyData.MinSpawnBoundary + (Size * 2);
        }
        else if(MaxX > NodyData.MaxSpawnBoundary)
        {
            MaxX = NodyData.MaxSpawnBoundary;
            MinX = NodyData.MaxSpawnBoundary - (Size * 2);
        }

        if (MinY < NodyData.MinSpawnBoundary)
        {
            MinY = NodyData.MinSpawnBoundary;
            MaxY = NodyData.MinSpawnBoundary + (Size * 2);
        }
        else if (MaxY > NodyData.MaxSpawnBoundary)
        {
            MaxY = NodyData.MaxSpawnBoundary;
            MinY = NodyData.MaxSpawnBoundary - (Size * 2);
        }

        while (count < 10)
        {
            Vector3 SpawnPos = new Vector3(UnityEngine.Random.Range(MinX, MaxX), UnityEngine.Random.Range(MinY, MaxY), 0f);

            Collider2D Hit = Physics2D.OverlapCircle(SpawnPos, 1f);
            if (Hit == null)
            {
                return SpawnPos;
            }

            count++;
        }

        return GenerateRandomSpawnPos();
    }

    private NodyStatic FindNodyAtMousePointer()
    {
        Collider2D Hit = Physics2D.OverlapCircle(Camera.main.ScreenToWorldPoint(Input.mousePosition), 3f);
        if(!Hit ||!Hit.gameObject)
            return null;
        return Hit.GetComponent<NodyStatic>();
    }

    private void GenerateRandomFood()
    {
        Food.Init(GenerateRandomSpawnPos(), this);
        TotalFoodCount++;
    }

    private void GenerateRandomNody()
    {
        NodyStaticMeta Meta = new NodyStaticMeta(NodyData.SpeciesId);
        CreateNodyAtPosition(Meta,GenerateRandomSpawnPos());
    }

    void FixedUpdate()
    {
        TotalWorldTime += Time.fixedDeltaTime;

        ////////////////////////////////////////////
        if(FoodSpawnTime >= NodyData.MaxFoodSpawnTime)
        {
            while (TotalFoodCount < SpawnFoodCount)
            {
                GenerateRandomFood();
            }

            FoodSpawnTime = 0f;
        }
        else
        {
            FoodSpawnTime += Time.fixedDeltaTime;
        }

        ////////////////////////////////////////////
        float TimeSin = (Mathf.Sin((CurrentFoodSpawnTime/NodyData.MaxFoodSpawnTimeIntervalYears)*Mathf.PI*2f)+1f)/2f;
        float _MaxFoodCount = ((float)(NodyData.MaxFoodCount - NodyData.MinFoodCount)* TimeSin) + NodyData.MinFoodCount;
        SpawnFoodCount = (int)_MaxFoodCount;
        CurrentFoodSpawnTime += Time.fixedDeltaTime;
        if (CurrentFoodSpawnTime > NodyData.MaxFoodSpawnTimeIntervalYears) CurrentFoodSpawnTime = 0f;

        ////////////////////////////////////////////
        if (SpawnNodyTime >= NodyData.MaxNodyWaitSpawnTime)
        {
            for (int i = 0; i < NodyData.InitialSpanwCount; i++)
            {
                GenerateRandomNody();
            }

            SpawnNodyTime = 0f;
        }
        else
        {
            SpawnNodyTime += Time.fixedDeltaTime;
        }
    }

    void Update()
    {
        Vector2 MousePosition = Input.mousePosition;
        Vector2 WorldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float CurrentOrthoSize = Camera.main.orthographicSize;
        float scrollDetla = Input.GetAxis("Mouse ScrollWheel");
        float CurrentZoomLevel01 = ((CurrentOrthoSize - NodyData.MinCameraOrthoSize) / (NodyData.MaxCameraOrthoSize - NodyData.MinCameraOrthoSize));
        float ZoomDelta = CurrentZoomLevel01 * (6f + (6f * (1f - CurrentZoomLevel01)));

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            LeftClickDown = true;
            WorldLeftClickLocation = WorldMousePosition;
            WorldCameraLocation = Camera.main.transform.position;
            LeftClickMousePosition = MousePosition;
        }

        if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            LeftClickDown = false;
        }

        if(LeftClickDown && PreviousMousePosition!=MousePosition)
        {
            //Camera.main.transform.position = WorldCameraLocation + (Vector3)(WorldLeftClickLocation - WorldMousePosition);
            Camera.main.transform.position = WorldCameraLocation + ((Vector3)(LeftClickMousePosition - MousePosition) * 0.5f * CurrentZoomLevel01);
        }

        
        if (scrollDetla>0)
        {
            CurrentOrthoSize -= ZoomDelta;
        }
        else if(scrollDetla<0)
        {
            CurrentOrthoSize += ZoomDelta;
        }

        if (CurrentOrthoSize < NodyData.MinCameraOrthoSize)
        {
            CurrentOrthoSize = NodyData.MinCameraOrthoSize;
        }
        else if (CurrentOrthoSize > NodyData.MaxCameraOrthoSize)
        {
            CurrentOrthoSize = NodyData.MaxCameraOrthoSize;
        }

        if(Input.GetKeyUp(KeyCode.L))
        {
            ToggleNextEyeLine();

            foreach (NodyStatic N in NodyList)
            {
                N.SetVisualEyeLineType(EyeLineType);
            }
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            MovingBoundariesTimer = MaxMovingBoundariesTimer;
            MoveCreaturesOutOfPlaySpace();
            Boundary.BoundariesToggle(BoundaryPrefab);
        }
        else
        {
            if (MovingBoundariesTimer > 0)
            {
                MovingBoundariesTimer--;
                if (MovingBoundariesTimer == 0)
                {
                    MoveCreatureIntoPlaySpace();
                }
            }
        }
        


        

        if (Input.GetKeyUp(KeyCode.S))
        {
            SaveState();
        }

        if (Input.GetKeyUp(KeyCode.K))
        {
            HideTexts = !HideTexts;

            foreach (NodyStatic N in NodyList)
            {
                N.SetHideTexts(HideTexts);
            }
        }


        if (Input.GetKeyUp(KeyCode.T))
        {
            switch(Time.timeScale)
            {
                case 1f:
                    Time.timeScale = 2f;
                    break;

                case 2f:
                    Time.timeScale = 3f;
                    break;

                case 3f:
                    Time.timeScale = 16f;
                    break;

                case 16f:
                    Time.timeScale = 32f;
                    break;

                case 32f:
                    Time.timeScale = 0.33f;
                    break;

                case 0.33f:
                    Time.timeScale = 0.66f;
                    break;

                case 0.66f:
                    Time.timeScale = 1f;
                    break;

                default:
                    Time.timeScale = 1f;
                    break;
            }

            Debug.Log("New Time Scale: "+Time.timeScale);
        }

        Camera.main.orthographicSize = CurrentOrthoSize;
        PreviousMousePosition = MousePosition;

        //if(Input.GetKeyUp(KeyCode.P))
        int OldestSpeciesId = SpeciesCountDataList.Count == 0 ? -1 : SpeciesCountDataList[0].SpeciesId;
        if(IsPrintLogs)
            Debug.Log("Creature Count:"+NodyList.Count+", Fixed Delta Time:"+ TotalWorldTime+", Food Count:"+TotalFoodCount+", Oldest Species Id:"+ OldestSpeciesId+", Spawn Food:"+ SpawnFoodCount);


        if(Input.GetKeyUp(KeyCode.Mouse1))
        {
            /*if(SelectedNody && SelectedNody.gameObject)
            {
                SelectedNody.SetHideEyeLines(true);
                SelectedNody.SetHideTexts(true);
            }*/
            SelectedNodyMeta = null;
            NodyStatic SelectedNody = FindNodyAtMousePointer();

            if(SelectedNody)
            {
                SelectedNody.SetVisualEyeLineType(VisualEyeLine.ALL_LINE);
                SelectedNody.SetHideTexts(false);
                SelectedNodyMeta = SelectedNody.Meta;
            }

            TextureUpdateBrain();
        }

        Camera.main.backgroundColor = Color.HSVToRGB(InitialColorH, InitialColorS, InitialColorV);
        InitialColorH += Time.fixedDeltaTime/100f;
        InitialColorH = InitialColorH > 1f ? 0f : InitialColorH;

        if(SelectedNodyMeta != null)
        {
            if(SpeciesTime >= NodyData.MaxRecalculateSpeciesTime)
            {
                SpeciesCountDataList = new List<SpeciesCountData>();
                Dictionary<int, int> SpeciesCountDataDict = new Dictionary<int, int>();
                foreach (NodyStatic N in NodyList)
                {
                    int Id = N.Meta.SpeciesId;
                    if (SpeciesCountDataDict.ContainsKey(Id))
                    {
                        SpeciesCountDataDict[Id]++;
                    }
                    else
                    {
                        SpeciesCountDataDict.Add(Id, 1);
                    }
                }

                IEnumerator<int> I = SpeciesCountDataDict.Keys.GetEnumerator();
                while (I.MoveNext())
                {
                    SpeciesCountDataList.Add(new SpeciesCountData(I.Current, SpeciesCountDataDict[I.Current]));
                }

                SpeciesCountDataList.Sort((SpeciesCountData a, SpeciesCountData b) =>
                {
                    return a.SpeciesId < b.SpeciesId ? -1 : a.SpeciesId > b.SpeciesId ? 1 : 0;
                    //return a.Count > b.Count ? -1 : a.Count < b.Count ? 1 : a.SpeciesId < b.SpeciesId ? -1 : a.SpeciesId > b.SpeciesId ? 1 : 0;
                });

                SpeciesTime = 0f;
            }
            else
            {
                SpeciesTime += Time.deltaTime;
            }
        }
    }

    private void MoveCreaturesOutOfPlaySpace()
    {
        foreach(NodyStatic N in NodyList)
        {
            N.transform.position += new Vector3(5000,5000,0);
        }
    }

    private void MoveCreatureIntoPlaySpace()
    {
        foreach (NodyStatic N in NodyList)
        {
            Vector3 Pos = N.transform.position - new Vector3(5000, 5000, 0);
            Pos = GenerateRandomSpawnPos(Pos);
            N.transform.position = Pos;
        }
    }

    private void LoadState()
    {
        string FolderName = NodyData.CreatureListFolder +"/"+ NodyData.GameStateUUID +"_"+ NodyData.LoadSessionUUD +"/";
        print("Loading Game State from Folder:" + FolderName);
        string[] Files = Directory.GetFiles(FolderName);
        IFormatter formatter = new BinaryFormatter();
        for (int i =0;i<Files.Length;i++)
        {
            Stream streamDSer = new FileStream(Files[i], FileMode.Open, FileAccess.Read);
            NodyStaticMeta MDSer = (NodyStaticMeta)formatter.Deserialize(streamDSer);
            CreateNodyAtPosition(MDSer, GenerateRandomSpawnPos());

            if(NodyData._SpeciesId<=MDSer.SpeciesId)
            {
                NodyData._SpeciesId = MDSer.SpeciesId + 1;
            }
        }
    }

    //https://www.guru99.com/c-sharp-serialization.html
    private void SaveState()
    {
        DirectoryInfo creatureDir = Directory.CreateDirectory(NodyData.CreatureListFolder);
        string FolderName = NodyData.CreatureListFolder + "/"+ NodyData.GameStateUUID +"_"+ NodyData.SessionUUID +"/";
        print("Save Game State from Folder:" + FolderName);
        DirectoryInfo sessionDir = Directory.CreateDirectory(FolderName);
        for(int i = 0; i <NodyList.Count; i++)
        {
            NodyStaticMeta MSer = NodyList[i].Meta;
            string fileName = MSer.UUID +  "_" + MSer.SpeciesId +"_"+ MSer.Name+".dat";
            IFormatter formatter = new BinaryFormatter();
            Stream streamSer = new FileStream(FolderName+fileName, FileMode.Create, FileAccess.Write);
            formatter.Serialize(streamSer, MSer);
            streamSer.Close();
        }

        NodyData.SessionUUID = NodyData.UUID;
    }

    private void RemoveNody(NodyStatic Nody)
    {
        if(Nody && Nody.gameObject && Nody.gameObject.activeSelf)
        {
            NodyList.Remove(Nody);

            Nody.gameObject.SetActive(false);
            Destroy(Nody.gameObject);
            

            if (NodyList.Count < NodyData.InitialSpanwCount)
            {
                GenerateRandomNody();
            }
        }
        
    }

    private void RemoveFood(GameObject Food)
    {
        //Food.GetComponent<Food>().ResetPosition();

        if(Food!=null && Food.gameObject!=null && Food.gameObject.activeSelf)
        {
            DestoryFood(Food);
        }
    }

    private void DestoryFood(GameObject Food)
    {
        Food.SetActive(false);
        Destroy(Food);
        TotalFoodCount--;
    }

    private void CreateNodyAroundPosition(NodyStaticMeta Meta, Vector3 Position)
    {
        CreateNodyAtPosition(Meta, GenerateRandomSpawnPos(Position));
    }

    private void CreateNodyAtPosition(NodyStaticMeta Meta, Vector3 Position)
    {
        Meta.MainBodySpawnPosition = Position;
        NodyStatic NS = NodyStatic.Init(Meta, EyeLineType, HideTexts);
        NodyList.Add(NS);
    }

    private void TextureUpdateBrain()
    {
        if (SelectedNodyMeta != null)
        {
            float ScreenWidth = Screen.width * 0.2f;
            float ScreenHight = Screen.height;

            int InputNeurons = SelectedNodyMeta._InputNeuronCount;
            int OutputNeurons = SelectedNodyMeta._OutputNeuronCount;
            int HiddenNeurons = SelectedNodyMeta.NEAT_Decoder.NodeArr.Length - (InputNeurons + OutputNeurons);
            float RadiansDiff = HiddenNeurons == 0 ? 0 : (Mathf.PI*2f)/(float)HiddenNeurons;
            float NeuronSize = 10;

            TextureNodyBrain = new Texture2D(1, 1, TextureFormat.RGBA32,false);
            RectNodeArr = new RectNode[SelectedNodyMeta.NEAT_Decoder.NodeArr.Length];

            for(int i = 0; i < RectNodeArr.Length; i++)
            {
                float x = 0f, y = 0f;
                Color _Color;

                //Hidden Neurons
                if (i>=InputNeurons+OutputNeurons)
                {
                    float index = i - (InputNeurons + OutputNeurons);
                    x = Mathf.Cos(RadiansDiff*index) * (ScreenWidth * 0.2f) + (ScreenWidth/2f);
                    y = Mathf.Sin(RadiansDiff*index) * (ScreenWidth * 0.2f) + (InputNeurons * NeuronSize * 1.2f) + MinHeight;
                    _Color = ActivationTypeToColor(SelectedNodyMeta.NEAT_Decoder.NodeArr[i].Type);
                }
                //OuputNeurons
                else if(i>=InputNeurons)
                {
                    float index = i - InputNeurons;
                    x = ScreenWidth - (NeuronSize*2f);
                    y = index * (NeuronSize* 1.2f) + MinHeight;
                    _Color = ActivationTypeToColor(SelectedNodyMeta.NEAT_Decoder.NodeArr[i].Type);
                }
                //Input neurons
                else
                {
                    float index = i;
                    x = NeuronSize;
                    y = index * (NeuronSize * 1.2f)  + MinHeight;
                    _Color = Color.magenta;
                }

                RectNodeArr[i] = new RectNode(_Color,new Rect(x, y, NeuronSize, NeuronSize));
            }
        }
        else
        {
            RectNodeArr = null;
            TextureNodyBrain = null;
        }
       
    }

    public Color ActivationTypeToColor(NodyBrainActivationType type)
    {
        switch (type)
        {
            case NodyBrainActivationType.TANH: return Color.red;
            case NodyBrainActivationType.FTANH: return Color.gray;
            case NodyBrainActivationType.DTANH: return Color.magenta;
            case NodyBrainActivationType.SIGMOID: return Color.green;
            case NodyBrainActivationType.DSIGMOID: return Color.black;
            case NodyBrainActivationType.SIN: return Color.cyan;
            case NodyBrainActivationType.COS: return Color.yellow;
            
            default: return Color.red;
        }
    }

    public void ToggleNextEyeLine()
    {
        int NextEyeLineInt = (int)EyeLineType+1;
        if (NextEyeLineInt > (int)VisualEyeLine.TOUCH_LINE)
        {
            NextEyeLineInt = 0;
        }
        EyeLineType = (VisualEyeLine)NextEyeLineInt;
    }


    private void OnGUI()
    {
        if (TextureNodyBrain!=null && RectNodeArr != null && SelectedNodyMeta != null)
        {
            //NodyStaticMeta M = SelectedNodyMeta;

            float ScreenWidth = Screen.width*0.2f;
            float ScreenHight = Screen.height;

            GUIStyle style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 12;
            style.normal.textColor = Color.white;

            //Background
            //GUI.color = Color.white;
            //GUI.DrawTexture(new Rect(0, 0, ScreenWidth, ScreenHight), TextureNodyBrain,ScaleMode.ScaleToFit,false);

            //Draw Nodes and Connection
            NodyBrainConnectionMeta[] ConnectionArr = SelectedNodyMeta.NEAT_Decoder.ConnectionArr;

            GUI.Label(new Rect(10, 10, ScreenWidth, 10)
                    , string.Format("World Time:{0}, World Ticks:{1}"
                        ,(int)TotalWorldTime
                        ,(int)(TotalWorldTime/Time.fixedDeltaTime))
                    , style);

            GUI.Label(new Rect(10, 20, ScreenWidth, 20)
                    , string.Format("Name:{0}\n  Health:{1}, Max Health:{2}\n  Connection Count:{3}, Time Lived:{4}, Species Id:{5}, Children:{6}"
                        , SelectedNodyMeta.Name
                        , (int)SelectedNodyMeta.Health
                         , (int)SelectedNodyMeta.Calc_MaxHealthAllowed
                        , ConnectionArr.Length
                        , (int)SelectedNodyMeta.TimeLived
                        , SelectedNodyMeta.SpeciesId
                        , SelectedNodyMeta.ChildCount)
                    , style);
            int HiddenIndex = SelectedNodyMeta._InputNeuronCount + SelectedNodyMeta._OutputNeuronCount;
            for (int i = 0; i < ConnectionArr.Length; i++)
            {
                NodyBrainConnectionMeta C = ConnectionArr[i];
                RectNode ToR = RectNodeArr[C.NodeIndex];
                RectNode FromR = RectNodeArr[C.OtherNodeIndex];
                Color _Color = C.Weight >= 0f ? Color.green : Color.red;
                float _Thickness = Mathf.Abs(C.Weight) / 0.75f;
                _Thickness = _Thickness < 0.75f ? 0.75f : _Thickness;

                if (C.NodeIndex == C.OtherNodeIndex)
                {
                    Vector2 Down = ToR._Rect.center + new Vector2(7f,7f);
                    UtiltiyTexture.DrawLine(ToR._Rect.center, Down, Color.cyan, _Thickness);
                }
                else
                {
                    UtiltiyTexture.DrawLine(ToR._Rect.center, FromR._Rect.center, _Color, _Thickness);
                }
            }

            for (int i = 0; i < RectNodeArr.Length; i++)
            {
                RectNode R = RectNodeArr[i];

                float V = SelectedNodyMeta.NEAT_Decoder.NodeArr[i].Value;
                
                Color c = Color.HSVToRGB(Mathf.Abs(V), V>=0f?1f:0.5f,1f);
                /*Color c = Color.HSVToRGB(1f, V, 1f);
                if (SelectedNodyMeta.NEAT_Decoder.NodeArr[i].Value>=0f)
                {
                    
                    c = Color.HSVToRGB(0.5f, V, 1f);
                }*/

                GUI.color = c;

               

                GUI.DrawTexture(R._Rect, TextureNodyBrain,ScaleMode.StretchToFill, false);
            }

            if (SpeciesCountDataList.Count > 0)
            {
                float Total = 0f;
                for (int i = 0; i < SpeciesCountDataList.Count; i++)
                {
                    SpeciesCountData S = SpeciesCountDataList[i];
                    Total += S.Count;
                }
                float PerUnit = ScreenWidth / Total;

          
                float XOffset = 0f;
                int ColorIndex = 0;
                float SpeciesSeperationPixels = 2;
                for (int i = 0; i < SpeciesCountDataList.Count; i++)
                {
                    if(SpeciesCountDataList[i].Count>1)
                    {
                        GUI.color = Color.white;
                        GUI.DrawTexture(new Rect(XOffset, SpeciesDataMinHeight, SpeciesSeperationPixels, 35), TextureNodyBrain, ScaleMode.StretchToFill, false);

                        XOffset += SpeciesSeperationPixels;

                        float EndXOffset = XOffset + (SpeciesCountDataList[i].Count);
                        Color C = Colors[ColorIndex % Colors.Length];


                        GUI.color = C;
                        GUI.DrawTexture(new Rect(XOffset, SpeciesDataMinHeight, (EndXOffset - XOffset), 35), TextureNodyBrain, ScaleMode.StretchToFill, false);
                        XOffset = EndXOffset;
                        ColorIndex++;

                    }
                }
            }


        }
    } 

}
