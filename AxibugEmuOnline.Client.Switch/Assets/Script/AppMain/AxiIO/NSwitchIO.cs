using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxiIO
{
	public class NSwitchIO : IAxiIO
	{
		public NSwitchIO()
		{
			Debug.Log($"NSwitchIO Init");
			//return;
			AxiNS.instance.Init();
		}

		public void Ping()
		{
			throw new NotImplementedException();
		}
		public void dir_CreateDirectory(string dirpath)
		{
			//Debug.Log($"dir_CreateDirectory");
			//return;
			AxiNS.instance.io.CreateDir(dirpath);
		}

		public void dir_Delete(string path, bool recursive)
		{
			//Debug.Log($"dir_Delete recursive:{recursive}");
			//return;
			if (recursive)
				AxiNS.instance.io.DeletePathDirRecursively(path);
			else
				AxiNS.instance.io.DeletePathDir(path);
		}

		public IEnumerable<string> dir_EnumerateFiles(string path, string searchPattern)
		{
			//Debug.Log($"dir_EnumerateFiles path=>{path} searchPattern=>{searchPattern}");
			//return default;
			return AxiNS.instance.io.EnumerateFiles(path, searchPattern);
		}

		public bool dir_Exists(string dirpath)
		{
			//Debug.Log($"dir_Exists path=>{dirpath}");
			//return default;
			return AxiNS.instance.io.CheckPathExists(dirpath);
		}

		public string[] dir_GetDirectories(string path)
		{
			//Debug.Log($"dir_GetDirectories path=>{path}");
			//return default;
			if (!AxiNS.instance.io.GetDirectoryDirs(path, out string[] result))
			{
				return new string[0];
			}
			return result;
		}

		public string[] dir_GetFiles(string path)
		{
			//Debug.Log($"dir_GetFiles path=>{path}");
			//return default;
			if (!AxiNS.instance.io.GetDirectoryFiles(path, out string[] result))
			{
				return new string[0];
			}
			return result;
		}

		public void file_Delete(string filePath)
		{
			//Debug.Log($"file_Delete path=>{filePath}");
			//return;
			AxiNS.instance.io.DeletePathFile(filePath);
		}

		public bool file_Exists(string filePath)
		{
			//Debug.Log($"file_Exists path=>{filePath}");
			//return default;
			bool result = AxiNS.instance.io.CheckPathExists(filePath);
			//Debug.Log($"file_Exists path=>{filePath} result=>{result}");
			return result;
		}

		public byte[] file_ReadAllBytes(string filePath)
		{
			//Debug.Log($"file_ReadAllBytes path=>{filePath}");
			//return default;
			return AxiNS.instance.io.LoadSwitchDataFile(filePath);
		}

		public int file_ReadBytesToArr(string filePath, byte[] readToArr, int start, int len)
		{
			//Debug.Log($"file_ReadBytesToArr filePath=>{filePath},readToArr.Length=>{readToArr.Length},start=>{start},len=>{len}");
			//return default;
			byte[] bytes = file_ReadAllBytes(filePath);
			int templen = Math.Min(len, bytes.Length);
			Array.Copy(readToArr, readToArr, len);
			return templen;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="data"></param>
		/// <param name="immediatelyCommit">是否立即Commit到物理存储</param>
		public void file_WriteAllBytes(string filePath, byte[] data, bool immediatelyCommit = true)
		{
			//Debug.Log($"file_WriteAllBytes filePath=>{filePath},data.Length=>{data.Length},immediatelyCommit=>{immediatelyCommit}");
			//return;
			AxiNS.instance.io.FileToSaveWithCreate(filePath, data, immediatelyCommit);
		}

		public void file_WriteAllBytes(string filePath, System.IO.MemoryStream ms)
		{
			//Debug.Log($"file_WriteAllBytes filePath=>{filePath},ms.Length=>{ms.Length}");
			//return;
			AxiNS.instance.io.FileToSaveWithCreate(filePath, ms);
		}

	}
}