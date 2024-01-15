using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using SFB;
using System.Text.RegularExpressions;

public class EmailForm : MonoBehaviour
{
    public TMP_InputField to;
    public TMP_InputField subject;
    public TMP_InputField message;
    public GameObject attachmentScrollView;
    public Button sendButton;
    public Button closeButton;
    public TMP_Text prompt;
    public GameObject attachmentPrefab;

    private List<string> attachmentsPath;
    private List<string> attachmentsName;
    private Transform attachmentsWindow;

    private string[] openTags;
    private string[] closeTags;

    void Start()
    {
        openTags = new string[] { "<b>", "<i>", "<u>" };
        closeTags = new string[] { "</b>", "</i>", "</u>" };

        attachmentsPath = new List<string>();
        attachmentsName = new List<string>();
        attachmentsWindow = attachmentScrollView.transform.GetChild(0).GetChild(0);
    }

    public bool Send()
    {
        // check if email address is valid
        if (string.IsNullOrEmpty(to.text))
        {
            prompt.text = "Email address is empty";
            prompt.gameObject.SetActive(true);
            return false;
        }
        else if (!to.text.Contains("@"))
        {
            prompt.text = "Invalid email address";
            prompt.gameObject.SetActive(true);
            return false;
        }
        else
        {
            string[] tokens = to.text.Split('@');
            if (tokens.Length != 2 || string.IsNullOrEmpty(tokens[0]) || string.IsNullOrEmpty(tokens[1]))
            {
                prompt.text = "Invalid email address";
                prompt.gameObject.SetActive(true);
            }
            else
            {
                EmailUtils.SendEmail(to.text, subject.text, message.text.Replace("\n", "<br>"), attachmentsPath);
            }
            return true;
        }
    }

