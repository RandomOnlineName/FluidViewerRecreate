using Assets;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class liquidArea : MonoBehaviour
{
    public Mesh particleMesh;
    public Material particleMaterial;
    public Material particleMaterialCompare;
    public Material obstacleMaterial;
    public Transform cameraTransform;
    List<Particle> particles = new List<Particle>();
    List<Particle> particlesCompare = new List<Particle>();
    public bool paused;
    public bool compare;
    public bool renderCompare;
    Mesh obstacle;
    Mesh obstacleCompare;
    byte[] fileBinary;
    byte[] fileBinaryCompare;
    public bool obstacleRender = false;
    public bool obstacleRenderCompare = false;
    CustomBuffer buffer;
    CustomBuffer bufferCompare;
    private int limit;
    private int limitCompare;
    string binaryFileName = null;
    string binaryFileNameCompare = null;
    bool finnishedBinaryFile = false;
    bool finnishedBinaryFileCompare = false;

    void Start() {
        obstacle = new Mesh();
        obstacle.Clear();


        var extensions = new[] {
            new ExtensionFilter("Binary File", "binfluid"),
            new ExtensionFilter("All Files", "*" )
        };

        StandaloneFileBrowser.OpenFilePanelAsync("Open fluid file", "", extensions, false, (string[] paths) => { binaryFileName = paths[0]; });
        if (compare)
        {
            StandaloneFileBrowser.OpenFilePanelAsync("Open fluid file compare", "", extensions, false, (string[] paths) => { binaryFileNameCompare = paths[0]; });
        }
    }

    // Update is called once per frame
    public int tickCount = 0;
    double readNextFrameTime = 0;
    double renderPariclesTime = 0;
    void reset() {
        tickCount = 0;
        finnishedBinaryFile = false;
        buffer = new CustomBuffer(fileBinary);
        particles = new List<Particle>();
        if (compare)
        {
            finnishedBinaryFileCompare = false;
            bufferCompare = new CustomBuffer(fileBinary);
            particlesCompare = new List<Particle>();
        }
        paused = false;
    }

    void Update() {
        if (tickCount == 1) {
            paused = true;
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            reset();
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            paused = !paused;
        }

        if (!finnishedBinaryFile && binaryFileName != null) {
            FileStream file = File.Open(binaryFileName, FileMode.Open);
            int length = (int)file.Length;
            int triangleCount;
            using (BinaryReader br = new BinaryReader(file)) {
                triangleCount = br.ReadInt32();
                var list = new List<Vector3>(triangleCount * 3 * 2);
                for (int i = 0; i < triangleCount; i++) {
                    float x1 = br.ReadSingle();
                    float y1 = br.ReadSingle();
                    float z1 = br.ReadSingle();
                    float x2 = br.ReadSingle();
                    float y2 = br.ReadSingle();
                    float z2 = br.ReadSingle();
                    float x3 = br.ReadSingle();
                    float y3 = br.ReadSingle();
                    float z3 = br.ReadSingle();
                    list.Add(new Vector3(x1, y1, z1));
                    list.Add(new Vector3(x2, y2, z2));
                    list.Add(new Vector3(x3, y3, z3));
                    list.Add(new Vector3(x1, y1, z1));
                    list.Add(new Vector3(x3, y3, z3));
                    list.Add(new Vector3(x2, y2, z2));
                }
                obstacle.vertices = list.ToArray();
                int[] indexArray = new int[list.Count];
                for (int i = 0; i < indexArray.Length; i += 3) {
                    indexArray[i] = i;
                    indexArray[i + 1] = i + 1;
                    indexArray[i + 2] = i + 2;
                }
                obstacle.triangles = indexArray;
                obstacle.RecalculateNormals();
            }
            byte[] fullBinary = File.ReadAllBytes(binaryFileName);
            fileBinary = new byte[length - (triangleCount * 9 + 1) * sizeof(float)];
            Array.Copy(fullBinary, (triangleCount * 9 + 1) * sizeof(float), fileBinary, 0, length - (triangleCount * 9 + 1) * sizeof(float));
            buffer = new CustomBuffer(fileBinary);
            this.limit = buffer.readInt();
            finnishedBinaryFile = true;
        }
        if (!finnishedBinaryFileCompare && binaryFileNameCompare != null && compare) {
            FileStream file = File.Open(binaryFileNameCompare, FileMode.Open);
            int length = (int)file.Length;
            int triangleCount;
            using (BinaryReader br = new BinaryReader(file)) {
                triangleCount = br.ReadInt32();
                var list = new List<Vector3>(triangleCount * 3 * 2);
                for (int i = 0; i < triangleCount; i++) {
                    float x1 = br.ReadSingle();
                    float y1 = br.ReadSingle();
                    float z1 = br.ReadSingle();
                    float x2 = br.ReadSingle();
                    float y2 = br.ReadSingle();
                    float z2 = br.ReadSingle();
                    float x3 = br.ReadSingle();
                    float y3 = br.ReadSingle();
                    float z3 = br.ReadSingle();
                    list.Add(new Vector3(x1, y1, z1));
                    list.Add(new Vector3(x2, y2, z2));
                    list.Add(new Vector3(x3, y3, z3));
                    list.Add(new Vector3(x1, y1, z1));
                    list.Add(new Vector3(x3, y3, z3));
                    list.Add(new Vector3(x2, y2, z2));
                }
                obstacle.vertices = list.ToArray();
                int[] indexArray = new int[list.Count];
                for (int i = 0; i < indexArray.Length; i += 3) {
                    indexArray[i] = i;
                    indexArray[i + 1] = i + 1;
                    indexArray[i + 2] = i + 2;
                }
                obstacle.triangles = indexArray;
                obstacle.RecalculateNormals();
            }
            byte[] fullBinary = File.ReadAllBytes(binaryFileNameCompare);
            fileBinaryCompare = new byte[length - (triangleCount * 9 + 1) * sizeof(float)];
            Array.Copy(fullBinary, (triangleCount * 9 + 1) * sizeof(float), fileBinaryCompare, 0, length - (triangleCount * 9 + 1) * sizeof(float));
            bufferCompare = new CustomBuffer(fileBinaryCompare);
            this.limitCompare = bufferCompare.readInt();
            finnishedBinaryFileCompare = true;
        }

        if(finnishedBinaryFile && (!compare || finnishedBinaryFileCompare))
            run();
    }

    void run() {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        if (!paused) {
            if (tickCount < limit) {
                readNextFrame();
                watch.Stop();
                readNextFrameTime += (watch.ElapsedMilliseconds);
            }
            else if (tickCount == limit) {
                paused = true;
                Debug.Log(readNextFrameTime / tickCount);
                Debug.Log(renderPariclesTime / tickCount);
                Debug.Log(particles.Count);
            }
            tickCount++;
        }
        watch = System.Diagnostics.Stopwatch.StartNew();
        renderParicles();
        watch.Stop();
        renderPariclesTime += (watch.ElapsedMilliseconds);
    }

    void readNextFrame() {
        if(tickCount < limit) {
            int particleNum = buffer.readInt();
            for(int i = 0; i < particleNum; i++){
                //Debug.Log(i);
                if (i < particles.Count) {
                    particles[i].position.x = buffer.readFloat();
                    particles[i].position.y = buffer.readFloat();
                    particles[i].position.z = buffer.readFloat();
                }
                else {
                    Vector3 position = new Vector3();
                    position.x = buffer.readFloat();
                    position.y = buffer.readFloat();
                    position.z = buffer.readFloat();
                    this.particles.Add(new Particle(position));
                }
            }
        }
        if(tickCount < limitCompare && compare) {
            int particleNum = bufferCompare.readInt();
            for(int i = 0; i < particleNum; i++){
                //Debug.Log(i);
                if (i < particlesCompare.Count) {
                    particlesCompare[i].position.x = bufferCompare.readFloat();
                    particlesCompare[i].position.y = bufferCompare.readFloat();
                    particlesCompare[i].position.z = bufferCompare.readFloat();
                }
                else {
                    Vector3 position = new Vector3();
                    position.x = bufferCompare.readFloat();
                    position.y = bufferCompare.readFloat();
                    position.z = bufferCompare.readFloat();
                    this.particlesCompare.Add(new Particle(position));
                }
            }
        }
        if (compare)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].position != particlesCompare[i].position)
                {
                    Debug.Log("particle id: " + i + ", frame: " + tickCount);
                }
            }
        }
    }

    public Vector3 renderScale = new Vector3(0.2f,0.2f,0.2f);
    void renderParicles() {
        for (int i = 0; i < particles.Count; i += 1023) {
            int length = Mathf.Min((particles.Count - i), 1023);
            Matrix4x4[] transforms = new Matrix4x4[length];
            for (int j = 0; j < transforms.Length; j++) {
                Vector3 cameraOffset = particles[i + j].position - cameraTransform.position;
                Vector3 cameraUp = cameraTransform.up;
                Quaternion q = Quaternion.LookRotation(cameraOffset.normalized, cameraUp);

                transforms[j] = Matrix4x4.TRS(particles[i + j].position, q, renderScale);
            }
            Graphics.DrawMeshInstanced(particleMesh, 0, particleMaterial, transforms);
        }
        if (obstacle != null) {
            if (obstacleRender) {
                Matrix4x4 transform = Matrix4x4.identity;
                
                Graphics.DrawMesh(obstacle, transform, obstacleMaterial, 0);
            }
        }
        if (compare && renderCompare)
        {
            for (int i = 0; i < particlesCompare.Count; i += 1023)
            {
                int length = Mathf.Min((particlesCompare.Count - i), 1023);
                Matrix4x4[] transforms = new Matrix4x4[length];
                for (int j = 0; j < transforms.Length; j++)
                {
                    Vector3 cameraOffset = particlesCompare[i + j].position - cameraTransform.position;
                    Vector3 cameraUp = cameraTransform.up;
                    Quaternion q = Quaternion.LookRotation(cameraOffset.normalized, cameraUp);

                    transforms[j] = Matrix4x4.TRS(particlesCompare[i + j].position, q, renderScale);
                }
                Graphics.DrawMeshInstanced(particleMesh, 0, particleMaterialCompare, transforms);
            }
        }
    }
}
