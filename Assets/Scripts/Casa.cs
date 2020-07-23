using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class Casa : MonoBehaviour
{
    public float profundidade = 10f;
    public float largura = 10f;
    public float altura = 10f;
    public float variacaoTetoX = 1f;
    public float velocidade = 1f;

    public MeshFilter meshFilter;
    public Animation simulationAnimation;
    public Camera mainCamera;
    public GameObject pessoa;

    public List<GameObject> pontosCasa;

    public float fps = 30f;

    public int bloqueioDeCarregamento = 14;

    private Mesh _meshGerada;

    private List<Andar> _andares = new List<Andar>();

    private string _path = "Assets/Resources/frames-1.txt";

    private float horizontalSpeed = 7.0f;
    private float verticalSpeed = 7.0f;

    private List<List<List<Keyframe>>> _verticesKeyframes = new List<List<List<Keyframe>>>();
    private AnimationClip _simulationClip;

    private Dictionary<TypesSurface, int> _superficiesSubMeshes = new Dictionary<TypesSurface, int>()
    {
        {TypesSurface.OUTWALL, 1 },
        {TypesSurface.INWALL, 1 },
        {TypesSurface.ROOF, 1 },
        {TypesSurface.TOPROOF, 1 },
        {TypesSurface.BASE, 0 }
    };

    private int _numSubMeshes = 2;

    // Start is called before the first frame update
    void Start()
    {
        StreamReader textReader = new StreamReader(_path, true);
        _simulationClip = new AnimationClip();
        _simulationClip.legacy = true;
        _simulationClip.name = "Simulation";
        _simulationClip.wrapMode = WrapMode.Loop;
        _simulationClip.frameRate = fps;

        CarregarCasa(textReader);
        
        simulationAnimation.AddClip(_simulationClip, _simulationClip.name);
        simulationAnimation.Play(_simulationClip.name); 
    }

    // Update is called once per frame
    void Update()
    {
        UpdateVertices();

        float h  = horizontalSpeed * Input.GetAxis("Mouse X");
        pessoa.transform.Rotate(0, h, 0);
        float v  = verticalSpeed * Input.GetAxis("Mouse Y");
        mainCamera.transform.Rotate(-v, 0, 0);
    }

    internal int GetNumSubMeshs()
    {
        return _numSubMeshes;
    }

    internal int GetSubMeshParaSuperficie(TypesSurface tipo)
    {
        return _superficiesSubMeshes[tipo];
    }

    internal Vector3 GetPonto(int id)
    {
        return pontosCasa[id - 1].transform.position;
    }

    internal void AddBoxCollider()
    {
        gameObject.AddComponent<BoxCollider>();
    }

    private void CarregarCasa(StreamReader arquivoDaCasa)
    {
        _meshGerada = new Mesh();
        _meshGerada.name = "CasaMesh";
        string linhaAtual;
        bool firstFrame = true;
        int frameCount = 0;
        while((linhaAtual = arquivoDaCasa.ReadLine()) != null)
        {
            if(linhaAtual == "")
            {
                continue;
            }
            else if(linhaAtual == "#faces")
            {
                CarregarSuperficies(arquivoDaCasa);
            }
            else if(linhaAtual == "#frame")
            {
                if(firstFrame)
                {
                    LoadFrame(arquivoDaCasa, firstFrame, ref frameCount);
                    firstFrame = false;
                    UpdateVertices();
                    List<List<int>> triangulos = new List<List<int>>();
                    List<List<Vector2>> uvs = new List<List<Vector2>>();
                    for (int i = 0; i < _numSubMeshes; i++)
                    {
                        triangulos.Add(new List<int>());
                        uvs.Add(new List<Vector2>());
                    }
                    foreach (var andar in _andares)
                    {
                        andar.AdicionarAndar(ref triangulos, ref uvs);
                    }
                    _meshGerada.subMeshCount = _numSubMeshes;
                    for (int i = 0; i < _numSubMeshes; i++)
                    {
                        _meshGerada.SetUVs(i, uvs[i]);
                        _meshGerada.SetTriangles(triangulos[i].ToArray(), i);
                    }
                    _meshGerada.RecalculateNormals();
                }
                else
                {
                    LoadFrame(arquivoDaCasa, firstFrame, ref frameCount);
                }
                // TODO: CarregaFrame();
            }
            else
            {
                continue;
            }
        }
        SaveVerticesKeyframes();
        meshFilter.mesh = _meshGerada;
    }

    private void SaveVerticesKeyframes()
    {
        for (int i = 0; i < _verticesKeyframes.Count; i++)
        {
            List<List<Keyframe>> vertexKeyframes = _verticesKeyframes[i];

            var curveX = new AnimationCurve(vertexKeyframes[0].ToArray());
            _simulationClip.SetCurve("P" + (i + 1), typeof(Transform), "localPosition.x", curveX);

            var curveY = new AnimationCurve(vertexKeyframes[1].ToArray());
            _simulationClip.SetCurve("P" + (i + 1), typeof(Transform), "localPosition.y", curveY);

            var curveZ = new AnimationCurve(vertexKeyframes[2].ToArray());
            _simulationClip.SetCurve("P" + (i + 1), typeof(Transform), "localPosition.z", curveZ);
        }
        
    }

    private void UpdateVertices()
    {
        List<Vector3> vertices = new List<Vector3>();
        foreach (var andar in _andares)
        {
            andar.UpdateVertices(ref vertices);
        }
        _meshGerada.SetVertices(vertices);

    }

    private void CarregarSuperficies(StreamReader arquivoDaCasa)
    {
        string linhaAtual;
        string idAndar = "";
        while (!string.IsNullOrEmpty(linhaAtual = arquivoDaCasa.ReadLine()))
        {
            string[] currentSurface = linhaAtual.Split(' ');
            idAndar = currentSurface[0];
            int surfaceId = int.Parse(currentSurface[1]);
            Andar andar = new Andar(int.Parse(idAndar), this);
            andar.CarregaSuperficie(currentSurface);
            _andares.Add(andar);
        }
    }

    private void LoadFrame(StreamReader arquivoDaCasa, bool firstFrame, ref int count)
    {
        Debug.Log("Loading frame #" + count);
        string linhaAtual;
        while (!string.IsNullOrEmpty(linhaAtual = arquivoDaCasa.ReadLine()))
        {
            if(linhaAtual.Contains("#frame.nodes.aux"))
            {
                if(firstFrame)
                {
                    var cameraPosData = arquivoDaCasa.ReadLine().Split(' ');
                    var cameraLookAtData = arquivoDaCasa.ReadLine().Split(' ');

                    pessoa.transform.position = new Vector3(
                        float.Parse(cameraPosData[1], NumberStyles.Any, CultureInfo.InvariantCulture),
                        float.Parse(cameraPosData[2], NumberStyles.Any, CultureInfo.InvariantCulture),
                        float.Parse(cameraPosData[3], NumberStyles.Any, CultureInfo.InvariantCulture)
                    );
                    mainCamera.transform.Rotate(new Vector3(0f, 1f, 0f), 45f);
                }
                else
                {
                    arquivoDaCasa.ReadLine();
                    arquivoDaCasa.ReadLine();
                }
                break;

            }
            string[] vertexData = linhaAtual.Split(' ');
            if (vertexData.Length != 4)
            {
                throw new Exception("Arquivo da casa mal formatado na linha: " + linhaAtual);
            }
            float x = float.Parse(vertexData[1], NumberStyles.Any, CultureInfo.InvariantCulture);
            float y = float.Parse(vertexData[2], NumberStyles.Any, CultureInfo.InvariantCulture);
            float z = float.Parse(vertexData[3], NumberStyles.Any, CultureInfo.InvariantCulture);

            if (firstFrame)
            {
                var vertexKeyFrames = new List<List<Keyframe>>();

                Vector3 p = new Vector3(x, y, z);
                GameObject goPoint = new GameObject("P" + vertexData[0]);
                goPoint.transform.parent = transform;
                goPoint.transform.position = p;
                pontosCasa.Add(goPoint);

                var xKeyframes = new List<Keyframe>();
                xKeyframes.Add(new Keyframe(0.0f, x));
                vertexKeyFrames.Add(xKeyframes);

                var yKeyframes = new List<Keyframe>();
                yKeyframes.Add(new Keyframe(0.0f, y));
                vertexKeyFrames.Add(yKeyframes);

                var zKeyframes = new List<Keyframe>();
                zKeyframes.Add(new Keyframe(0.0f, z));
                vertexKeyFrames.Add(zKeyframes);

                _verticesKeyframes.Add(vertexKeyFrames);
            }
            else
            {
                int vertexId = int.Parse(vertexData[0]) - 1;
                
                var xKeyframes = _verticesKeyframes[vertexId][0];
                xKeyframes.Add(new Keyframe((60f / fps) / 60f * count, x));

                var yKeyframes = _verticesKeyframes[vertexId][1];
                yKeyframes.Add(new Keyframe((60f / fps) / 60f * count, y));

                var zKeyframes = _verticesKeyframes[vertexId][2];
                zKeyframes.Add(new Keyframe((60f / fps) / 60f * count, z));
            }
        }

        count++;
    }
}