    public void Attach()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select File(s)", "", "", true); // paths of files selected in file browser
        if (paths.Length > 0)
        {
            foreach (string path in paths)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    attachmentsPath.Add(path);
                    string filename = path.Substring(path.LastIndexOf('\\') + 1);
                    attachmentsName.Add(filename);
                    AddAttachment(filename);
                }
            }
        }
    }

    private void AddAttachment(string filename)
    {
        if (attachmentsWindow.childCount == 0)
        {
            attachmentScrollView.SetActive(true);
        }

        // show attachment in attachment window
        GameObject attachmentObj = Instantiate(attachmentPrefab, attachmentsWindow.transform);
        AttachmentItem attachmentItem = attachmentObj.GetComponent<AttachmentItem>();

        attachmentItem.attachmentName.text = filename;
        attachmentItem.cancelButton.onClick.AddListener(() => { // click button to cancel the file
            int idx = attachmentsName.IndexOf(attachmentItem.attachmentName.text);
            attachmentsPath.RemoveAt(idx);
            attachmentsName.RemoveAt(idx); 
            if (attachmentsWindow.childCount == 1)
            {
                attachmentScrollView.SetActive(false);
            }
            Destroy(attachmentObj);
        });
    }

    public void HidePrompt()
    {
        prompt.gameObject.SetActive(false);
    }

    public void Bold()
    {
        AddTag("b");
    }
    
    public void Italic()
    {
        AddTag("i");
    }

    public void Underline()
    {
        AddTag("u");
    }

    private void AddTag(string tag)
    {
        if (message.selectionStringFocusPosition == message.selectionStringAnchorPosition)
        {
            return;
        }

        string openTag = "<" + tag + ">";
        string closeTag = "</" + tag + ">";

        int startPos;
        int endPos;

        if (message.selectionStringFocusPosition < message.selectionStringAnchorPosition)
        {
            startPos = message.selectionStringFocusPosition;
            endPos = message.selectionStringAnchorPosition;
        }
        else
        {
            startPos = message.selectionStringAnchorPosition;
            endPos = message.selectionStringFocusPosition;
        }

        // split message into leftStr, str and rightStr
        string str = message.text.Substring(startPos, endPos - startPos);
        string leftStr = message.text.Substring(0, startPos);
        string rightStr = message.text.Substring(endPos);

        // sometime may highlighting the existing tags
        Regex regex = new Regex(@"^(<.>|<\/.>)+", RegexOptions.Singleline); 
        MatchCollection matches = regex.Matches(str); // find the tags at the beginning of str
        if (matches.Count > 0) // move the tags to leftStr
        {
            leftStr += matches[0].Value;
            str = str.Substring(matches[0].Index + matches[0].Length);
        }
        regex = new Regex(@"(<.>|<\/.>)+$", RegexOptions.Singleline);
        matches = regex.Matches(str); // find the tags at the end of str
        if (matches.Count > 0) // move the tags to rightStr
        {
            rightStr = matches[0].Value + rightStr;
            str = str.Substring(0, str.Length - matches[0].Length);
        }

        if (str.Contains(openTag) && str.Contains(closeTag))
        {
            List<int> openTagIdx = IndexOfAllSubstring(str, openTag);
            List<int> closeTagIdx = IndexOfAllSubstring(str, closeTag);
            int firstOpenTagIdx = openTagIdx[0];
            int lastOpenTagIdx = openTagIdx[openTagIdx.Count - 1];
            int firstCloseTagIdx = closeTagIdx[0];
            int lastCloseTagIdx = closeTagIdx[closeTagIdx.Count - 1];

            str = str.Replace(openTag, "").Replace(closeTag, "");
            if (firstOpenTagIdx < firstCloseTagIdx && lastOpenTagIdx > lastCloseTagIdx) // a<i>b</i>c<i>d...</i> => <i>abcd...</i>
            {
                message.text = Concat(leftStr, openTag + str, rightStr);
            }
            else if (firstCloseTagIdx < firstOpenTagIdx && lastCloseTagIdx > lastOpenTagIdx) // <i>...a</i>b<i>c</i>d => <i>...</i>abcd
            {
                message.text = Concat(leftStr, closeTag + str, rightStr);
            }
            else if (firstOpenTagIdx < lastCloseTagIdx) // a<i>b</i>c => <i>abc</i>
            {
                message.text = Concat(leftStr, openTag + str + closeTag, rightStr);
            }
            else // <i>...a</i>b<i>c...</i> => <i>...</i>abc<i>...</i>
            {
                message.text = Concat(leftStr, closeTag + str + openTag, rightStr);
            }

        }
        else if (str.Contains(openTag)) // a<i>b...</i> => ab<i>...</i>
        {
            str = str.Replace(openTag, "");
            message.text = Concat(leftStr, openTag + str, rightStr);
        }
        else if (str.Contains(closeTag)) // <i>...a</i>b => <i>...</i>ab
        {
            str = str.Replace(closeTag, "");
            message.text = Concat(leftStr, closeTag + str, rightStr);
        }
        else // ...abc...
        {
            if (!leftStr.Contains(openTag) && !rightStr.Contains(openTag))
            {
                message.text = Concat(leftStr, openTag + str + closeTag, rightStr);
            }
            else
            {
                bool hasLeftOpenTag = false;
                bool hasRightOpenTag = false;
                bool hasLeftCloseTag = false;
                bool hasRightCloseTag = false;

                regex = new Regex(@"(<.>|<\/.>)+$", RegexOptions.Singleline);
                matches = regex.Matches(leftStr);
                if (matches.Count > 0)
                {
                    if (matches[0].Value.Contains(openTag))
                    {
                        hasLeftOpenTag = true;
                    }
                    if (matches[0].Value.Contains(closeTag))
                    {
                        hasLeftCloseTag = true;
                    }
                }
                regex = new Regex(@"^(<.>|<\/.>)+", RegexOptions.Singleline);
                matches = regex.Matches(rightStr);
                if (matches.Count > 0)
                {
                    if (matches[0].Value.Contains(openTag))
                    {
                        hasRightOpenTag = true;
                    }
                    if (matches[0].Value.Contains(closeTag))
                    {
                        hasRightCloseTag = true;
                    }
                }

                if (hasLeftOpenTag)
                {
                    leftStr = ReplaceAt(leftStr, leftStr.LastIndexOf(openTag), openTag.Length, "");
                }
                if (hasRightOpenTag)
                {
                    rightStr = ReplaceAt(rightStr, rightStr.IndexOf(openTag), openTag.Length, "");
                }
                if (hasLeftCloseTag)
                {
                    leftStr = ReplaceAt(leftStr, leftStr.LastIndexOf(closeTag), closeTag.Length, "");
                }
                if (hasRightCloseTag)
                {
                    rightStr = ReplaceAt(rightStr, rightStr.IndexOf(closeTag), closeTag.Length, "");
                }

                if (!hasLeftOpenTag && !hasRightCloseTag && !hasLeftCloseTag && !hasRightOpenTag)
                {
                    if (leftStr.Contains(openTag))
                    {
                        if (leftStr.Contains(closeTag))
                        {
                            if (leftStr.LastIndexOf(openTag) > leftStr.LastIndexOf(closeTag)) // <i>...</i>...<i>...abc...</i> => <i>...</i>...<i>...</i>abc<i>...</i>
                            {
                                str = Concat(closeTag, str, openTag);
                            }
                            else // <i>...</i>...abc... => <i>...</i>...<i>abc</i>...
                            {
                                str = Concat(openTag, str, closeTag);
                            }
                        }
                        else // <i>...abc...</i> => <i>...</i>abc<i>...</i>
                        {
                            str = Concat(closeTag, str, openTag);
                        }
                    }
                    else // ...abc...<i>...</i> => ...<i>abc</i>...<i>...</i>
                    {
                        str = Concat(openTag, str, closeTag);
                    }
                }
                else if (!(hasLeftCloseTag && hasRightOpenTag) && !(hasLeftOpenTag && hasRightCloseTag)) // not <i>...</i>abc<i>...</i> and <i>abc</i>
                {
                    if (hasLeftOpenTag) // ...<i>abc...</i> => ...abc<i>...</i>
                    {
                        str = str + openTag;
                    }
                    if (hasRightOpenTag) // </i>...abc<i>... => </i>...<i>abc...
                    {
                        str = openTag + str;
                    }
                    if (hasLeftCloseTag) // ...</i>abc...<i> => ...abc</i>...<i>
                    {
                        str = str + closeTag;
                    }
                    if (hasRightCloseTag) // <i>...abc</i>... => <i>...</i>abc...
                    {
                        str = closeTag + str;
                    }
                }

                message.text = Concat(leftStr, str, rightStr);
            }
        }

        NestedTags(); // make sure the tags are nested in the correct order

        // remove redundant tags
        for (int i = 0; i < openTags.Length; i++)
        {
            message.text = message.text.Replace(openTags[i] + openTags[i], openTags[i])
                                       .Replace(closeTags[i] + closeTags[i], closeTags[i])
                                       .Replace(openTags[i] + closeTags[i], "")
                                       .Replace(closeTags[i] + openTags[i], "");
        }

        message.selectionStringAnchorPosition = endPos;
    }

    private void NestedTags()
    {
        Stack<string> tagsStack = new Stack<string>();
        bool findTag;
        int tagLength = 1;

        for (int i = 0; message.text.Length - i >= closeTags[0].Length; i += findTag ? tagLength : 1)
        {
            findTag = false;
            foreach (string tag in openTags)
            {
                if (message.text.Substring(i, tag.Length) == tag) // if encounter opening tag, push it into the stack 
                {
                    tagsStack.Push(tag);
                    tagLength = tag.Length;
                    findTag = true;
                    break;
                }
            }
            if (!findTag && message.text.Length - i >= closeTags[0].Length)
            {
                foreach (string tag in closeTags)
                {
                    if (message.text.Substring(i, tag.Length) == tag)
                    {
                        if (MatchTags(tag, tagsStack.Peek())) // <i> <b> </b> </i>
                        {
                            tagLength = tag.Length;
                        }
                        else // <i> <b> </i> </b> => <i> <b> </b></i><b> </b>
                        {
                            string openTag = tagsStack.Peek();
                            message.text = ReplaceAt(message.text, i, tag.Length, ToCloseTag(openTag) + tag + openTag);
                            tagLength = openTag.Length;
                        }
                        tagsStack.Pop();
                        findTag = true;
                        break;
                    }
                }
            }
        }
    }

    private bool MatchTags(string openTag, string closeTag)
    {
        return openTag[openTag.Length - 2] == closeTag[closeTag.Length - 2];
    }
    
    private string ToCloseTag(string tag)
    {
        return tag.Insert(1, "/");
    }

    private string ReplaceAt(string str, int index, int length, string replace)
    {
        StringBuilder s = new StringBuilder(str);
        return s.Remove(index, Mathf.Min(length, message.text.Length - index)).Insert(index, replace).ToString();
    }

    private string Concat(string leftStr, string midStr, string rightStr)
    {
        return leftStr + midStr + rightStr;
    }

    private List<int> IndexOfAllSubstring(string str, string substr)
    {
        List<int> indexes = new List<int>();
        for (int index = 0; ; index += substr.Length)
        {
            index = str.IndexOf(substr, index);
            if (index == -1)
            {
                return indexes;
            }
            indexes.Add(index);
        }
    }
}
