using FiroozehCorp.ZigZagGame.scripts.data.ZigZag;

namespace FiroozehCorp.ZigZagGame.scripts.menu {
	namespace ZigZag {

		/// <summary>
		/// Child of ButtonEvent, a custom world space UI event class
		/// </summary>
		public class OpenAchievementsEvent : ButtonEvent {

			SessionManager session;

			/// <summary>
			/// Initialize required variables
			/// </summary>
			public override void Init() {
			#if UNITY_WEBGL
				img.enabled = false;
				return;
			#endif
				base.Init();
				session = SessionManager.Instance;
			}

			/// <summary>
			/// If clicked, request to see achievements
			/// </summary>
			public override void OnClick() {
				if (!game.GameOver) return;
				base.OnClick();
				session.ShowAchievements();
			}
		}
	}
}
