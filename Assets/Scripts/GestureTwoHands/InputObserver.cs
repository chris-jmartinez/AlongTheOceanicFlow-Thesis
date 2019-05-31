/**
 * The input observer is in charge of user interaction with UI
 **/
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class InputObserver : MonoBehaviour, IInputHandler, ISourceStateHandler
{
    public static int _InputStatus;
    public static int _SourceStatus;
    public static bool _MenuStatus;
    public GameObject _menu;

    public void OnInputDown(InputEventData eventData)
    {
        _InputStatus += 1;
    }

    public void OnInputUp(InputEventData eventData)
    {
        _InputStatus -= 1;
    }

    public void OnSourceDetected(SourceStateEventData eventData)
    {
        _SourceStatus += 1;
    }

    public void OnSourceLost(SourceStateEventData eventData)
    {
        _SourceStatus -= 1;
    }

    // Use this for initialization
    void Start()
    {
        _InputStatus = 0;
        _SourceStatus = 0;
        _menu.SetActive(false);
        _MenuStatus = false;
    }

    private void Update()
    {
        if (_InputStatus < 0 || _SourceStatus < 0)
        {
            Debug.Log("WARNING - Observing irregular source state! [" + _InputStatus + "," + _SourceStatus + "]");
        }
        else if (_SourceStatus == 2 && !_MenuStatus)
        {
            _menu.SetActive(true);
            _MenuStatus = true;
        }
        //if (_menu.GetComponentInChildren<Resume_Script>()._isClicked)
        //{
        //    _menu.SetActive(false);
        //    _menu.GetComponentInChildren<Resume_Script>()._isClicked = false;
        //    _MenuStatus = false;
        //}
    }

}
