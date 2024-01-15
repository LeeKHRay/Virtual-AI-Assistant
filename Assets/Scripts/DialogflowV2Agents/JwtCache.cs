using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Globalization;

namespace Syrus.Plugins.DFV2Client
{
	public static class JwtCache
	{
		public struct JwtToken
		{
			/// <summary>
			/// The JWT token used for DF2 access.
			/// </summary>
			internal string token;

			/// <summary>
			/// The expire time in Unity time.
			/// </summary>
			internal string expireTime;

			internal JwtToken(string token, string expireTime)
			{
				this.token = token;
				this.expireTime = expireTime;
			}
		}

		/// <summary>
		/// Maps each project to an access token.
		/// </summary>
		private static Dictionary<string, JwtToken> tokens;

		static JwtCache()
		{
			tokens = new Dictionary<string, JwtToken>();
		}

		/// <summary>
		/// Tries to acquire a JWT token from the cache.
		/// </summary>
		/// <param name="serviceAccount">The Google Service Account that wants to use the chatbot.</param>
		/// <param name="token">The access token.</param>
		/// <returns>True if the acccess token was found, false otherwise.</returns>
		public static bool TryGetToken(string serviceAccount, out string token)
		{
			LoadToken(serviceAccount);
			if (tokens.TryGetValue(serviceAccount, out JwtToken jwt) && DateTime.Compare(DateTime.Now, DateTime.Parse(jwt.expireTime)) < 0)
			{
				token = jwt.token;
				return true;
			}

			token = string.Empty;
			return false;
		}

		/// <summary>
		/// Acquires a new JWT token and updates the cache.
		/// </summary>
		/// <param name="credentialsFileName">The name of the .p12 file that contains the credentials.</param>
		/// <param name="serviceAccount">The name of the service account which is making the request.</param>
		public static IEnumerator GetToken(string credentialsFileName, string serviceAccount)
		{
			TextAsset p12File = Resources.Load<TextAsset>("DialogflowV2/" + credentialsFileName);
			var jwt = GoogleJsonWebToken.GetJwt(serviceAccount, p12File.bytes, GoogleJsonWebToken.SCOPE_DIALOGFLOWV2);
			UnityWebRequest tokenRequest = GoogleJsonWebToken.GetAccessTokenRequest(jwt);
			yield return tokenRequest.SendWebRequest();
			if (tokenRequest.isNetworkError || tokenRequest.isHttpError)
			{
				Debug.LogError("Error " + tokenRequest.responseCode + ": " + tokenRequest.error);
				yield break;
			}
			string serializedToken = Encoding.UTF8.GetString(tokenRequest.downloadHandler.data);
			var jwtJson = JsonConvert.DeserializeObject<GoogleJsonWebToken.JwtTokenResponse>(serializedToken);
			tokens[serviceAccount] = new JwtToken(jwtJson.access_token, DateTime.Now.AddHours(jwtJson.expires_in / 3600).ToString("G", CultureInfo.GetCultureInfo("en-US")));
			SaveToken(tokens[serviceAccount]);
		}

		// Save JWT token to file
		public static void SaveToken(JwtToken jwtToken)
		{
			string path = Application.dataPath + "\\Resources\\DialogflowV2\\accessToken.dat";
			FileStream fs = new FileStream(path, FileMode.Create);
			StreamWriter sw = new StreamWriter(fs);

			sw.WriteLine(Encode(jwtToken.token));
			sw.WriteLine(Encode(jwtToken.expireTime));
			sw.Close();
			fs.Close();
		}

		// Load JWT token from file
		public static void LoadToken(string serviceAccount)
		{
			string path = Application.dataPath + "\\Resources\\DialogflowV2\\accessToken.dat";
			if (!File.Exists(path))
			{
				Debug.Log("File not found, create accessToken.dat");
				File.Create(path);
				return;
			}
            
            FileStream fs = new FileStream(path, FileMode.Open);
			StreamReader sr = new StreamReader(fs);
			string token = Decode(sr.ReadLine());
			string expireTime = Decode(sr.ReadLine());
			tokens[serviceAccount] = new JwtToken(token, expireTime);
			sr.Close();
			fs.Close();
		}

		static public string Encode(string str)
		{
			return Convert.ToBase64String(Encoding.ASCII.GetBytes(str));
		}

		static public string Decode(string str)
		{
			return Encoding.ASCII.GetString(Convert.FromBase64String(str));
		}
	}
}

