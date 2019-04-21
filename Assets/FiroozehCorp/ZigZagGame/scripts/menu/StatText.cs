using System;
using System.Collections;
using FiroozehCorp.ZigZagGame.scripts.data.ZigZag;
using UnityEngine;
using UnityEngine.UI;

namespace FiroozehCorp.ZigZagGame.scripts.menu {
	namespace ZigZag {

		/// <summary>
		/// Stat Text goes on a text component that is either a high score, current score, or # of attempts
		/// </summary>
		public class StatText : MonoBehaviour {

			public enum StatType {
				HighScore,
				Attempts,
				Score , 
				About
			}
			public StatType statType;
			public string prefix;

			Text txt;
			ProgressManager progress;

			/// <summary>
			/// Initialize required variables
			/// </summary>
			void Start() {
				txt = GetComponent<Text>();
				progress = ProgressManager.Instance;

				StartCoroutine(nameof(WaitToDisplay));
			}

			/// <summary>
			/// Wait to fill in any text until the player data has been loaded
			/// </summary>
			IEnumerator WaitToDisplay() {
				while (DataStorage.LOADING_USER) {
					yield return null;
				}

			#if UNITY_WEBGL

			#elif !UNITY_WEBGL
				switch (statType) {
					case StatType.HighScore:
						txt.text = prefix + progress.HighScore;
						break;
					case StatType.Attempts:
						txt.text = prefix + progress.Attempts.ToString();
						break;
					case StatType.Score:
						break;
					case StatType.About:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			#endif
			}

			/// <summary>
			/// Update score text while the player zigs and zags
			/// </summary>
			void Update() {
				if (statType != StatType.Score) return;
				txt.text = prefix + progress.Score.ToString();
			}
		}
	}
}
