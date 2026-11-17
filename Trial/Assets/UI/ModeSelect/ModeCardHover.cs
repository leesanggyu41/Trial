// ModeCardHover는 모드 선택 카드의 호버 연출을 담당합니다.
// 커서가 없으면 흑백 + 어둡게, 커서를 올리면 컬러로 밝아지며 테두리 빛(Glow)이 켜지고 살짝 커집니다.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModeCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("흑백 처리할 이미지 (UI/Grayscale Hover 셰이더 사용)")]
    public Graphic[] grayscaleTargets;

    [Header("빛 효과")]
    public Graphic glow;              // 카드 뒤 빛 (호버 시 나타남)
    public float glowAlpha = 0.9f;

    [Header("흑백 상태 값")]
    [Range(0f, 1f)] public float idleSaturation = 0f;
    [Range(0f, 2f)] public float idleBrightness = 0.4f;

    [Header("호버 상태 값")]
    [Range(0f, 1f)] public float hoverSaturation = 1f;
    [Range(0f, 2f)] public float hoverBrightness = 1.1f;
    public float hoverScale = 1.04f;

    [Header("전환 속도")]
    public float speed = 8f;

    [Header("글자 (흑백 상태에서 살짝 어둡게)")]
    public CanvasGroup contentGroup;
    [Range(0f, 1f)] public float idleContentAlpha = 0.6f;

    private Material[] _materials;
    private float _t;          // 0 = 흑백, 1 = 호버
    private bool _hovered;
    private Vector3 _baseScale;

    private static readonly int SaturationId = Shader.PropertyToID("_Saturation");
    private static readonly int BrightnessId = Shader.PropertyToID("_Brightness");

    private void Awake()
    {
        _baseScale = transform.localScale;

        // 카드마다 따로 움직이도록 머티리얼을 복제해서 사용
        if (grayscaleTargets != null)
        {
            _materials = new Material[grayscaleTargets.Length];
            for (int i = 0; i < grayscaleTargets.Length; i++)
            {
                if (grayscaleTargets[i] == null || grayscaleTargets[i].material == null) continue;
                _materials[i] = new Material(grayscaleTargets[i].material);
                grayscaleTargets[i].material = _materials[i];
            }
        }
        Apply(0f);
    }

    private void OnEnable()
    {
        _hovered = false;
        _t = 0f;
        Apply(0f);
    }

    private void OnDestroy()
    {
        if (_materials == null) return;
        foreach (var m in _materials) if (m != null) Destroy(m);
    }

    public void OnPointerEnter(PointerEventData eventData) => _hovered = true;
    public void OnPointerExit(PointerEventData eventData) => _hovered = false;
    public void OnSelect(BaseEventData eventData) => _hovered = true;     // 키보드/패드 선택도 같은 연출
    public void OnDeselect(BaseEventData eventData) => _hovered = false;

    private void Update()
    {
        float target = _hovered ? 1f : 0f;
        if (Mathf.Approximately(_t, target)) return;
        _t = Mathf.MoveTowards(_t, target, Time.unscaledDeltaTime * speed);
        Apply(_t);
    }

    private void Apply(float t)
    {
        float e = t * t * (3f - 2f * t); // 부드럽게 (smoothstep)

        if (_materials != null)
        {
            float sat = Mathf.Lerp(idleSaturation, hoverSaturation, e);
            float bri = Mathf.Lerp(idleBrightness, hoverBrightness, e);
            for (int i = 0; i < _materials.Length; i++)
            {
                if (_materials[i] == null) continue;
                _materials[i].SetFloat(SaturationId, sat);
                _materials[i].SetFloat(BrightnessId, bri);

                // Mask 안의 이미지는 화면에 그릴 때 스텐실용 복사본(materialForRendering)을 쓰므로 거기에도 적용
                var g = grayscaleTargets[i];
                if (g != null)
                {
                    var r = g.materialForRendering;
                    if (r != null && r != _materials[i])
                    {
                        r.SetFloat(SaturationId, sat);
                        r.SetFloat(BrightnessId, bri);
                    }
                }
            }
        }

        if (glow != null)
        {
            var c = glow.color;
            c.a = glowAlpha * e;
            glow.color = c;
        }

        if (contentGroup != null)
            contentGroup.alpha = Mathf.Lerp(idleContentAlpha, 1f, e);

        transform.localScale = _baseScale * Mathf.Lerp(1f, hoverScale, e);
    }
}
