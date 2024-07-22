using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GeneralManager : MonoBehaviour
{
    public MainSearchSection MainSearchSectionPrefab;
    public List<MainSearchSection> MainSearchSections = new List<MainSearchSection>();
    private void Start()
    {
        string[] departmentNames = PlayerPrefs.GetString(StringConstants.namesOfDepartments).Split(',');
        if (string.IsNullOrEmpty(departmentNames[0]))
        {
            return;
        }
        foreach(var item in departmentNames)
        {
            if(MainSearchSections.FirstOrDefault(x=>x.DepartmentName.text == item) != null)
            {
                continue;
            }
            string rssURL = PlayerPrefs.GetString(StringConstants.feedof+item);
            string webHookURL = PlayerPrefs.GetString(StringConstants.webHookURL+item);
            string imageURL = PlayerPrefs.GetString(StringConstants.imageURL+item);

            var mainSearch = Instantiate(MainSearchSectionPrefab, MainSearchSectionPrefab.transform.parent);
            mainSearch.gameObject.SetActive(true);
            mainSearch.RSSFeed.text = rssURL;
            mainSearch.WebhookURL.text = webHookURL;
            mainSearch.DepartmentName.text = item;
            mainSearch.ImageURL.text = imageURL;
            mainSearch.Title.text = item;
            mainSearch.RefreshTime.text = PlayerPrefs.GetString(StringConstants.refreshTime + item);
            mainSearch.myRoutine = StartCoroutine(mainSearch.ReadRSS());
            MainSearchSections.Add(mainSearch);
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
        mainSearch.ImageURL.text = "";
    }
    public void Refresh()
   {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("NewRSSImplementation");
   }
}
