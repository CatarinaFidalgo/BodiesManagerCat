﻿
using UnityEngine;
using System.Collections.Generic;

public class PointCloudMesh : MonoBehaviour
{
    uint _id;
    Texture2D _colorTex;
    Texture2D _depthTex;
    List<GameObject> _objs;
    Material _mat;
    RVLDecoder _decoder;
    Decompressor _colorDecoder;
    byte[] _depthBytes;
    byte[] _colorBytes;
    int _texScale;
    int _width;
    int _height;

    public float sigmaS = 3;
    public float sigmaT = 3;
    public int medianFilterSize = 2;
    public bool calculateNormals = true;

    private bool _local;

    public void Init(bool local)
    {
        _width = 512;
        _height = 424;
        _texScale = 1;
        _objs = null;

        _local = local;

        string matDescription = local ? "Materials/localmat" : "Materials/remotemat";

        _mat = Resources.Load(matDescription) as Material;

        _decoder = new RVLDecoder();
        _colorDecoder = new Decompressor();

        initStructs();
    }

    int createSubmesh(int h, int submeshHeight, int id)
    {
        List<Vector3> points = new List<Vector3>();
        //  List<int> ind = new List<int>();
        List<int> tri = new List<int>();
        int n = 0;

        for (int k = 0; k < submeshHeight; k++, h++)
        {
            for (int w = 0; w < _width; w++)
            {
                Vector3 p = new Vector3(w / (float)_width, h / (float)_height, 0);
                points.Add(p);
                // ind.Add(n);

                // Skip the last row/col
                if (w != (_width - 1) && k != (submeshHeight - 1))
                {
                    int topLeft = n;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + _width;
                    int bottomRight = bottomLeft + 1;

                    tri.Add(topLeft);
                    tri.Add(topRight);
                    tri.Add(bottomLeft);
                    tri.Add(bottomLeft);
                    tri.Add(topRight);
                    tri.Add(bottomRight);
                }
                n++;
            }
        }

        GameObject a = new GameObject("cloud" + id);
        MeshFilter mf = a.AddComponent<MeshFilter>();
        MeshRenderer mr = a.AddComponent<MeshRenderer>();
        mr.material = _mat;
        mf.mesh = new Mesh();
        mf.mesh.vertices = points.ToArray();
        //  mf.mesh.SetIndices(ind.ToArray(), MeshTopology.Triangles, 0);
        mf.mesh.SetTriangles(tri.ToArray(), 0);
        mf.mesh.bounds = new Bounds(new Vector3(0, 0, 4.5f), new Vector3(5, 5, 5));
        a.transform.parent = this.gameObject.transform;
        a.transform.localPosition = Vector3.zero;
        a.transform.localRotation = Quaternion.identity;
        a.transform.localScale = new Vector3(1, 1, 1);
        n = 0;
        _objs.Add(a);

        return h;
    }

