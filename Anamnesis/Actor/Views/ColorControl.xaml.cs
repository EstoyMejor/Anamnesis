// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System;
using System.Data.Common;
using System.Windows;
using System.Windows.Controls;
using Anamnesis.Actor.Utilities;
using Anamnesis.Memory;
using Anamnesis.Services;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using XivToolsWpf.DependencyProperties;
using static System.Windows.Forms.DataFormats;
using static Anamnesis.Memory.ActorCustomizeMemory;
using WpfColor = System.Windows.Media.Color;

/// <summary>
/// Interaction logic for ColorControl.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class ColorControl : UserControl
{
	public static readonly IBind<byte> ValueDp = Binder.Register<byte, ColorControl>(nameof(Value), OnValueChanged);
	public static readonly IBind<ActorCustomizeMemory.Genders> GenderDp = Binder.Register<ActorCustomizeMemory.Genders, ColorControl>(nameof(Gender), OnGenderChanged);
	public static readonly IBind<ActorCustomizeMemory.Tribes> TribeDp = Binder.Register<ActorCustomizeMemory.Tribes, ColorControl>(nameof(Tribe), OnTribeChanged);
	public static readonly IBind<string> ToolTipKeyDp = Binder.Register<string, ColorControl>(nameof(ToolTipKey));

	private ColorData.Entry[]? colors;

	public ColorControl()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public enum ColorType
	{
		Skin,
		Eyes,
		Lips,
		FacePaint,
		Hair,
		HairHighlights,
	}

	public ColorType Type
	{
		get;
		set;
	}

	public ActorCustomizeMemory.Genders Gender
	{
		get => GenderDp.Get(this);
		set => GenderDp.Set(this, value);
	}

	public ActorCustomizeMemory.Tribes Tribe
	{
		get => TribeDp.Get(this);
		set => TribeDp.Set(this, value);
	}

	[AlsoNotifyFor(nameof(WpfColor))]
	public byte Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public string ToolTipKey
	{
		get => ToolTipKeyDp.Get(this);
		set => ToolTipKeyDp.Set(this, value);
	}

	public WpfColor WpfColor
	{
		get
		{
			if (this.colors == null || this.colors.Length <= 0 || this.Value >= this.colors.Length)
				return System.Windows.Media.Colors.Transparent;

			return this.colors[this.Value].WpfColor;
		}
	}

	private static void OnGenderChanged(ColorControl sender, ActorCustomizeMemory.Genders value)
	{
		sender.colors = sender.GetColors();
		sender.PreviewColor.Color = sender.WpfColor;
	}

	private static void OnTribeChanged(ColorControl sender, ActorCustomizeMemory.Tribes value)
	{
		if (!Enum.IsDefined<ActorCustomizeMemory.Tribes>(value))
			return;

		sender.colors = sender.GetColors();
		sender.PreviewColor.Color = sender.WpfColor;
	}

	private static void OnValueChanged(ColorControl sender, byte value)
	{
		sender.PreviewColor.Color = sender.WpfColor;
	}

	private async void OnClick(object sender, RoutedEventArgs e)
	{
		if (this.colors == null)
			return;

		FxivColorSelectorDrawer selector = new FxivColorSelectorDrawer(this.colors, this.Value);

		selector.SelectionChanged += (v) =>
		{
			if (selector.Selected < 0 || selector.Selected >= this.colors.Length)
				return;

			this.Value = (byte)v;
		};

		await ViewService.ShowDrawer(selector);
	}

	private ColorData.Entry[]? GetColors()
	{
		if (!Enum.IsDefined(this.Tribe))
			return null;

		// this.SuperSecretNotEverSeenClass();
		switch (this.Type)
		{
			case ColorType.Skin: return ColorData.GetSkin(this.Tribe, this.Gender);
			case ColorType.Eyes: return ColorData.GetEyeColors();
			case ColorType.Lips: return ColorData.GetLipColors();
			case ColorType.FacePaint: return ColorData.GetFacePaintColor();
			case ColorType.Hair: return ColorData.GetHair(this.Tribe, this.Gender);
			case ColorType.HairHighlights: return ColorData.GetHairHighlights();
		}

		throw new Exception("Unsupported color type: " + this.Type);
	}

	private void SuperSecretNotEverSeenClass()
	{
		int counter = 0;
		ColorData.Entry[][] savedArrays = new ColorData.Entry[64][];
		foreach (Tribes tribe in Enum.GetValues(typeof(Tribes)))
		{
			// do something with the current tribe value, for example:
			Console.WriteLine($"Tribe value: {(byte)tribe}, name: {tribe}");

			ColorData.Entry[]? skinFem = ColorData.GetSkin(tribe, ActorCustomizeMemory.Genders.Feminine);
			ColorData.Entry[]? hairFem = ColorData.GetHair(tribe, ActorCustomizeMemory.Genders.Feminine);
			ColorData.Entry[]? skinMal = ColorData.GetSkin(tribe, ActorCustomizeMemory.Genders.Masculine);
			ColorData.Entry[]? hairMal = ColorData.GetHair(tribe, ActorCustomizeMemory.Genders.Masculine);
			ColorData.Entry[]? facialFeature = ColorData.GetLimbalColors();

			savedArrays[counter] = skinFem;
			savedArrays[counter + 1] = hairFem;
			savedArrays[counter + 2] = skinMal;
			savedArrays[counter + 3] = hairMal;

			counter += 4;
		}

		// Define the path to the CSV file you want to write
		string csvFilePath = "C:\\Users\\marth\\Desktop\\ColorDataXIV\\savedArrays.csv";

		// Create a new StreamWriter object to write to the CSV file
		using (System.IO.StreamWriter sw = new System.IO.StreamWriter(csvFilePath))
		{
			// Loop over each row in the savedArrays array
			for (int i = 0; i < savedArrays.Length; i++)
			{
				// Loop over each column in the current row
				for (int j = 0; j < savedArrays[i].Length; j++)
				{
					// Write the current value to the file
					sw.Write(savedArrays[i][j].Hex);

					// If this is not the last column, write a comma separator
					if (j < savedArrays[i].Length - 1)
					{
						sw.Write(";");
					}
				}

				// Write a new line to separate the rows
				sw.WriteLine();
			}
		}

		// var skinFemMiqoSun = ColorData.GetSkin(ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Genders.Feminine);
		// var hairFemMiqoSun = ColorData.GetHair(ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Genders.Feminine);
		// var skinMalMiqoSun = ColorData.GetSkin(ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Genders.Masculine);
		// var hairMalMiqoSun = ColorData.GetHair(ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Genders.Masculine);
		// var skinFemMiqoMoon = ColorData.GetSkin(ActorCustomizeMemory.Tribes.KeeperOfTheMoon, ActorCustomizeMemory.Genders.Feminine);
		// var hairFemMiqoMoon = ColorData.GetHair(ActorCustomizeMemory.Tribes.KeeperOfTheMoon, ActorCustomizeMemory.Genders.Feminine);
		// var skinMalMiqoMoon = ColorData.GetSkin(ActorCustomizeMemory.Tribes.KeeperOfTheMoon, ActorCustomizeMemory.Genders.Masculine);
		// var hairMalMiqoMoon = ColorData.GetHair(ActorCustomizeMemory.Tribes.KeeperOfTheMoon, ActorCustomizeMemory.Genders.Masculine);
		Console.WriteLine("I am done?");
	}
}