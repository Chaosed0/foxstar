using UnityEngine;
using System.Collections;
using System.IO;

[System.Serializable]
public class DetailGroup {
    public Transform[] prefabs;
    public float density = 0.01f;
    public float scaleMin = 1.0f;
    public float scaleMax = 2.0f;
}

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

    public DetailGroup[] detailGroups;

    private Vector3[,] sharedVertices;
    private float minHeight = System.Int32.MaxValue;
    private float maxHeight = System.Int32.MinValue;

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
        GenerateDetailMeshes();
    }

    public float GetMaxHeightIn(Vector2 bounds) {
        bounds = bounds / xscale;
        int startx = -(int)Mathf.Floor(bounds.x);
        int endx = (int)Mathf.Ceil(bounds.x);
        int starty = -(int)Mathf.Floor(bounds.y);
        int endy = (int)Mathf.Ceil(bounds.y);
        float max = System.Int32.MinValue;

        for (int y = starty; y < endy; y++) {
            for (int x = startx; x < endx; x++) {
                float elevation = GetElevation(x, y);
                if (elevation > max) {
                    max = elevation;
                }
            }
        }

        return max;
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
        return sharedVertices[localY,localX].y;
    }

    public void GenerateDetailMeshes() {
        for (int i = 0; i < detailGroups.Length; i++) {
            DetailGroup group = detailGroups[i];
            int numberToGenerate = (int)(xsize*ysize * group.density);
            for (int j = 0; j < numberToGenerate; j++) {
                Transform randomDetail = group.prefabs[(int)Random.Range(0.0f, group.prefabs.Length)];

                float x = Random.Range(-xsize*xscale/2.0f, xsize*xscale/2.0f);
                float z = Random.Range(-ysize*yscale/2.0f, ysize*yscale/2.0f); 
                float y = GetElevation(x, z);
                float scale = Random.Range(group.scaleMin, group.scaleMax);
                Quaternion rotation = Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), Vector3.up);

                Transform detail = Instantiate(randomDetail, new Vector3(x,y,z), rotation) as Transform;
                detail.localScale = detail.localScale * scale;
                detail.parent = this.transform;
            }
        }
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
                float elevation = 0.0f;
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

                elevation = elevation / maxVal * zscale;
                //octavePixels[octaves][y * xsize + x] = new Color(elevation / maxVal, elevation / maxVal, elevation / maxVal);
                sharedVertices[y,x] = new Vector3((x + Random.Range(-xPerturb, xPerturb)) * xscale, elevation, (y + Random.Range(-yPerturb, yPerturb)) * yscale);

                if (elevation < minHeight) {
                    minHeight = elevation;
                }
                if (elevation > maxHeight) {
                    maxHeight = elevation;
                }
            }
        }

        for (int y = 0; y < ysize; y++) {
            for (int x = 0; x < xsize; x++) {
                /* Set the minimum height to 0 */
                sharedVertices[y,x].y = sharedVertices[y,x].y - minHeight;

                /* Make features more exaggerated as we reach the edge of the map */
                Vector2 offset = new Vector2(Mathf.Abs(x - xsize/2.0f), Mathf.Abs(y - ysize/2.0f));
                float mp = (offset.x > offset.y ? offset.x : offset.y);
                float ms = (offset.x > offset.y ? xsize/2.0f : ysize/2.0f);
                sharedVertices[y,x].y = sharedVertices[y,x].y * (1.0f + 2.0f * (mp * mp) / (ms * ms));

                if (sharedVertices[y,x].y > maxHeight) {
                    maxHeight = sharedVertices[y,x].y;
                }
            }
        }
        minHeight = 0.0f;

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