    void createStitchingMesh(int submeshHeight, int id)
    {
        List<Vector3> points = new List<Vector3>();
        //  List<int> ind = new List<int>();
        List<int> tri = new List<int>();
        int n = 0;

        for (int h = submeshHeight - 1; h < _height; h += submeshHeight)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int w = 0; w < _width; w++)
                {
                    Vector3 p = new Vector3(w / (float)_width, (h + i) / (float)_height, 0);

                    points.Add(p);
                    // ind.Add(n);

                    // Skip the last row/col
                    if (w != (_width - 1) && i == 0)
                    {
                        int topLeft = n;
                        int topRight = topLeft + 1;
                        int bottomLeft = topLeft + _width;
                        int bottomRight = bottomLeft + 1;

                        tri.Add(topLeft);
                        tri.Add(topRight);
                        tri.Add(bottomLeft);
                        tri.Add(bottomLeft);
                        tri.Add(topRight);
                        tri.Add(bottomRight);
                    }
                    n++;
                }
            }
        }

        GameObject a = new GameObject("cloud" + id);
        MeshFilter mf = a.AddComponent<MeshFilter>();
        MeshRenderer mr = a.AddComponent<MeshRenderer>();
        mr.material = _mat;
        mf.mesh = new Mesh();
        mf.mesh.vertices = points.ToArray();
        //  mf.mesh.SetIndices(ind.ToArray(), MeshTopology.Triangles, 0);
        mf.mesh.SetTriangles(tri.ToArray(), 0);
        mf.mesh.bounds = new Bounds(new Vector3(0, 0, 4.5f), new Vector3(5, 5, 5));
        a.transform.parent = this.gameObject.transform;
        a.transform.localPosition = Vector3.zero;
        a.transform.localRotation = Quaternion.identity;
        a.transform.localScale = new Vector3(1, 1, 1);
        n = 0;
        _objs.Add(a);
    }

    void initStructs()
    {
        _colorTex = new Texture2D(_width, _height, TextureFormat.BGRA32, false);
        _depthTex = new Texture2D(_width, _height, TextureFormat.BGRA32, false);
        _colorTex.filterMode = FilterMode.Point;
        _depthTex.filterMode = FilterMode.Point;
        _depthBytes = new byte[_width * _height * 4];
        if (_objs != null)
        {
            foreach (GameObject g in _objs)
            {
                GameObject.Destroy(g);
            }
        }
        _objs = new List<GameObject>();

        List<Vector3> points = new List<Vector3>();
        List<int> ind = new List<int>();
       
        int h = 0;
        int submeshes;
        for (submeshes = 0; submeshes < 4; submeshes++)
        {
            h = createSubmesh(h, _height / 4, submeshes);

        }
        createStitchingMesh(_height / 4, submeshes);
    }

    public void hide()
    {
        foreach (GameObject a in _objs)
            a.SetActive(false);
    }

    public void show()
    {
        foreach (GameObject a in _objs)
            a.SetActive(true);
    }

    public void setPoints(byte[] colorBytes, byte[] depthBytes, bool compressed, int sizec, int scale)
    {
        if (scale != _texScale)
        {
            _texScale = scale;
            _width = Mathf.CeilToInt(512.0f / scale);
            _height = Mathf.CeilToInt(424.0f / scale);
            initStructs();
        }

        if (compressed)
        {
            bool ok = _decoder.DecompressRVL(depthBytes, _depthBytes, _width * _height);
            _colorDecoder.Decompress(colorBytes, colorBytes, sizec);
            if (ok)
            {
                _depthTex.LoadRawTextureData(_depthBytes);
                _colorTex.LoadRawTextureData(colorBytes);
            }
        }
        else
        {
            _depthTex.LoadRawTextureData(depthBytes);
            _colorTex.LoadRawTextureData(colorBytes);
        }
        _colorTex.Apply();
        _depthTex.Apply();



        


        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer mr = renderers[i];
            mr.material.SetInt("_TexScale", _texScale);
            mr.material.SetTexture("_ColorTex", _colorTex);
            mr.material.SetTexture("_DepthTex", _depthTex);
            mr.material.SetFloat("_sigmaS", sigmaS);
            mr.material.SetFloat("_sigmaS", sigmaS);
            mr.material.SetInt("_SizeFilter", medianFilterSize);
            mr.material.SetInt("_calculateNormals", calculateNormals? 1:0);




            //mr.material.SetInt("_LeftWarping", wi.leftWarping ? 1 : 0);
            //mr.material.SetInt("_RightWarping", wi.rightWarping ? 1 : 0);
            //mr.material.SetFloat("_Distance", 0.5f);

            //mr.material.SetVector("_LEFT_OriginalShoulder", wi.LEFT_OriginalShoulder);
            //mr.material.SetVector("_LEFT_OriginalElbow", wi.LEFT_OriginalElbow);
            //mr.material.SetVector("_LEFT_OriginalWrist", wi.LEFT_OriginalWrist);
            //mr.material.SetVector("_LEFT_OriginalHandTip", wi.LEFT_OriginalHandTip);

            //mr.material.SetVector("_RIGHT_OriginalShoulder", wi.RIGHT_OriginalShoulder);
            //mr.material.SetVector("_RIGHT_OriginalElbow", wi.RIGHT_OriginalElbow);
            //mr.material.SetVector("_RIGHT_OriginalWrist", wi.RIGHT_OriginalWrist);
            //mr.material.SetVector("_RIGHT_OriginalHandTip", wi.RIGHT_OriginalHandTip);
        }

    }

    public void setPointsUncompressed(byte[] colorBytes, byte[] depthBytes)
    {
       
        _depthTex.LoadRawTextureData(depthBytes);
        _colorTex.LoadRawTextureData(colorBytes);

        _colorTex.Apply();
        _depthTex.Apply();

        //BodiesManager b = GameObject.Find( _local ? "LocalBodiesManager" : "RemoteBodiesManager").GetComponent<BodiesManager>();
        //IKWarpInfo wi = b.armsWarpInfo;



        try
        {
            //Body b = GameObject.Find("BodiesManager").GetComponent<BodiesManager>().human.body;
            BodiesManager bm = GameObject.Find("BodiesManagerCat").GetComponent<BodiesManager>();


            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                MeshRenderer mr = renderers[i];
                mr.material.SetInt("_TexScale", _texScale);
                mr.material.SetTexture("_ColorTex", _colorTex);
                mr.material.SetTexture("_DepthTex", _depthTex);
                mr.material.SetFloat("_sigmaS", sigmaS);
                mr.material.SetFloat("_sigmaS", sigmaS);
                mr.material.SetInt("_SizeFilter", medianFilterSize);
                mr.material.SetInt("_calculateNormals", calculateNormals ? 1 : 0);


                mr.material.SetInt("_RemoveHead", bm.removeHead ? 1 : 0);
                mr.material.SetVector("_VRHead", bm.head.position);
                mr.material.SetFloat("_HeadSize", bm.headSize);
                mr.material.SetFloat("_Y_HeadOffset", bm.Y_HeadOffset);



                //mr.material.SetInt("_Warping",  0);

                //mr.material.SetInt("_RemoveHead", wi.removeHead ? 1 : 0);
                //mr.material.SetVector("_VRHead", wi.VRHead);
                //mr.material.SetFloat("_HeadSize", wi.headSize);
                //mr.material.SetFloat("_Y_HeadOffset", wi.Y_HeadOffset);

                //if (!_local)
                //{
                //    //mr.material.SetInt("_LeftWarping", wi.leftWarping ? 1 : 0);
                //    //mr.material.SetInt("_RightWarping", wi.rightWarping ? 1 : 0);
                //    mr.material.SetInt("_LeftWarping", 1);
                //    mr.material.SetInt("_RightWarping", 1);

                //    mr.material.SetInt("_Debug", wi.debug ? 1 : 0);

                //    mr.material.SetFloat("_tabley", b.table.position.y);

                //    mr.material.SetVector("_LEFT_OriginalShoulder", wi.LEFT_OriginalShoulder);
                //    mr.material.SetVector("_LEFT_OriginalElbow", wi.LEFT_OriginalElbow);
                //    mr.material.SetVector("_LEFT_OriginalWrist", wi.LEFT_OriginalWrist);
                //    mr.material.SetVector("_LEFT_OriginalHandTip", wi.LEFT_OriginalHandTip);

                //    mr.material.SetVector("_RIGHT_OriginalShoulder", wi.RIGHT_OriginalShoulder);
                //    mr.material.SetVector("_RIGHT_OriginalElbow", wi.RIGHT_OriginalElbow);
                //    mr.material.SetVector("_RIGHT_OriginalWrist", wi.RIGHT_OriginalWrist);
                //    mr.material.SetVector("_RIGHT_OriginalHandTip", wi.RIGHT_OriginalHandTip);

                //    mr.material.SetMatrix("_LEFT_UpperArmMatrix", wi.LEFT_UpperArmMatrix);
                //    mr.material.SetMatrix("_LEFT_ForearmMatrix", wi.LEFT_ForearmMatrix);
                //    mr.material.SetMatrix("_LEFT_HandMatrix", wi.LEFT_HandMatrix);

                //    mr.material.SetMatrix("_RIGHT_UpperArmMatrix", wi.RIGHT_UpperArmMatrix);
                //    mr.material.SetMatrix("_RIGHT_ForearmMatrix", wi.RIGHT_ForearmMatrix);
                //    mr.material.SetMatrix("_RIGHT_HandMatrix", wi.RIGHT_HandMatrix);

                //    //////// BODY
                //    mr.material.SetVector("head", b.head.position);
                //    mr.material.SetVector("neck", b.head.position);
                //    mr.material.SetVector("spineShoulder", b.spineShoulder.position);
                //    mr.material.SetVector("spineMid", b.spineMid.position);
                //    mr.material.SetVector("spineBase", b.spineBase.position);
                //    mr.material.SetVector("leftShoulder", wi.LEFT_OriginalShoulder);
                //    mr.material.SetVector("leftElbow", wi.LEFT_OriginalElbow);
                //    mr.material.SetVector("leftWrist", wi.LEFT_OriginalWrist);
                //    mr.material.SetVector("leftHand", b.leftHand.position);
                //    mr.material.SetVector("leftThumb", b.leftThumb.position);
                //    mr.material.SetVector("leftHandTip", wi.LEFT_OriginalHandTip);
                //    mr.material.SetVector("leftHip", b.leftHip.position);
                //    mr.material.SetVector("leftKnee", b.leftKnee.position);
                //    mr.material.SetVector("leftAnkle", b.leftAnkle.position);
                //    mr.material.SetVector("leftFoot", b.leftFoot.position);
                //    mr.material.SetVector("rightShoulder", wi.RIGHT_OriginalShoulder);
                //    mr.material.SetVector("rightElbow", wi.RIGHT_OriginalElbow);
                //    mr.material.SetVector("rightWrist", wi.RIGHT_OriginalWrist);
                //    mr.material.SetVector("rightHand", b.rightHand.position);
                //    mr.material.SetVector("rightThumb", b.rightThumb.position);
                //    mr.material.SetVector("rightHandTip", wi.RIGHT_OriginalHandTip);
                //    mr.material.SetVector("rightHip", b.rightHip.position);
                //    mr.material.SetVector("rightKnee", b.rightKnee.position);
                //    mr.material.SetVector("rightAnkle", b.rightAnkle.position);
                //    mr.material.SetVector("rightFoot", b.rightFoot.position);
                //    mr.material.SetVector("LEGBONE", b.LEGBONE.position);
                //}
            }
        }
        catch
        {
            print("no body");
        }
    }

}
