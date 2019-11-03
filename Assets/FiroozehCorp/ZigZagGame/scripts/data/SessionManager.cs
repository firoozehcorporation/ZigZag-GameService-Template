using System;
using System.Collections;
using FiroozehCorp.ZigZagGame.scripts.game.ZigZag;
using FiroozehGameServiceAndroid;
using FiroozehGameServiceAndroid.Builders;
using FiroozehGameServiceAndroid.Core;
using FiroozehGameServiceAndroid.Enums;
using UnityEngine;

#if UNITY_ANDROID

#endif

namespace FiroozehCorp.ZigZagGame.scripts.data {
	namespace ZigZag {

		/// <summary>
		/// The Session Manager is the entry point to the game
		/// </summary>
		public class SessionManager : MonoBehaviour {

			public static SessionManager Instance;
		

			private string userId { get; set; }
			public bool validUser { 
				get { 
				#if UNITY_ANDROID
					return true;
				#elif UNITY_IOS
					//return //ios implementation...if different
				#endif
					return false;
				} 
			}

			GameManager game;

			/// <summary>
			/// Singleton pattern. Only one Session Manager allowed
			/// </summary>
			void Awake() {
				if (Instance != null) {
					Destroy(gameObject);
				}
				else {
					Instance = this;
					DontDestroyOnLoad(gameObject);
				}
			}

			/// <summary>
			/// Begin configuring google play
			/// </summary>
			void Start() {
				game = GameManager.Instance;
				ConfigureGameService();
			}

			
			void ConfigureGameService() {
		#if UNITY_ANDROID

				var config = new GameServiceClientConfiguration
					.Builder(LoginType.Normal)
					.SetClientId("GameService_ZigZag")
					.SetClientSecret("styo69b77wr6x3p3uw9jp9")
					.IsLogEnable(true)
					.IsNotificationEnable(true)
					.SetNotificationListener(NotificationListener)
					.CheckGameServiceInstallStatus(true)
					.CheckGameServiceOptionalUpdate(false)
					.Build();
				
				FiroozehGameService.ConfigurationInstance(config);
				FiroozehGameService.Run(DataStorage.GetDataFromGameService,Debug.LogError);


		#elif UNITY_IOS
				//ios implementation...if different
		#endif
				Social.localUser.Authenticate(ProcessAuthentication);
			}


			void NotificationListener(string jsonData)
			{
				Debug.LogError("NotificationListener : " + jsonData);
			}

			/// <summary>
			/// Called at the end of authentication attempt
			/// If success, the player successfully logged in 
			/// Regardless, data storage will load some user (default user or not) and save data to local player prefs
			/// </summary>
			void ProcessAuthentication(bool success) {
				if (success) {
					userId = Social.localUser.id;
					Debug.Log("Google Play Authentication Success!");
				} else {
					userId = string.Empty;
					Debug.Log("Google Play Authentication Failure!");
				}
				DataStorage.LoadUser(userId, false);
				StartCoroutine(nameof(WaitToStart));
			}

			/// <summary>
			/// Initiated at the end of authentication
			/// Wait until all player data has been loaded before starting the game
			/// </summary>
			IEnumerator WaitToStart() {
				while (DataStorage.LOADING_USER) {
					yield return null;
				}
				game.StartApp();
			}

			public void ShowAchievements() {
		#if UNITY_ANDROID
				FiroozehGameService.Instance.ShowAchievementsUi(error=>{});
		#elif UNITY_IOS
				//ios implementation...if different
		#endif
			}

			public void ShowLeaderboard() {
		#if UNITY_ANDROID
				FiroozehGameService.Instance.ShowLeaderBoardsUi(error=>{});
		#elif UNITY_IOS
				//ios implementation...if different
		#endif
			}
		}
	}
}
