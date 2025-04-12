using UnityEngine;

namespace Projectiles
{
	public static class UIExtensions
	{
		// enable or disable the visibility of a UI
		public static void SetVisibility(this CanvasGroup @this, bool value)
		{
			if (@this == null)
				return;

			@this.alpha = value == true ? 1f : 0f;
			@this.interactable = value;
			@this.blocksRaycasts = value;
		}

		// if value is null, do nothing, else set the text to a value
		public static void SetTextSafe(this TMPro.TextMeshProUGUI @this, string text)
		{
			if (@this == null)
				return;

			@this.text = text;
		}
		// if value is null, do nothing, else get the text value
		public static string GetTextSafe(this TMPro.TextMeshProUGUI @this)
		{
			if (@this == null)
				return null;

			return @this.text;
		}
	}
}
