using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class LayerConfig
    {
        public string resourceName;
        public float speed;
        public int sortingOrder;
    }

    public LayerConfig[] layers = new LayerConfig[]
    {
        new LayerConfig { resourceName = "1", speed = 0.2f, sortingOrder = 0 },
        new LayerConfig { resourceName = "2", speed = 0.5f, sortingOrder = 1 },
        new LayerConfig { resourceName = "3", speed = 0.8f, sortingOrder = 2 },
        new LayerConfig { resourceName = "4", speed = 1.2f, sortingOrder = 3 },
    };

    private SpriteRenderer[][] copies;
    private float[] layerWidths;

    void Start()
    {
        int layerCount = layers.Length;
        copies = new SpriteRenderer[layerCount][];
        layerWidths = new float[layerCount];

        for (int i = 0; i < layerCount; i++)
        {
            Sprite sprite = Resources.Load<Sprite>("BG/" + layers[i].resourceName);
            if (sprite == null)
            {
                Debug.LogError("Parallax: Sprite BG/" + layers[i].resourceName + " not found!");
                continue;
            }

            layerWidths[i] = sprite.bounds.size.x;
            float w = layerWidths[i];

            GameObject layerObj = new GameObject("Layer_" + layers[i].resourceName);
            layerObj.transform.SetParent(transform, false);

            copies[i] = new SpriteRenderer[2];
            for (int j = 0; j < 2; j++)
            {
                GameObject copy = new GameObject("Copy_" + j);
                copy.transform.SetParent(layerObj.transform, false);
                SpriteRenderer sr = copy.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = layers[i].sortingOrder;
                copy.transform.localPosition = new Vector3(j * w, 0, 0);
                copies[i][j] = sr;
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < copies.Length; i++)
        {
            if (copies[i] == null) continue;

            float movement = layers[i].speed * Time.deltaTime;
            float w = layerWidths[i];

            for (int j = 0; j < copies[i].Length; j++)
            {
                Transform t = copies[i][j].transform;
                Vector3 pos = t.localPosition;
                pos.x -= movement;
                t.localPosition = pos;

                if (pos.x <= -w)
                {
                    t.localPosition = new Vector3(pos.x + w * 2, pos.y, pos.z);
                }
            }
        }
    }
}
