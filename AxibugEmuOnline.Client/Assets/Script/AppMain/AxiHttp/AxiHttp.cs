using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

public static class PSVThread
{

#if UNITY_PSP2
	static AutoResetEvent autoEvent = new AutoResetEvent(false);
	static Queue<Action> qActs = new Queue<Action>();
	static Queue<Action> qWork = new Queue<Action>();
#endif

    public static void DoTask(Action act)
    {
#if UNITY_PSP2
		AddSingleTask(act);
#else
        ThreadPool.QueueUserWorkItem(new WaitCallback((state) => act.Invoke()));
#endif
    }

#if UNITY_PSP2
	static Thread psvThread = new Thread(Loop);
	static bool bSingleInit = false;
	static void SingleInit()
	{
		if (bSingleInit) return;
		psvThread.Start();
		bSingleInit = true;
	}
	static void AddSingleTask(Action act)
	{
		SingleInit();
		lock (qActs)
		{
			qActs.Enqueue(act);
		}
		autoEvent.Set();
	}

	static void Loop()
	{
		while (autoEvent.WaitOne())
		{
			lock (qActs)
			{
				while (qActs.Count > 0) { qWork.Enqueue(qActs.Dequeue()); }
			}
			while (qWork.Count > 0)
			{
				Action act = qWork.Dequeue();
				try
				{
					act.Invoke();
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.Log(ex.ToString());
				}
			}
		}
	}
#endif
}

public static class AxiHttp
{
    public const char T = '\n';
    public const string CT = "\r\n";
    public const string CTRL = "\r\n\r\n";
    public const string Content_Length_Str = "content-length: ";
    public const string Content_Length_Str_M = "Content-Length: ";
    public const string Content_Length = "content-length";
    public const string Content_Encoding = "content-encoding";
    public const string Transfer_Encoding = "transfer-encoding";
    public const string Connection = "connection";
    public static long index = 0;
    static int singlePkgMaxRead = 1024;

    public class WaitAxiRequest : UnityEngine.CustomYieldInstruction
    {
        public AxiRespInfo mReqAsync;
        public WaitAxiRequest(AxiRespInfo reqAsync)
        {
            mReqAsync = reqAsync;
        }
        ~WaitAxiRequest()
        {
            mReqAsync = null;
        }
        public override bool keepWaiting
        {
            get { return !mReqAsync.isDone; }
        }
    }

    public static void Log(string log)
    {
        UnityEngine.Debug.Log(log);
        //Console.WriteLine(log);
    }

    static Dictionary<string, IPAddress> dictIP2Address = new Dictionary<string, IPAddress>();

    public class AxiRespInfo
    {
        public bool isDone = false;
        public AxiDownLoadMode downloadMode = AxiDownLoadMode.NotDownLoad;
        public bool bHadErr
        {
            get
            {
                return
                    isDone = true
                    &&
                    (
                    !string.IsNullOrEmpty(ErrInfo)
                    ||
                    code != 200
                    );
            }
        }
        public string ErrInfo;
        //public string Err = null;
        public string host = "";//host主机头
        public string url = "";//pathAndQuery
        public int port = 80;
        public string requestRaw = "";
        public string encoding = "";
        public string header = "";
        public string text { get { return body; } }
        public string body = "";
        public string reuqestBody = "";
        public string reuqestHeader = "";
        public Dictionary<string, string> headers = new Dictionary<string, string>();
        public string response = "";
        //public string gzip = "";
        public bool isGzip = false;
        public int length = 0;
        public int code = 0;
        public int location = 0;
        public int runTime = 0;//获取网页消耗时间，毫秒
        public int sleepTime = 0;//休息时间
        public string cookies = "";
        public bool bTimeOut = false;

        public int NeedloadedLenght;
        public int loadedLenght;
        public byte[] data { get { return bodyRaw; } }
        public byte[] bodyRaw;
        public string fileName;
        public float DownLoadPr =>
            NeedloadedLenght <= 0 ? -1 : (float)loadedLenght / NeedloadedLenght;
        public System.IO.BinaryWriter binaryWriter;
    }

    public static IPAddress GetDnsIP(string str)
    {
        lock (dictIP2Address)
        {
            if (!dictIP2Address.ContainsKey(str))
            {
                IPHostEntry host = Dns.GetHostEntry(str);
                IPAddress ip = null;
                foreach (var item in host.AddressList)
                {
                    if (item.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ip = item; break;
                    }
                }
                dictIP2Address[str] = ip;
            }
            return dictIP2Address[str];
        }
    }

