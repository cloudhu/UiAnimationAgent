using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLOUDHU.UIAnimationAgent {
    public class CSVLine : IEnumerable
    {
        private Dictionary<string, string> dataContainer = new Dictionary<string, string>();

        private void AddItem(string key, string value)
        {
            if (dataContainer.ContainsKey(key))
            {
                Debug.Log(string.Format("CSVLine AddItem: there is a same key you want to add. key = {0}", key));
            }
            else
            {
                dataContainer.Add(key, value);
            }
        }

        public string this[string key]
        {
            get { return dataContainer[key]; }
            set { AddItem(key, value); }
        }

        public IEnumerator GetEnumerator()
        {
            foreach (KeyValuePair<string, string> item in dataContainer)
            {
                yield return item;
            }
        }
    }

    public class CSVTable : IEnumerable
    {
        private Dictionary<string, CSVLine> dataContainer = new Dictionary<string, CSVLine>();

        private void AddLine(string key, CSVLine line)
        {
            if (dataContainer.ContainsKey(key))
            {
                Debug.Log(string.Format("CSVTable AddLine: there is a same key you want to add. key = {0}", key));
            }
            else
            {
                dataContainer.Add(key, line);
            }
        }

        public bool ContainsKey(string key)
        {
            if (dataContainer.ContainsKey(key))
            {
                return true;
            }
            else
                return false;
        }

        public CSVLine this[string key]
        {
            get { return dataContainer[key]; }
            set { AddLine(key, value); }
        }

        public IEnumerator GetEnumerator()
        {
            foreach (var item in dataContainer)
            {
                yield return item.Value;
            }
        }

        public CSVLine WhereIDEquals(string id)
        {
            CSVLine result = null;
            if (!dataContainer.TryGetValue(id, out result))
            {
                Debug.Log(string.Format("CSVTable WhereIDEquals: The line you want to get data from is not found. id:{0}", id));
            }
            return result;
        }
    }

    public delegate void ReadCSVFinished(CSVTable result);

    public class CSVHelper : MonoBehaviour
    {
        #region singleton
        private static GameObject container = null;
        private static CSVHelper instance = null;
        public static CSVHelper Instance()
        {
            if (instance == null)
            {
                container = new GameObject("CSVHelper");
                instance = container.AddComponent<CSVHelper>();
            }
            return instance;
        }
        #endregion

        #region mono
        void Awake()
        {
            DontDestroyOnLoad(container);
        }
        #endregion

        #region private members
        //不同平台下StreamingAssets的路径是不同的，这里需要注意一下。
        public static readonly string csvFilePath =
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
			"file://" + Application.dataPath + "/StreamingAssets/GameConfig/";
#elif UNITY_IOS
            "file://" + Application.dataPath + "/Raw/GameConfig/";
#elif UNITY_ANDROID
			"jar:file://" + Application.dataPath + "!/assets/GameConfig/";
#else
            string.Empty;
#endif
        private Dictionary<string, CSVTable> readedTable = new Dictionary<string, CSVTable>();
        #endregion

        #region public interfaces
        public void ReadCSVFile(string fileName, ReadCSVFinished callback)
        {

            if (null == readedTable)
                readedTable = new Dictionary<string, CSVTable>();
            CSVTable result;
            if (readedTable.TryGetValue(fileName, out result))
            {
                Debug.LogWarning(string.Format("CSVHelper ReadCSVFile: You already read the file:{0}", fileName));
                return;
            }
            if (readedTable.ContainsKey(fileName))
            {
                return;
            }
            StartCoroutine(LoadCSVCoroutine(fileName, callback));
        }



        public CSVTable SelectFrom(string tableName)
        {
            CSVTable result = null;
            if (!readedTable.TryGetValue(tableName, out result))
            {
                Debug.Log(string.Format("CSVHelper SelectFrom: The table you want to get data from is not readed. table name:{0}", tableName));
            }
            return result;
        }
        #endregion

        #region private imp
        private IEnumerator LoadCSVCoroutine(string fileName, ReadCSVFinished callback)
        {
            string fileFullName = csvFilePath + fileName + ".csv";
            using (WWW www = new WWW(fileFullName))
            {
                yield return www;
                string text = string.Empty;
                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.LogError(string.Format("CSVHelper LoadCSVCoroutine:Load file failed file = {0}, error message = {1}", fileFullName, www.error));
                    yield break;
                }
                text = www.text;
                if (string.IsNullOrEmpty(text))
                {
                    Debug.LogError(string.Format("CSVHelper LoadCSVCoroutine:Loaded file is empty file = {0}", fileFullName));
                    yield break;
                }
                CSVTable table = ReadTextToCSVTable(text);
                readedTable.Add(fileName, table);
                if (callback != null)
                {
                    callback.Invoke(table);
                }
            }
        }

        private CSVTable ReadTextToCSVTable(string text)
        {
            CSVTable result = new CSVTable();
            text = text.Replace("\r", "");
            string[] lines = text.Split('\n');
            if (lines.Length < 2)
            {
                Debug.LogError("CSVHelper ReadTextToCSVData: Loaded text is not csv format");//必需包含一行键，一行值，至少两行
            }
            string[] keys = lines[0].Split(',');//第一行是键
            for (int i = 1; i < lines.Length; i++)//第二行开始是值
            {
                CSVLine curLine = new CSVLine();
                string line = lines[i];
                if (string.IsNullOrEmpty(line.Trim()))//略过空行
                {
                    break;
                }
                string[] items = line.Split(',');
                string key = items[0].Trim();//每一行的第一个值是唯一标识符
                for (int j = 0; j < items.Length; j++)
                {
                    string item = items[j].Trim();
                    curLine[keys[j]] = item;
                }
                result[key] = curLine;
            }
            return result;
        }
        #endregion
    }
}
