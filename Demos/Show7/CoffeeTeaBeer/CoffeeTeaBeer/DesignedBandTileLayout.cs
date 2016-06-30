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

namespace CoffeeTeaBeer
{
	internal class DesignedBandTileLayout
	{
		private readonly PageLayout pageLayout;
		private readonly PageLayoutData pageLayoutData;
		
		private readonly ScrollFlowPanel panel = new ScrollFlowPanel();
		private readonly FlowPanel panel2 = new FlowPanel();
		private readonly TextBlock textBlock = new TextBlock();
		private readonly TextBlock textBlock2 = new TextBlock();
		private readonly TextBlock textBlock3 = new TextBlock();
		private readonly TextBlock textBlock4 = new TextBlock();
		private readonly FlowPanel panel3 = new FlowPanel();
		internal TextBlock txtCoffee = new TextBlock();
		internal TextBlock txtTea = new TextBlock();
		internal TextBlock txtBeer = new TextBlock();
		internal TextBlock txtWine = new TextBlock();
		private readonly FlowPanel panel4 = new FlowPanel();
		internal TextButton btnCoffee = new TextButton();
		internal TextButton btnTea = new TextButton();
		internal TextButton btnBeer = new TextButton();
		internal TextButton btnWine = new TextButton();
		
		private readonly TextBlockData textBlockData = new TextBlockData(3, "Coffee");
		private readonly TextBlockData textBlock2Data = new TextBlockData(4, "Tea");
		private readonly TextBlockData textBlock3Data = new TextBlockData(5, "Beer");
		private readonly TextBlockData textBlock4Data = new TextBlockData(6, "Wine");
		internal TextBlockData txtCoffeeData = new TextBlockData(8, "TextBlock");
		internal TextBlockData txtTeaData = new TextBlockData(9, "TextBlock");
		internal TextBlockData txtBeerData = new TextBlockData(10, "TextBlock");
		internal TextBlockData txtWineData = new TextBlockData(11, "TextBlock");
		internal TextButtonData btnCoffeeData = new TextButtonData(13, "Add");
		internal TextButtonData btnTeaData = new TextButtonData(14, "Add");
		internal TextButtonData btnBeerData = new TextButtonData(15, "Add");
		internal TextButtonData btnWineData = new TextButtonData(16, "Add");
		