    public enum AxiDownLoadMode
    {
        NotDownLoad = 0,
        DownLoadBytes = 1,
        DownloadToBinaryWriter = 2
    }

    public static AxiRespInfo AxiRequest(string url)
    {
        AxiRespInfo respInfo = new AxiRespInfo();
        respInfo.downloadMode = AxiDownLoadMode.NotDownLoad;
        SendAxiRequest(url, ref respInfo);
        return respInfo;
    }

    public static WaitAxiRequest AxiRequestAsync(string url)
    {
        AxiRespInfo respInfo = new AxiRespInfo();
        respInfo.downloadMode = AxiDownLoadMode.NotDownLoad;
        WaitAxiRequest respAsync = new WaitAxiRequest(respInfo);
        //Task task = new Task(() => SendAxiRequest(url, ref respInfo));
        //task.Start()
        PSVThread.DoTask(() => SendAxiRequest(url, ref respInfo));
        return respAsync;
    }

    public static AxiRespInfo AxiDownload(string url)
    {
        AxiRespInfo respInfo = new AxiRespInfo();
        respInfo.downloadMode = AxiDownLoadMode.DownLoadBytes;
        SendAxiRequest(url, ref respInfo);
        return respInfo;
    }

    public static AxiRespInfo AxiDownloadAsync(string url)
    {
        AxiRespInfo respInfo = new AxiRespInfo();
        respInfo.downloadMode = AxiDownLoadMode.DownLoadBytes;
        //Task task = new Task(() => SendAxiRequest(url, ref respInfo));
        //task.Start();
        PSVThread.DoTask(() => SendAxiRequest(url, ref respInfo));
        return respInfo;
    }

    static void SendAxiRequest(string url, ref AxiRespInfo respinfo, int timeout = 1000 * 1000, string encoding = "UTF-8")
    {
        if (url.ToLower().StartsWith("https://"))
            SendAxiRequestHttps(url, ref respinfo, timeout, encoding);// SendAxiRequestHttps(url, ref respinfo, timeout, encoding);
        else
            SendAxiRequestHttp(url, ref respinfo, timeout, encoding);
    }

