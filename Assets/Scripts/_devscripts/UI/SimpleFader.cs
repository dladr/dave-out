using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Helpers
{
    /// <summary>
    /// Simple screen fader that can fade to/from black using a UI Image.
    /// Commonly used for scene transitions and cinematic effects.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class SimpleFader : MonoBehaviour
    {
        [BoxGroup("Fade Settings")]
        [Tooltip("Default fade duration in seconds")]
        public float DefaultFadeDuration = 1f;

        [BoxGroup("Fade Settings")]
        [Tooltip("Color to fade to (usually black)")]
        public Color FadeColor = Color.black;

        [BoxGroup("Canvas Settings")]
        [Tooltip("Render mode for the canvas. Screen Space Camera is recommended to avoid world space conflicts.")]
        public RenderMode CanvasRenderMode = RenderMode.ScreenSpaceCamera;

        [BoxGroup("Canvas Settings")]
        [ShowIf("CanvasRenderMode", RenderMode.ScreenSpaceCamera)]
        [Tooltip("Camera to render the canvas on. If null, will use Camera.main")]
        public Camera TargetCamera;

        [BoxGroup("Canvas Settings")]
        [Tooltip("Plane distance when using Screen Space Camera mode")]
        public float PlaneDistance = 100f;

        [BoxGroup("Canvas Settings")]
        [Tooltip("Extra pixels to extend fade image beyond canvas bounds (prevents edge gaps)")]
        public float EdgeOversizing = 10f;

        [BoxGroup("Image Settings")]
        [Tooltip("Force use solid color instead of sprite (recommended for clean fades)")]
        public bool UseSolidColor = true;

        [BoxGroup("Auto Setup")]
        [Tooltip("Should this fader auto-setup on Awake?")]
        public bool AutoSetup = true;

        [BoxGroup("Debug")]
        [SerializeField, ReadOnly]
        private bool _isFading;

        [BoxGroup("Debug")]
        [SerializeField, ReadOnly]
        private float _currentAlpha;

        private Image _fadeImage;
        private Canvas _canvas;
        private Coroutine _fadeCoroutine;

        void Awake()
        {
            if (AutoSetup)
            {
                SetupFader();
            }
        }

        /// <summary>
        /// Sets up the fader with proper canvas settings and image configuration.
        /// </summary>
        [Button("Setup Fader")]
        public void SetupFader()
        {
            _fadeImage = GetComponent<Image>();
            _canvas = GetComponent<Canvas>();


            var mainCamera = Camera.main;            
            // add my camera to the camera stack, if not already present
            if (mainCamera != null && !mainCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>().cameraStack.Contains(TargetCamera))
            {
                mainCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>().cameraStack.Add(TargetCamera);
            }

            // Setup canvas if not already configured
            if (_canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
            }

            // Configure canvas based on render mode
            _canvas.renderMode = CanvasRenderMode;
            _canvas.sortingOrder = 1000; // High sort order to appear on top

            if (CanvasRenderMode == RenderMode.ScreenSpaceCamera)
            {
                // Use specified camera or find main camera
                if (TargetCamera == null)
                {
                    TargetCamera = Camera.main;
                    if (TargetCamera == null)
                    {
                        TargetCamera = FindFirstObjectByType<Camera>();
                    }
                }

                if (TargetCamera != null)
                {
                    _canvas.worldCamera = TargetCamera;
                    _canvas.planeDistance = PlaneDistance;
                    Debug.Log($"SimpleFader: Using Screen Space Camera mode with camera {TargetCamera.name}");
                }
                else
                {
                    Debug.LogWarning("SimpleFader: No camera found for Screen Space Camera mode, falling back to Overlay", this);
                    _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
            }

            // Add CanvasScaler for better scaling
            var canvasScaler = GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                canvasScaler = gameObject.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f;
            }

            // Setup image
            _fadeImage.color = FadeColor.SetAlpha(0f);
            _fadeImage.raycastTarget = false;
            
            // Handle sprite vs solid color
            if (UseSolidColor)
            {
                // Use solid color instead of sprite to avoid edge artifacts
                _fadeImage.sprite = null; // Remove any assigned sprite
                _fadeImage.type = Image.Type.Simple;
                Debug.Log("SimpleFader: Using solid color (no sprite) for clean fade");
            }
            else if (_fadeImage.sprite != null)
            {
                // Validate sprite settings if using one
                ValidateSpriteSettings();
            }

            // Make sure we fill the screen (with configurable oversizing to prevent edge gaps)
            var rectTransform = _fadeImage.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(-EdgeOversizing, -EdgeOversizing);
            rectTransform.offsetMax = new Vector2(EdgeOversizing, EdgeOversizing);

            _currentAlpha = 0f;
            Debug.Log($"SimpleFader: Setup complete with {CanvasRenderMode} render mode");
        }

        /// <summary>
        /// Fades out (to the fade color) over the specified duration.
        /// </summary>
        /// <param name="duration">Time to fade in seconds. Uses DefaultFadeDuration if not specified.</param>
        public void FadeOut(float duration = -1f)
        {
            if (duration < 0) duration = DefaultFadeDuration;
            StartFade(0f, 1f, duration);
        }

        /// <summary>
        /// Fades in (from the fade color to transparent) over the specified duration.
        /// </summary>
        /// <param name="duration">Time to fade in seconds. Uses DefaultFadeDuration if not specified.</param>
        public void FadeIn(float duration = -1f)
        {
            if (duration < 0) duration = DefaultFadeDuration;
            StartFade(1f, 0f, duration);
        }

        /// <summary>
        /// Fades from current alpha to target alpha over the specified duration.
        /// </summary>
        /// <param name="targetAlpha">Target alpha value (0-1)</param>
        /// <param name="duration">Time to fade in seconds</param>
        public void FadeTo(float targetAlpha, float duration)
        {
            StartFade(_currentAlpha, targetAlpha, duration);
        }

        /// <summary>
        /// Sets the fade alpha immediately without animation.
        /// </summary>
        /// <param name="alpha">Alpha value (0-1)</param>
        public void SetAlpha(float alpha)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            _currentAlpha = Mathf.Clamp01(alpha);
            if (_fadeImage != null)
            {
                _fadeImage.color = FadeColor.SetAlpha(_currentAlpha);
            }
            _isFading = false;
        }

        /// <summary>
        /// Starts a fade sequence (used by SceneLoader).
        /// </summary>
        public void StartFadeSequence()
        {
            FadeOut();
        }

        private void StartFade(float fromAlpha, float toAlpha, float duration)
        {
            if (_fadeImage == null)
            {
                Debug.LogWarning("SimpleFader: Image component not found. Call SetupFader() first.", this);
                return;
            }

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeCoroutine(fromAlpha, toAlpha, duration));
        }

        private IEnumerator FadeCoroutine(float fromAlpha, float toAlpha, float duration)
        {
            _isFading = true;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                _currentAlpha = Mathf.Lerp(fromAlpha, toAlpha, progress);
                
                _fadeImage.color = FadeColor.SetAlpha(_currentAlpha);
                
                yield return null;
            }

            _currentAlpha = toAlpha;
            _fadeImage.color = FadeColor.SetAlpha(_currentAlpha);
            _isFading = false;
            _fadeCoroutine = null;
        }

        // Public properties
        public bool IsFading => _isFading;
        public float CurrentAlpha => _currentAlpha;

        [Button("Test Fade Out")]
        private void TestFadeOut()
        {
            FadeOut();
        }

        [Button("Test Fade In")]
        private void TestFadeIn()
        {
            FadeIn();
        }

        /// <summary>
        /// Validates sprite settings and warns about potential issues.
        /// </summary>
        private void ValidateSpriteSettings()
        {
            if (_fadeImage.sprite == null) return;

            var sprite = _fadeImage.sprite;
            var texture = sprite.texture;

            // Check for common issues
            if (texture.filterMode != FilterMode.Point)
            {
                Debug.LogWarning($"SimpleFader: Sprite '{sprite.name}' uses {texture.filterMode} filtering. Consider using Point filtering for solid fades.", this);
            }

            if (texture.format != TextureFormat.RGB24 && texture.format != TextureFormat.RGBA32)
            {
                Debug.LogWarning($"SimpleFader: Sprite '{sprite.name}' uses {texture.format} format. Consider using RGB24 for solid colors.", this);
            }

            // Check if sprite has transparent pixels around edges
            if (_fadeImage.type == Image.Type.Sliced)
            {
                Debug.LogWarning($"SimpleFader: Using Sliced image type with sprite '{sprite.name}'. Consider using Simple type for fades.", this);
            }

            Debug.Log($"SimpleFader: Sprite validation complete for '{sprite.name}'");
        }

        [Button("Fix Sprite Settings")]
        private void FixSpriteSettings()
        {
            if (_fadeImage.sprite != null)
            {
                Debug.LogWarning("SimpleFader: Cannot modify sprite settings at runtime. Please check sprite import settings in the Inspector.", this);
            }
            
            // What we can fix at runtime
            _fadeImage.type = Image.Type.Simple;
            _fadeImage.preserveAspect = false;
            
            Debug.Log("SimpleFader: Applied runtime fixes to Image component");
        }
    }
}
