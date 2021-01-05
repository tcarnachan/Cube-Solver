using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System;
using System.Linq;
using System.Collections;
using System.Security.Cryptography;

public class LoginSystem : MonoBehaviour
{
    public Selectable[] uiElems;
    private EventSystem system;

    private InputField nameField, passwordField;
    private Button loginButton, registerButton;
    public Text errorText;

    private const int saltSize = 16, hashSize = 20;
    private const int iterations = 10000;

    enum UIField { NameField, PasswordField, LoginButton, RegisterButton };

    private void Start()
    {
        uiElems[0].Select();
        system = EventSystem.current;

        nameField = (InputField)uiElems[(int)UIField.NameField];
        passwordField = (InputField)uiElems[(int)UIField.PasswordField];
        loginButton = (Button)uiElems[(int)UIField.LoginButton];
        registerButton = (Button)uiElems[(int)UIField.RegisterButton];

        DBManager.StartServer();
    }

    private void OnDestroy()
    {
        DBManager.StopServer();
    }

    void Update()
    {
        int currSelected;
        try
        {
            currSelected = Array.IndexOf(uiElems, system.currentSelectedGameObject.GetComponent<Selectable>());
        }
        catch
        {
            return; // Nothing is currently selected
        }

        // Tab to next element, Shift+Tab to previous element
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            do
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    currSelected--;
                    if (currSelected < 0) currSelected += uiElems.Length;
                }
                else
                    currSelected = (currSelected + 1) % uiElems.Length;
            } while (!uiElems[currSelected].interactable); // Only select an interactable element
            uiElems[currSelected].Select();
        }

        // Enter to submit form if input field selected
        if (Input.GetKeyDown(KeyCode.Return) && (currSelected == (int)UIField.NameField || currSelected == (int)UIField.PasswordField))
            loginButton.onClick.Invoke();
    }

    public void VerifyInputs()
    {
        string usr = nameField.text;
        string pwd = passwordField.text;
        // Username must not be empty, password has to be 8 characters or longer
        loginButton.interactable = registerButton.interactable = (usr.Length >= 1 && pwd.Length >= 8);
    }

    public void CallLogin() => StartCoroutine(Login());

    public void CallRegister() => StartCoroutine(Register());

    private IEnumerator Register()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", nameField.text);
        form.AddField("password", HashPassword(passwordField.text));

        WWW www = new WWW("http://localhost:8888/sqlconnect/register.php", form);
        yield return www;
        if (www.text == "0")
            DBManager.LogIn(nameField.text);
        else
            errorText.text = $"Error in creating account: {www.text}";
    }

    private IEnumerator Login()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", nameField.text);

        WWW www = new WWW("http://localhost:8888/sqlconnect/login.php", form);
        yield return www;
        if (www.text[0] == '0')
        {
            string password = www.text.Substring(1);
            if (VerifyPassword(passwordField.text, password))
                DBManager.LogIn(nameField.text);
            else
                errorText.text = "Invalid password";
        }
        else
            errorText.text = $"Error in loging in: {www.text}";
    }

    private string HashPassword(string password)
    {
        // Create the salt
        byte[] salt = new byte[saltSize];
        new RNGCryptoServiceProvider().GetBytes(salt);
        // Create Rfc2898DeriveBytes and get the hashed value
        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
        byte[] hash = pbkdf2.GetBytes(hashSize);
        // Return the password hash
        return Convert.ToBase64String(salt.Concat(hash).ToArray());
    }

    private bool VerifyPassword(string toVerify, string passwordHash)
    {
        // Extract the bytes from the password hash
        byte[] hashBytes = Convert.FromBase64String(passwordHash);
        byte[] salt = new byte[saltSize];
        Array.Copy(hashBytes, 0, salt, 0, saltSize);
        // Compute the hash on the password entered by the user
        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(toVerify, salt, iterations);
        byte[] hash = pbkdf2.GetBytes(hashSize);
        // Compare the results
        return hash.SequenceEqual(hashBytes.Skip(saltSize));
    }
}
