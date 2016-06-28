using Microsoft.Band;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace EmergencyCall
{
	internal class TileLayout
	{
		private readonly PageLayout pageLayout;
		private readonly PageLayoutData pageLayoutData;
		
		private readonly FilledPanel panel = new FilledPanel();
		private readonly TextBlock textBlock = new TextBlock();
		private readonly TextBlock textBlock2 = new TextBlock();
		private readonly Icon icon = new Icon();
		
		private readonly TextBlockData textBlockData = new TextBlockData(4, "calling now");
		private readonly TextBlockData textBlock2Data = new TextBlockData(3, "Do Not Panic");
		private readonly IconData iconData = new IconData(2, 1);
		
		public TileLayout()
		{
			LoadIconMethod = LoadIcon;
			AdjustUriMethod = (uri) => uri;
			
			panel = new FilledPanel();
			panel.BackgroundColorSource = ElementColorSource.Custom;
			panel.BackgroundColor = new BandColor(0, 0, 0);
			panel.Rect = new PageRect(0, 0, 258, 128);
			panel.ElementId = 1;
			panel.Margins = new Margins(0, 0, 0, 0);
			panel.HorizontalAlignment = HorizontalAlignment.Left;
			panel.VerticalAlignment = VerticalAlignment.Top;
			
			textBlock = new TextBlock();
			textBlock.Font = TextBlockFont.Small;
			textBlock.Baseline = 0;
			textBlock.BaselineAlignment = TextBlockBaselineAlignment.Automatic;
			textBlock.AutoWidth = true;
			textBlock.ColorSource = ElementColorSource.Custom;
			textBlock.Color = new BandColor(255, 255, 255);
			textBlock.Rect = new PageRect(24, 57, 32, 32);
			textBlock.ElementId = 4;
			textBlock.Margins = new Margins(0, 0, 0, 0);
			textBlock.HorizontalAlignment = HorizontalAlignment.Left;
			textBlock.VerticalAlignment = VerticalAlignment.Top;
			
			panel.Elements.Add(textBlock);
			
			textBlock2 = new TextBlock();
			textBlock2.Font = TextBlockFont.Small;
			textBlock2.Baseline = 0;
			textBlock2.BaselineAlignment = TextBlockBaselineAlignment.Automatic;
			textBlock2.AutoWidth = true;
			textBlock2.ColorSource = ElementColorSource.Custom;
			textBlock2.Color = new BandColor(255, 255, 255);
			textBlock2.Rect = new PageRect(23, 30, 32, 32);
			textBlock2.ElementId = 3;
			textBlock2.Margins = new Margins(0, 0, 0, 0);
			textBlock2.HorizontalAlignment = HorizontalAlignment.Left;
			textBlock2.VerticalAlignment = VerticalAlignment.Top;
			
			panel.Elements.Add(textBlock2);
			
			icon = new Icon();
			icon.ColorSource = ElementColorSource.BandHighlight;
			icon.Rect = new PageRect(193, 55, 64, 64);
			icon.ElementId = 2;
			icon.Margins = new Margins(0, 0, 0, 0);
			icon.HorizontalAlignment = HorizontalAlignment.Left;
			icon.VerticalAlignment = VerticalAlignment.Top;
			
			panel.Elements.Add(icon);
			pageLayout = new PageLayout(panel);
			
			PageElementData[] pageElementDataArray = new PageElementData[3];
			pageElementDataArray[0] = textBlockData;
			pageElementDataArray[1] = textBlock2Data;
			pageElementDataArray[2] = iconData;
			
			pageLayoutData = new PageLayoutData(pageElementDataArray);
		}
		
		public PageLayout Layout
		{
			get
			{
				return pageLayout;
			}
		}
		
		public PageLayoutData Data
		{
			get
			{
				return pageLayoutData;
			}
		}
		
		public Func<string, Task<BandIcon>> LoadIconMethod
		{
			get;
			set;
		}
		
		public Func<string, string> AdjustUriMethod
		{
			get;
			set;
		}
		
		private static async Task<BandIcon> LoadIcon(string uri)
		{
			StorageFile imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));
			
			using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
			{
				WriteableBitmap bitmap = new WriteableBitmap(1, 1);
				await bitmap.SetSourceAsync(fileStream);
				return bitmap.ToBandIcon();
			}
		}
		
		public async Task LoadIconsAsync(BandTile tile)
		{
			int firstIconIndex = tile.AdditionalIcons.Count + 2; // First 2 are used by the Tile itself
			tile.AdditionalIcons.Add(await LoadIconMethod(AdjustUriMethod("ms-appx:///Assets/smallCalling.png")));
			pageLayoutData.ById<IconData>(2).IconIndex = (ushort)(firstIconIndex + 0);
		}
		
		public static BandTheme GetBandTheme()
		{
			var theme = new BandTheme();
			theme.Base = new BandColor(51, 102, 204);
			theme.HighContrast = new BandColor(58, 120, 221);
			theme.Highlight = new BandColor(58, 120, 221);
			theme.Lowlight = new BandColor(49, 101, 186);
			theme.Muted = new BandColor(43, 90, 165);
			theme.SecondaryText = new BandColor(137, 151, 171);
			return theme;
		}
		
		public static BandTheme GetTileTheme()
		{
			var theme = new BandTheme();
			theme.Base = new BandColor(51, 102, 204);
			theme.HighContrast = new BandColor(58, 120, 221);
			theme.Highlight = new BandColor(58, 120, 221);
			theme.Lowlight = new BandColor(49, 101, 186);
			theme.Muted = new BandColor(43, 90, 165);
			theme.SecondaryText = new BandColor(137, 151, 171);
			return theme;
		}
		
		public class PageLayoutData
		{
			private readonly PageElementData[] array;
			
			public PageLayoutData(PageElementData[] pageElementDataArray)
			{
				array = pageElementDataArray;
			}
			
			public int Count
			{
				get
				{
					return array.Length;
				}
			}
			
			public T Get<T>(int i) where T : PageElementData
			{
				return (T)array[i];
			}
			
			public T ById<T>(short id) where T:PageElementData
			{
				return (T)array.FirstOrDefault(elm => elm.ElementId == id);
			}
			
			public PageElementData[] All
			{
				get
				{
					return array;
				}
			}
		}
		
	}
}
