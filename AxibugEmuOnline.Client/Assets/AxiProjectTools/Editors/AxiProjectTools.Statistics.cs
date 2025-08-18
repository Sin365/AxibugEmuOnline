#if UNITY_EDITOR  && UNITY_2020_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class AxiProjectToolsStatistics : EditorWindow
{
    static string cachecfgPath = "Assets/AxiStatisticsDatas.asset";
    static Dictionary<string, AxiStatisticsDatas> dictTempData = new Dictionary<string, AxiStatisticsDatas>();

    static void ClearTempData()
    {
        dictTempData.Clear();
    }
    static string GetRootTempKey(int type, string rootName)
    {
        return type + "_" + rootName;
    }


    // 添加Hierarchy右键菜单项
    [MenuItem("GameObject/AxiStatistics/GetAxiNodeHash", false, 10)]
    public static void GetAxiNodeHash()
    {
        // 获取当前右键选中的Transform
        Transform selectedTransform = Selection.activeTransform;
        if (selectedTransform != null)
        {
            Debug.Log("选中的对象"+selectedTransform.name+",Hash=>"+ GetNodeDataHash(selectedTransform,true));
        }
    }

    static int GetNodeDataHash(Transform trans,bool bLog = false)
    {
        long hashplus = 0;
        hashplus += trans.position.GetHashCode();
        hashplus += trans.localPosition.GetHashCode();


#if UNITY_2017_1_OR_NEWER
        int count = trans.childCount;
#else
        int count = trans.GetChildCount();
#endif
        hashplus += count;
        for  (int i = 0; i < count; i++)
        {
            hashplus += trans.GetChild(i).name.GetHashCode();
        }


        if (bLog)
        {
            //Debug.Log("trans.position.GetHashCode()=>" + trans.position.GetHashCode());
            //Debug.Log("trans.localPosition.GetHashCode()=>" + trans.localPosition.GetHashCode());
            //Debug.Log("childCount =>" + count);
            //Debug.Log("hashplus =>" + hashplus);
            //Debug.Log("hashplus.GetHashCode() =>" + hashplus.GetHashCode());
        }

        return hashplus.GetHashCode();
    }
    static int GetNodeLinkListHash(List<AxiStatistics_Node_Link> nodes)
    {
        string hashplus = string.Empty;

        foreach (var node in nodes)
        {
            hashplus += node.Name;
            hashplus += node.Idx.ToString();
        }

        return hashplus.GetHashCode();
    }

    static string GetNodeLinkListStr(List<AxiStatistics_Node_Link> nodes)
    {
        string linkstr = string.Empty;

        foreach (var node in nodes)
        {
            linkstr += "/";
            linkstr += node.Name;
            linkstr += "[" + node.Idx + "]";
        }
        return linkstr;
    }

    static void AddComponentData(int _type, string _rootPath, AxiStatistics_Node_Component _comdata, string _nodepath, Component lastcom)
    {
        string rootKey = GetRootTempKey(_type, _rootPath);

        if (!dictTempData.ContainsKey(rootKey))
        {
            dictTempData[rootKey] = new AxiStatisticsDatas() { type = _type, FullPath = _rootPath, nodes = new List<AxiStatistics_Node>() };
        }
        AxiStatisticsDatas rootData = dictTempData[rootKey];

        List<AxiStatistics_Node_Link> link = new List<AxiStatistics_Node_Link>();
        if (lastcom.transform.parent != null)
        {
            Transform currNode = lastcom.transform;
            while (currNode != null)
            {
                //最顶层了
                if (currNode.parent == null)
                {
                    link.Insert(0, new AxiStatistics_Node_Link()
                    {
                        NodeHash = GetNodeDataHash(currNode),
                        Idx = 0,
                        OnlyOne = true,
                        Name = currNode.gameObject.name
                    });
                    break;
                }

                int thisNameAllCount = 0;
                int thisNodeIdx = -1;
#if UNITY_2017_1_OR_NEWER
                int count = currNode.parent.childCount;
#else
                int count = currNode.parent.GetChildCount();
#endif
                bool bFind = false;
                for (int i = 0; i < count; i++)
                {
                    GameObject checkGobj = currNode.parent.GetChild(i).gameObject;
                    if (checkGobj.name == currNode.name)
                    {
                        thisNameAllCount++;
                        if (checkGobj == currNode.gameObject)
                        {
                            thisNodeIdx = thisNameAllCount - 1;
                            bFind = true;
                        }
                    }
                }

                if (bFind)
                {
                    link.Insert(0, new AxiStatistics_Node_Link()
                    {
                        NodeHash = GetNodeDataHash(currNode),
                        Idx = thisNodeIdx,
                        OnlyOne = thisNameAllCount == 1,
                        Name = currNode.gameObject.name
                    });
                    currNode = currNode.parent;
                }
                else
                    break;
            }
        }
        else
        {
            link.Insert(0, new AxiStatistics_Node_Link()
            {
                NodeHash = GetNodeDataHash(lastcom.transform),
                Idx = 0,
                OnlyOne = true,
                Name = lastcom.gameObject.name
            });
        }

        int linkhash = GetNodeLinkListHash(link);
        AxiStatistics_Node nodeData = rootData.nodes.Where(w => w.LinkHash == linkhash).FirstOrDefault();
        if (nodeData == null)
        {
            nodeData = new AxiStatistics_Node();
            nodeData.Name = Path.GetFileName(_nodepath);
            //nodeData.NodeFullPath = _nodepath;
            nodeData.components = new List<AxiStatistics_Node_Component>();
            //nodeData.NodeIdx = thisNodeIdx;
            //nodeData.NodeIdxOnlyOne = bNodeIdxOnlyOne;

            nodeData.link = link;
            nodeData.LinkHash = linkhash;
            nodeData.LinkFullStr = GetNodeLinkListStr(link);

            rootData.nodes.Add(nodeData);
        }

        nodeData.components.Add(_comdata);
    }

    static bool CheckCom(Component[] allcoms, int comRealIdx, int _type, string _rootPath, Component com, string nodepath)
    {
        if (com is BoxCollider2D)
        {
            BoxCollider2D bc = com as BoxCollider2D;
#if UNITY_2017_1_OR_NEWER
            Debug.Log(nodepath + "BoxCollider2D->center=>(" + bc.offset.x + "," + bc.offset.y + ") size=>(" + bc.size.x + "," + bc.size.y + "");
#else
			Debug.Log(nodepath +"BoxCollider2D->center=>("+ bc.center.x+","+bc.center.y+") size=>("+ bc.size.x+","+bc.size.y+"");
#endif
            AxiStatistics_Node_Component _com = new AxiStatistics_Node_Component();
            _com.type = typeof(BoxCollider2D).ToString();
#if UNITY_2017_1_OR_NEWER
            _com.center = bc.offset;
#else
			_com.center = bc.center;
#endif
            _com.size = bc.size;
            SetCompnentIdxNum<BoxCollider2D>(_com, allcoms, comRealIdx, bc);
            AddComponentData(_type, _rootPath, _com, nodepath, com);
        }
        if (com is Rigidbody2D)
        {
            Rigidbody2D rig2d = com as Rigidbody2D;
            Debug.Log(_rootPath + "Rigidbody2D->simulated=>(" + rig2d.simulated + ")");
            Debug.Log(_rootPath + "Rigidbody2D->IsSleeping=>(" + rig2d.isKinematic.ToString() + ")");

            AxiStatistics_Node_Component _com = new AxiStatistics_Node_Component();
            _com.type = typeof(Rigidbody2D).ToString();
            _com.isKinematic = rig2d.isKinematic;
            _com.simulated = rig2d.simulated;
            _com.gravityScale = rig2d.gravityScale;
            SetCompnentIdxNum<Rigidbody2D>(_com, allcoms, comRealIdx, rig2d);
            AddComponentData(_type, _rootPath, _com, nodepath, com);
        }
        return true;
    }

    /// <summary>
    /// 找出同类Idx
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="allcoms"></param>
    /// <param name="comRealIdx"></param>
    /// <param name="com"></param>
    /// <returns></returns>
    static void SetCompnentIdxNum<T>(AxiStatistics_Node_Component _comData, Component[] allcoms, int comRealIdx, T com) where T : Component
    {
        int ComIdxNum;
        bool ComTypeIsOnlyOne = false;
        int TCount = com.transform.GetComponents<T>().Length;
        if (TCount == 1)
        {
            ComIdxNum = 0;
            ComTypeIsOnlyOne = true;
        }
        else if (TCount < 1)
        {
            Debug.LogError("找不到，不应该");
            ComIdxNum = -1;
        }

        ComIdxNum = -1;
        for (int i = 0; i < allcoms.Length; i++)
        {
            //他自己自然是了
            if (i == comRealIdx)
            {
                ComIdxNum++;
                break;
            }
            if (allcoms[i] is T)
                ComIdxNum++;
        }

        _comData.ComIdxNum = ComIdxNum;
        _comData.ComTypeOnlyOne = ComTypeIsOnlyOne;
    }

    [MenuItem("Axibug移植工具/Statistics/[1]统计所有预制体和场景下的Collider和RigBody")]

    public static void StatisticsCollider()
    {
        ClearTempData();
        StatisticsCollider<BoxCollider2D>();
        StatisticsCollider<Rigidbody2D>();

        AxiStatisticsCache cache = ScriptableObject.CreateInstance<AxiStatisticsCache>();
        foreach (var data in dictTempData)
            cache.caches.Add(data.Value);
        AssetDatabase.CreateAsset(cache, cachecfgPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void StatisticsCollider<T>() where T : Component
    {
        AxiProjectTools.GoTAxiProjectToolsSence();

        string[] sceneGuids = AssetDatabase.FindAssets("t:scene");
        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains(AxiProjectTools.toolSenceName))
                continue;

#if UNITY_4_6
			EditorApplication.OpenScene(path);
#else
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
#endif

            // 创建一个列表来存储根节点
            List<GameObject> rootNodes = new List<GameObject>();

            // 遍历场景中的所有对象
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                // 检查对象是否有父对象
                if (obj.transform.parent == null)
                {
                    // 如果没有父对象，则它是一个根节点
                    rootNodes.Add(obj);
                }
            }

            foreach (var node in rootNodes)
                LoopPrefabNode<T>(0, path, path, node, 0);

        }
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GetPrefab<T>(path);
        }

        AxiProjectTools.GoTAxiProjectToolsSence();
        Debug.Log("<Color=#FFF333>处理完毕  统计所有预制体和场景下的" + typeof(T).FullName + "</color>");
    }

    static void GetPrefab<T>(string path) where T : Component
    {
#if UNITY_4_6
		GameObject prefab = AssetDatabase.LoadAssetAtPath(path,typeof(GameObject)) as GameObject;
#else
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
#endif

        LoopPrefabNode<T>(1, path, path, prefab.gameObject, 0);
    }

    static void LoopPrefabNode<T>(int _type, string _rootPath, string noderootPath, GameObject trans, int depth) where T : Component
    {
        //		#if UNITY_2018_4_OR_NEWER
        string nodename = noderootPath + "/" + trans.name;
        GameObject prefabRoot = trans.gameObject;

        Component[] components = prefabRoot.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            var com = components[i];

            if (com == null)
                continue;

            T comobj = com as T;
            if (comobj == null)
                continue;

            if (CheckCom(components, i, _type, _rootPath, comobj, nodename))
                continue;
        }

        //遍历
        foreach (Transform child in trans.transform)
            LoopPrefabNode<T>(_type, _rootPath, nodename, child.gameObject, depth + 1);
        //#else
        //		Debug.Log("低版本不要执行本函数");
        //#endif
    }

