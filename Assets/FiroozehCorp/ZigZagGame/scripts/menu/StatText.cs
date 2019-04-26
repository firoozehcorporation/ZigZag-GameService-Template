using System;
using System.Collections;
using FiroozehCorp.ZigZagGame.scripts.data.ZigZag;
using FiroozehCorp.ZigZagGame.scripts.game;
using FiroozehGameServiceAndroid.Utils;
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
				About ,
				Start ,
				Continue ,
				GameName ,
				GameDescription
		
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
						txt.text = FarsiTextUtil.Fix("بیشترین امتیاز : " + ProgressManager.HighScore);
						break;
					case StatType.Attempts:
						txt.text = FarsiTextUtil.Fix("تعداد بازی : "+ progress.Attempts);
						break;
					case StatType.Score:
						txt.text = FarsiTextUtil.Fix( "امتیاز : "+ progress.Score);
						break;
					case StatType.About:
						txt.text = FarsiTextUtil.Fix(
							"بازی زیگزاگ \n قدرت گرفته از ");
						break;
					case StatType.Start:
						txt.text = FarsiTextUtil.Fix("شروع بازی");
						break;
					case StatType.Continue:
						txt.text = FarsiTextUtil.Fix("دوباره");
						break;
					case StatType.GameName:
						txt.text = FarsiTextUtil.Fix("زیگزاگ");
						break;
					case StatType.GameDescription:
						txt.text = FarsiTextUtil.Fix("قدرت گرفته از");
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
				txt.text = FarsiTextUtil.Fix(progress.Score + " امتیاز ");
			}
		}
	}
}
