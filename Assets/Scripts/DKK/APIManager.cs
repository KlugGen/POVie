using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Linq;
using System;
using System.Text;

namespace DKK
{
    public class APIManager : MonoBehaviour
    {
        public string serverUrl;
        public int timeout = 120;

        static private APIManager _instance;

        private Dictionary<string, List<UnityWebRequest>> collections = new Dictionary<string, List<UnityWebRequest>>();

        static public APIManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.FindObjectsOfTypeAll(typeof(APIManager))[0] as APIManager;
                return _instance;
            }
        }

        static public string CombineDefault(string api, params string[] parameters)
        {

            StringBuilder sb = new StringBuilder(Instance.serverUrl);

            sb.Append("/");
            sb.Append(api);

            if (parameters.Length > 0)
            {
                sb.Append("?");

                for (int i = 0; i < parameters.Length; i++)
                {
                    sb.Append(parameters[i]);
                    if (i != parameters.Length - 1)
                    {
                        sb.Append("&");
                    }
                }
            }

            return sb.ToString();
        }

        static public string Combine(string serverUrl, string api, params string[] parameters)
        {

            StringBuilder sb = new StringBuilder(serverUrl);

            sb.Append("/");
            sb.Append(api);

            if (parameters.Length>0)
            {
                sb.Append("?");

                for(int i=0; i<parameters.Length; i++)
                {
                    sb.Append(parameters[i]);
                    if(i!=parameters.Length-1)
                    {
                        sb.Append("&");
                    }
                }
            }

            return sb.ToString();
        }

        static public string AppendParameters(string url, params string[] parameters)
        {

            StringBuilder sb = new StringBuilder(url);

            if (parameters.Length > 0)
            {
                sb.Append("?");

                for (int i = 0; i < parameters.Length; i++)
                {
                    sb.Append(parameters[i]);
                    if (i != parameters.Length - 1)
                    {
                        sb.Append("&");
                    }
                }
            }

            return sb.ToString();
        }

        public void PostRawJSON(string url, string collection, string jsonString, UnityAction<long, string> Callback)
        {
            Debug.Log(url);
            UnityWebRequest www = ForPostRawJSON(url, jsonString);
            
            StartCoroutine(PostCo(www, collection, (code, resp) =>
            {
                //if (code == 401)
                //{
                //    Debug.Log(url + " Brak autoryzacji. Ponawaianie logowania.");
                //    GetLogin((relog_code, relog_resp) =>
                //    {
                //        if (IsSuccess(relog_code))
                //        {
                //            Debug.Log("Ponawaianie logowania. Zalogowano.");
                //            UnityWebRequest www_ret = ForPostRawJSON(url, jsonString);
                //            www_ret.SetRequestHeader("Content-Type", "application/json");
                //            StartCoroutine(PostCo(www_ret, Callback));
                //        }
                //        else
                //        {
                //            Callback?.Invoke(relog_code, relog_resp + ". Wylogowano.");
                //        }
                //    });
                //}
                //else
                if (!IsSuccess(code))
                {
                    Debug.Log("B£¥D: " + code + " " + resp + " " + www.url);
                    Callback?.Invoke(code, resp);
                }
                else
                {
                    Callback?.Invoke(code, resp);
                }
            }));
        }

        private UnityWebRequest ForPostRawJSON(string url, string jsonString)
        {
            UnityWebRequest www = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            return www;
        }

        public void PostRawBytes(string url, string collection, string path, UnityAction<long, string> Callback)
        {
            Debug.Log(url);
            UnityWebRequest www = ForPostRawBytes(url, path);

            StartCoroutine(PostCo(www, collection, (code, resp) =>
            {
                //if (code == 401)
                //{
                //    Debug.Log(url + " Brak autoryzacji. Ponawaianie logowania.");
                //    GetLogin((relog_code, relog_resp) =>
                //    {
                //        if (IsSuccess(relog_code))
                //        {
                //            Debug.Log("Ponawaianie logowania. Zalogowano.");
                //            UnityWebRequest www_ret = ForPostRawBytes(url, path);
                //            StartCoroutine(PostCo(www_ret, Callback));
                //        }
                //        else
                //        {
                //            Callback?.Invoke(relog_code, relog_resp + ". Wylogowano.");
                //        }
                //    });
                //}
                //else
                if (!IsSuccess(code))
                {
                    Debug.Log("B£¥D: " + code + " " + resp);
                    Callback?.Invoke(code, resp);
                }
                else
                {
                    Callback?.Invoke(code, resp);
                }
            }));
        }

        private UnityWebRequest ForPostRawBytes(string url, string path)
        {
            UnityWebRequest www = new UnityWebRequest(url, "POST");
            www.uploadHandler = (UploadHandler)new UploadHandlerFile(path);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            return www;
        }

        public void Post(string url, string collection, WWWForm form, UnityAction<long, string> Callback)
        {
            Debug.Log(url);
            UnityWebRequest www = ForPost(url, form);

            StartCoroutine(PostCo(www, collection, (code, resp) =>
            {
                //if (code == 401)
                //{
                //    Debug.Log(url + " Brak autoryzacji. Ponawaianie logowania.");
                //    GetLogin((relog_code, relog_resp) =>
                //    {
                //        if (IsSuccess(relog_code))
                //        {
                //            Debug.Log("Ponawaianie logowania. Zalogowano.");
                //            UnityWebRequest www_ret = ForPost(url, form);
                //            StartCoroutine(PostCo(www_ret, Callback));
                //        }
                //        else
                //        {
                //            Callback?.Invoke(relog_code, relog_resp + ". Wylogowano.");
                //        }
                //    });
                //}
                //else
                if (!IsSuccess(code))
                {
                    Debug.Log("B£¥D: " + code + " " + resp);
                    Callback?.Invoke(code, resp);
                }
                else
                {
                    Callback?.Invoke(code, resp);
                }
            }));
        }

        private UnityWebRequest ForPost(string url, WWWForm form)
        {
            return UnityWebRequest.Post(url, form);
        }

        IEnumerator PostCo(UnityWebRequest www, string collection, UnityAction<long, string> Callback)
        {
            yield return StartCoroutine(Send(www, collection));
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.DataProcessingError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }

            if (Callback != null)
                Callback(www.responseCode, GetErrorResponse(www));
        }

        public void GetBytes(string url, string collection, UnityAction<bool, long, byte[]> Callback, UnityAction<float> progressLogger = null)
        {
            //Debug.Log(url);
            StartCoroutine(GetBytesCo(url, collection, (noError, code, resp) =>
            {
                //if (code == 401)
                //{
                //    Debug.Log(url + " Brak autoryzacji. Ponawaianie logowania.");
                //    GetLogin((relog_code, relog_resp) =>
                //    {
                //        if (IsSuccess(relog_code))
                //        {
                //            Debug.Log("Ponawaianie logowania. Zalogowano.");
                //            StartCoroutine(GetBytesCo(url, Callback));
                //        }
                //        else
                //        {
                //            Callback?.Invoke(relog_code, resp);
                //        }
                //    });
                //}
                //else
                if (!IsSuccess(code))
                {
                    Debug.Log("B£¥D: " + code + " " + resp + " " + url);
                    Callback?.Invoke(noError, code, resp);
                }
                else
                {
                    url.Log();
                    Callback?.Invoke(noError, code, resp);
                }
            }, progressLogger));
        }

        IEnumerator GetBytesCo(string url, string collection, UnityAction<bool, long, byte[]> Callback, UnityAction<float> progressLogger = null)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);

            yield return StartCoroutine(Send(www, collection, progressLogger));
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.DataProcessingError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
                Callback?.Invoke(false, www.responseCode, null);
                yield break;
                //Native.Message.Show("Problem", FormatError(GetErrorResponse(www)));
            }

            Callback?.Invoke(true, www.responseCode, www.downloadHandler.data);

            //Debug.Log("RESULT: " + url);
            //Debug.Log(www.responseCode);
            //Debug.Log(www.result);
            //Debug.Log(www.downloadProgress);
            //if (www.downloadHandler.data != null)
            //{
            //    Debug.Log(www.downloadHandler.data.Length);
            //}

        }

        public void GetText(string url, string collection, UnityAction<long, string> Callback = null)
        {
            Debug.Log(url);
            StartCoroutine(GetTextCo(url, collection, (code, resp) =>
            {
                //if (code == 401)
                //{
                //    Debug.Log(url + " Brak autoryzacji. Ponawaianie logowania.");
                //    GetLogin((relog_code, relog_resp) =>
                //    {
                //        if (IsSuccess(relog_code))
                //        {
                //            Debug.Log("Ponawaianie logowania. Zalogowano.");
                //            StartCoroutine(GetTextCo(url, Callback));
                //        }
                //        else
                //        {
                //            Callback?.Invoke(relog_code, relog_resp + ". Wylogowano.");
                //        }
                //    });
                //}
                //else 
                if (!IsSuccess(code))
                {
                    Debug.Log("B£¥D: " + code + " " + resp);
                    Callback?.Invoke(code, resp);
                }
                else
                {
                    Callback?.Invoke(code, resp);
                }
            }));
        }

        IEnumerator GetTextCo(string url, string collection, UnityAction<long, string> Callback)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);

            yield return StartCoroutine(Send(www, collection));
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.DataProcessingError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(url);
                Debug.Log(www.error);
            }

            if (Callback != null)
                Callback(www.responseCode, GetErrorResponse(www));
        }

        IEnumerator Send(UnityWebRequest www, string collection, UnityAction<float> progressLogger = null)
        {
            //string auth = BearerAuth();
            www.timeout = timeout;
            //if (!string.IsNullOrEmpty(auth))
            //{
            //    www.SetRequestHeader("Authorization", auth);
            //}

            AddToCollection(collection, www);

            www.SendWebRequest();

            while(!www.isDone)
            {
                yield return null;
                progressLogger?.Invoke(www.downloadProgress);
            }

            RemoveFromCollection(collection, www);
        }

        private void AddToCollection(string collection, UnityWebRequest www)
        {
            if (!collections.ContainsKey(collection))
                collections.Add(collection, new List<UnityWebRequest>() { www });
            else
                collections[collection].Add(www);
        }

        private void RemoveFromCollection(string collection, UnityWebRequest www)
        {
            if (collections.ContainsKey(collection))
            {
                collections[collection].Remove(www);
                if (collections[collection].Count == 0)
                    collections.Remove(collection);
            }
            else
            {
                Debug.Log("There is no collection called "+collection);
            }
        }

        public void Abort(string collection)
        {
            if (collections.ContainsKey(collection))
            {
                foreach (UnityWebRequest www in collections[collection])
                {
                    www.Abort();
                }
            }
        }

        //public void GetLogout(UnityAction<long, string> Callback = null)
        //{
        //    StartCoroutine(GetLogoutCo(Callback));
        //}

        //IEnumerator GetLogoutCo(UnityAction<long, string> Callback)
        //{
        //    UnityWebRequest www = UnityWebRequest.Get(logout_url);
        //    string auth = BearerAuth();
        //    if (!string.IsNullOrEmpty(auth))
        //    {
        //        www.SetRequestHeader("Authorization", auth);
        //    }

        //    yield return www.SendWebRequest();
        //    if (www.isNetworkError || www.isHttpError)
        //    {
        //        Debug.Log(www.error);
        //        Debug.Log(www.downloadHandler.text);
        //    }
        //    else
        //    {
        //        token = "";
        //        PlayerPrefs.SetString("token", "");
        //    }

        //    if (Callback != null)
        //        Callback(www.responseCode, GetErrorResponse(www));
        //}

        //public bool GetLogin(UnityAction<long, string> Callback = null)
        //{
        //    if (!string.IsNullOrEmpty(this.username))
        //    {
        //        StartCoroutine(GetLoginCo(login_url, Callback, this.username, this.password));
        //        return true;
        //    }
        //    return false;
        //}

        ////public void GetLogin(string username, string password, UnityAction<long, string> Callback = null)
        ////{
        ////    StartCoroutine(GetLoginCo(login_url, Callback, username, password));
        ////}

        //IEnumerator GetLoginCo(string url, UnityAction<long, string> Callback, string username, string password)
        //{
        //    Debug.Log(url);

        //    UnityWebRequest www = UnityWebRequest.Get(url);
        //    www.SetRequestHeader("Authorization", BasicAuth(username, password));
        //    www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        //    Debug.Log("LOGIN AUTH URL: " + url);

        //    yield return www.SendWebRequest();
        //    if (www.isNetworkError || www.isHttpError)
        //    {
        //        Debug.Log(www.error);
        //        Debug.Log(www.downloadHandler.text);
        //    }
        //    else
        //    {
        //        JSONObject json = new JSONObject(www.downloadHandler.text);
        //        token = json["data"]["api_token"].str;
        //        PlayerPrefs.SetString("token", token);
        //    }

        //    if (Callback != null)
        //        Callback(www.responseCode, GetErrorResponse(www));
        //}

        private string BasicAuth(string username, string password)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.ASCII.GetBytes(username + ":" + password);
            string base64 = string.Format("Basic {0}", Convert.ToBase64String(toEncodeAsBytes));
            Debug.Log(base64);
            return base64;
        }

        private string BearerAuth()
        {
            string token = PlayerPrefs.GetString("token", null);
            if (!string.IsNullOrEmpty(token))
            {
                return string.Format("Bearer {0}", token);
            }
            else
                return null;
        }

        static public bool IsSuccess(long code)
        {
            return code >= 200 && code <= 210;
        }

        private string GetErrorResponse(UnityWebRequest www)
        {
            if (string.IsNullOrEmpty(www.downloadHandler.text))
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    if (www.error.Contains("Cannot connect") || www.error.Contains("Cannot resolve destination host"))
                    {
                        //IcosManager.Instance.ShowNoWiFi();
                        return "Brak po³¹czenia z serwerem. SprawdŸ swoje po³¹czenie.";
                    }
                    else
                    {
                        //IcosManager.Instance.HideNoWiFi();
                        return www.error;
                    }
                }
            }
            //IcosManager.Instance.HideNoWiFi();
            return www.downloadHandler.text;
        }

        static public string FormatError(string resp)
        {
            string response = resp;

            // check if it is not json format
            JSONObject json = new JSONObject(response);
            if (json != null && json.list != null && json.list.Count > 0 && !string.IsNullOrEmpty(json.list[0].str))
                response = json.list[0].str;

            Debug.Log("Format error: " + response);

            return response;
        }
    }
}