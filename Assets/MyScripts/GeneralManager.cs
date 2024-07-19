using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GeneralManager : MonoBehaviour
{
    public MainSearchSection MainSearchSectionPrefab;
    private void Start()
    {
        string[] departmentNames = PlayerPrefs.GetString(StringConstants.namesOfDepartments).Split(',');
        if (string.IsNullOrEmpty(departmentNames[0]))
        {
            return;
        }
        foreach(var item in departmentNames)
        {
            string rssURL = PlayerPrefs.GetString(StringConstants.feedof+item);
            string webHookURL = PlayerPrefs.GetString(StringConstants.webHookURL+item);

            var mainSearch = Instantiate(MainSearchSectionPrefab, MainSearchSectionPrefab.transform.parent);
            mainSearch.gameObject.SetActive(true);
            mainSearch.RSSFeed.text = rssURL;
            mainSearch.WebhookURL.text = webHookURL;
            mainSearch.DepartmentName.text = item;
            mainSearch.Title.text = item;
            mainSearch.RefreshTime.text = PlayerPrefs.GetString(StringConstants.refreshTime + item);
            StartCoroutine(mainSearch.ReadRSS());
        }
    }

    public void AddNew()
    {
        var mainSearch = Instantiate(MainSearchSectionPrefab, MainSearchSectionPrefab.transform.parent);
        mainSearch.gameObject.SetActive(true);
        mainSearch.RSSFeed.text = "";
        mainSearch.WebhookURL.text = "";
        mainSearch.DepartmentName.text = "";
        mainSearch.Title.text = "Set Department Name";
        mainSearch.RefreshTime.text = "10";
    }
}
