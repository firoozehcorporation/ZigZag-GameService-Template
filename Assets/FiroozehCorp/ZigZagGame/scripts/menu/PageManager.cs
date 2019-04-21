using System.Collections;
using FiroozehCorp.ZigZagGame.scripts.game.ZigZag.Types;
using UnityEngine;

namespace FiroozehCorp.ZigZagGame.scripts.menu {
	namespace ZigZag {

		/// <summary>
		/// The page manager is in charge of a set of page controllers
		/// It stores pages in a hashtable for quick access, and controls the enter and exit process for each page
		/// </summary>
		public class PageManager : MonoBehaviour {

			public PageController[] pages;
			Hashtable pageHash;

			/// <summary>
			/// Initialize the page hash based on the public pages collection
			/// </summary>
			void Start()
			{
				pageHash = new Hashtable();
				
				foreach (var t in pages)
				{
					pageHash.Add(t.pageType, t);
				}
			}

			/// <summary>
			/// Remove a page before turning on a new page
			/// </summary>
			public void TurnPageOn(PageType pageToRemove, PageType pageToLoad) {
				if (pageToRemove == PageType.None) {
					Page(pageToLoad).TurnOn();
				}
				else {
					StartCoroutine(WaitToLoadPage(pageToRemove, pageToLoad));
				}
			}

			/// <summary>
			/// Disable a page. See PageController.cs
			/// </summary>
			public void TurnPageOff(PageType page) {
				Page(page).TurnOff();
			}

			/// <summary>
			/// Enable a page. See PageController.cs
			/// </summary>
			public bool PageIsOn(PageType page) {
				return Page(page).IsOn;
			}

			/// <summary>
			/// Wait until 'pageToRemove' is done with animating out before turning on 'pageToLoad'
			/// </summary>
			IEnumerator WaitToLoadPage(PageType pageToRemove, PageType pageToLoad) {
				Page(pageToRemove).TurnOff();
				while (Page(pageToRemove).IsOn) {
					yield return null;
				}
				Page(pageToLoad).TurnOn();
			}

			/// <summary>
			/// Retrieve a page from the page hash if it exists
			/// </summary>
			PageController Page(PageType page) {
				if (pageHash.Contains(page)) return (PageController) pageHash[page];
				Debug.LogError("The page you are trying to access could not be found => "+page);
				return null;
			}
		}
	}
}
