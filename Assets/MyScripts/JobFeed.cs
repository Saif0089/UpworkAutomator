using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Text;
using UnityEngine.Networking;


public class JobFeed : MonoBehaviour
{
    public TMP_Text FeedInfo;
    public TMP_Text CoverLetter;
    public Button MyButton;
    public string Title;
    public string jobDescription;
    public string openAIApiKey;
    public TMP_Dropdown CoverLetterToCopy;
    public GameObject QuestionPanel;

    public void OpenPopup(){
        QuestionPanel.SetActive(true);
    }
    public void CheckCoverLetterThing()
    {
        StartCoroutine(CallFeed(CoverLetterToCopy.captionText.text));
    }
    private IEnumerator CallFeed(string coverLetterName)
    {
        TextAsset textAsset = (TextAsset)Resources.Load(coverLetterName);
        string modifiedCoverLetter = "";
        //Debug.Log(textAsset.text);
        yield return StartCoroutine(GenerateCoverLetter(jobDescription, textAsset.text, result => modifiedCoverLetter = result));
        //Debug.Log(modifiedCoverLetter);
        
        CoverLetter.text = modifiedCoverLetter;

    }
    private IEnumerator GenerateCoverLetter(string jobDescription, string coverLetterTemplate, System.Action<string> callback)
    {
        var prompt = $"The following is a cover letter for a job application. Please add a few sentences about the job description provided in the cover letter and improve it according to the job post.\n\nJob Description:\n{jobDescription}\n\nCover Letter:\n{coverLetterTemplate}";
        UniClipboard.SetText(prompt);

        // var body = new
        // {
        //     inputs = prompt
        // };

        // string jsonBody = JsonConvert.SerializeObject(body);

        // // Log the JSON body to debug
        // Debug.Log("Request Body: " + jsonBody);

        // using (UnityWebRequest www = new UnityWebRequest("https://api-inference.huggingface.co/models/gpt2", "POST"))
        // {
        //     byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        //     www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        //     www.downloadHandler = new DownloadHandlerBuffer();
        //     www.SetRequestHeader("Content-Type", "application/json");
        //     www.SetRequestHeader("Authorization", $"Bearer {openAIApiKey}");

        //     // Send the request
        //     yield return www.SendWebRequest();

        //     if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        //     {
        //         Debug.LogError("Error: " + www.error);
        //         Debug.LogError("Response: " + www.downloadHandler.text);
        //         callback(null);
        //     }
        //     else
        //     {
        //         Debug.Log("Response: " + www.downloadHandler.text);
        //         var result = JsonConvert.DeserializeObject<List<HuggingFaceResponse>>(www.downloadHandler.text);
        //         if (result != null )
        //         {
        //             callback(result[0].generated_text);
        //         }
        //         else
        //         {
        //             callback(null);
        //         }
        //     }
        // }

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        yield return null;
    }

    private class HuggingFaceResponse
    {
        public string generated_text { get; set; }
    }

    private class OpenAIResponse
    {
        public Choice[] choices { get; set; }
    }

    private class Choice
    {
        public Message message { get; set; }
    }
    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

}
