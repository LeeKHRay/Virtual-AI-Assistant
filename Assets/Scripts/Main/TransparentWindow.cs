using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TransparentWindow : MonoBehaviour
{
    private GraphicRaycaster graphicRaycaster;

    private Camera fullscreenCamera;
    private Camera desktopCamera;
    private Canvas fullscreenUI;
    private Canvas desktopUI;

    private bool clickThrough = true;
    private bool prevClickThrough = true;

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    static extern int SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern int ShowWindow(int hwnd, int nCmdShow);
    [DllImport("user32.dll")]
    public static extern int FindWindow(string lpClassName, string lpWindowName);

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    const int GWL_EXSTYLE = -20;

    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020;

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

    private IntPtr hWnd;

    private void Start()
    {
#if !UNITY_EDITOR
        hWnd = GetActiveWindow();

		MARGINS margins;
		if (SystemManager.Instance.prevIsFullscreen)
		{
			margins = new MARGINS { cxLeftWidth = 0, cxRightWidth = 0, cyTopHeight = 0, cyBottomHeight = 0};
		}
		else 
		{
			margins = new MARGINS { cxLeftWidth = -1 }; // -1 means transparent background
		}
        DwmExtendFrameIntoClientArea(hWnd, ref margins); // set border
        
		if (SystemManager.Instance.prevIsFullscreen)
		{
			SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
		}
		else 
		{
			SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
		}
        
        SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, 0);
#endif

        Application.runInBackground = true;
    }

    private void Update()
    {
        if (!SystemManager.Instance.isFullscreen)
        {
            // check if the mouse is pointing at the assistant or UI
            Vector3 mouseScreenPos = desktopCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseScreenPos.z = 0;
            mouseScreenPos = Input.mousePosition;
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = mouseScreenPos;
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerEventData, results);

            clickThrough = results.Count == 0 && !Physics.Raycast(desktopCamera.ScreenPointToRay(mouseScreenPos));
            if (clickThrough != prevClickThrough)
            {
                if (clickThrough)
                {
                    Debug.Log("clickthrough");
                    SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
                }
                else
                {
                    Debug.Log("not clickthrough");
                    SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
                }
            }
            prevClickThrough = clickThrough;
        }
    }

    public void SwitchDisplayMode()
    {
        if (SystemManager.Instance.isFullscreen)
        {
            SetDesktopMode();
        }
        else
        {
            SetFullscreenMode();
        }
    }

    public void SetFullscreenMode()
    {
        SystemManager.Instance.isFullscreen = true;

#if !UNITY_EDITOR
        // reset window attributes
        MARGINS margins = new MARGINS { cxLeftWidth = 0, cxRightWidth = 0, cyTopHeight = 0, cyBottomHeight = 0};
        DwmExtendFrameIntoClientArea(hWnd, ref margins);
		SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
        SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, 0);
#endif

        if (SceneManager.GetActiveScene().name == "Main")
        {
            fullscreenUI.enabled = true; // show fullscreen mode UI
            fullscreenCamera.enabled = true; // use fullscreen mode camera

            desktopUI.enabled = false;
            desktopCamera.enabled = false;
        }
    }

    public void SetDesktopMode()
    {
        SystemManager.Instance.isFullscreen = false;
#if !UNITY_EDITOR
        // transparent background
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT); // can click through
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, 0); // show application at the top of other opened windows
#endif
        if (SceneManager.GetActiveScene().name == "Main")
        {
            fullscreenUI.enabled = false;
            fullscreenCamera.enabled = false;

            desktopUI.enabled = true; // show desktop mode UI
            desktopCamera.enabled = true; // use desktop mode camera
        }
    }

    public void SetReference()
    {
        fullscreenCamera = GameObject.Find("FullscreenCamera").GetComponent<Camera>();
        desktopCamera = GameObject.Find("DesktopCamera").GetComponent<Camera>();
        fullscreenUI = GameObject.Find("FullscreenUI").GetComponent<Canvas>();
        desktopUI = GameObject.Find("DesktopUI").GetComponent<Canvas>();
        graphicRaycaster = desktopUI.GetComponent<GraphicRaycaster>();
    }
}