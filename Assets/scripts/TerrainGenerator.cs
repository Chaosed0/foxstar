using UnityEngine;
using System.Collections;
using System.IO;

public class TerrainGenerator : MonoBehaviour {
    public int xsize = 128;
    public int ysize = 128;
    public float xscale = 1.0f;
    public float yscale = 1.0f;
    public float zscale = 1.0f;

    public int octaves = 8;
    public float frequencyBase = 2;
    public float persistence = 1.1f;

    public int chunksize = 64;
    public MeshFilter chunkPrefab;

    public float xPerturb = 0.2f;
    public float yPerturb = 0.2f;

    private Vector3[,] sharedVertices;

    /*private Texture2D noiseTex;
    private Color[][] octavePixels;*/

    int id(int x, int y, int xc) {
        return y * (xc - 1) + x;
    }

    void saveTextureToFile(Texture2D texture, string filename) {
        byte[] bytes = texture.EncodeToPNG();
        BinaryWriter binary= new BinaryWriter(File.Open(Application.dataPath + "/" + filename, FileMode.Create));
        binary.Write(bytes);
        binary.Close();
    }

	void Start () {
        Generate();
    }

    /* Returns a rough approximation of the height at a given point in worldspace */
    public float GetElevation(float x, float y) {
        if (x < -xsize/2.0f * xscale || x > xsize/2.0f * xscale ||
                y < -ysize/2.0f * yscale || y > ysize/2.0f * yscale) {
            Debug.Log("Worldspace " + x + "," + y + " out of terrain bounds");
            return 0.0f;
        }

        int localX = (int)(x/xscale + xsize/2.0f);
        int localY = (int)(y/yscale + ysize/2.0f);
        Debug.Log(x + " " + y + " " + localX + " " + localY);
        return sharedVertices[localY,localX].y;
    }

    public void Generate() {
        float[] seed = new float[octaves];
        sharedVertices = new Vector3[xsize,ysize];

        //octavePixels = new Color[octaves+1][];
        for (int i = 0; i < octaves; i++) {
            seed[i] = Random.Range(0.0f, 100.0f);
            //octavePixels[i] = new Color[ysize *xsize];
        }
        //octavePixels[octaves] = new Color[ysize *xsize];

        for (int y = 0; y < ysize; y++) {
            for (int x = 0; x < xsize; x++) {
                float elevation = 0;
                float amplitude = Mathf.Pow(persistence, octaves);
                float frequency = 1.0f;
                float maxVal = 0.0f;
                for (int i = 0; i < octaves; i++) {
                    float sample = (Mathf.PerlinNoise(seed[i] + x / (float)xsize * frequency, seed[i] + y / (float)ysize * frequency) - 0.5f) * amplitude;
                    //octavePixels[i][y * xsize + x] = new Color(sample / amplitude, sample / amplitude, sample / amplitude);
                    elevation += sample;
                    maxVal += amplitude;
                    amplitude /= persistence;
                    frequency *= frequencyBase;
                }
                //octavePixels[octaves][y * xsize + x] = new Color(elevation / maxVal, elevation / maxVal, elevation / maxVal);
                sharedVertices[y,x] = new Vector3((x + Random.Range(-xPerturb, xPerturb)) * xscale, elevation / maxVal * zscale, (y + Random.Range(-yPerturb, yPerturb)) * yscale);
            }
        }

        /*for (int i = 0; i < octaves+1; i++) {
            Texture2D noiseTex = new Texture2D(xsize, ysize);
            noiseTex.SetPixels(octavePixels[i]);
            noiseTex.Apply();
            saveTextureToFile(noiseTex, "octave" + i + ".png");
        }*/

        int xchunks = (int)Mathf.Ceil(xsize / (float)chunksize);
        int ychunks = (int)Mathf.Ceil(ysize / (float)chunksize);
        for (int xchunk = 0; xchunk < xchunks; xchunk++) {
            for (int ychunk = 0; ychunk < ychunks; ychunk++) {
                int xstart = xchunk * chunksize;
                int ystart = ychunk * chunksize;
                int xchunksize = (int)Mathf.Min(xsize, (xchunk + 1) * chunksize) - xstart + (xchunk == xchunks-1 ? 0 : 1);
                int ychunksize = (int)Mathf.Min(ysize, (ychunk + 1) * chunksize) - ystart + (ychunk == ychunks-1 ? 0 : 1);

                Vector3[] vertices = new Vector3[(xchunksize-1)*(ychunksize-1) * 6];
                Vector3[] normals = new Vector3[(xchunksize-1)*(ychunksize-1) * 6];
                Vector2[] uvs = new Vector2[(xchunksize-1)*(ychunksize-1) * 6];

                Vector3 offset = new Vector3(- xsize / 2.0f * xscale, 0.0f, - ysize / 2.0f * yscale);
                int[] triangles = new int[(xchunksize-1)*(ychunksize-1) * 6];
                for (int y = 0; y < ychunksize-1; y++) {
                    for (int x = 0; x < xchunksize-1; x++) {
                        int x0 = xstart + x;
                        int y0 = ystart + y;
                        int i = id(x, y, xchunksize);
                        Vector3 v11, v12, v13, v21, v22, v23;
                        vertices[i*6+0] = v11 = sharedVertices[y0+0, x0+0];
                        vertices[i*6+1] = v12 = sharedVertices[y0+1, x0+0];
                        vertices[i*6+2] = v13 = sharedVertices[y0+0, x0+1];
                        vertices[i*6+3] = v21 = sharedVertices[y0+1, x0+0];
                        vertices[i*6+4] = v22 = sharedVertices[y0+1, x0+1];
                        vertices[i*6+5] = v23 = sharedVertices[y0+0, x0+1];
                        Vector3 n1 = Vector3.Cross(v12 - v11, v13 - v11);
                        Vector3 n2 = Vector3.Cross(v22 - v21, v23 - v21);
                        Vector3 normal = (n1 + n2) / 2.0f;
                        normals[i*6  ] = normal; 
                        normals[i*6+1] = normal;
                        normals[i*6+2] = normal;
                        normals[i*6+3] = normal;
                        normals[i*6+4] = normal;
                        normals[i*6+5] = normal;
                        uvs[i*6  ] = new Vector2(0.0f, 1.0f);
                        uvs[i*6+1] = new Vector2(1.0f, 0.0f);
                        uvs[i*6+2] = new Vector2(0.0f, 0.0f);
                        uvs[i*6+3] = new Vector2(1.0f, 0.0f);
                        uvs[i*6+4] = new Vector2(1.0f, 1.0f);
                        uvs[i*6+5] = new Vector2(0.0f, 1.0f);
                        triangles[i*6  ] = i*6;
                        triangles[i*6+1] = i*6+1;
                        triangles[i*6+2] = i*6+2;
                        triangles[i*6+3] = i*6+3;
                        triangles[i*6+4] = i*6+4;
                        triangles[i*6+5] = i*6+5;
                    }
                }

                Mesh mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.uv = uvs;
                mesh.triangles = triangles;
                mesh.normals = normals;

                MeshFilter chunk = Instantiate(chunkPrefab, transform.position + offset, transform.rotation) as MeshFilter;
                chunk.mesh = mesh;
                chunk.GetComponent<MeshCollider>().sharedMesh = mesh;
                chunk.transform.parent = transform;
            }
        }
	}
}
