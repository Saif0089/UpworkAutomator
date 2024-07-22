using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Xml;
using UnityEngine.Networking;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class MainSearchSection : MonoBehaviour
{
    RectTransform MyRect;
    public RectTransform MenuButton;
    public RectTransform SideBar;

    public TMP_InputField DepartmentName;
    public TMP_InputField RSSFeed;
    public TMP_InputField WebhookURL;
    public TMP_InputField RefreshTime;
    public TMP_InputField ImageURL;

    public TMP_Text Title;
    public TMP_Text TimerShower;

    public JobFeed JobFeedPrefab;
    List<JobFeed> jobFeeds = new List<JobFeed>();
    bool isGenerated;
    
    
    [ContextMenu("Resize Me")]
    public void Resize()
    {
        MyRect = GetComponent<RectTransform>();
        MyRect.sizeDelta = new Vector2(Screen.width +107, MyRect.sizeDelta.y);
    }

    public void OpenSideBar(bool value)
    {
        if (value)
        {
            MenuButton.DORotate(new Vector3(0, 0, -90), 0.2f);
            SideBar.DOScale(new Vector3(1, 1, 1),0.2f);
        }
        else
        {
            MenuButton.DORotate(new Vector3(0, 0, -0), 0.2f);
            SideBar.DOScale(new Vector3(0, 1, 1), 0.2f);
        }
    }
    public Coroutine myRoutine;
    public void SetFeed()
    {
        gameObject.name = DepartmentName.text;
        Title.text = DepartmentName.text;
        SetPlayerPrefName();
        SetRSSFeed();
        SetWebHookURL();
        SetRefreshTime();
        SetImageURL();
        if(myRoutine == null)
        myRoutine = StartCoroutine(ReadRSS());
    }
    
    public void SetImageURL()
    {
        PlayerPrefs.SetString(StringConstants.imageURL + DepartmentName.text, ImageURL.text);
    }
    public void SetPlayerPrefName()
    {
        if (string.IsNullOrEmpty(PlayerPrefs.GetString(StringConstants.namesOfDepartments)))
        {
            PlayerPrefs.SetString(StringConstants.namesOfDepartments, DepartmentName.text);
        }
        else
        {
            if(PlayerPrefs.GetString(StringConstants.namesOfDepartments).Contains(DepartmentName.text))
            {
                return;
            }
            PlayerPrefs.SetString(StringConstants.namesOfDepartments, PlayerPrefs.GetString(StringConstants.namesOfDepartments) + "," + DepartmentName.text);
        }
    }
    public void SetRSSFeed()
    {
        PlayerPrefs.SetString(StringConstants.feedof + DepartmentName.text, RSSFeed.text);
    }
    public void SetWebHookURL()
    {
        PlayerPrefs.SetString(StringConstants.webHookURL + DepartmentName.text, WebhookURL.text);
    }
    public void SetRefreshTime()
    {
        PlayerPrefs.SetString(StringConstants.refreshTime + DepartmentName.text, RefreshTime.text);
    }

    public IEnumerator ReadRSS()
    {
        isGenerated = false;
        while (true)
        {
            int timer = int.Parse(RefreshTime.text);
            UnityWebRequest request = UnityWebRequest.Get(RSSFeed.text);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                RSSFeedParser parser = new RSSFeedParser();
                Debug.Log(request.downloadHandler.text);
                parser.Parse(request.downloadHandler.text);
                foreach (var item in parser.Items)
                {
                    if (jobFeeds.FirstOrDefault(x => x.Title == item.Title) == null)
                    {
                        JobFeed jobFeed = Instantiate(JobFeedPrefab, JobFeedPrefab.transform.parent);
                        jobFeed.gameObject.SetActive(true);
                        jobFeed.jobDescription = FormatHtmlToUnityText(item.Description, out string skillsss, out string hourlyRangess, out string budgetss, out string applyLinkss, out string countryss, out string postedOnss);
                        jobFeed.FeedInfo.text = $"\n<color=#00ff00ff><b>{item.Title}</b></color>\n\n<color=#c0c0c0ff>{FormatHtmlToUnityText(item.Description, out string skills, out string hourlyRange,out string budget, out string applyLink,out string country,out string postedOn)}</color>" +
                            $"\n<b>HourlyRange:</b> \n{hourlyRange}\n\n<b>Budget:</b>\n{budget}\n\n<b>Country:</b> \n{item.Country}\n\n<b>Posted On:</b> \n{item.PostedOn}\n\n";
                        jobFeed.Title = item.Title;
                        jobFeeds.Add(jobFeed);
                        jobFeed.MyButton.onClick.AddListener(() => Application.OpenURL(applyLink));
                       
                        if (isGenerated)
                        {
                            jobFeed.transform.SetAsFirstSibling();
                            string description = FormatHtmlToUnityText(item.Description, out string skillss, out string hourlyRanges, out string budgets, out string applyLinks, out string countrys, out string postedOns);
                            string message = FormatMessage(item.Title, description, skills, budget, applyLink, item.Country, item.PostedOn);
                            if (string.IsNullOrEmpty(hourlyRange))
                                StartCoroutine(PostToDiscord(item.Title, "", skills, budget, applyLink, item.Country, item.PostedOn));
                            else
                                StartCoroutine(PostToDiscord(item.Title, "", skills, "Hourly: " + hourlyRange, applyLink, item.Country, item.PostedOn));

                        }
                        
                    }
                    LayoutRebuilder.ForceRebuildLayoutImmediate(JobFeedPrefab.transform.parent.GetComponent<RectTransform>());
                }
                isGenerated = true;
            }
            else
            {
                Debug.LogError("RSS Feed Error: " + request.error);
            }
            for (int i = 0; i < int.Parse(RefreshTime.text); i++)
            {
                yield return new WaitForSeconds(1); // Check for new feeds every 60 seconds
                timer -= 1;
                TimerShower.text = timer.ToString();
            }
            
        }
    }
    string FormatHtmlToUnityText(string htmlString, out string skills, out string hourlyRange,out string budget, out string applyLink, out string country, out string postedOn)
    {
        // Extract and remove the "Skills" section
        var skillsPattern = @"<b>Skills</b>:(.*?)<br />";
        skills = ExtractAndRemove(ref htmlString, skillsPattern).Trim();

        // Extract and remove the "Hourly Range" section
        var hourlyRangePattern = @"<b>Hourly Range</b>:\s*(.*?)<br />";
        hourlyRange = ExtractAndRemove(ref htmlString, hourlyRangePattern).Trim();

        var budgetPattern = @"<b>Budget</b>:\s*(.*?)<br />";
        budget = ExtractAndRemove(ref htmlString, budgetPattern).Trim();

        var countryPattern = @"<b>Country</b>:\s*(.*?)<br />";
        country = ExtractAndRemove(ref htmlString, countryPattern).Trim();

        var postedOnPattern = @"<b>Posted On</b>:\s*(.*?)<br />";
        postedOn = ExtractAndRemove(ref htmlString, postedOnPattern).Trim();

        // Extract and remove the "Apply Link"
        var applyLinkPattern = @"<a href=""(.*?)"">click to apply</a>";
        applyLink = ExtractAndRemove(ref htmlString, applyLinkPattern).Trim();

        // Replace line breaks
        htmlString = htmlString.Replace("<br />", "\n");

        // Replace bold tags
        htmlString = htmlString.Replace("<b>", "<b>");
        htmlString = htmlString.Replace("</b>", "</b>");

        // Remove remaining HTML tags
        htmlString = Regex.Replace(htmlString, "<.*?>", string.Empty);

        return htmlString;
    }
    private string ExtractAndRemove(ref string input, string pattern)
    {
        var match = Regex.Match(input, pattern, RegexOptions.Singleline);
        if (match.Success)
        {
            input = input.Replace(match.Value, string.Empty);
            return match.Groups[1].Value;
        }
        return string.Empty;
    }
    [ContextMenu("Test")]
    public void SendTestMessage(){
        StartCoroutine(PostToDiscord("test", "test", "Test", "Test", "Test", "Test", "Test"));
    }
    public IEnumerator PostToDiscord(string title, string description, string skills, string budget, string applyLink,string country, string postedOn)
    {
        // Construct the JSON payload
        var embed = new
        {
            thumbnail = new { url = ImageURL.text },
            title = title,
            description = description,
            color = 5814783, // A nice color to match the theme
            fields = new[]
            {
                new { name = "Skills Required", value = skills.Replace(", ", "\n- ") },
                new { name = "Budget", value = budget },
                new { name = "Country", value = country },
                new { name = "Apply Link", value = $"[Click here to apply]({applyLink})" }
            },
            footer = new { text = $"Posted on: {postedOn}" }
            
        };

        var payload = new
        {
            content = "", // A line before the message starts
            embeds = new[] { embed }
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);
        Debug.Log(jsonPayload);
        // Create the request
        using (UnityWebRequest www = new UnityWebRequest(WebhookURL.text, "POST"))
        {
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            // Send the request
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
    public string FormatMessage(string title, string description, string skills, string budget, string applyLink,string country, string postedOn)
    {
        var sb = new StringBuilder();
        sb.AppendLine("---");  // Draw a line before the message starts
        sb.AppendLine();
        sb.AppendLine($"**{title}**");
        //sb.AppendLine();
        //sb.AppendLine(description);
        sb.AppendLine();
        sb.AppendLine("**Skills Required:**");
        sb.AppendLine(skills.Replace(", ", "\n- "));
        sb.AppendLine();
        sb.AppendLine("**Budget:**");
        sb.AppendLine(budget);
        sb.AppendLine();
        sb.AppendLine($"**[Apply Here]({applyLink})**");
        sb.AppendLine();
        sb.AppendLine($"*Posted on: {postedOn}*");
        sb.AppendLine();
        sb.AppendLine("**Country:**");
        sb.AppendLine(country);
        sb.AppendLine();
        sb.AppendLine("**Posted On:**");
        sb.AppendLine(postedOn);

        return sb.ToString();
    }
   
}
