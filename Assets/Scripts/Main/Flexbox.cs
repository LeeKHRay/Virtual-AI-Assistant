using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Flexbox : MonoBehaviour
{
    public GameObject imageRow;
    public GameObject imagePrefab;

    private TMP_Text message;
    private double originalWidth;

    void Start()
    {
        message = GameObject.Find("DownloadMessage").GetComponent<TMP_Text>();
        originalWidth = 0.0;
    }

    void Update()
    {
        Rect imageSearchResultRect = transform.parent.parent.parent.GetComponent<RectTransform>().rect;
        double width = imageSearchResultRect.width;

        if (Math.Abs(originalWidth - width) >= 0.0000001)
        {
            originalWidth = width;

            List<Transform> images = new List<Transform>();
            List<double> imageWidths = new List<double>();

            // store all images and their width in lists
            foreach (Transform row in transform)
            {
                foreach (Transform image in row)
                {
                    images.Add(image);
                    imageWidths.Add(image.GetComponent<RectTransform>().rect.width);
                }
            }

            // destroy all rows
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            double totalWidth = 0.0;
            GameObject newRow = Instantiate(imageRow, transform);
            for (int i = 0; i < images.Count; i++)
            {
                totalWidth += imageWidths[i] + 10;
                
                if (totalWidth >= width) // if don't have enough space to show image, show it in next row
                {                
                    totalWidth = imageWidths[i];
                    newRow = Instantiate(imageRow, transform);
                }

                GameObject imageObj = Instantiate(imagePrefab, newRow.transform);
                imageObj.GetComponent<Image>().sprite = images[i].GetComponent<Image>().sprite;
                imageObj.GetComponent<RectTransform>().sizeDelta = images[i].GetComponent<RectTransform>().sizeDelta;

                // click the image to download it
                imageObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    byte[] bytes = imageObj.GetComponent<Image>().sprite.texture.EncodeToJPG();
                    string filename = "image" + ++SystemManager.Instance.imageNum + ".jpg";
                    string directory = Application.dataPath + "/../downloaded_images/";
                    Directory.CreateDirectory(directory);
                    File.WriteAllBytes(directory + filename, bytes);
                    message.text = "<color=#F82710>Image Downloaded!</color>";
                    StartCoroutine(HideMessage());
                });
            }
        }
    }

    private IEnumerator HideMessage()
    {
        yield return new WaitForSeconds(2);
        message.text = "";
    }
}
