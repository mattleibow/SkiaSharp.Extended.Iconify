using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

using SkiaSharp.Extended.Iconify;

namespace SkiaSharpDemo
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		private void OnPainting(object sender, SKPaintSurfaceEventArgs e)
		{
			var surface = e.Surface;
			var canvas = surface.Canvas;

			canvas.Clear(SKColors.White);

			var fontAwesome = "I {{fa-heart-o color=ff0000}} to {{fa-code}} on {{fa-windows color=1BA1E2}}!";
			var ionIcons = "{{ion-ios-cloud-download-outline color=0000ff}} the SkiaSharp source from {{ion-social-github}}.";

			using (var lookup = new SKTextRunLookup())
			using (var textPaint = new SKPaint())
			{
				// the lookup does not have to be re-created on each draw
				// instead, it can be created and re-used
				FontAwesome.AddTo(lookup);
				IonIcons.AddTo(lookup);

				textPaint.IsAntialias = true;
				textPaint.TextSize = 75;
				textPaint.Typeface = SKTypeface.FromFamilyName("Arial");

				// the DrawIconifiedText method will re-calculate the text runs
				// it may be better to cache this using the:
				//     var runs = SKTextRun.Create(text, lookup);
				// and then drawing it using the DrawText method.
				var yOffset = 25 + textPaint.TextSize;
				canvas.DrawIconifiedText(fontAwesome, 25, yOffset, lookup, textPaint);
				yOffset += 12 + textPaint.TextSize;
				canvas.DrawIconifiedText(ionIcons, 25, yOffset, lookup, textPaint);
			}
		}
	}
}
