using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif
using System.Linq;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using System.Runtime.InteropServices;
using Vuopaja;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;


public class MyRSSReader : MonoBehaviour
{
    public bool allowSendMessages;
    public string rssUrl;
    private List<string> previousTitles = new List<string>();
    public FeedShower feedshowerPefab;
    public List<FeedShower> feedShowers = new List<FeedShower>();
    public string feedOf = "unity";
    public GameObject Popup;

    bool isGenerated;
    bool isBackground;
    int localTime;
    public TMP_Text TimerShower;
    public int refreshTime = 60;

    public SendMessage sendMessage;
    public TMP_InputField refreshTimeIP;

    public TheJobScroll DefaultJobScroll;
    public List<TheJobScroll> theJobScrolls = new List<TheJobScroll>();

    public NewSearchAdder newSearchAdderPrefab;

    public List<NewSearchAdder> newSearchAddersList = new List<NewSearchAdder>();
    public RectTransform searcherContainer;

    private void Awake()
    {
        Application.runInBackground = true;
        DefaultJobScroll.Title.text = feedOf;
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("FEED")))
        {
            feedOf = PlayerPrefs.GetString("FEED");
        }
#if UNITY_IOS
        StartCoroutine(RequestAuthorization());
#endif

    }
    public void ChangeMessageAllowance(bool value)
    {
        allowSendMessages = value;
    }
    public void Start()
    {
        feedShowers.ForEach(x => Destroy(x.gameObject));
        feedShowers.Clear();
        StopAllCoroutines();
        StartCoroutine(ReadRSS(feedOf));
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("FeedsToSearch")))
        {
            string[] feeds = PlayerPrefs.GetString("FeedsToSearch").Split(',');
            foreach (var item in feeds)
            {
                string searchTerm = item.Split('-')[0];
                string webhook = item.Split('-')[1];
                NewSearchAdder newSearchAdder = Instantiate(newSearchAdderPrefab, newSearchAdderPrefab.transform.parent);
                newSearchAdder.SearchTerm.text = searchTerm;
                newSearchAdder.WebhookURL.text = webhook;
                newSearchAddersList.Add(newSearchAdder);
                InstantiateSearchField(searchTerm, webhook);
            }
        }
        // TimerCallback timeCB = new TimerCallback(PrintTime);
        // Timer time = new Timer(timeCB, null, 0, 1000);
    }
    public void MoveRight()
    {
        searcherContainer.DOAnchorPosX(searcherContainer.anchoredPosition.x - DefaultJobScroll.rectTransform.sizeDelta.x, 0.5f, true);
    }

    public void MoveLeft()
    {
        searcherContainer.DOAnchorPosX(searcherContainer.anchoredPosition.x + DefaultJobScroll.rectTransform.sizeDelta.x, 0.5f, true);
    }
    public void InitiateNewSearch()
    {
        NewSearchAdder newSearchAdder = Instantiate(newSearchAdderPrefab, newSearchAdderPrefab.transform.parent);
        newSearchAdder.SearchTerm.interactable = true;
        newSearchAdder.WebhookURL.interactable = true;
        newSearchAdder.SearchTerm.text = string.Empty;
        newSearchAdder.WebhookURL.text = string.Empty;
        newSearchAdder.addButton.interactable = true;
        newSearchAddersList.Add(newSearchAdder);
    }
    public void AddNewSearching(string searchTerm, string webHook)
    {
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("FeedsToSearch")))
        {
            PlayerPrefs.SetString("FeedsToSearch", $"{searchTerm}-{webHook}");
        }
        else
        {
            PlayerPrefs.SetString("FeedsToSearch", PlayerPrefs.GetString("FeedsToSearch") + $",{searchTerm}-{webHook}");
        }
        InstantiateSearchField(searchTerm, webHook);
    }

    private void InstantiateSearchField(string searchTerm, string webHook)
    {
        TheJobScroll newScroll = Instantiate(DefaultJobScroll, DefaultJobScroll.transform.parent);
        newScroll.rectTransform.anchoredPosition += new Vector2(newScroll.rectTransform.rect.width + 20, 0);
        newScroll.Title.text = searchTerm;
        theJobScrolls.Add(newScroll);
        StartCoroutine(ReadRSS(searchTerm, webHook));
    }

    public void ChangeURL(string url)
    {
        url = url.Replace(" ", "+");
        feedOf = url;
        PlayerPrefs.SetString("FEED", feedOf);
    }
    public void ChangeRefreshTime()
    {
        if (int.Parse(refreshTimeIP.text) < 10)
        {
            refreshTimeIP.GetComponent<UnityEngine.UI.Image>().color = Color.red;
            refreshTime = 60;
            return;
        }
        refreshTimeIP.GetComponent<UnityEngine.UI.Image>().color = Color.white;
        refreshTime = int.Parse(refreshTimeIP.text);
    }
    void PrintTime(object state)
    {
        // if (isBackground)
        // {

        // }
        // Debug.LogFormat("Timer {0}", DateTime.Now.ToLongTimeString());
    }
    public void ClearData()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void TestNotification()
    {

        sendMessage.messageBody = $"Tested at {DateTime.Now}";
        // sendMessage.SendWhatsAppMessage();
        StartCoroutine(sendMessage.PostToDiscord());
    }
    IEnumerator ReadRSS(string feedName, string webhook = "")
    {
        isGenerated = false;
        rssUrl = $"https://www.upwork.com/ab/feed/jobs/rss?paging=0%3B10&q={feedName}&sort=recency&api_params=1&securityToken=38f61ddd185c0f1f85c2241a2892c271d57e6690db7ba606a41ba19a3246cd605cb90772bb51acbf881f8dc9ab1dfa69bdf0d4f19b19c987290206cb98297c34&userUid=1472960028498313216&orgUid=1472960028498313217";
        while (true)
        {
            int timer = refreshTime;
            UnityWebRequest request = UnityWebRequest.Get(rssUrl);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                XmlDocument rssXmlDoc = new XmlDocument();
                rssXmlDoc.LoadXml(request.downloadHandler.text);
                Debug.Log(rssXmlDoc.InnerText);
                ParseRssFeed(rssXmlDoc, feedName, webhook);
            }
            else
            {
                Debug.LogError("RSS Feed Error: " + request.error);
            }
            for (int i = 0; i < refreshTime; i++)
            {
                yield return new WaitForSeconds(1); // Check for new feeds every 60 seconds
                TheJobScroll timerOfShower = theJobScrolls.FirstOrDefault(x => x.Title.text == feedName);

                timer -= 1;
                if (timerOfShower != null)
                    timerOfShower.Timer.text = timer.ToString();
                else
                    TimerShower.text = timer.ToString();
            }
            isGenerated = true;
        }
    }
    void ParseRssFeed(XmlDocument xmlDoc, string searchTerm, string webhook = "")
    {
        XmlNodeList itemNodes = xmlDoc.SelectNodes("rss/channel/item");
        foreach (XmlNode itemNode in itemNodes)
        {
            string title = itemNode.SelectSingleNode("title").InnerText;
            string link = itemNode.SelectSingleNode("link").InnerText;
            string description = itemNode.SelectSingleNode("description").InnerText;

            // Extract budget, country, and href link
            string budget = ExtractFromString(description, "<b>Budget</b>:", "<br");
            string hourly = ExtractFromString(description, "<b>Hourly Range</b>:", "<br");
            string country = ExtractFromString(description, "<b>Country</b>:", "<br");
            string postedOn = ExtractFromString(description, "<b>Posted On</b>:", "<br");
            string hrefLink = ExtractHref(description);

            // Debug.Log($"Title: {title}\nLink: {link}\nBudget: {budget}\nCountry: {country}\nHref Link: {hrefLink}\n");

            // Create a button or other UI element for each item
            CreateButtonForFeedItem(title, link, budget, hourly, country, hrefLink, postedOn, searchTerm, webhook);
        }
        // isGenerated = true;
    }
    string ExtractFromString(string text, string startString, string endString)
    {
        int startIndex = text.IndexOf(startString) + startString.Length;
        int endIndex = text.IndexOf(endString, startIndex);
        return text.Substring(startIndex, endIndex - startIndex).Trim();
    }
    string ExtractHref(string text)
    {
        Match match = Regex.Match(text, "href=\"[^\"]*\"");
        if (match.Success)
        {
            return match.Value.Substring(6).Trim('"');
        }
        return string.Empty;
    }
    void CreateButtonForFeedItem(string title, string url, string budget, string hourly, string country, string hrefLink, string postedOn, string searchTerm, string webhook)
    {
        var alreadyPresent = feedShowers.FirstOrDefault(x => x.Title.text == title && x.DatePosted == postedOn);
        if (alreadyPresent != null)
        {
            return;
        }
        FeedShower feedShower = Instantiate(feedshowerPefab, feedshowerPefab.transform.parent);
        TheJobScroll searchListForThisOne = theJobScrolls.FirstOrDefault(x => x.Title.text.Equals(searchTerm));
        if (searchListForThisOne != null)
        {
            feedShower.transform.parent = searchListForThisOne.Content;
        }
        feedShower.DatePosted = postedOn;
        feedShower.gameObject.SetActive(true);
        feedShower.Title.text = title;
        DateTime utcTime = DateTime.ParseExact(postedOn, "MMMM dd, yyyy HH:mm 'UTC'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        DateTime localTime = utcTime.ToLocalTime();
        string hourlyText = "";
        string CountryText = "";
        string timeText = "";
        if (!budget.Contains("$"))
        {
            hourlyText = hourly;
            CountryText = country;
            timeText = localTime.ToString("");
            // TriggerNotification(title, $"{hourly}, {country}, {localTime}");
            feedShower.Price.text = hourly;
        }
        else
        {
            hourlyText = budget;
            CountryText = country;
            timeText = localTime.ToString("");
            // TriggerNotification(title, $"{budget}, {country}, {localTime}");
            feedShower.Price.text = budget;
        }

        feedShower.PostedOn.text = FormatDifference(TimeDifference(postedOn));
        feedShower.Location.text = country;
        string chromeLink = hrefLink.Replace("https", "googlechrome");
        string operLink = hrefLink.Replace("https", "touch-https");
        string firefoxLink = hrefLink.Replace("https", "firefox");
        feedShower.MyButton.onClick.AddListener(() =>
        {
            Popup.SetActive(true);
            feedShower.ChromeButton.onClick.RemoveAllListeners();
            feedShower.OperaButton.onClick.RemoveAllListeners();
            feedShower.FirefoxButton.onClick.RemoveAllListeners();
            feedShower.ChromeButton.onClick.AddListener(() => { Application.OpenURL(@chromeLink); Popup.SetActive(false); });
            feedShower.OperaButton.onClick.AddListener(() => { Application.OpenURL(operLink); Popup.SetActive(false); });
            feedShower.FirefoxButton.onClick.AddListener(() => { Application.OpenURL(@firefoxLink); Popup.SetActive(false); });
        });


        if (isGenerated)
        {
#if UNITY_IOS
            TriggerNotification(title, $"{hourlyText}, {CountryText}, {timeText}");
#endif
            GetComponent<AudioSource>().Play();
#if UNITY_IOS
            Handheld.Vibrate();
#endif
            feedShower.transform.SetAsFirstSibling();
            // if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
            // {
            if (allowSendMessages)
            {
                sendMessage.messageBody = $"Found {searchTerm}:\n \n {title}\n \n{hourlyText}\n \n{CountryText}\n \n{timeText}\n \n{hrefLink}";
                // sendMessage.SendWhatsAppMessage();
                StartCoroutine(sendMessage.PostToDiscord(webhook));
            }
            // }
        }
        feedShowers.Add(feedShower);
    }
#if UNITY_IOS
    IEnumerator RequestAuthorization()
    {
        var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
        using (var req = new AuthorizationRequest(authorizationOption, true))
        {
            while (!req.IsFinished)
            {
                yield return null;
            };

            string res = "\n RequestAuthorization:";
            res += "\n finished: " + req.IsFinished;
            res += "\n granted :  " + req.Granted;
            res += "\n error:  " + req.Error;
            res += "\n deviceToken:  " + req.DeviceToken;
            Debug.Log("asdfasdf" + res);
        }
    }
    void TriggerNotification(string title, string body)
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(0, 0, 1),
            Repeats = false
        };
        var notification = new iOSNotification()
        {
            Title = title,
            Body = body,
            ShowInForeground = true, // Set to false if you want to show only when the app is backgrounded
            ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
            CategoryIdentifier = "category_a",
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }
#endif
    public TimeSpan TimeDifference(string utcTimeString)
    {
        DateTime utcTime = DateTime.ParseExact(utcTimeString, "MMMM dd, yyyy HH:mm 'UTC'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        DateTime localTime = utcTime.ToLocalTime();
        // Get the current local time
        DateTime currentTime = DateTime.Now;

        // Calculate the difference
        TimeSpan difference = currentTime - localTime;

        return difference;
    }

    string FormatDifference(TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays} day(s) ago";
        else if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours} hour(s) ago";
        else if (timeSpan.TotalMinutes >= 1)
            return $"{(int)timeSpan.TotalMinutes} minute(s) ago";
        else if (timeSpan.TotalSeconds >= 1)
            return $"{(int)timeSpan.TotalSeconds} second(s) ago";
        else
            return "just now";
    }

    async Task ReadRSST()
    {
        isGenerated = false;
        rssUrl = $"https://www.upwork.com/ab/feed/jobs/rss?paging=0%3B10&q={feedOf}&sort=recency&api_params=1&securityToken=38f61ddd185c0f1f85c2241a2892c271d57e6690db7ba606a41ba19a3246cd605cb90772bb51acbf881f8dc9ab1dfa69bdf0d4f19b19c987290206cb98297c34&userUid=1472960028498313216&orgUid=1472960028498313217";
        // while (true)
        // {
        Debug.LogFormat("Sending Request in background");
        UnityWebRequest request = UnityWebRequest.Get(rssUrl);
        AsyncOperation asyncOperation = request.SendWebRequest();
        while (!asyncOperation.isDone)
        {
            Debug.LogFormat("Waiting for call respponse");
            await Task.Delay(10);
        }
#if UNITY_IOS
        TriggerNotification("title", $"{"budget"}, {"country"}, {localTime}");
#endif
        if (request.result == UnityWebRequest.Result.Success)
        {
            XmlDocument rssXmlDoc = new XmlDocument();
            rssXmlDoc.LoadXml(request.downloadHandler.text);
            Debug.LogFormat(rssXmlDoc.InnerText);
            ParseRssFeedT(rssXmlDoc);
            // ParseRssFeedT(rssXmlDoc);
        }
        else
        {
            Debug.LogError("RSS Feed Error: " + request.error);
        }

        // await Task.Delay(60000);
        // }
    }
    void ParseRssFeedT(XmlDocument xmlDoc)
    {
        XmlNodeList itemNodes = xmlDoc.SelectNodes("rss/channel/item");

        foreach (XmlNode itemNode in itemNodes)
        {
            string title = itemNode.SelectSingleNode("title").InnerText;
            string link = itemNode.SelectSingleNode("link").InnerText;
            string description = itemNode.SelectSingleNode("description").InnerText;

            // Extract budget, country, and href link
            string budget = ExtractFromString(description, "<b>Budget</b>:", "<br");
            string hourly = ExtractFromString(description, "<b>Hourly Range</b>:", "<br");
            string country = ExtractFromString(description, "<b>Country</b>:", "<br");
            string postedOn = ExtractFromString(description, "<b>Posted On</b>:", "<br");
            string hrefLink = ExtractHref(description);

            // Debug.Log($"Title: {title}\nLink: {link}\nBudget: {budget}\nCountry: {country}\nHref Link: {hrefLink}\n");
            DateTime utcTime = DateTime.ParseExact(postedOn, "MMMM dd, yyyy HH:mm 'UTC'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            DateTime localTime = utcTime.ToLocalTime();
            // Create a button or other UI element for each item
#if UNITY_IOS
            TriggerNotification(title, $"{budget}, {country}, {localTime}");
#endif
        }
        isGenerated = true;
    }
    Task task, task2;
    public int currentTaskID = -1;
    void OnApplicationPause(bool focusStatus)
    {
        // if (focusStatus)
        // {
        //     isBackground = false;
        //     Background.StopTask();
        // }
        // else
        // {
        //     isBackground = true;
        //     localTime = 0;
        //     Background.StartTask();
        //     // task = Task.Run(() => ReadRSST());

        // }

        // if (focusStatus)
        // {
        //     localTime = 0;
        //     // Start a task that invokes once every second (1000 milliseconds)
        //     currentTaskID = TaskInvoker.StartTask(1000, onInvoke, onExpire);

        // }
        // else if (currentTaskID != -1)
        // {
        //     // Stop the running task when entering foreground
        //     TaskInvoker.StopTask(currentTaskID);
        // }
    }

    private void onExpire(int taskId)
    {
        TaskInvoker.StopTask(taskId);
        Debug.Log("Task Expired");
        currentTaskID = TaskInvoker.StartTask(1000, onInvoke, onExpire);
    }

    public void onInvoke(int taskId)
    {
        localTime += 1;
        Debug.LogFormat("Counting: " + localTime);
        if (localTime % 20 == 0)
        {
            Debug.LogFormat("CallingAPI");
            _ = ReadRSST();
        }
    }
}
