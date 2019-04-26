using System.Collections.Generic;
using System.Linq;
using FiroozehGameServiceAndroid;
using FiroozehGameServiceAndroid.Models;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_ANDROID

#endif

namespace FiroozehCorp.ZigZagGame.scripts.data {
	namespace ZigZag {

		/// <summary>
		/// Data Storage is your access point to the Google Play Services API
		/// Increment Events, Update Leaderboards, and Apply Achievements
		/// The functions in this class should only be called by ProgressManager.cs
		/// </summary>
		public class DataStorage
		{
			
			public static bool LOADING_USER 
			{
				get { 
					if (!initialLoadBegan) return true;
					return loadJobs > 0; 
				}
			}

			static readonly string INITIALIZED = "INITIALIZED";
			static string USER_ID;

			static int loadJobs;
			static bool initialLoadBegan;			
			
			/// <summary>
			/// Only called once per user.
			/// When a new username is passed to this function, a check is done to see if her data has been initialized
			/// If not, then a few fetch operations will pull the data and save it locally with player prefs
			/// </summary>
			public static void LoadUser(string user, bool reset) {

				initialLoadBegan = true;
				USER_ID = user;		

				//determine if this user has data stored on this device
				var localStorageStatus = PlayerPrefs.GetInt(USER_ID+INITIALIZED);

				if (localStorageStatus == 1 && !reset) return;
				SaveLocalData("Audio", 1);
				FetchEventForLocalStorage(GPGSIds.event_attempts);
				FetchLeaderB‌‌oardForLocalStorage(GPGSIds.leaderboard_high_score);
				
				
				
				if (USER_ID != string.Empty) loadJobs += 2;

				PlayerPrefs.SetInt(USER_ID+INITIALIZED, 1);
			}

			/// <summary>
			/// Standard Leaderboard set function
			/// If the user is not logged in, it will simply save to a default profile in the player prefs
			/// </summary>
			public static void ReportLeaderboardScore(string leaderboardId, uint score) {
				if (!SessionManager.Instance.validUser) {
					SaveLocalData(leaderboardId, (int)score);
					return;
				}

			#if UNITY_ANDROID || UNITY_IOS
				Social.ReportScore(score, leaderboardId, 
					success => 
					{
						SaveLocalData(leaderboardId, (int)score);
					}
				);
			#endif
				
         #if UNITY_ANDROID
			if(ProgressManager.HighScore % 100 == 0)
	            SessionManager.GameService?.SubmitScore(leaderboardId, (int) score, success => { }, error => { });
				
#endif

			}

			/// <summary>
			/// Standard data increment function
			/// If the user is not logged in, it will simply save to a default profile in the player prefs
			/// </summary>
			public static void IncrementEvent(string eventId, uint amount) {

				var newValue = GetLocalData(eventId) + (int)amount;
				SaveLocalData(eventId, newValue);

				if (!SessionManager.Instance.validUser) {
				}

			#if UNITY_ANDROID
				//PlayGamesPlatform.Instance.Events.IncrementEvent(eventId, amount);
			#elif UNITY_IOS
				//ios implementation...if different
			#endif

			}

			/// <summary>
			/// Standard Achievement set function
			/// </summary>
			public static void UnlockAchievement(string achievementId) {
			#if UNITY_WEBGL
				return; //no achievements on webgl
			#endif
				if (achievementId == string.Empty) {
					return;
				}

			#if UNITY_ANDROID || UNITY_IOS
				Social.ReportProgress(achievementId, 100.0f, success => {});
			#endif
				
                #if UNITY_ANDROID
				if (isAchievementUnlocked(achievementId)) return;
				

					SessionManager.GameService?.UnlockAchievement(achievementId, addAchievementToAchievementList, error => { });
				
#endif
			}

			/// <summary>
			/// Retrieve a value from local data
			/// </summary>
			public static int GetLocalData(string dataId) {
			#if UNITY_WEBGL
				return 0; //no achievements on webgl
			#endif
				var ret = PlayerPrefs.GetInt(USER_ID+dataId);
				return ret;
			}

