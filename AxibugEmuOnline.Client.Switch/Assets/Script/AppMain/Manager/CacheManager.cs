﻿using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class CacheManager
    {
        static readonly string CacheDirPath = $"{App.PersistentDataRoot()}/Caches";
        static readonly string TextureCacheDirPath = $"{CacheDirPath}/Texture";

        public void GetSpriteCache(string url, Action<Sprite, string> callback)
        {
            GetCacheData(url, TextureCacheDirPath, callback);
        }

        /// <summary> 移除文件缓存 </summary>
        public void ClearCaches()
        {
            if (AxiIO.Directory.Exists(CacheDirPath))
                AxiIO.Directory.Delete(CacheDirPath, true);
        }

        IEnumerator DownloadFromURL(string url, string path, Action<byte[]> callback)
        {
            AxiHttpProxy.SendDownLoadProxy request = AxiHttpProxy.GetDownLoad($"{App.httpAPI.WebHost}/{url}");

            while (!request.downloadHandler.isDone)
            {
                yield return null;
                //Debug.Log($"下载进度：{respInfo.DownLoadPr} ->{respInfo.loadedLenght}/{respInfo.NeedloadedLenght}");
            }
            AxiHttpProxy.ShowAxiHttpDebugInfo(request.downloadHandler);


            if (!request.downloadHandler.bHadErr)
            {
                AxiIO.Directory.CreateDirectory(path);
                AxiIO.File.WriteAllBytes($"{path}/{url.GetHashCode()}", request.downloadHandler.data, false);
                callback.Invoke(request.downloadHandler.data);
            }
            else
            {
                callback.Invoke(null);
            }

            /*
            var request = UnityWebRequest.Get($"{App.httpAPI.WebHost}/{url}");
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Directory.CreateDirectory(path);
                File.WriteAllBytes($"{path}/{url.GetHashCode()}", request.downloadHandler.data);
                callback.Invoke(request.downloadHandler.data);
            }
            else
                callback.Invoke(null);
            */
        }

        private Dictionary<string, object> cachesInMemory = new Dictionary<string, object>();
        void GetCacheData<T>(string url, string path, Action<T, string> callback) where T : class
        {
            object cacheObj;

            if (cachesInMemory.TryGetValue(url, out cacheObj) && cacheObj is T)
            {
                T obj = (T)cacheObj;
                callback.Invoke(obj, url);
                return;
            }

            var fileName = $"{url.GetHashCode()}";
            byte[] rawData = null;

            var filePath = $"{path}/{fileName}";
            if (AxiIO.File.Exists(filePath))
            {
                rawData = AxiIO.File.ReadAllBytes(filePath);
                var @out = RawDataConvert<T>(rawData);
                cachesInMemory[url] = @out;
                callback.Invoke(@out, url);
            }
            else
            {
                App.StartCoroutine(DownloadFromURL(url, path, (data) =>
                {
                    var @out = RawDataConvert<T>(data);
                    cachesInMemory[url] = @out;
                    callback.Invoke(@out, url);
                }));
            }
        }

        Type t_texture2d = typeof(Texture2D);
        Type t_sprite = typeof(Sprite);
        Type t_byteArray = typeof(byte[]);
        Type t_object = typeof(object);

        T RawDataConvert<T>(byte[] data) where T : class
        {
            var vt = typeof(T);

            if (vt == t_texture2d || vt == t_sprite)
            {
                Texture2D texture = new Texture2D(2, 2);
                if (data == null)
                {
                    var errorSpr = Resources.Load<Sprite>("UIImage/error");
                    if (vt == t_texture2d) return errorSpr.texture as T;
                    else return errorSpr as T;
                }
                else
                {
                    texture.LoadImage(data);

                    if (vt == t_texture2d) return texture as T;
                    else return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)) as T;
                }
            }
            else if (vt == t_byteArray)
            {
                return data as T;
            }
            else
            {
                return data as T;
            }

            throw new NotImplementedException($"{vt.Name}");
        }
    }
}
