using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class SendMessage : MonoBehaviour
{
    public string accountSid = "YOUR_TWILIO_ACCOUNT_SID";
    public string authToken = "YOUR_TWILIO_AUTH_TOKEN";
    public string fromNumber = "whatsapp:+YOUR_TWILIO_NUMBER";
    public string toNumber = "whatsapp:+TARGET_NUMBER";  // This should be input by the user
    public string messageBody = "Hello from Unity!";  // This should be input by the user

    public TMP_InputField SID;
    public TMP_InputField AUTH;
    public TMP_InputField From;
    public TMP_InputField To;
    public MyRSSReader myRSSReader;

    public TMP_Text ErrorShower;

    public string webhookUrl = "https://discord.com/api/webhooks/1234460744435761162/preaFYnwJJnLEUCKAa4ztjzLkZfeP7zTNOHN4zY8CbiQDmAuhc9FihyfBngTViGpQNuk";

    private void Start()
    {
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("SID")))
        {
            accountSid = PlayerPrefs.GetString("SID");
        }
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("AUTH")))
        {
            authToken = PlayerPrefs.GetString("AUTH");
        }
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("FROM")))
        {
            fromNumber = PlayerPrefs.GetString("FROM");
        }
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("TO")))
        {
            toNumber = PlayerPrefs.GetString("TO");
        }
    }
    public void SendWhatsAppMessage()
    {
        StartCoroutine(PostWhatsAppMessage());
    }

    IEnumerator PostWhatsAppMessage()
    {
        string url = "https://api.twilio.com/2010-04-01/Accounts/" + accountSid + "/Messages.json";
        WWWForm form = new WWWForm();
        // try
        // {

        form.AddField("To", toNumber);
        form.AddField("From", fromNumber);
        form.AddField("Body", messageBody);
        // }
        // catch (System.Exception ex)
        // {
        //     ErrorShower.text += "\nException" + ex.Message;

        // }


        ErrorShower.text += "\n" + toNumber;
        ErrorShower.text += "\n" + fromNumber;
        ErrorShower.text += "\n" + messageBody;
        ErrorShower.text += "\n" + accountSid;
        ErrorShower.text += "\n" + authToken;

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.SetRequestHeader("Authorization", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(accountSid + ":" + authToken)));

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Request failed: " + www.error);
            Debug.LogError("Response: " + www.downloadHandler.text);
            ErrorShower.text += "\nError" + www.error;
        }
        else
        {
            Debug.Log("Message sent: " + www.downloadHandler.text);

        }
    }
    public IEnumerator PostToDiscord(string webhook = "")
    {
        // Create the webhook message payload
        WWWForm form = new WWWForm();
        form.AddField("content", messageBody);
        string url = "";
        if (string.IsNullOrEmpty(webhook))
        {
            url = webhookUrl;
        }
        else
        {
            url = webhook;
        }
        // Send the message to Discord channel via webhook
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                Debug.Log("Message sent successfully");
            }
        }
    }
    public void ChangeSID()
    {
        accountSid = SID.text;
        PlayerPrefs.SetString("SID", accountSid);
    }
    public void ChangeAUTH()
    {
        authToken = AUTH.text;
        PlayerPrefs.SetString("AUTH", authToken);
    }
    public void ChangeFrom()
    {
        fromNumber = From.text;
        PlayerPrefs.SetString("FROM", fromNumber);

    }
    public void ChangeTO()
    {
        toNumber = To.text;
        PlayerPrefs.SetString("TO", toNumber);
    }

    public void OpenSettings()
    {
        SID.text = accountSid;
        AUTH.text = authToken;
        From.text = fromNumber;
        To.text = toNumber;
        myRSSReader.refreshTimeIP.text = "Refresh Time: " + myRSSReader.refreshTime.ToString();
    }
}
