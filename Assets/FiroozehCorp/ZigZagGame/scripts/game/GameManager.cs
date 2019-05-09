using System.Collections;
using FiroozehCorp.ZigZagGame.scripts.data;
using FiroozehCorp.ZigZagGame.scripts.data.ZigZag;
using FiroozehCorp.ZigZagGame.scripts.game.ZigZag.Types;
using FiroozehCorp.ZigZagGame.scripts.menu.ZigZag;
using FiroozehGameServiceAndroid;
using FiroozehGameServiceAndroid.Builders;
using FiroozehGameServiceAndroid.Core;
using UnityEngine;

namespace FiroozehCorp.ZigZagGame.scripts.game {
	namespace ZigZag {

		/// <summary>
		/// The game manager is the overseer of major game processes such as game states and menu states
		/// </summary>
		public class GameManager : MonoBehaviour {

			public static GameManager Instance;
			
			public Material wallMat;
			public Color[] colorBin;
			public float colorSmooth = 2;
			public float scoreFrequency = 0.5f;

			PageManager worldSpacePageManager;
			PageManager pageManager;
			SceneLoader scene;
			ProgressManager progress;
			AudioManager audio;

			CameraController camera;
			bool gameOver = true;
			float scoreTimer = 0;

			public bool GameOver { get { return gameOver; } }

			/// <summary>
			/// Singleton Pattern. Only one Game Manager allowed.
			/// </summary>
			void Awake() {
				if (Instance != null) {
					Destroy(gameObject);
				}
				else {
					Instance = this;
					StartCoroutine(nameof(LerpColors));
					pageManager = GetComponent<PageManager>();
					DontDestroyOnLoad(gameObject);
				}
			}

			/// <summary>
			/// Initialize other class instances in Start() to ensure the instance always has a value
			/// Start is always called after Awake - where all instances are created
			/// </summary>
			void Start() {
				scene = SceneLoader.Instance;
				progress = ProgressManager.Instance;
				audio = AudioManager.Instance;
				
			}

			/// <summary>
			/// Subscribe to pickup and lose events from the player (TapController)
			/// </summary>
			void OnEnable() {
				TapController.OnPickup += OnPickup;
				TapController.OnLose += OnLose;
			}

			/// <summary>
			/// Unsubscribe from pickup and lose events from the player (TapController)
			/// </summary>
			void OnDisable() {
				TapController.OnPickup -= OnPickup;
				TapController.OnLose -= OnLose;
			}

			/// <summary>
			/// If game over, do nothing
			/// otherwise increment the player's score
			/// </summary>
			void Update() {
				if (gameOver) return;

				scoreTimer += Time.deltaTime;
				if (!(scoreTimer >= scoreFrequency)) return;
				progress.AddScore(1);
				scoreTimer = 0;
			}

			/// <summary>
			/// Invoked via Tap Controller event. See OnEnable
			/// Add and save score. See ProgressManager.cs
			/// </summary>
			private void OnPickup() {
				progress.AddScore(5);
			}

			/// <summary>
			/// Invoked via Tap Controller event. See OnEnable
			/// Disable Hud and Enable Game Over page
			/// Instruct the camera to rotate around the player
			/// </summary>
			void OnLose() {
				gameOver = true;
				pageManager.TurnPageOn(PageType.Hud, PageType.GameOver);
				if (Camera.main != null) camera = Camera.main.GetComponent<CameraController>();
				camera.StartLoseSequence();
			}

			/// <summary>
			/// Called by cameracontroller and on reload
			/// Reinitializes necessary game variables and reloads the main menu
			/// </summary>
			public void Init() {
				if (Camera.main != null) camera = Camera.main.GetComponent<CameraController>();
				camera.StartAtEntry();
				worldSpacePageManager = GameObject.FindWithTag("WorldSpacePageManager").GetComponent<PageManager>();
				worldSpacePageManager.TurnPageOn(PageType.None, PageType.MenuLeft);
				worldSpacePageManager.TurnPageOn(PageType.None, PageType.MenuRight);
				progress.ResetScore();
			}

			/// <summary>
			/// Called by Session Manager after user logs in
			/// </summary>
			public void StartApp()
			{
				if (Camera.main != null) camera = Camera.main.GetComponent<CameraController>();
				camera.StartEntrySequence();
			}

			/// <summary>
			/// Called by rightMenu button event
			/// </summary>
			public void StartGame() {
				worldSpacePageManager.TurnPageOff(PageType.MenuLeft);
				worldSpacePageManager.TurnPageOff(PageType.MenuRight);
				pageManager.TurnPageOn(PageType.None, PageType.Hud);
				gameOver = false;
				progress.IncrementAttempts();
			}

			/// <summary>
			/// Called by gameoverMenu button event
			/// </summary>
			public void RestartGame() {
				audio.PlaySound(SoundType.Click);
				pageManager.TurnPageOn(PageType.GameOver, PageType.Fade);
				StopCoroutine(nameof(ReloadScene));
				StartCoroutine(nameof(ReloadScene));
				
					
				FiroozehGameService.Instance.SaveGame(
					"ZigZagGame"
					,"ZigZagGameSave"
					,null
					,new Save {Attempts = ProgressManager.Attempts, HighScore = ProgressManager.HighScore, Score = ProgressManager.Score}
					, detail =>
					{
					
					},error => {}
				);

			}

			/// <summary>
			/// Wait until fade screen is completely black (page is on)
			/// Send pooled objects back to the persistent, undying parent. See Pool.cs (Objects are not reinstantiated)
			/// Reload the scene (easy way to get all objects back in place)
			/// </summary>
			private IEnumerator ReloadScene() {
				while (!pageManager.PageIsOn(PageType.Fade)) {
					yield return null;
				}
				
				FindObjectOfType<WallSpawner>().ReleasePoolObjects();
				scene.ReloadScene();

				while (scene.IsLoading) {
					yield return null;
				}

				Init();
				pageManager.TurnPageOff(PageType.Fade);
			}
			
			/// <summary>
			/// Change color of wall material over time. One material affects all the wall pieces
			/// </summary>
			private IEnumerator LerpColors() {
				var color = 1;
				var current = wallMat.GetColor("_Color");
				while (true) {
					while (!ColorsMatch(current, colorBin[color])) {
						current = Color.Lerp(current, colorBin[color], colorSmooth * Time.deltaTime);
						wallMat.SetColor("_Color", current);
						yield return null;
					}
					color = color < colorBin.Length - 1 ? color + 1 : 0;
					yield return new WaitForSeconds(10);
				}
			}

			/// <summary>
			/// Return true if two colors match. Used by the LerpColors coroutine
			/// </summary>
			static bool ColorsMatch(Color c1, Color c2)
			{
				return Mathf.Abs(c1.r - c2.r) < 0.02f &&
				       Mathf.Abs(c1.g - c2.g) < 0.02f &&
				       Mathf.Abs(c1.b - c2.b) < 0.02f;
			}

		}
	}
}
