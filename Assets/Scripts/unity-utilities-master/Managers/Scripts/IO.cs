using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

#if UNITY_EDITOR || !UNITY_ANDROID
using System.Drawing;
using System.Runtime.InteropServices;
#endif

namespace DKK
{

    public class IO : MonoBehaviour
    {
        public delegate void MethodFileInfo(FileInfo fileinfo);

        static public IO self;
        static public string dataSource;
        private LinkedList<Request> requests = new LinkedList<Request>();

        void Awake()
        {
            dataSource = Application.persistentDataPath;

            self = this;
            enabled = false;
        }

        /// <summary>
        /// Adds the request.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="obj">Object come back in callback.</param>
        /// <param name="useDefDataSource">If set to <c>true</c> use def data source.</param>
        /// <param name="highPriority">If set to <c>true</c> high priority.</param>
        static public void AddRequest(string name, BytesObjMethod callback, object objectForCallback = null, bool highPriority = false)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Request request = new Request(name, callback, objectForCallback);
                if (highPriority)
                {
                    self.requests.AddFirst(request);
                }
                else
                {
                    self.requests.AddLast(request);
                }
                self.enabled = true;
            }
        }

        static public void CancelAllRequests()
        {
            while (self.requests.Count > 0)
            {
                self.requests.First.Value.Cancel();
                self.requests.RemoveFirst();
            }
            self.enabled = false;
            System.GC.Collect();
        }

        static public bool CancelRequestByInt(ulong? objID)
        {
            return self.requests.Remove(x => (ulong?)x.MyObject == objID);
        }

        public float iterationTime = 0.1f;
        private float currentTime = 0;

        void Update()
        {
            if (requests.Count > 0)
            {
                float t = Time.realtimeSinceStartup;
                currentTime = 0;
                bool work = true;

                while (work)
                {

                    Request request = requests.First.Value;
                    request.Do();
                    requests.RemoveFirst();

                    currentTime += Time.realtimeSinceStartup - t;
                    t = Time.realtimeSinceStartup;
                    work = currentTime < iterationTime && requests.Count > 0;
                }
            }
            else
                enabled = false;
        }

        static public string GetFileName(string path)
        {
            int pos = path.LastIndexOf("/") + 1;
            string slash = "";
            if (pos >= 0)
                slash = path.Substring(pos, path.Length - pos);

            pos = path.LastIndexOf("\\") + 1;
            string backslash = "";
            if (pos >= 0)
                backslash = path.Substring(pos, path.Length - pos);

            if (backslash.Length == 0 && slash.Length == 0)
                return path;

            string ret = backslash.Length > slash.Length ? slash : backslash;

            return ret;
        }

        static public bool MoveFile(string source, string destination)
        {
            if (File.Exists(source))
            {
                PrepareDirectory(destination);
                File.Move(source, destination);
                return true;
            }
            return false;
        }

        static public bool SaveToFile(string path, object obj)
        {
            if (string.IsNullOrEmpty(path) || obj == null)
                return false;

            try
            {
                PrepareDirectory(path);
                Stream fileStream = File.Open(path, FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, obj);
                fileStream.Close();
                return true;

            }
            catch (Exception e)
            {

                string s = e.ToString();
                Debug.Log(s);
                ////DKK.UI.Dialogs.Information.Create("warning".LangGet(), s);
                return false;
            }
        }

        static public bool SaveToFile(string path, byte[] bytes)
        {
            if (string.IsNullOrEmpty(path) || bytes.Length == 0)
                return false;

            try
            {
                PrepareDirectory(path);
                File.WriteAllBytes(path, bytes);
                return true;

            }
            catch (Exception e)
            {

                string s = e.ToString();
                Debug.Log(s);
                //DKK.UI.Dialogs.Information.Create("warning".LangGet(), s);
                return false;
            }
        }

        static public bool SaveToFile(string path, string text)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(text))
                return false;

            try
            {
                PrepareDirectory(path);
                File.WriteAllText(path, text);
                return true;

            }
            catch (Exception e)
            {

                string s = e.ToString();
                Debug.Log(s);
                //DKK.UI.Dialogs.Information.Create("warning".LangGet(), s);
                return false;
            }
        }

        static private void PrepareDirectory(string path)
        {
            string dirPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }

        static public object LoadFromFile(string path)
        {
            try
            {
                Stream fileStream = File.Open(path, FileMode.Open, FileAccess.Read);
                BinaryFormatter formatter = new BinaryFormatter();
                object obj = formatter.Deserialize(fileStream);
                fileStream.Close();
                return obj;

            }
            catch (Exception e)
            {

                string s = e.ToString();
                Debug.Log(s);
                //DKK.UI.Dialogs.Information.Create("warning".LangGet(), s);
                return null;
            }
        }

        static public byte[] BytesFromFile(string path)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                return bytes;

            }
            catch (Exception)
            {

                //Debug.Log (e.ToString ());
                return null;
            }
        }

        static public string ReadFromFile(string path)
        {
            try
            {
                string ret = File.ReadAllText(path);
                return ret;

            }
            catch (Exception)
            {

                //				Debug.Log (e.ToString ());
                return null;
            }
        }

        static public bool CopyFile(string source, string destination)
        {
            try
            {
                byte[] bytes = BytesFromFile(source);
                if (bytes != null)
                {

                    string dn = Path.GetDirectoryName(destination);
                    if (!Directory.Exists(dn))
                    {
                        Directory.CreateDirectory(dn);
                    }

                    return SaveToFile(destination, bytes);
                }
            }
            catch (Exception e)
            {

                string s = e.ToString();
                Debug.Log(s);
                //DKK.UI.Dialogs.Information.Create("warning".LangGet(), s);
            }
            return false;
        }

        static public string RemoveFileWithEmptyDirectories(string filePath)
        {
            string ret = null;
            try
            {

                FileInfo fi = new FileInfo(filePath);
                DirectoryInfo di = fi.Directory;
                if (di.GetFiles().Length == 1)
                {

                    DirectoryInfo parent = di.Parent;
                    while (parent.GetFiles().Length + parent.GetDirectories().Length == 1)
                    {
                        di = parent;
                        parent = di.Parent;
                    }
                    ret = di.FullName;
                    Directory.Delete(di.FullName, true);

                }
                else
                {
                    ret = filePath;
                    File.Delete(filePath);
                }

            }
            catch (System.Exception e)
            {
                string s = e.ToString();
                Debug.Log(s);
                //DKK.UI.Dialogs.Information.Create("warning".LangGet(), s);
            }
            return ret;
        }

        static public void Remove(List<string> elements)
        {
            foreach (string str in elements)
            {
                try
                {
                    File.Delete(str);
                }
                catch (System.Exception)
                {
                    try
                    {
                        Directory.Delete(str, true);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.Log(ex.ToString());
                    }
                }
            }
        }

        static public int CountFiles(DirectoryInfo di)
        {
            int counter = di.GetFiles().Length;
            DirectoryInfo[] dis = di.GetDirectories();
            for (int i = 0; i < dis.Length; i++)
            {
                counter += CountFiles(dis[i]);
            }
            return counter;
        }

        static public void ForeachFile(DirectoryInfo di, MethodFileInfo method)
        {
            FileInfo[] fis = di.GetFiles();
            for (int i = 0; i < fis.Length; i++)
            {
                method(fis[i]);
            }
            DirectoryInfo[] dis = di.GetDirectories();
            for (int i = 0; i < dis.Length; i++)
            {
                ForeachFile(dis[i], method);
            }
        }

        static public List<FileInfo> GetAllFiles(DirectoryInfo di)
        {
            List<FileInfo> list = new List<FileInfo>();
            GetAllFiles(di, list);
            return list;
        }

        private static void GetAllFiles(DirectoryInfo di, List<FileInfo> list)
        {
            FileInfo[] fis = di.GetFiles();
            if (list.Capacity < list.Count + fis.Length)
                list.Capacity = list.Count + fis.Length;
            list.AddRange(fis);
            DirectoryInfo[] dis = di.GetDirectories();
            for (int i = 0; i < dis.Length; i++)
            {
                GetAllFiles(dis[i], list);
            }
        }

        static public float GetDirectorySize(DirectoryInfo di)
        {
            float size = 0;
            FileInfo[] fis = di.GetFiles();
            for (int i = 0; i < fis.Length; i++)
            {
                size += fis[i].Length;
            }
            DirectoryInfo[] dis = di.GetDirectories();
            for (int i = 0; i < dis.Length; i++)
            {
                size += GetDirectorySize(dis[i]);
            }
            return size;
        }


        //#if UNITY_EDITOR || !UNITY_ANDROID
        //        public class FileBrowser
        //        {
        //            static private IntPtr ptr;

        //            [DllImport("user32.dll")]
        //            private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        //            [DllImport("user32.dll")]
        //            private static extern IntPtr GetActiveWindow();

        //            static public string OpenSingleFile(ExtensionFilter[] extensions)
        //            {
        //                string dir = PlayerPrefs.GetString("fileBrowserPath", "");

        //                HideWinAppWindow();


        //                string path = Crosstales.FB.FileBrowser.OpenSingleFile("openFile".LangGet(), dir, extensions);

        //                ShowWinAppWindow();

        //                PlayerPrefs.SetString("fileBrowserPath", Path.GetDirectoryName(path));

        //                return path;
        //            }

        //            static public string[] GetPaths(ExtensionFilter[] extensions)
        //            {
        //                string dir = PlayerPrefs.GetString("fileBrowserPath", "");

        //                HideWinAppWindow();


        //                string[] paths = Crosstales.FB.FileBrowser.OpenFiles("openFile".LangGet(), dir, extensions);

        //                ShowWinAppWindow();

        //                if (paths.Length > 0)
        //                {
        //                    PlayerPrefs.SetString("fileBrowserPath", Path.GetDirectoryName(paths[0]));
        //                }
        //                return paths;
        //            }

        //            static public string SaveSingleFile(ExtensionFilter[] extensions, string fileName)
        //            {
        //                string dir = PlayerPrefs.GetString("fileBrowserPath", "");

        //                HideWinAppWindow();


        //                string path = Crosstales.FB.FileBrowser.SaveFile("saveFile".LangGet(), dir, fileName, extensions);

        //                ShowWinAppWindow();

        //                if (string.IsNullOrEmpty(path))
        //                    return "";

        //                PlayerPrefs.SetString("fileBrowserPath", Path.GetDirectoryName(path));

        //                return path;
        //            }

        //            static public string SaveTexture(Texture2D texture, string filename)
        //            {
        //                string dir = PlayerPrefs.GetString("fileBrowserPath", "");
        //                var extensions = new[] {
        //                    new ExtensionFilter ("Image Files", "png", "jpg", "jpeg"),
        //                    new ExtensionFilter ("All Files", "*"),
        //                };

        //                HideWinAppWindow();

        //                string path = Crosstales.FB.FileBrowser.SaveFile("saveFile".LangGet(), dir, filename, extensions);

        //                ShowWinAppWindow();

        //                if (string.IsNullOrEmpty(path))
        //                    return "";

        //                byte[] bytes;
        //                string ext = Path.GetExtension(path);
        //                switch (ext)
        //                {
        //                    case ".png":
        //                        bytes = texture.EncodeToPNG();
        //                        IO.SaveToFile(path, bytes);
        //                        break;
        //                    case ".jpg":
        //                    case ".jpeg":
        //                    default:
        //                        bytes = texture.EncodeToJPG();
        //                        IO.SaveToFile(path, bytes);
        //                        break;
        //                }

        //                PlayerPrefs.SetString("fileBrowserPath", Path.GetDirectoryName(path));

        //                return path;
        //            }

        //            static public Texture2D GetTexture()
        //            {
        //                var extensions = new[] {
        //                    new ExtensionFilter ("Image Files", "png", "jpg", "jpeg", "emf"),
        //                    new ExtensionFilter ("All Files", "*"),
        //                };
        //                try
        //                {
        //                    string path = OpenSingleFile(extensions);
        //                    byte[] bytes = IO.BytesFromFile(path);
        //                    Texture2D texture = new Texture2D(2, 2);
        //                    if (texture.LoadImage(bytes))
        //                    {
        //                        texture.Apply();
        //                        return texture;
        //                    }
        //                    else
        //                    {
        //                        Destroy(texture);
        //                        return null;
        //                    }

        //                }
        //                catch (System.Exception e)
        //                {

        //                    Debug.Log(e.ToString());
        //                    return null;
        //                }
        //            }

        //            static public string[] GetImagesPaths()
        //            {
        //                var extensions = new[] {
        //                    new ExtensionFilter ("Image Files", "png", "jpg", "jpeg", "emf"),
        //                    new ExtensionFilter ("All Files", "*"),
        //                };
        //                return FileBrowser.GetPaths(extensions);
        //            }

        //            static public string GetDirectory()
        //            {
        //                try
        //                {
        //                    string dir = PlayerPrefs.GetString("fileBrowserPath", "");

        //                    HideWinAppWindow();

        //                    string path = Crosstales.FB.FileBrowser.OpenSingleFolder("catalogs".LangGet(), dir);

        //                    ShowWinAppWindow();

        //                    if (string.IsNullOrEmpty(path))
        //                        return "";

        //                    PlayerPrefs.SetString("fileBrowserPath", Path.GetDirectoryName(path));

        //                    return path;
        //                }
        //                catch(Exception e)
        //                {
        //                    Debug.Log(e.ToString());
        //                    return null;
        //                }
        //            }

        //            static private void ShowWinAppWindow()
        //            {
        //                if(ptr!=null)
        //                ShowWindow(ptr, 1);
        //            }

        //            static private void HideWinAppWindow()
        //            {
        //                ptr = GetActiveWindow();
        //                ShowWindow(ptr, 6);
        //            }

        //            //static public string[] GetImagePath()
        //            //{
        //            //    var extensions = new[] {
        //            //        new ExtensionFilter ("Image Files", "png", "jpg", "jpeg"),
        //            //        new ExtensionFilter ("All Files", "*"),
        //            //    };
        //            //    return FileBrowser.GetImagesPaths(extensions);
        //            //}
        //        }
        //#endif