#if UNITY_2017_1_OR_NEWER
    [MenuItem("Axibug移植工具/Statistics/[2]通过记录，对组件进行修补")]
    public static void RepairRigBodyByStatistics()
    {
        List<string> errLog = new List<string>();
        List<string> doneLog = new List<string>();
        List<ValueTuple<string, string>> NeedRepair = new List<ValueTuple<string, string>>();
        List<ValueTuple<string, string>> FinishRepair = new List<ValueTuple<string, string>>();
        string CurrScenePath = string.Empty;
        AxiProjectTools.GoTAxiProjectToolsSence();
#if UNITY_4_6
		AxiStatisticsCache data = AssetDatabase.LoadAssetAtPath(cachecfgPath,typeof(AxiStatisticsCache)) as AxiStatisticsCache;
#else
        AxiStatisticsCache data = AssetDatabase.LoadAssetAtPath<AxiStatisticsCache>(cachecfgPath);
#endif
        string[] sceneGuids = AssetDatabase.FindAssets("t:scene");
        List<string> ScenePath = new List<string>();
        List<string> SceneName = new List<string>();
        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains(AxiProjectTools.toolSenceName))
                continue;
            ScenePath.Add(path);
            SceneName.Add(Path.GetFileName(path));
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:prefab");
        List<string> prefabPath = new List<string>();
        List<string> prefabName = new List<string>();
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            prefabPath.Add(path);
            prefabName.Add(Path.GetFileName(path));
        }

        foreach (var cache in data.caches.OrderBy(w => w.type))
        {
            //场景
            if (cache.type == 0)
            {
                #region 场景加载
                string targetName = Path.GetFileName(cache.FullPath);
                int Idx = SceneName.IndexOf(targetName);
                if (Idx < 0)
                {
                    Debug.LogError(targetName + "[Repair]找不到对应资源");
                    continue;
                }
                string targetpath = ScenePath[Idx];

                //保证场景切换
                if (!string.Equals(CurrScenePath, targetpath))
                {
#if UNITY_4_6
					EditorApplication.OpenScene(targetpath);
#else
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(targetpath);
#endif
                }
                CurrScenePath = targetpath;
                #endregion

                int DirtyCount = 0;
                foreach (var node in cache.nodes)
                {
                    GameObject targetNodePathObj = GetNodeByLink(cache.FullPath, node.link, out string errStr);
                    if (targetNodePathObj == null)
                    {
                        errLog.Add(errStr);
                        continue;
                    }

                    /*
                    string targetNodePath = node.NodeFullPath.Substring(cache.FullPath.Length, node.NodeFullPath.Length - cache.FullPath.Length);

                    //GameObject targetNodePathObj = GameObject.Find(targetNodePath);
                    GameObject targetNodePathObj = GetNodeByIdx(node, targetNodePath);

                    if (targetNodePathObj == null)
                    {
                        string err = "[Repair]" + node.NodeFullPath + "找不到对应节点";
                        errLog.Add(err);
                        Debug.LogError(err);
                        continue;
                    }
                    */
                    foreach (var com in node.components)
                    {
                        if (RepairComponent(node.LinkFullStr, targetNodePathObj, com, out var errlog))
                        {
                            NeedRepair.Add(new ValueTuple<string, string>($"{cache.FullPath}:{node.LinkFullStr}", $"{com.type}[{com.ComIdxNum}]"));
                            DirtyCount++;
                        }
                        errLog.AddRange(errlog);
                    }
                }
                if (DirtyCount > 0)
                {
                    Debug.Log($"[Repair][场景处理]{cache.FullPath}共{DirtyCount}个需要处理");


                    // 获取当前打开的场景
                    var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

                    // 保存场景
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);

                    Debug.Log("场景已保存: " + activeScene.path);
                    string donestr = $"[Repair][场景处理成功]{targetpath},共{DirtyCount}个";
                    doneLog.Add(donestr);

                }
            }
            else if (cache.type == 1)
            {
                string targetpath = cache.FullPath;

                //来到空场景
                if (!string.IsNullOrEmpty(CurrScenePath))
                {
                    AxiProjectTools.GoTAxiProjectToolsSence();
                    CurrScenePath = string.Empty;
                }


                GameObject prefabInstance = AssetDatabase.LoadAssetAtPath<GameObject>(targetpath);
                if (prefabInstance == null)
                {
                    Debug.LogError($"[Repair]Failed to load prefab at path: {prefabPath}");
                    return;
                }

                var obj = GameObject.Instantiate(prefabInstance, null);

                int DirtyCount = 0;
                foreach (var node in cache.nodes)
                {
                    GameObject targetNodePathObj = GetNodeByLink(cache.FullPath, node.link, out string errStr, obj);

                    if (targetNodePathObj == null)
                    {
                        errLog.Add(errStr);
                        continue;
                    }
                    //if (node.NodeFullPath == targetpath + "/" + Path.GetFileNameWithoutExtension(targetpath))
                    //{
                    //    //预制体自己就是目标
                    //    targetNodePathObj = obj;
                    //}
                    //else
                    //{
                    //    string targetNodePath = node.NodeFullPath.Substring(cache.FullPath.Length + prefabInstance.name.Length + 2, node.NodeFullPath.Length - cache.FullPath.Length - prefabInstance.name.Length - 2);
                    //    //targetNodePathObj = obj.transform.Find(targetNodePath)?.gameObject;
                    //    targetNodePathObj = GetNodeByIdx(node, targetNodePath, obj);

                    //    if (targetNodePathObj == null)
                    //    {
                    //        Debug.LogError("[Repair]" + targetNodePath + "找不到对应节点");
                    //        continue;
                    //    }
                    //}


                    foreach (var com in node.components)
                    {
                        if (RepairComponent(node.LinkFullStr, targetNodePathObj, com, out var errlog))
                        {
                            NeedRepair.Add(new ValueTuple<string, string>($"{cache.FullPath}:{node.LinkFullStr}", $"{com.type}[{com.ComIdxNum}]"));
                            DirtyCount++;
                        }

                        errLog.AddRange(errlog);
                    }
                }

                if (DirtyCount > 0)
                {
                    Debug.Log($"[Repair][预制体处理]{targetpath}共{DirtyCount}个需要处理");
                    PrefabUtility.SaveAsPrefabAsset(obj, targetpath);
                    string donestr = $"[Repair][预制体处理成功]{targetpath},共{DirtyCount}个";
                    doneLog.Add(donestr);
                }
                GameObject.DestroyImmediate(obj);
            }
        }

        AxiProjectTools.GoTAxiProjectToolsSence();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[Repair][统计]:");
        sb.AppendLine("----处理成功----");
        foreach (var val in doneLog.OrderBy(w => w))
        {
            sb.AppendLine(val);
        }
        sb.AppendLine("----异常统计----");
        foreach (var val in errLog.OrderBy(w => w))
        {
            sb.AppendLine(val);
        }
        sb.AppendLine("----需要处理----");
        foreach (var val in NeedRepair.OrderBy(w => w.Item1))
        {
            sb.AppendLine($"{val.Item1}=>{val.Item2}");
        }
        Debug.Log($"{sb}");

        File.WriteAllText("Assets/AxiNeedRepair.txt", sb.ToString());
    }


    //    static GameObject GetNodeByIdx(AxiStatistics_Node nodedata, string targetNodePath, GameObject root = null)
    //    {
    //        GameObject targetNodePathObj;

    //        if (root == null)
    //            targetNodePathObj = GameObject.Find(targetNodePath);
    //        else
    //            targetNodePathObj = root.transform.Find(targetNodePath)?.gameObject;

    //        if (targetNodePathObj == null)
    //            return null;

    //        string targetName = targetNodePathObj.name;
    //        int currIdx = -1;
    //        if (!nodedata.NodeIdxOnlyOne)
    //        {
    //            if (targetNodePathObj.transform.parent != null)
    //            {
    //#if UNITY_2017_1_OR_NEWER
    //                int count = targetNodePathObj.transform.parent.childCount;
    //#else
    //            int count = targetNodePathObj.transform.parent.GetChildCount();
    //#endif
    //                for (int i = 0; i < count; i++)
    //                {
    //                    GameObject checkGobj = targetNodePathObj.transform.parent.GetChild(i).gameObject;
    //                    if (checkGobj.name == targetName)
    //                    {
    //                        currIdx++;
    //                        if (nodedata.NodeIdx == currIdx)
    //                        {
    //                            targetNodePathObj = checkGobj;
    //                            break;
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        return targetNodePathObj;
    //    }


    static GameObject GetNodeByLink(string rootPath, List<AxiStatistics_Node_Link> linklist, out string errStr, GameObject PrefabRoot = null)
    {
        List<AxiStatistics_Node_Link> temp_useddlink = new List<AxiStatistics_Node_Link>();
        if (linklist.Count < 1)
        {
            errStr = $"[Repair] Link 为空";
            Debug.LogError(errStr);
            return null;
        }

        temp_useddlink.Add(linklist[0]);
        GameObject currRoot;

        if (PrefabRoot == null)
            currRoot = GameObject.Find(linklist[0].Name);
        else
        {
            currRoot = PrefabRoot;
            //currRoot = PrefabRoot.transform.Find(linklist[0].Name)?.gameObject;
        }

        if (currRoot == null)
        {
            errStr = $"[Repair] 根节点找不到{rootPath}:{GetNodeLinkListStr(linklist)} =>  null";
            Debug.LogError(errStr);
            return null;
        }

        for (int link_i = 1; link_i < linklist.Count; link_i++)
        {
            AxiStatistics_Node_Link targetLink = linklist[link_i];
            temp_useddlink.Add(targetLink);
            GameObject findNode = null;
#if UNITY_2017_1_OR_NEWER
            int count = currRoot.transform.childCount;
#else
            int count = currNode.transform.GetChildCount();
#endif

            if (targetLink.OnlyOne)
            {
                for (int i = 0; i < count; i++)
                {
                    GameObject checkGobj = currRoot.transform.GetChild(i).gameObject;
                    if (checkGobj.name == targetLink.Name)
                    {
                        findNode = checkGobj;
                        break;
                    }
                }
            }
            else
            {

                Dictionary<int, GameObject> tempHash2Node = new Dictionary<int, GameObject>();
                List<GameObject> tempGobjList = new List<GameObject>();
                bool HashDrity = false;
                for (int i = 0; i < count; i++)
                {
                    GameObject checkGobj = currRoot.transform.GetChild(i).gameObject;
                    if (checkGobj.name == targetLink.Name)
                    {
                        int temphash = GetNodeDataHash(checkGobj.transform);
                        if (!tempHash2Node.ContainsKey(temphash))
                        {
                            tempHash2Node.Add(GetNodeDataHash(checkGobj.transform), checkGobj);
                        }
                        else
                        {
                            HashDrity = true;
                        }
                        tempGobjList.Add(checkGobj);
                    }
                }

                //Hash严格模式
                if (!HashDrity && tempHash2Node.TryGetValue(targetLink.NodeHash, out var val))
                {
                    findNode = val;
                }
                //下标模式
                else
                {
                    if (targetLink.Idx < 0 || tempGobjList.Count == 0 || (tempGobjList.Count != 0 && targetLink.Idx >= tempGobjList.Count))
                    {
                        errStr = $"[Repair]link 下标模式 找不到=>{rootPath}:{GetNodeLinkListStr(temp_useddlink)}[{targetLink.Idx}] => 完整链路{rootPath}:{GetNodeLinkListStr(linklist)}";
                        Debug.LogError(errStr);
                        return null;
                    }
                    else
                    {
                        findNode = tempGobjList[targetLink.Idx];
                    }
                }
            }


            currRoot = findNode;
            if (currRoot == null)
                break;
        }

        if (currRoot == null)
        {
            errStr = $"[Repair]link 找不到[{rootPath}:{GetNodeLinkListStr(temp_useddlink)}] => 完整链路{rootPath}:{GetNodeLinkListStr(linklist)}";
            Debug.LogError(errStr);
            return null;
        }
        else
        {
            errStr = string.Empty;
            return currRoot;
        }
    }

    static bool RepairComponent(string NodePath, GameObject targetNodePathObj, AxiStatistics_Node_Component comdata, out List<string> Errlog)
    {
        Errlog = new List<string>();
        string err;
        bool Dirty = false;
        if (comdata.type == typeof(Rigidbody2D).ToString())
        {
            Rigidbody2D rg2d = GetCompnentById<Rigidbody2D>(targetNodePathObj, comdata);
            if (rg2d == null)
            {
                err = $"[Repair]{NodePath}=> Rigidbody2D[{comdata.ComIdxNum}] == null";
                Debug.LogError(err);
                Errlog.Add(err);
                Dirty = false;
            }

            /*
与新版Unity的差异​
​无BodyType选项​：Unity 4.6.7中所有Rigidbody2D默认等效于新版的Dynamic类型（受重力影响），但无法设置为Static或Kinematic。
​无Simulated选项​：只要物体启用了Rigidbody2D组件且Gravity Scale > 0，就会受重力作用。

            所以，一旦老版本gravityScale > 0,就受重力作用，直接设置新版本这边：simulated = true;bodyType = RigidbodyType2D.Dynamic;
             */


            if (rg2d.gravityScale != comdata.gravityScale)
            {
                Debug.Log($"[Repair]{NodePath}=> Rigidbody2D[{comdata.ComIdxNum}] simulated:{rg2d.gravityScale} != :{comdata.gravityScale}  rg2d.bodyType => {rg2d.bodyType} ");

                rg2d.gravityScale = comdata.gravityScale;
                Dirty = true;
            }

            //if (rg2d.gravityScale > 0 && (!rg2d.simulated || rg2d.bodyType != RigidbodyType2D.Dynamic))
            if (!rg2d.simulated || rg2d.bodyType != RigidbodyType2D.Dynamic)
            {
                Debug.Log($"[Repair]{NodePath}=> Rigidbody2D[{comdata.ComIdxNum}] simulated:{rg2d.simulated} != :{comdata.simulated}  rg2d.bodyType => {rg2d.bodyType} ");

                rg2d.simulated = true;
                rg2d.bodyType = RigidbodyType2D.Dynamic;
                Dirty = true;
            }
        }
        else if (comdata.type == typeof(BoxCollider2D).ToString())
        {
            BoxCollider2D bc = GetCompnentById<BoxCollider2D>(targetNodePathObj, comdata);
            if (bc == null)
            {
                err = $"[Repair]{NodePath}=> BoxCollider2D[{comdata.ComIdxNum}] == null";
                Debug.LogError(err);
                Errlog.Add(err);
                Dirty = false;
            }
            else
            {
                if (bc.size != comdata.size)
                {
                    Debug.Log($"[Repair]{NodePath} BoxCollider2D[{comdata.ComIdxNum}] => size:{bc.size} != {comdata.size} ");
                    bc.size = comdata.size;
                    Dirty = true;
                }
                if (bc.offset != comdata.center)
                {
                    Debug.Log($"[Repair]{NodePath} BoxCollider2D[{comdata.ComIdxNum}] => offset:{bc.offset} != center{comdata.center} ");
                    bc.offset = comdata.center;
                    Dirty = true;
                }

                if (Dirty)
                {
                    bc.size = comdata.size;
                    bc.offset = comdata.center;
                }
            }
        }


        return Dirty;
    }

    static T GetCompnentById<T>(GameObject gobj, AxiStatistics_Node_Component node) where T : Component
    {
        if (node.ComIdxNum == 0)
            return gobj.GetComponent<T>();
        else if (node.ComIdxNum > 0)
        {
            T[] coms = gobj.GetComponents<T>();
            if (node.ComIdxNum < coms.Length)
                return coms[node.ComIdxNum];
        }

        return null;
    }

#endif
}
#endif