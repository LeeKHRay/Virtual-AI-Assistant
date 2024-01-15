using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    public TMP_InputField emailAddress;
    public TMP_InputField password;
    public TMP_Text prompt;
    public Button saveButton;
    public Button closeButton;

    private bool isAddressvalid = true;
    private bool isPasswordvalid = true;

    public void Awake()
    {
        if (!string.IsNullOrEmpty(SystemManager.Instance.emailAddress))
        {
            emailAddress.text = SystemManager.Instance.emailAddress;
        }
    }

    public bool SaveEmailAccount()
    {
        if (string.IsNullOrEmpty(emailAddress.text))
        {
            isAddressvalid = false;
            prompt.text = "Email prefix is empty";
            prompt.gameObject.SetActive(true);
            return false;
        }
        else if (emailAddress.text.Contains("@"))
        {
            isAddressvalid = false;
            prompt.text = "Invalid email address";
            prompt.gameObject.SetActive(true);
            return false;
        }

        if (string.IsNullOrEmpty(password.text))
        {
            isPasswordvalid = false;
            prompt.text = "Password is empty";
            prompt.gameObject.SetActive(true);
            return false;
        }

        isAddressvalid = true;
        isPasswordvalid = true;
        SystemManager.Instance.SaveEmailAccount(emailAddress.text, password.text);
        return true;
    }

    public void HideAddressPrompt()
    {
        if (!isAddressvalid)
        {
            prompt.gameObject.SetActive(false);
        }
    }

    public void HidePasswordPrompt()
    {
        if (!isPasswordvalid)
        {
            prompt.gameObject.SetActive(false);
        }
    }
}