		public DesignedBandTileLayout()
		{
			LoadIconMethod = LoadIcon;
			AdjustUriMethod = (uri) => uri;
			
			panel = new ScrollFlowPanel();
			panel.ScrollBarColorSource = ElementColorSource.Custom;
			panel.ScrollBarColor = new BandColor(255, 255, 255);
			panel.Orientation = FlowPanelOrientation.Vertical;
			panel.Rect = new PageRect(-1, 0, 259, 128);
			panel.ElementId = 1;
			panel.Margins = new Margins(0, 0, 0, 0);
			panel.HorizontalAlignment = HorizontalAlignment.Left;
			panel.VerticalAlignment = VerticalAlignment.Top;
			
			panel2 = new FlowPanel();
			panel2.Orientation = FlowPanelOrientation.Vertical;
			panel2.Rect = new PageRect(0, 0, 78, 128);
			panel2.ElementId = 2;
			panel2.Margins = new Margins(0, 0, 0, 0);
			panel2.HorizontalAlignment = HorizontalAlignment.Left;
			panel2.VerticalAlignment = VerticalAlignment.Top;
			
			textBlock = new TextBlock();
			textBlock.Font = TextBlockFont.Small;
			textBlock.Baseline = 0;
			textBlock.BaselineAlignment = TextBlockBaselineAlignment.Automatic;
			textBlock.AutoWidth = true;
			textBlock.ColorSource = ElementColorSource.Custom;
			textBlock.Color = new BandColor(255, 255, 255);
			textBlock.Rect = new PageRect(0, 0, 32, 32);
			textBlock.ElementId = 3;
			textBlock.Margins = new Margins(0, 0, 0, 0);
			textBlock.HorizontalAlignment = HorizontalAlignment.Left;
			textBlock.VerticalAlignment = VerticalAlignment.Top;
			
			panel2.Elements.Add(textBlock);
			
			textBlock2 = new TextBlock();
			textBlock2.Font = TextBlockFont.Small;
			textBlock2.Baseline = 0;
			textBlock2.BaselineAlignment = TextBlockBaselineAlignment.Automatic;
			textBlock2.AutoWidth = true;
			textBlock2.ColorSource = ElementColorSource.Custom;
			textBlock2.Color = new BandColor(255, 255, 255);
			textBlock2.Rect = new PageRect(0, 0, 32, 32);
			textBlock2.ElementId = 4;
			textBlock2.Margins = new Margins(0, 0, 0, 0);
			textBlock2.HorizontalAlignment = HorizontalAlignment.Left;
			textBlock2.VerticalAlignment = VerticalAlignment.Top;
			
			panel2.Elements.Add(textBlock2);
			
			textBlock3 = new TextBlock();
			textBlock3.Font = TextBlockFont.Small;
			textBlock3.Baseline = 0;
			textBlock3.BaselineAlignment = TextBlockBaselineAlignment.Automatic;
			textBlock3.AutoWidth = true;
			textBlock3.ColorSource = ElementColorSource.Custom;
			textBlock3.Color = new BandColor(255, 255, 255);
			textBlock3.Rect = new PageRect(0, 0, 32, 32);
			textBlock3.ElementId = 5;
			textBlock3.Margins = new Margins(0, 0, 0, 0);
			textBlock3.HorizontalAlignment = HorizontalAlignment.Left;
			textBlock3.VerticalAlignment = VerticalAlignment.Top;
			
			panel2.Elements.Add(textBlock3);
			
			textBlock4 = new TextBlock();
			textBlock4.Font = TextBlockFont.Small;
			textBlock4.Baseline = 0;
			textBlock4.BaselineAlignment = TextBlockBaselineAlignment.Automatic;
			textBlock4.AutoWidth = true;
			textBlock4.ColorSource = ElementColorSource.Custom;
			textBlock4.Color = new BandColor(255, 255, 255);
			textBlock4.Rect = new PageRect(0, 0, 32, 32);
			textBlock4.ElementId = 6;
			textBlock4.Margins = new Margins(0, 0, 0, 0);
			textBlock4.HorizontalAlignment = HorizontalAlignment.Left;
			textBlock4.VerticalAlignment = VerticalAlignment.Top;
			
			panel2.Elements.Add(textBlock4);
			
			panel.Elements.Add(panel2);
			
			panel3 = new FlowPanel();
			panel3.Orientation = FlowPanelOrientation.Vertical;
			panel3.Rect = new PageRect(0, 0, 47, 128);
			panel3.ElementId = 7;
			panel3.Margins = new Margins(86, -128, 0, 0);
			panel3.HorizontalAlignment = HorizontalAlignment.Left;
			panel3.VerticalAlignment = VerticalAlignment.Top;
			
			txtCoffee = new TextBlock();
			txtCoffee.Font = TextBlockFont.Small;
			txtCoffee.Baseline = 0;
			txtCoffee.BaselineAlignment = TextBlockBaselineAlignment.Automatic;
			txtCoffee.AutoWidth = true;
			txtCoffee.ColorSource = ElementColorSource.BandHighlight;
			txtCoffee.Rect = new PageRect(0, 0, 32, 32);
			txtCoffee.ElementId = 8;
			txtCoffee.Margins = new Margins(0, 0, 0, 0);
			txtCoffee.HorizontalAlignment = HorizontalAlignment.Left;
			txtCoffee.VerticalAlignment = VerticalAlignment.Top;
			
			panel3.Elements.Add(txtCoffee);
			
			txtTea = new TextBlock();
			txtTea.Font = TextBlockFont.Small;
			txtTea.Baseline = 0;
			txtTea.BaselineAlignment = TextBlockBaselineAlignment.Automatic;
			txtTea.AutoWidth = true;
			txtTea.ColorSource = ElementColorSource.BandHighlight;
			txtTea.Rect = new PageRect(0, 0, 32, 32);
			txtTea.ElementId = 9;
			txtTea.Margins = new Margins(0, 0, 0, 0);
			txtTea.HorizontalAlignment = HorizontalAlignment.Left;
			txtTea.VerticalAlignment = VerticalAlignment.Top;
			
			panel3.Elements.Add(txtTea);
			
			txtBeer = new TextBlock();
			txtBeer.Font = TextBlockFont.Small;
			txtBeer.Baseline = 0;
			txtBeer.BaselineAlignment = TextBlockBaselineAlignment.Automatic;
			txtBeer.AutoWidth = true;
			txtBeer.ColorSource = ElementColorSource.BandHighlight;
			txtBeer.Rect = new PageRect(0, 0, 32, 32);
			txtBeer.ElementId = 10;
			txtBeer.Margins = new Margins(0, 0, 0, 0);
			txtBeer.HorizontalAlignment = HorizontalAlignment.Left;
			txtBeer.VerticalAlignment = VerticalAlignment.Top;
			
			panel3.Elements.Add(txtBeer);
			
			txtWine = new TextBlock();
			txtWine.Font = TextBlockFont.Small;
			txtWine.Baseline = 0;
			txtWine.BaselineAlignment = TextBlockBaselineAlignment.Automatic;
			txtWine.AutoWidth = true;
			txtWine.ColorSource = ElementColorSource.BandHighlight;
			txtWine.Rect = new PageRect(0, 0, 32, 32);
			txtWine.ElementId = 11;
			txtWine.Margins = new Margins(0, 0, 0, 0);
			txtWine.HorizontalAlignment = HorizontalAlignment.Left;
			txtWine.VerticalAlignment = VerticalAlignment.Top;
			
			panel3.Elements.Add(txtWine);
			
			panel.Elements.Add(panel3);
			
			panel4 = new FlowPanel();
			panel4.Orientation = FlowPanelOrientation.Vertical;
			panel4.Rect = new PageRect(0, 0, 90, 130);
			panel4.ElementId = 12;
			panel4.Margins = new Margins(157, -128, 0, 0);
			panel4.HorizontalAlignment = HorizontalAlignment.Left;
			panel4.VerticalAlignment = VerticalAlignment.Top;
			
			btnCoffee = new TextButton();
			btnCoffee.PressedColor = new BandColor(32, 32, 32);
			btnCoffee.Rect = new PageRect(0, 0, 90, 32);
			btnCoffee.ElementId = 13;
			btnCoffee.Margins = new Margins(0, 0, 0, 0);
			btnCoffee.HorizontalAlignment = HorizontalAlignment.Center;
			btnCoffee.VerticalAlignment = VerticalAlignment.Top;
			
			panel4.Elements.Add(btnCoffee);
			
			btnTea = new TextButton();
			btnTea.PressedColor = new BandColor(32, 32, 32);
			btnTea.Rect = new PageRect(0, 0, 90, 32);
			btnTea.ElementId = 14;
			btnTea.Margins = new Margins(0, 0, 0, 0);
			btnTea.HorizontalAlignment = HorizontalAlignment.Center;
			btnTea.VerticalAlignment = VerticalAlignment.Top;
			
			panel4.Elements.Add(btnTea);
			
			btnBeer = new TextButton();
			btnBeer.PressedColor = new BandColor(32, 32, 32);
			btnBeer.Rect = new PageRect(0, 0, 90, 32);
			btnBeer.ElementId = 15;
			btnBeer.Margins = new Margins(0, 0, 0, 0);
			btnBeer.HorizontalAlignment = HorizontalAlignment.Center;
			btnBeer.VerticalAlignment = VerticalAlignment.Top;
			
			panel4.Elements.Add(btnBeer);
			
			btnWine = new TextButton();
			btnWine.PressedColor = new BandColor(32, 32, 32);
			btnWine.Rect = new PageRect(0, 0, 90, 32);
			btnWine.ElementId = 16;
			btnWine.Margins = new Margins(0, 0, 0, 0);
			btnWine.HorizontalAlignment = HorizontalAlignment.Center;
			btnWine.VerticalAlignment = VerticalAlignment.Top;
			
			panel4.Elements.Add(btnWine);
			
			panel.Elements.Add(panel4);
			pageLayout = new PageLayout(panel);
			
			PageElementData[] pageElementDataArray = new PageElementData[12];
			pageElementDataArray[0] = textBlockData;
			pageElementDataArray[1] = textBlock2Data;
			pageElementDataArray[2] = textBlock3Data;
			pageElementDataArray[3] = textBlock4Data;
			pageElementDataArray[4] = txtCoffeeData;
			pageElementDataArray[5] = txtTeaData;
			pageElementDataArray[6] = txtBeerData;
			pageElementDataArray[7] = txtWineData;
			pageElementDataArray[8] = btnCoffeeData;
			pageElementDataArray[9] = btnTeaData;
			pageElementDataArray[10] = btnBeerData;
			pageElementDataArray[11] = btnWineData;
			
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
			await Task.Run(() => { }); // Dealing with CS1998
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
