using UnityEngine;

public class Screen_MUI : MonoBehaviour
{
    private MeshRenderer mesh;
    private Material tx;
    private Material ns;
    private AudioSource au;

    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        au = GetComponent<AudioSource>();

        tx = mesh.materials[4];
        ns = mesh.materials[7];

        NullChick();
    }

    [ContextMenu("하늘에서 주사기가 내려와~~!")]
    public void UpdateText(int Toxin_Count, int NS_Count)
    {
        if (mesh == null || tx == null || ns == null)
            return;

        float[] toxinOffsets =
        {
        -0.006f, 0.092f, 0.19f, 0.288f, 0.388f,
         0.485f, 0.578f, 0.675f, 0.772f, 0.869f
    };

        float[] nsOffsets =
        {
        0.004f, 0.103f, 0.2f, 0.296f, 0.398f,
        0.492f, 0.587f, 0.684f, 0.782f, 0.88f
    };

        int toxinIndex = Mathf.Clamp(Toxin_Count, 0, toxinOffsets.Length - 1);
        int nsIndex = Mathf.Clamp(NS_Count, 0, nsOffsets.Length - 1);

        // TX
        Vector2 txOffset = tx.GetVector("_Offset");
        txOffset.x = toxinOffsets[toxinIndex];
        tx.SetVector("_Offset", txOffset);

        // NS
        Vector2 nsOffset = ns.GetVector("_Offset");
        nsOffset.x = nsOffsets[nsIndex];
        ns.SetVector("_Offset", nsOffset);
    }

    [ContextMenu("아직 몇개일지 생각 안했어용")]
    public void NullChick()
    {

        Vector2 txOffset = tx.GetVector("_Offset");
        Vector2 nsOffset = ns.GetVector("_Offset");
        txOffset.x = -5f;
        nsOffset.x = -5f;
        tx.SetVector("_Offset", txOffset);
        ns.SetVector("_Offset", nsOffset);
    }

    public void AudioPlay()
    {
        au.Play();
    }
}
