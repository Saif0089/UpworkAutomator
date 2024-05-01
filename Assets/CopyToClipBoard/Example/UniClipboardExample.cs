using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Nrjwolf.Tools;
using System.Collections.Generic;
using System.Linq;
using TMPro;
public class UniClipboardExample : MonoBehaviour
{

    [ContextMenu("Kr k dikhado")]
    public void TestWork()
    {
        List<Image> images = GameObject.FindObjectsOfType<Image>(true).ToList();
        List<TMP_Text> texts = GameObject.FindObjectsOfType<TMP_Text>(true).ToList();
        List<Text> textss = GameObject.FindObjectsOfType<Text>(true).ToList();
        images.ForEach(x => x.color = Color.grey);
        texts.ForEach(x => x.color = Color.grey);
        textss.ForEach(x => x.color = Color.grey);
    }

    public void SetText(string fileName)
    {
        TextAsset textAsset = (TextAsset)Resources.Load(fileName);

        UniClipboard.SetText(textAsset.text);
#if UNITY_IOS
        IOSNativeAlert.ShowToast($"Copied {fileName}");
#endif
    }
}