//        static public byte[] GetImageFileBytes(string filepath)
//        {
//#if UNITY_STANDALONE || UNITY_EDITOR

//            string ext = Path.GetExtension(filepath);
//            ext = ext.ToLower();
//            switch (ext)
//            {
//                case ".emf":
//                    Bitmap bm = new Bitmap(filepath);
//                    using (var stream = new MemoryStream())
//                    {
//                        bm.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
//                        return stream.ToArray();
//                    }

//                default:
//                    return File.ReadAllBytes(filepath);
//            }
//#else
//        return File.ReadAllBytes(filepath);
//#endif
//        }

//        private Texture2D GetTextureFromFile(string path)
//        {
//            try
//            {

//                if (string.IsNullOrEmpty(path))
//                    return null;
//                byte[] byteImage = GetImageFileBytes(path);

//                Texture2D n = new Texture2D(2, 2, TextureFormat.RGB24, false);

//                if (!n.LoadImage(byteImage))
//                    return null;
//                if (n.width == 8)
//                    return null;

//                return n;

//            }
//            catch (Exception e)
//            {

//                string s = e.ToString();
//                Debug.Log(s);
//                //DKK.UI.Dialogs.Information.Create("warning".LangGet(), s);
//                return null;
//            }
//        }
    }

    public class Request
    {
        private string name;
        private object obj;

        public object MyObject
        {
            get
            {
                return obj;
            }
        }

        private BytesObjMethod method;

        public Request(string name, BytesObjMethod method, object obj)
        {
            if (method == null)
                throw new Exception("No method in request");

            if (string.IsNullOrEmpty(name))
            {
                Debug.Log("The name is null or empty");
            }

            this.obj = obj;
            this.name = name;
            this.method = method;
        }

        public void Cancel()
        {
            if (method != null)
                method(null, obj);
        }

        public void Do()
        {
            try
            {
                if (method != null)
                    method(IO.BytesFromFile(name), obj);
            }
            catch (Exception e)
            {
                Debug.Log("Requested object is null! " + e.ToString());
            }
        }
    }
}