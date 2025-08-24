using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Helpers
{
    /// <summary>
    /// Utility class with common solutions for Unity UI and Camera positioning challenges.
    /// Addresses typical issues with canvas positioning and camera setup in single-screen games.
    /// </summary>
    public static class UnityUIHelper
    {
        /// <summary>
        /// Creates a UI canvas that doesn't interfere with world space objects.
        /// Uses Screen Space Camera mode with proper camera assignment.
        /// </summary>
        /// <param name="canvasName">Name for the created canvas</param>
        /// <param name="targetCamera">Camera to render on (null for main camera)</param>
        /// <param name="sortingOrder">Sorting order for the canvas</param>
        /// <returns>The created canvas component</returns>
        public static Canvas CreateNonInterferingCanvas(string canvasName = "UI Canvas", Camera targetCamera = null, int sortingOrder = 100)
        {
            var canvasGO = new GameObject(canvasName);
            var canvas = canvasGO.AddComponent<Canvas>();
            var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
            var graphicRaycaster = canvasGO.AddComponent<GraphicRaycaster>();

            // Setup canvas for Screen Space Camera mode
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.sortingOrder = sortingOrder;

            // Find appropriate camera
            if (targetCamera == null)
            {
                targetCamera = Camera.main ?? GameObject.FindFirstObjectByType<Camera>();
            }

            if (targetCamera != null)
            {
                canvas.worldCamera = targetCamera;
                canvas.planeDistance = 100f; // Safe distance from camera
            }

            // Setup canvas scaler for consistent UI scaling
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            Debug.Log($"UnityUIHelper: Created non-interfering canvas '{canvasName}' with Screen Space Camera mode");
            return canvas;
        }

        /// <summary>
        /// Sets up a camera specifically for UI rendering, separate from the main game camera.
        /// Useful when you want complete separation between game world and UI.
        /// </summary>
        /// <param name="cameraName">Name for the UI camera</param>
        /// <param name="depth">Camera depth (higher renders on top)</param>
        /// <param name="cullingMask">What layers this camera should render</param>
        /// <returns>The created UI camera</returns>
        public static Camera CreateUICameraSetup(string cameraName = "UI Camera", int depth = 10, LayerMask cullingMask = default)
        {
            var uiCameraGO = new GameObject(cameraName);
            var uiCamera = uiCameraGO.AddComponent<Camera>();

            // Configure UI camera
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.depth = depth; // Render after main camera
            uiCamera.orthographic = true;
            uiCamera.orthographicSize = 5f;
            uiCamera.nearClipPlane = -100f;
            uiCamera.farClipPlane = 100f;

            // Set culling mask if provided, otherwise use UI layer
            if (cullingMask.value == 0)
            {
                cullingMask = LayerMask.GetMask("UI");
            }
            uiCamera.cullingMask = cullingMask;

            Debug.Log($"UnityUIHelper: Created UI camera '{cameraName}' with depth {depth}");
            return uiCamera;
        }

        /// <summary>
        /// Alternative approach: Creates a world space canvas positioned safely away from game objects.
        /// Useful when you want the UI to exist in world space but not interfere with gameplay.
        /// </summary>
        /// <param name="position">World position for the canvas</param>
        /// <param name="canvasName">Name for the canvas</param>
        /// <param name="scale">Scale of the world space canvas</param>
        /// <returns>The created world space canvas</returns>
        public static Canvas CreateWorldSpaceUICanvas(Vector3 position, string canvasName = "World UI Canvas", float scale = 0.01f)
        {
            var canvasGO = new GameObject(canvasName);
            canvasGO.transform.position = position;
            canvasGO.transform.localScale = Vector3.one * scale;

            var canvas = canvasGO.AddComponent<Canvas>();
            var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
            var graphicRaycaster = canvasGO.AddComponent<GraphicRaycaster>();

            // Setup for world space
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;

            // Configure canvas scaler for world space
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            canvasScaler.scaleFactor = 1f;

            // Set a reasonable size for the RectTransform
            var rectTransform = canvas.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1920, 1080);

            Debug.Log($"UnityUIHelper: Created world space canvas '{canvasName}' at position {position}");
            return canvas;
        }

        /// <summary>
        /// Fixes common canvas sizing issues that cause edge gaps with Screen Space Camera canvases.
        /// </summary>
        /// <param name="canvas">The canvas to fix</param>
        /// <param name="oversizeAmount">How many pixels to extend beyond normal bounds</param>
        public static void FixCanvasEdgeGaps(Canvas canvas, float oversizeAmount = 10f)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                Debug.LogWarning("FixCanvasEdgeGaps: This fix is designed for Screen Space Camera canvases", canvas);
                return;
            }

            // Option 1: Adjust canvas scaler settings
            var canvasScaler = canvas.GetComponent<CanvasScaler>();
            if (canvasScaler != null)
            {
                // Use a slightly larger reference resolution
                canvasScaler.referenceResolution = new Vector2(1920 + oversizeAmount, 1080 + oversizeAmount);
            }

            // Option 2: For full-screen UI elements, add oversizing
            var fullScreenElements = canvas.GetComponentsInChildren<RectTransform>()
                .Where(rt => rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one);

            foreach (var rectTransform in fullScreenElements)
            {
                rectTransform.offsetMin = new Vector2(-oversizeAmount, -oversizeAmount);
                rectTransform.offsetMax = new Vector2(oversizeAmount, oversizeAmount);
            }

            Debug.Log($"UnityUIHelper: Applied edge gap fixes to canvas {canvas.name}");
        }

        /// <summary>
        /// Creates a canvas specifically optimized to avoid edge gaps and sizing issues.
        /// </summary>
        /// <param name="canvasName">Name for the canvas</param>
        /// <param name="targetCamera">Camera to render on</param>
        /// <returns>The created canvas</returns>
        public static Canvas CreatePerfectFitCanvas(string canvasName = "Perfect Fit Canvas", Camera targetCamera = null)
        {
            var canvas = CreateNonInterferingCanvas(canvasName, targetCamera, 1000);

            // Use Screen Space Overlay for perfect pixel alignment
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Or if you must use Screen Space Camera, optimize settings
            if (targetCamera != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = targetCamera;
                canvas.planeDistance = 0.1f; // Very close to camera
                
                // Use constant pixel size for better precision
                var canvasScaler = canvas.GetComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                canvasScaler.scaleFactor = 1f;
            }

            Debug.Log($"UnityUIHelper: Created perfect fit canvas '{canvasName}'");
            return canvas;
        }
    }

    /// <summary>
    /// MonoBehaviour helper for common camera and UI setup scenarios.
    /// Add this to a GameObject in your scene for easy setup buttons.
    /// </summary>
    public class UnityUISetupHelper : MonoBehaviour
    {
        [BoxGroup("Quick Setup")]
        [Tooltip("Name for created UI elements")]
        public string UIElementName = "Game UI";

        [BoxGroup("Quick Setup")]
        [Tooltip("Target camera for UI rendering")]
        public Camera TargetCamera;

        [BoxGroup("Canvas Position (World Space)")]
        [Tooltip("Position for world space UI (alternative approach)")]
        public Vector3 WorldUIPosition = new Vector3(-50, 0, 0);

        [Button("Create Screen Space Camera Canvas")]
        public void CreateScreenSpaceCameraCanvas()
        {
            UnityUIHelper.CreateNonInterferingCanvas(UIElementName, TargetCamera);
        }

        [Button("Create UI Camera Setup")]
        public void CreateUICameraSetup()
        {
            var uiCamera = UnityUIHelper.CreateUICameraSetup($"{UIElementName} Camera");
            var canvas = UnityUIHelper.CreateNonInterferingCanvas(UIElementName, uiCamera);
        }

        [Button("Create World Space Canvas (Away from Game)")]
        public void CreateWorldSpaceCanvas()
        {
            UnityUIHelper.CreateWorldSpaceUICanvas(WorldUIPosition, UIElementName);
        }

        [Button("Move Main Camera Away from Origin")]
        public void MoveCameraAwayFromOrigin()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("No main camera found!");
                return;
            }

            var newPosition = new Vector3(-50, 0, mainCamera.transform.position.z);
            mainCamera.transform.position = newPosition;
            Debug.Log($"Moved main camera to {newPosition} to avoid UI overlap");
        }
    }
}