    static void SendAxiRequestHttp(string url, ref AxiRespInfo respinfo, int timeout, string encoding)
    {
        Log("SendAxiRequestHttp");
        respinfo.url = url;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        respinfo.loadedLenght = 0;
        try
        {
            string strURI = url;
            string strHost = "";
            string strIP = "";
            int port = 0;
            string strRelativePath = "";
            bool bSSL = false;
            bool foward_302 = true;
            string ourErrMsg = "";

            if (!ParseURI(strURI, ref bSSL, ref strHost, ref strIP, ref port, ref strRelativePath, ref ourErrMsg))
            {
                Log("ParseURI False");
                respinfo.ErrInfo = ourErrMsg;
                respinfo.code = 0;
                respinfo.isDone = true;
                return;
            }



            var ip = GetDnsIP(strHost);
            var ipEndPoint = new IPEndPoint(ip, port);

            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                client.Connect(ipEndPoint);
                if (!client.Connected)
                {
                    client.Close();
                    sw.Stop();
                    respinfo.code = 0;
                    respinfo.isDone = true;
                    return;
                }

                //string requestRaw = $"GET {strRelativePath} HTTP/1.1\r\nHost: {strHost}\r\nConnection: Close\r\n\r\n";
                string request = $"GET {strURI} HTTP/1.1\r\nHost: {strHost}\r\nConnection: Close\r\n\r\n";

                checkContentLength(ref respinfo, ref request);
                respinfo.requestRaw = request;
                byte[] temp_responseBody = new byte[singlePkgMaxRead];

                byte[] buffer = Encoding.ASCII.GetBytes(request);
                client.Send(buffer);

                string tmp = "";
                int len = 0;
                StringBuilder sb = new StringBuilder();
                do
                {
                    byte[] responseHeader = new byte[1];
                    len = client.Receive(responseHeader, 1, SocketFlags.None);
                    if (len == 1)
                    {
                        char c = (char)responseHeader[0];
                        sb.Append(c);
                        if (c.Equals(T))
                        {
                            tmp = String.Concat(sb[sb.Length - 4], sb[sb.Length - 3], sb[sb.Length - 2], c);
                        }
                    }
                } while (!tmp.Equals(CTRL)
                && sw.ElapsedMilliseconds < timeout
                );


                respinfo.header = sb.ToString().Replace(CTRL, "");
                string[] headers = Regex.Split(respinfo.header, CT);
                if (headers != null && headers.Length > 0)
                {
                    //处理header
                    doHeader(ref respinfo, ref headers);
                }
                //自动修正编码
                if (!String.IsNullOrEmpty(respinfo.encoding))
                {
                    encoding = respinfo.encoding;
                }
                Encoding encod = Encoding.GetEncoding(encoding);

                //302 301跳转
                if ((respinfo.code == 302 || respinfo.code == 301) && foward_302)
                {
                    StringBuilder rsb = new StringBuilder(respinfo.requestRaw);
                    int urlStart = respinfo.requestRaw.IndexOf(" ") + 1;
                    int urlEnd = respinfo.requestRaw.IndexOf(" HTTP");
                    if (urlStart != -1 && urlEnd != -1)
                    {
                        url = respinfo.requestRaw.Substring(urlStart, urlEnd - urlStart);
                        rsb.Remove(urlStart, url.Length);
                        String location = respinfo.headers["location"];
                        if (!respinfo.headers["location"].StartsWith("/") && !respinfo.headers["location"].StartsWith("http"))
                        {
                            location = Tools.getCurrentPath(url) + location;
                        }
                        rsb.Insert(urlStart, location);
                        //return sendHTTPRequest(count, host, port, payload, rsb.ToString(), timeout, encoding, false);
                        client.Close();
                        sw.Stop();
                        SendAxiRequest(url, ref respinfo, timeout, encoding);
                        return;
                    }
                }

                //根据请求头解析
                if (respinfo.headers.ContainsKey(Content_Length))
                {
                    Log("User Head");
                    int length = int.Parse(respinfo.headers[Content_Length]);
                    respinfo.NeedloadedLenght = length;

                    //               while (respinfo.loadedLenght < length
                    //	&& sw.ElapsedMilliseconds < timeout
                    //	)
                    //{
                    //	int readsize = length - respinfo.loadedLenght;
                    //	len = client.Receive(temp_responseBody, respinfo.loadedLenght, readsize, SocketFlags.None);

                    //                   if (len > 0)
                    //	{
                    //		respinfo.loadedLenght += len;
                    //	}
                    //}

                    while (respinfo.loadedLenght < length
                        && sw.ElapsedMilliseconds < timeout
                        )
                    {
                        //len = client.Receive(temp_responseBody, respinfo.loadedLenght, readsize, SocketFlags.None);
                        int readsize = length - respinfo.loadedLenght;
                        readsize = Math.Min(readsize, singlePkgMaxRead);
                        len = client.Receive(temp_responseBody, 0, readsize, SocketFlags.None);
                        if (len > 0)
                        {
                            memoryStream.Write(temp_responseBody, 0, len);
                            respinfo.loadedLenght += len;
                        }
                    }
                }
                //解析chunked传输
                else if (respinfo.headers.ContainsKey(Transfer_Encoding))
                {
                    Log("User chunked");
                    //读取长度
                    int chunkedSize = 0;
                    byte[] chunkedByte = new byte[1];
                    //读取总长度
                    respinfo.loadedLenght = 0;
                    do
                    {
                        string ctmp = "";
                        do
                        {
                            len = client.Receive(chunkedByte, 1, SocketFlags.None);
                            ctmp += Encoding.UTF8.GetString(chunkedByte);

                        } while ((ctmp.IndexOf(CT) == -1)
                        && (sw.ElapsedMilliseconds < timeout)
                        );

                        chunkedSize = Tools.convertToIntBy16(ctmp.Replace(CT, ""));
                        //chunked的结束0\r\n\r\n是结束标志，单个chunked块\r\n结束
                        if (ctmp.Equals(CT))
                        {
                            continue;
                        }
                        if (chunkedSize == 0)
                        {
                            //结束了
                            break;
                        }
                        //int onechunkLen = 0;
                        //while (onechunkLen < chunkedSize
                        //	&& sw.ElapsedMilliseconds < timeout
                        //	)
                        //{

                        //                      len = client.Receive(responseBody, respinfo.loadedLenght, chunkedSize - onechunkLen, SocketFlags.None);
                        //	if (len > 0)
                        //	{
                        //		onechunkLen += len;
                        //		respinfo.loadedLenght += len;
                        //	}
                        //}

                        int onechunkLen = 0;
                        while (onechunkLen < chunkedSize
                            && sw.ElapsedMilliseconds < timeout
                            )
                        {
                            //len = client.Receive(responseBody, respinfo.loadedLenght, chunkedSize - onechunkLen, SocketFlags.None);

                            int readsize = chunkedSize - onechunkLen;
                            readsize = Math.Min(readsize, singlePkgMaxRead);
                            len = client.Receive(temp_responseBody, 0, readsize, SocketFlags.None);
                            if (len > 0)
                            {
                                memoryStream.Write(temp_responseBody, 0, len);
                                onechunkLen += len;
                                respinfo.loadedLenght += len;
                            }
                        }

                        //判断
                    } while (sw.ElapsedMilliseconds < timeout);
                }
                //connection close方式或未知body长度
                else
                {
                    Log("connection close or Unknow bodylenght");
                    while (sw.ElapsedMilliseconds < timeout)
                    {
                        if (client.Poll(timeout, SelectMode.SelectRead))
                        {
                            if (client.Available > 0)
                            {
                                //len = client.Receive(responseBody, respinfo.loadedLenght, (1024 * 200) - respinfo.loadedLenght, SocketFlags.None);
                                int readsize = (1024 * 200) - respinfo.loadedLenght;
                                readsize = Math.Min(readsize, singlePkgMaxRead);
                                len = client.Receive(temp_responseBody, 0, readsize, SocketFlags.None);

                                if (len > 0)
                                {
                                    memoryStream.Write(temp_responseBody, 0, len);
                                    respinfo.loadedLenght += len;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }


                byte[] responseBody = memoryStream.ToArray();
                //如果是下载
                if (respinfo.downloadMode > AxiDownLoadMode.NotDownLoad)
                {
                    //判断是否gzip
                    if (respinfo.headers.ContainsKey(Content_Encoding))
                    {
                        respinfo.bodyRaw = unGzipBytes(responseBody, respinfo.loadedLenght);
                    }
                    else
                    {
                        respinfo.bodyRaw = responseBody;
                    }

                    // 使用Uri类解析URL  
                    Uri uri = new Uri(url);
                    respinfo.fileName = System.IO.Path.GetFileName(uri.LocalPath);
                }
                else
                {
                    //判断是否gzip
                    if (respinfo.headers.ContainsKey(Content_Encoding))
                    {
                        respinfo.body = unGzip(responseBody, respinfo.loadedLenght, encod);
                    }
                    else
                    {
                        respinfo.body = encod.GetString(responseBody, 0, respinfo.loadedLenght);
                    }
                }

                client.Close();
            }

        }
        catch (Exception ex)
        {
            respinfo.ErrInfo = $"ex : {ex.ToString()}";
        }
        finally
        {
            sw.Stop();
            respinfo.length = respinfo.loadedLenght;
            respinfo.runTime = (int)sw.ElapsedMilliseconds;
            respinfo.bTimeOut = sw.ElapsedMilliseconds >= timeout;
            //if (socket != null)
            //{
            //	clientSocket.Close();
            //}
            respinfo.isDone = true;
        }
    }


    static void SendAxiRequestHttps(string url, ref AxiRespInfo respinfo, int timeout, string encoding)
    {
        respinfo.url = url;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        respinfo.loadedLenght = 0;
        TcpClient client = null;
        try
        {
            string strURI = url;
            string strHost = "";
            string strIP = "";
            int port = 0;
            string strRelativePath = "";
            bool bSSL = false;
            bool foward_302 = true;
            string ourErrMsg = "";

            if (!ParseURI(strURI, ref bSSL, ref strHost, ref strIP, ref port, ref strRelativePath, ref ourErrMsg))
            {
                Log("ParseURI False");
                respinfo.ErrInfo = ourErrMsg;
                respinfo.code = 0;
                respinfo.isDone = true;
                return;
            }

            //var ip = Dns.GetHostEntry(strHost).AddressList[0];
            //var ipEndPoint = new IPEndPoint(ip, port);

            //using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            //using (TcpClient tcpclient = new TcpClient())
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                //client.Connect(ipEndPoint);

                TimeOutSocket tos = new TimeOutSocket();
                client = tos.Connect(strHost, port, timeout);
                if (!client.Connected)
                {
                    client.Close();
                    sw.Stop();
                    respinfo.code = 0;
                    respinfo.isDone = true;
                    return;
                }
                SslStream ssl = null;

                //string requestRaw = $"GET {strRelativePath} HTTP/1.1\r\nHost: {strHost}\r\nConnection: Close\r\n\r\n";
                string request = $"GET {strURI} HTTP/1.1\r\nHost: {strHost}\r\nConnection: Close\r\n\r\n";

                ssl = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate));
                SslProtocols protocol = SslProtocols.Ssl3 | SslProtocols.Ssl2 | SslProtocols.Tls;
                ssl.AuthenticateAsClient(strHost, null, protocol, false);
                if (ssl.IsAuthenticated)
                {
                    checkContentLength(ref respinfo, ref request);
                    respinfo.requestRaw = request;
                    byte[] requestByte = Encoding.UTF8.GetBytes(request);
                    ssl.Write(requestByte);
                    ssl.Flush();
                }


                checkContentLength(ref respinfo, ref request);
                respinfo.requestRaw = request;
                byte[] temp_responseBody = new byte[singlePkgMaxRead];

                //byte[] buffer = Encoding.ASCII.GetBytes(requestRaw);
                //client.Send(buffer);

                string tmp = "";
                int len = 0;
                StringBuilder sb = new StringBuilder();
                do
                {
                    byte[] responseHeader = new byte[1];
                    int read = ssl.ReadByte();

                    char c = (char)read;
                    sb.Append(c);
                    if (c.Equals(T))
                    {
                        tmp = String.Concat(sb[sb.Length - 4], sb[sb.Length - 3], sb[sb.Length - 2], c);
                    }

                } while (!tmp.Equals(CTRL)
                && sw.ElapsedMilliseconds < timeout
                );


                respinfo.header = sb.ToString().Replace(CTRL, "");
                string[] headers = Regex.Split(respinfo.header, CT);
                if (headers != null && headers.Length > 0)
                {
                    //处理header
                    doHeader(ref respinfo, ref headers);
                }
                //自动修正编码
                if (!String.IsNullOrEmpty(respinfo.encoding))
                {
                    encoding = respinfo.encoding;
                }
                Encoding encod = Encoding.GetEncoding(encoding);

                //302 301跳转
                if ((respinfo.code == 302 || respinfo.code == 301) && foward_302)
                {
                    int urlStart = respinfo.requestRaw.IndexOf(" ");
                    int urlEnd = respinfo.requestRaw.IndexOf(" HTTP");
                    if (urlStart != -1 && urlEnd != -1)
                    {
                        url = respinfo.requestRaw.Substring(urlStart + 1, urlEnd - urlStart - 1);
                        if (!respinfo.headers["location"].StartsWith("/") && !respinfo.headers["location"].StartsWith("https"))
                        {
                            respinfo.requestRaw = respinfo.requestRaw.Replace(url, Tools.getCurrentPath(url) + respinfo.headers["location"]);
                        }
                        else
                        {
                            respinfo.requestRaw = respinfo.requestRaw.Replace(url, respinfo.headers["location"]);
                        }
                        //return sendHTTPRequest(count, host, port, payload, rsb.ToString(), timeout, encoding, false);
                        client.Close();
                        sw.Stop();
                        SendAxiRequest(url, ref respinfo, timeout, encoding);
                        return;
                    }
                }

                //根据请求头解析
                if (respinfo.headers.ContainsKey(Content_Length))
                {
                    Log("Use Head");
                    int length = int.Parse(respinfo.headers[Content_Length]);
                    respinfo.NeedloadedLenght = length;
                    while (respinfo.loadedLenght < length && sw.ElapsedMilliseconds < timeout)
                    {
                        //len = ssl.Read(responseBody, respinfo.loadedLenght, length - respinfo.loadedLenght);
                        int readsize = length - respinfo.loadedLenght;
                        readsize = Math.Min(readsize, singlePkgMaxRead);
                        len = ssl.Read(temp_responseBody, 0, readsize);
                        if (len > 0)
                        {
                            memoryStream.Write(temp_responseBody, 0, len);
                            respinfo.loadedLenght += len;
                        }
                    }
                }
                //解析chunked传输
                else if (respinfo.headers.ContainsKey(Transfer_Encoding))
                {
                    Log("User chunked");
                    //读取长度
                    int chunkedSize = 0;
                    byte[] chunkedByte = new byte[1];
                    //读取总长度
                    respinfo.loadedLenght = 0;
                    do
                    {
                        string ctmp = "";
                        do
                        {
                            len = ssl.Read(chunkedByte, 0, 1);
                            ctmp += Encoding.UTF8.GetString(chunkedByte);

                        } while (ctmp.IndexOf(CT) == -1 && sw.ElapsedMilliseconds < timeout);


                        chunkedSize = Tools.convertToIntBy16(ctmp.Replace(CT, ""));

                        //chunked的结束0\r\n\r\n是结束标志，单个chunked块\r\n结束
                        if (ctmp.Equals(CT))
                        {
                            continue;
                        }
                        if (chunkedSize == 0)
                        {
                            //结束了
                            break;
                        }
                        int onechunkLen = 0;
                        while (onechunkLen < chunkedSize && sw.ElapsedMilliseconds < timeout)
                        {
                            //len = ssl.Read(responseBody, respinfo.loadedLenght, chunkedSize - onechunkLen);
                            int readsize = chunkedSize - onechunkLen;
                            readsize = Math.Min(readsize, singlePkgMaxRead);
                            len = ssl.Read(temp_responseBody, 0, readsize);
                            if (len > 0)
                            {
                                memoryStream.Write(temp_responseBody, 0, len);
                                onechunkLen += len;
                                respinfo.loadedLenght += len;
                            }
                        }

                        //判断
                    } while (sw.ElapsedMilliseconds < timeout);
                }
                //connection close方式或未知body长度
                else
                {
                    Log("connection close or Unknow bodylenght");
                    while (sw.ElapsedMilliseconds < timeout)
                    {
                        while (sw.ElapsedMilliseconds < timeout)
                        {
                            if (client.Client.Poll(timeout, SelectMode.SelectRead))
                            {
                                if (client.Available > 0)
                                {
                                    //len = ssl.Read(responseBody, respinfo.loadedLenght, (1024 * 200) - respinfo.loadedLenght);
                                    int readsize = (1024 * 200) - respinfo.loadedLenght;
                                    readsize = Math.Min(readsize, singlePkgMaxRead);
                                    len = ssl.Read(temp_responseBody, 0, readsize);
                                    if (len > 0)
                                    {
                                        memoryStream.Write(temp_responseBody, 0, len);
                                        respinfo.loadedLenght += len;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                byte[] responseBody = memoryStream.ToArray();
                //如果是下载
                if (respinfo.downloadMode > AxiDownLoadMode.NotDownLoad)
                {
                    //判断是否gzip
                    if (respinfo.isGzip)
                    {
                        respinfo.bodyRaw = unGzipBytes(responseBody, respinfo.loadedLenght);
                    }
                    else
                    {
                        respinfo.bodyRaw = responseBody;
                    }

                    // 使用Uri类解析URL  
                    Uri uri = new Uri(url);
                    respinfo.fileName = System.IO.Path.GetFileName(uri.LocalPath);
                }
                else
                {
                    //判断是否gzip
                    if (respinfo.isGzip)
                    {
                        respinfo.body = unGzip(responseBody, respinfo.loadedLenght, encod);
                    }
                    else
                    {
                        respinfo.body = encod.GetString(responseBody, 0, respinfo.loadedLenght);
                    }
                }

            }

        }
        catch (Exception ex)
        {
            respinfo.ErrInfo = $"ex : {ex.ToString()}";
        }
        finally
        {
            client?.Close();
            sw.Stop();
            respinfo.length = respinfo.loadedLenght;
            respinfo.runTime = (int)sw.ElapsedMilliseconds;
            respinfo.bTimeOut = sw.ElapsedMilliseconds >= timeout;
            //if (socket != null)
            //{
            //	clientSocket.Close();
            //}
            respinfo.isDone = true;
        }

        if (client != null)
            client.Dispose();
    }


    private static void doHeader(ref AxiRespInfo respinfo, ref string[] headers)
    {

        for (int i = 0; i < headers.Length; i++)
        {
            if (i == 0)
            {
                respinfo.code = Tools.convertToInt(headers[i].Split(' ')[1]);
                if (respinfo.code != 200 && respinfo.code != 301 && respinfo.code != 302)
                    respinfo.ErrInfo = "code:" + respinfo.code;
            }
            else
            {
                String[] kv = Regex.Split(headers[i], ": ");
                String key = kv[0].ToLower();
                if (!respinfo.headers.ContainsKey(key))
                {
                    //自动识别编码
                    if ("content-type".Equals(key))
                    {
                        String hecnode = getHTMLEncoding(kv[1], "");
                        if (!String.IsNullOrEmpty(hecnode))
                        {
                            respinfo.encoding = hecnode;
                        }
                    }
                    if (kv.Length > 1)
                    {
                        respinfo.headers.Add(key, kv[1]);
                    }
                    else
                    {
                        respinfo.headers.Add(key, "");
                    }
                }
            }
            respinfo.isGzip = respinfo.headers.ContainsKey(Content_Encoding);
        }

    }
    public static string unGzip(byte[] data, int len, Encoding encoding)
    {

        string str = "";
        System.IO.MemoryStream ms = new System.IO.MemoryStream(data, 0, len);
        GZipStream gs = new GZipStream(ms, CompressionMode.Decompress);
        System.IO.MemoryStream outbuf = new System.IO.MemoryStream();
        byte[] block = new byte[1024];

        try
        {

            while (true)
            {
                int bytesRead = gs.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                {
                    break;
                }
                else
                {
                    outbuf.Write(block, 0, bytesRead);
                }
            }
            str = encoding.GetString(outbuf.ToArray());
        }
        catch (Exception e)
        {
            Log("解压Gzip发生异常----" + e.Message);
        }
        finally
        {
            outbuf.Close();
            gs.Close();
            ms.Close();

        }
        return str;

    }

    public static byte[] unGzipBytes(byte[] data, int len)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(data, 0, len);
        GZipStream gs = new GZipStream(ms, CompressionMode.Decompress);
        System.IO.MemoryStream outbuf = new System.IO.MemoryStream();
        byte[] block = new byte[1024];
        byte[] result;
        try
        {

            while (true)
            {
                int bytesRead = gs.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                {
                    break;
                }
                else
                {
                    outbuf.Write(block, 0, bytesRead);
                }
            }
            result = outbuf.ToArray();
        }
        catch (Exception e)
        {
            Log("解压Gzip发生异常----" + e.Message);
            result = new byte[0];
        }
        finally
        {
            outbuf.Close();
            gs.Close();
            ms.Close();

        }
        return result;

    }
    public static string getHTMLEncoding(string header, string body)
    {
        if (string.IsNullOrEmpty(header) && string.IsNullOrEmpty(body))
        {
            return "";
        }
        body = body.ToUpper();
        Match m = Regex.Match(header, @"charset\b\s*=\s*""?(?<charset>[^""]*)", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            return m.Groups["charset"].Value.ToUpper();
        }
        else
        {
            if (string.IsNullOrEmpty(body))
            {
                return "";
            }
            m = Regex.Match(body, @"charset\b\s*=\s*""?(?<charset>[^""]*)", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                return m.Groups["charset"].Value.ToUpper();
            }
        }
        return "";
    }
    private static void checkContentLength(ref AxiRespInfo respinfo, ref string request)
    {

        //重新计算并设置Content-length
        int sindex = request.IndexOf(CTRL);
        respinfo.reuqestHeader = request;
        if (sindex != -1)
        {
            respinfo.reuqestHeader = request.Substring(0, sindex);
            respinfo.reuqestBody = request.Substring(sindex + 4, request.Length - sindex - 4);
            int contentLength = Encoding.UTF8.GetBytes(respinfo.reuqestBody).Length;
            String newContentLength = Content_Length_Str_M + contentLength;

            if (request.IndexOf(Content_Length_Str_M) != -1)
            {
                request = Regex.Replace(request, Content_Length_Str_M + "\\d+", newContentLength);
            }
            else
            {
                request = request.Insert(sindex, "\r\n" + newContentLength);
            }
        }
        else
        {
            request = Regex.Replace(request, Content_Length_Str + "\\d+", Content_Length_Str_M + "0");
            request += CTRL;
        }


    }
    private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }
    public static bool ParseURI(string strURI,
        ref bool bIsSSL,
        ref string strHost,
        ref string strIP,
        ref int Port,
        ref string strRelativePath,
        ref string errMsg)
    {
        string strAddressRet;
        string strPortRet;
        string strRelativePathRet;
        string strIPRet;

        /*string strProtocol = strURI.Substring(0, 7);
        if (strProtocol != "http://"
            ||
            strProtocol != "https://")
            return false;*/

        if (!strURI.ToLower().StartsWith("http://") || strURI.ToLower().StartsWith("https://"))
            return false;

        bIsSSL = strURI.ToLower().StartsWith("https://");

        string strLeft = strURI.Substring(7, strURI.Length - 7);
        int nIndexPort = strLeft.IndexOf(':');
        if (nIndexPort == -1)
        {
            if (bIsSSL)
                strPortRet = "443";
            else
                strPortRet = "80";
            int nIndexRelative = strLeft.IndexOf('/');
            if (nIndexRelative != -1)
            {
                strAddressRet = strLeft.Substring(0, nIndexRelative);
                strRelativePathRet = strLeft.Substring(nIndexRelative, strLeft.Length - nIndexRelative);
            }
            else
            {
                errMsg = "Err Url";
                return false;
            }
        }
        else
        {
            strAddressRet = strLeft.Substring(0, nIndexPort);
            int nIndexRelative = strLeft.IndexOf('/');
            if (nIndexRelative != -1)
            {
                strPortRet = strLeft.Substring(nIndexPort + 1, nIndexRelative - (nIndexPort + 1));
                strRelativePathRet = strLeft.Substring(nIndexRelative, strLeft.Length - nIndexRelative);
            }
            else
            {
                errMsg = "Err Url";
                return false;
            }
        }
        strHost = strAddressRet;
        try
        {
            strIPRet = GetDnsIP(strAddressRet).ToString();
        }
        catch (Exception ex)
        {
            errMsg = ex.ToString();
            return false;
        }

        strIP = strIPRet;
        Port = int.Parse(strPortRet);
        strRelativePath = UrlEncode(strRelativePathRet);
        return true;
    }


    public static string UrlEncode(string str)
    {
        string sb = "";
        List<char> filter = new List<char>() { '!', '#', '$', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '=', '?', '@', '_', '~' };
        byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //System.Text.Encoding.Default.GetBytes(str)
        for (int i = 0; i < byStr.Length; i++)
        {
            if (filter.Contains((char)byStr[i]))
                sb += (char)byStr[i];
            else if ((char)byStr[i] >= 'a' && (char)byStr[i] <= 'z')
                sb += (char)byStr[i];
            else if ((char)byStr[i] >= 'A' && (char)byStr[i] <= 'Z')
                sb += (char)byStr[i];
            else if ((char)byStr[i] >= '0' && (char)byStr[i] <= '9')
                sb += (char)byStr[i];
            else
                sb += (@"%" + Convert.ToString(byStr[i], 16));
        }
        return sb;
    }


    class Tools
    {
        public static long currentMillis()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// 将16进制转换成10进制
        /// </summary>
        /// <param name="str">16进制字符串</param>
        /// <returns></returns>
        public static int convertToIntBy16(String str)
        {
            try
            {
                //Log($"convertToIntBy16 str- {str} lenght->{str.Length}");
                if (str.Length == 0)
                    return 0;
                return Convert.ToInt32(str, 16);
            }
            catch (Exception e)
            {
                Log($"convertToIntBy16 - {e.ToString()}");
            }
            return 0;

        }
        public static String getCurrentPath(String url)
        {
            int index = url.LastIndexOf("/");

            if (index != -1)
            {
                return url.Substring(0, index) + "/";
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// 将字符串转换成数字，错误返回0
        /// </summary>
        /// <param name="strs">字符串</param>
        /// <returns></returns>
        public static int convertToInt(String str)
        {

            try
            {
                return int.Parse(str);
            }
            catch (Exception e)
            {
                Log("info:-" + e.Message);
            }
            return 0;

        }

    }
    class TimeOutSocket
    {
        private bool IsConnectionSuccessful = false;
        private Exception socketexception = null;
        private ManualResetEvent TimeoutObject = new ManualResetEvent(false);
        public int useTime = 0;
        public TcpClient Connect(string host, int port, int timeoutMSec)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            TimeoutObject.Reset();
            socketexception = null;

            TcpClient tcpclient = new TcpClient();

            //IPHostEntry hostinfo = Dns.GetHostEntry("emu.axibug.com");
            //IPAddress[] aryIP = hostinfo.AddressList;
            //host = aryIP[0].ToString();

            Log($"BeginConnect {host}:{port} timeoutMSec=>{timeoutMSec}");
            tcpclient.BeginConnect(host, port, new AsyncCallback(CallBackMethod), tcpclient);

            if (TimeoutObject.WaitOne(timeoutMSec, false))
            {
                if (IsConnectionSuccessful)
                {
                    sw.Stop();
                    useTime = (int)sw.ElapsedMilliseconds;
                    return tcpclient;
                }
                else
                {
                    throw socketexception;
                }
            }
            else
            {
                tcpclient.Close();
                throw new TimeoutException("TimeOut Exception");
            }
        }
        private void CallBackMethod(IAsyncResult asyncresult)
        {
            try
            {
                IsConnectionSuccessful = false;
                TcpClient tcpclient = asyncresult.AsyncState as TcpClient;

                if (tcpclient.Client != null)
                {
                    tcpclient.EndConnect(asyncresult);
                    IsConnectionSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                Log($"CallBackMethod - {ex.ToString()}");
                IsConnectionSuccessful = false;
                socketexception = ex;
            }
            finally
            {
                TimeoutObject.Set();
            }
        }
    }

}