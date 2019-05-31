using Proyecto26;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WebClientSender : MonoBehaviour
{

    private string ipDolphin = "192.168.43.42";

    public static WebClientSender instance;
    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        //DontDestroyOnLoad(gameObject);
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    //public void MoveMouthDolphinOld()
    //{
    //    RestClient.Post(ipDolphin, "{\"requestType\": \"set\",\"motorControllerSetter\": [{\"type\":\"dc\",\"id\": 1,\"direction\" : \"cw\",\"speed\": 90,\"duration\": 500}]}").Then(response =>
    //    {
    //        Debug.Log("web sender MOVE OLD:: " + response.StatusCode.ToString());
            
    //    });
    //}

    public void MoveMouthAndEyesDolphinTogether()
    {
        RestClient.Post(ipDolphin, "{\"requestType\":\"deviceSpecificApi\",\"actions\":[{\"target\":\"eyes\",\"action\":\"blink\",\"rep\":1},{\"target\":\"mouth\",\"action\":\"move\"}]}").Then(response =>
        {
            Debug.Log("web sender MOVE EYES AND MOUTH:: " + response.StatusCode.ToString());

        });

    }

    public void OpenEyesDolphin()
    {
        RestClient.Post(ipDolphin, "{\"requestType\":\"deviceSpecificApi\",\"actions\":[{\"target\":\"eyes\",\"action\":\"open\"}]}").Then(response =>
        {
            Debug.Log("web sender OPEN EYES:: " + response.StatusCode.ToString());

        });
    }

    public void CloseEyesDolphin()
    {
        RestClient.Post(ipDolphin, "{\"requestType\":\"deviceSpecificApi\",\"actions\":[{\"target\":\"eyes\",\"action\":\"close\"}]}").Then(response =>
        {
            Debug.Log("web sender CLOSE EYES:: " + response.StatusCode.ToString());

        });
    }



    //Command ColorSinglePartDolphin
    public void SetLedGreen()
    {
        RestClient.Post(ipDolphin, "{\"requestType\": \"set\",\"lightControllerSetter\": [{\"code\":\"parthead\",\"color\":\"#00FF00\"},{\"code\":\"partleftfin\",\"color\":\"#00FF00\"},{\"code\":\"partrightfin\",\"color\":\"#00FF00\"},{\"code\":\"partbelly\",\"color\":\"#00FF00\"}]}").Then(response =>
        {
            Debug.Log("WEB SENDER SET LED GREEN:: " + response.StatusCode.ToString());
            
        });
    }

    //Command ColorSinglePartDolphin
    public void SetLedRed()
    {
        RestClient.Post(ipDolphin, "{\"requestType\": \"set\",\"lightControllerSetter\": [{\"code\":\"parthead\",\"color\":\"#FF0000\"},{\"code\":\"partleftfin\",\"color\":\"#FF0000\"},{\"code\":\"partrightfin\",\"color\":\"#FF0000\"},{\"code\":\"partbelly\",\"color\":\"#FF0000\"}]}").Then(response =>
        {
            Debug.Log("WEB SENDER SET LED RED:: " + response.StatusCode.ToString());

        });
    }

    //Command ColorSinglePartDolphin
    public void SetLedYellow()
    {
        RestClient.Post(ipDolphin, "{\"requestType\": \"set\",\"lightControllerSetter\": [{\"code\":\"parthead\",\"color\":\"#FFFF00\"},{\"code\":\"partleftfin\",\"color\":\"#FFFF00\"},{\"code\":\"partrightfin\",\"color\":\"#FFFF00\"},{\"code\":\"partbelly\",\"color\":\"#FFFF00\"}]}").Then(response =>
        {
            Debug.Log("WEB SENDER SET LED YELLOW:: " + response.StatusCode.ToString());

        });
    }


    //Command ColorSinglePartDolphin
    public void ResetLedDolphin()
    {
        RestClient.Post(ipDolphin, "{\"requestType\": \"set\",\"lightControllerSetter\": [{\"code\":\"parthead\",\"color\":\"#000000\"},{\"code\":\"partleftfin\",\"color\":\"#000000\"},{\"code\":\"partrightfin\",\"color\":\"#000000\"},{\"code\":\"partbelly\",\"color\":\"#000000\"}]}").Then(response =>
        {
            Debug.Log("WEB SENDER RESET LED:: " + response.StatusCode.ToString());
            
        });
    }


    public void PlayMusicDolphin()
    {
        RestClient.Post(ipDolphin, "{\"requestType\": \"set\",\"soundControllerSetter\": [{\"type\":\"music\",\"track\":\"10\",\"volume\":\"10\"}]}").Then(response =>
        {
            Debug.Log("WEB SENDER PLAY MUSIC DOLPHIN:: " + response.StatusCode.ToString());
            
        });
    }

}
