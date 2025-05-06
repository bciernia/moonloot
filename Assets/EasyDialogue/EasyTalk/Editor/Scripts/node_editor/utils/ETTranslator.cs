using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EasyTalk.Editor.Utils
{
    public class ETTranslator
    {
        /// <summary>
        /// Retrieves a stored Google access token, if there is one and it isn't within 10 minutes of expiration, otherwise returns null.
        /// </summary>
        /// <returns>The stored Google access token.</returns>
        public static string GetStoredAccessToken()
        {
            string tokenTimeString = EditorPrefs.GetString("et-google-translate-token-expire");
            if (tokenTimeString != null && tokenTimeString.Length > 0)
            {
                int tokenTime = int.Parse(tokenTimeString);
                if (DateTime.UtcNow.Second < tokenTime - 600)
                {
                    string token = EditorPrefs.GetString("et-google-translate-token");
                    return token;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates or retrieves a Google access token for translation services if the PATH and Google CLI are set up correctly or a valid key is stored.
        /// </summary>
        /// <returns>A Google access token if one could be retrieved successfully, null otherwise.</returns>
        public static string GetGoogleAccessToken()
        {
            string accessToken = null;// GetStoredAccessToken();

            if (accessToken == null || accessToken.Length == 0)
            {
                System.Diagnostics.Process process = CreateAccessTokenRetrievalProcess();
                process.Start();
                
                string error = process.StandardError.ReadToEnd();

                if (error != null && error.Length > 0)
                {
                    Debug.LogWarning("The access token couldn't be retrieved for Google Translation. Error: " + error);
                }
                else
                {
                    accessToken = process.StandardOutput.ReadToEnd();
                    accessToken = accessToken.Replace("\n", "").Replace("\r", "");

                    EditorPrefs.SetString("et-google-translate-token", accessToken);
                    EditorPrefs.SetString("et-google-translate-token-expire", "" + (DateTime.UtcNow.Second + 3300));
                }
            }

            return accessToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static System.Diagnostics.Process CreateAccessTokenRetrievalProcess()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();

#if UNITY_EDITOR_WIN
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/C gcloud auth print-access-token";
#elif UNITY_EDITOR_LINUX || UNITY_EDITOR_OSX
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = "-c gcloud auth print-access-token";
#endif

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            return process;
        }

        /// <summary>
        /// Makes a call to the Google Cloud Translation API to translate the specified lines of text from one language to another.
        /// </summary>
        /// <param name="projectId">The project ID of the Google Cloud project to use.</param>
        /// <param name="accessToken">The Google Cloud access token to use when making the API call.</param>
        /// <param name="sourceLanguageCode">The ISO-639 language code for the origin language of the lines which are to be translated.</param>
        /// <param name="targetLanguageCode">The ISO-639 language code for the target language of the lines which are to be translated.</param>
        /// <param name="lines">The lines of text to translate.</param>
        /// <returns>The results of the translation request.</returns>
        public static GoogleTranslationResult Translate(string projectId, string accessToken, string sourceLanguageCode, string targetLanguageCode, List<string> lines)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                
                //URL for the translation API
                string url = $"https://translate.googleapis.com/v3beta1/projects/{projectId}:translateText";

                string contentText = "";

                for(int i = 0; i < lines.Count; i++)
                {
                    string htmlLine = WebUtility.HtmlEncode(lines[i]);
                    contentText += $@"""{htmlLine}""";

                    if(i < lines.Count - 1)
                    {
                        contentText += ",";
                    }
                }

                //Create the request body
                string requestBody = $@"
                {{
                    ""sourceLanguageCode"": ""{sourceLanguageCode}"",
                    ""targetLanguageCode"": ""{targetLanguageCode}"",
                    ""contents"": [{contentText}]
                }}";

                StringContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                content.Headers.ContentLength = Encoding.UTF8.GetByteCount(requestBody);

                HttpResponseMessage response = httpClient.PostAsync(url, content).Result;

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read and display the response
                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    jsonResponse = WebUtility.HtmlDecode(jsonResponse);
                    GoogleTranslationResult translationResult = GoogleTranslationResult.FromJSON(jsonResponse);
                    return translationResult;
                }
                else
                {
                    Debug.LogWarning("Failed with status code " + response.StatusCode + ": " + response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception e)
            {
                Debug.Log("An error occurred: " + e);
            }

            return null;
        }

        public class GoogleTranslationResult
        {
            public List<GoogleTranslation> translations = new List<GoogleTranslation>();

            public static GoogleTranslationResult FromJSON(string json)
            {
                GoogleTranslationResult result = new GoogleTranslationResult();

                json = json.Replace("\n", "").Replace("\r", "");

                int nextIdx = -1;
                while((nextIdx = json.IndexOf("translatedText")) > 0)
                {
                    int startIdx = nextIdx + 18;
                    int endIdx = json.IndexOf("\"", startIdx);
                    string value = json.Substring(startIdx, endIdx - startIdx);

                    GoogleTranslation translation = new GoogleTranslation();
                    translation.translatedText = value;

                    result.translations.Add(translation);

                    json = json.Substring(endIdx);
                }

                //JsonConvert fails on certain characters.
                //GoogleTranslationResult t = JsonConvert.DeserializeObject<GoogleTranslationResult>(json);
                return result;
            }
        }

        [Serializable]
        public class GoogleTranslation
        {
            [SerializeField]
            public string translatedText;
        }
    }
}
