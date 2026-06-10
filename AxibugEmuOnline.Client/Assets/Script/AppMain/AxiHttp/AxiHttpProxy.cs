using UnityEngine;
using static AxiHttp;

public static class AxiHttpProxy
{
    public static SendWebRequestProxy Get(string url)
    {
        return new SendWebRequestProxy(AxiRequestAsync(url));
    }

    public static SendDownLoadProxy GetDownLoad(string url)
    {
        return new SendDownLoadProxy(AxiDownloadAsync(url));
    }

    public class SendWebRequestProxy
    {
        public WaitAxiRequest SendWebRequest;
        public AxiRespInfo downloadHandler => SendWebRequest.mReqAsync;
        public SendWebRequestProxy(WaitAxiRequest request)
        {
            SendWebRequest = request;
        }
        ~SendWebRequestProxy()
        {
            SendWebRequest = null;
        }
    }


    public class SendDownLoadProxy
    {
        public AxiRespInfo downloadHandler;
        public SendDownLoadProxy(AxiRespInfo re)
        {
            downloadHandler = re;
        }
        ~SendDownLoadProxy()
        {
            downloadHandler = null;
        }
    }


    public static void ShowAxiHttpDebugInfo(AxiRespInfo resp,bool bOnlyShowErr = false)
    {

#if UNITY_EDITOR
        Debug.Log($"");
        Debug.Log($"==== request ====");
        Debug.Log($"url =>{resp.url}");
        Debug.Log($"Raw =>{resp.requestRaw}");
        Debug.Log($"code =>{resp.code}");
        Debug.Log($"respInfo.bTimeOut =>{resp.isTimeOut}");
        Debug.Log($"");
        Debug.Log($"==== response ====");
        Debug.Log($"==== header ====");
        Debug.Log($"header =>{resp.header}");
        Debug.Log($"HeadersCount =>{resp.headers.Count}");
        foreach (var kv in resp.headers)
            Debug.Log($"{kv.Key} => {kv.Value}");
        Debug.Log($"");
        Debug.Log($"==== body ====");
        Debug.Log($"body_text =>{resp.body}");
        Debug.Log($"body_text.Length =>{resp.body.Length}");
        Debug.Log($"bodyRaw.Length =>{resp.bodyraw?.Length}");
        Debug.Log($"");
        Debug.Log($"==== download ====");
        Debug.Log($"downloadMode =>{resp.downloadMode}");
        Debug.Log($"respInfo.fileName =>{resp.filename}");
        Debug.Log($"respInfo.NeedloadedLenght =>{resp.needdownloadLenght}");
        Debug.Log($"respInfo.loadedLenght =>{resp.loadedlenght}");
        if (resp.bHadErr)
        {
            Debug.LogError($"code->{resp.code} err->{resp.errInfo} url->{resp.url}");
        }
#else
        if (bOnlyShowErr && resp.code == 200)
            return;

		Debug.Log($"==== request url => {resp.url}");
		Debug.Log($"code =>{resp.code} bTimeOut =>{resp.isTimeOut}");
		if (resp.downloadMode == AxiDownLoadMode.NotDownLoad)
		{
			Debug.Log($"= response = body_text =>{resp.body} body_text.Length =>{resp.body.Length}");
		}
		else
		{
			Debug.Log($"= response = = download = respInfo.loadedLenght =>{resp.loadedlenght}");
		}
#endif
    }

}