			/// <summary>
			/// Save a value to local
			/// </summary>
			public static void SaveLocalData(string dataId, int amount) {
			#if UNITY_WEBGL
				return; //no playerprefs on webgl due to mem consumption
			#endif
				PlayerPrefs.SetInt(USER_ID+dataId, amount);
				
			}

			public static void SaveDataFromGameService(FiroozehGameService gameService)
			{
				gameService?.GetSaveGame<Save>( save =>
				{
					SaveLocalData(GPGSIds.event_attempts, save.Attempts);
					SaveLocalData(GPGSIds.leaderboard_high_score,save.HighScore);
					
				},error => {});
				
				gameService?.GetAchievements(achievements =>
				{
					SaveAchievementList("Achievements",achievements);
				},e=>{});
				
				gameService?.GetLeaderBoards(leaderBoards =>
				{
					SaveLeaderBoardList("LeaderBoards",leaderBoards);
				},e=>{});

			}
			
			/// <summary>
			/// Save a LeaderBoard to local
			/// </summary>
			public static void SaveLeaderBoardList(string dataId, List<LeaderBoard> data) {
				PlayerPrefs.SetString(USER_ID+dataId,JsonConvert.SerializeObject(data));
			}
			
			
			/// <summary>
			/// Save a Achievements to local
			/// </summary>
			public static void SaveAchievementList(string dataId, List<Achievement> data) {
				PlayerPrefs.SetString(USER_ID+dataId,JsonConvert.SerializeObject(data));
			}

			/// <summary>
			/// Request a leaderboard value from the remote server (cloud)
			/// If no user is logged in, the local data for 'leaderboardId' is saved to player prefs
			/// </summary>
			static void FetchLeaderB‌‌oardForLocalStorage(string leaderboardId) {
			#if UNITY_WEBGL
				loadJobs--;
				return; //no leaderboard on webgl
			#endif
				if (SessionManager.Instance.validUser) return;
				//no valid user..no need to poll the cloud. initialize local
				SaveLocalData(leaderboardId, 0);
				loadJobs--;

            #if UNITY_ANDROID
				
			#elif UNITY_IOS
				//ios implementation...if different
				//dont forget to decrement load jobs
			#endif
			}

			/// <summary>
			/// Request an event value from the remote server (cloud)
			/// If no user is logged in, the local data for 'eventId' is saved to player prefs
			/// </summary>
			/// 
			static void FetchEventForLocalStorage(string eventId) {
			#if UNITY_WEBGL
				loadJobs--;
				return; //no events on webgl
			#endif
				if (SessionManager.Instance.validUser) return;
				//no valid user..no need to poll the cloud. initialize local
				SaveLocalData(eventId, 0);
				loadJobs--;


#if UNITY_ANDROID
				/*PlayGamesPlatform.Instance.Events.FetchEvent(
					DataSource.ReadCacheOrNetwork,
					eventId,
					(rs, e) => {
						fetchData = (int)e.CurrentCount; //store data

						SaveLocalData(eventId, fetchData);
						loadJobs--;
				});
				*/
			#elif UNITY_IOS
				//ios implementation...if different
				//dont forget to decrement load jobs
			#endif

			}


			private static bool isAchievementUnlocked(string key)
			{
				var data = PlayerPrefs.GetString(USER_ID + "Achievements");
				var list = JsonConvert.DeserializeObject<List<Achievement>>(data);
				return list.Any(a => a.key == key);
			}

			private static void addAchievementToAchievementList(Achievement achievement)
			{
				var data = PlayerPrefs.GetString(USER_ID + "Achievements");
				var list = JsonConvert.DeserializeObject<List<Achievement>>(data);
				 list.Add(achievement);
				SaveAchievementList("Achievements",list);
			}

		}	

	}
}
