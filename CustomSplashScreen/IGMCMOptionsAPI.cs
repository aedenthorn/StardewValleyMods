// Copyright 2022-2023 Jamie Taylor
//
// To facilitate other mods which would like to use the GMCMOptions API,
// the license for this file (and only this file) is modified by removing the
// notice requirements for binary distribution.  The license (as amended)
// is included below, making this file self-contained.
//
// In other words, anyone may copy this file into their own mod (and edit
// it if they want, e.g. to remove the methods they are not using, so long
// as the license comment is retained).(If all you want in your mod is
// _just_ the function declaration(s) and not any comments or other creative
// expression that may be in the file, then that is permissible fair use
// as a matter of law in the US according to Google v. Oracle, 593 U.S. ___ (2021),
// and does not require any license.)
//

//  Copyright(c) 2022-2023, Jamie Taylor
//All rights reserved.
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1.Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
//2. [condition removed for this file]
//
//3. Neither the name of the copyright holder nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
//AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
//IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
//FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
//DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
//SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
//CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
//OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
//OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace CustomSplashScreen
{
    /// <summary>The API which lets other mods add a config UI using one of the complex options defined in GMCMOptions.</summary>
    public interface IGMCMOptionsAPI
    {
        /// <summary>Add a <c cref="Color">Color</c> option at the current position in the GMCM form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="showAlpha">Whether the color picker should allow setting the Alpha channel</param>
        /// <param name="colorPickerStyle">Flags to control how the color picker is rendered.  <see cref="ColorPickerStyle"/></param>
        /// <param name="fieldId">The unique field ID for use with GMCM's <c>OnFieldChanged</c>, or <c>null</c> to auto-generate a randomized ID.</param>
        /// <param name="drawSample">
        ///   A function to draw a sample of the current color.  The arguments are the SpriteBatch, x and y coordinates
        ///   of the top left corner of the area in which to draw the sample, and the Color to render.
        ///   Passing <c>null</c> is equivalent to passing the result of <c>MakeColorSwatchDrawer()</c>.
        /// </param>
        void AddColorOption(IManifest mod, Func<Color> getValue, Action<Color> setValue, Func<string> name,
            Func<string>? tooltip = null, bool showAlpha = true, uint colorPickerStyle = 0, string? fieldId = null,
            Action<SpriteBatch, int, int, Color>? drawSample = null);

        #pragma warning disable format
        /// <summary>
        /// Flags to control how the <c cref="ColorPickerOption">ColorPickerOption</c> widget is displayed.
        /// </summary>
        [Flags]
        public enum ColorPickerStyle : uint {
            Default = 0,
            RGBSliders    = 0b00000001,
            HSVColorWheel = 0b00000010,
            HSLColorWheel = 0b00000100,
            AllStyles     = 0b11111111,
            NoChooser     = 0,
            RadioChooser  = 0b01 << 8,
            ToggleChooser = 0b10 << 8
        }
        #pragma warning restore format

        /// <summary>
        ///   Return a function (suitable for passing as the <c>drawSample</c> parameter of <c>AddColorOption</c>)
        ///   that draws a color swatch.
        /// </summary>
        /// <param name="drawBackground">
        ///   A function that draws the background of the color swatch.  By default (i.e., if passed <c>null</c>),
        ///   this draws a black and white checkerboard pattern.
        /// </param>
        /// <param name="drawForeground">
        ///   A function that draws the foreground of the color swatch.  By default (i.e., if passed <c>null</c>),
        ///   this draws a square of the given Color.
        /// </param>
        /// <returns>A function that draws a color swatch</returns>
        Action<SpriteBatch, int, int, Color> MakeColorSwatchDrawer(
            Action<SpriteBatch, Rectangle>? drawBackground = null,
            Action<SpriteBatch, Rectangle, Color>? drawForeground = null);

        /// <summary>
        /// Add an image picker option.  This is really an "array index picker" where you can specify what to draw
        /// for each index.  The underlying value is always a <c>uint</c> (the index).
        /// </summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="getMaxValue">
        ///   The maximum value this option can have, and thus the maximum value that will be passed to
        ///   <paramref name="drawImage"/> and <paramref name="label"/>.  Note that this is a function, so
        ///   theoretically the number of options does not have to be fixed.  Should this function return a
        ///   value greater than the option's current value then the option's current value will be clamped.
        ///   In common usage, this parameter should be a function that returns one less than the number
        ///   of images.
        /// </param>
        /// <param name="maxImageHeight">
        ///   A function that returns the maximum image height.  Used to report the option's height to GMCM (which
        ///   before version 1.8.2 will not recompute how much space to reserve for the option until the page is re-opened) and to center
        ///   arrows vertically in the <c cref="ImageOptionArrowLocation.Sides">Sides</c> arrow placement option.
        /// </param>
        /// <param name="maxImageWidth">
        ///   A function that returns the maximum image width.  This is used to place the arrows and label.
        /// </param>
        /// <param name="drawImage">A function which draws the image for the given index at the given location</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="label">A function to return the string to display given the image index, or <c>null</c> to disable that display.</param>
        /// <param name="imageTooltipTitle">
        ///   A function to return the string to use as the tooltip title when hovering over the image itself.
        ///   A <c>null</c> value or returning a <c>null</c> string will fall back to the value returned by
        ///   <paramref name="label"/>, or the empty string if that is null.  Whether the tooltip is displayed
        ///   is controlled by the <paramref name="imageTooltipText"/> parameter.
        /// </param>
        /// <param name="imageTooltipText">
        ///   A function to return the string to use as the tooltip text when hovering over the image itself.
        ///   A <c>null</c> value or returning a <c>null</c> string disables the tooltip.
        /// </param>
        /// <param name="arrowLocation">Where to render the arrows.  Use a value from the <c cref="ImageOptionArrowLocation">ImageOptionArrowLocation</c> enum.</param>
        /// <param name="labelLocation">Where to render the label.  Use a value from the <c cref="ImageOptionLabelLocation">ImageOptionLabelLocation</c> enum.</param>
        /// <param name="fieldId">The unique field ID for use with GMCM's <c>OnFieldChanged</c>, or <c>null</c> to auto-generate a randomized ID.</param>
        void AddImageOption(IManifest mod,
                            Func<uint> getValue,
                            Action<uint> setValue,
                            Func<string> name,
                            Func<uint> getMaxValue,
                            Func<int> maxImageHeight,
                            Func<int> maxImageWidth,
                            Action<uint, SpriteBatch, Vector2> drawImage,
                            Func<string>? tooltip = null,
                            Func<uint, String?>? label = null,
                            Func<uint, String?>? imageTooltipTitle = null,
                            Func<uint, String?>? imageTooltipText = null,
                            int arrowLocation = (int)ImageOptionArrowLocation.Top,
                            int labelLocation = (int)ImageOptionLabelLocation.Top,
                            string? fieldId = null);

        /// <summary>
        /// Add an image picker option.  A simplified interface to the full <c>AddImageOption</c> signature.
        /// To use this signature, you supply a function that returns an array of tuples containing the
        /// different image <paramref name="choices"/>.  The underlying value is the <c>uint</c> that is the
        /// index of the selected image.
        /// </summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="choices">
        ///   A function that returns an array of tuples describing the image choices.  Each tuple contains:
        ///   <list type="bullet">
        ///     <item>A function to return the label string (or <c>null</c> for no label)</item>
        ///     <item>The <c cref="Texture2D">Texture2D</c> containing the image (i.e., the sprite sheet)</item>
        ///     <item>The source rectangle for the image within the texture, or <c>null</c> to indicate the entire texture</item>
        ///   </list>
        /// </param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="imageTooltipTitle">
        ///   A function to return the string to use as the tooltip title when hovering over the image itself.
        ///   A <c>null</c> value or returning a <c>null</c> string will fall back to the value returned by
        ///   <paramref name="label"/>, or the empty string if that is null.  Whether the tooltip is displayed
        ///   is controlled by the <paramref name="imageTooltipText"/> parameter.
        /// </param>
        /// <param name="imageTooltipText">
        ///   A function to return the string to use as the tooltip text when hovering over the image itself.
        ///   A <c>null</c> value or returning a <c>null</c> string disables the tooltip.
        /// </param>
        /// <param name="arrowLocation">Where to render the arrows.  Use a value from the <c cref="ImageOptionArrowLocation">ImageOptionArrowLocation</c> enum.</param>
        /// <param name="labelLocation">Where to render the label.  Use a value from the <c cref="ImageOptionLabelLocation">ImageOptionLabelLocation</c> enum.</param>
        /// <param name="fieldId">The unique field ID for use with GMCM's <c>OnFieldChanged</c>, or <c>null</c> to auto-generate a randomized ID.</param>
        void AddImageOption(IManifest mod,
                            Func<uint> getValue,
                            Action<uint> setValue,
                            Func<string> name,
                            Func<(Func<String?> label, Texture2D sheet, Rectangle? sourceRect)[]> choices,
                            Func<string>? tooltip = null,
                            Func<uint, String?>? imageTooltipTitle = null,
                            Func<uint, String?>? imageTooltipText = null,
                            int arrowLocation = (int)ImageOptionArrowLocation.Top,
                            int labelLocation = (int)ImageOptionLabelLocation.Top,
                            string? fieldId = null);
        /// <summary>
        /// Valid values for the <c>arrowLocation</c> parameter of <c>AddImageOption</c>
        /// </summary>
        public enum ImageOptionArrowLocation
        {
            Top = -1,
            Sides = 0,
            Bottom = 1
        }
        /// <summary>
        /// Valid values for the <c>labelLocation</c> parameter of <c>AddImageOption</c>
        /// </summary>
        public enum ImageOptionLabelLocation
        {
            Top = -1,
            None = 0,
            Bottom = 1
        }

        /// <summary>
        /// Add a horizontal separator.
        /// </summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getWidthFraction">
        ///   A function that returns the fraction of the GMCM window that the separator
        ///   should occupy.  1.0 is the entire window.  Defaults to 0.85.
        /// </param>
        /// <param name="height">The height of the separator (in pixels)</param>
        /// <param name="padAbove">How much padding (in pixels) to place above the separator</param>
        /// <param name="padBelow">How much padding (in pixels) to place below the separator</param>
        /// <param name="alignment">
        ///   The horizontal alignment of the separator.
        ///   Use a value from the <c cref="HorizontalAlignment">HorizontalAlignment enumeration</c>.
        /// </param>
        /// <param name="getColor">
        ///   A function to return the color to use for the separator.  Defaults to the game's text color.
        /// </param>
        /// <param name="getShadowColor">
        ///   A function to return the color to use for the shadow drawn under the separator.  Defaults to the
        ///   game's text shadow color.  Return <c>Color.Transparent</c> to remove the shadow completely.
        /// </param>
        void AddHorizontalSeparator(IManifest mod,
                                    Func<double>? getWidthFraction = null,
                                    int height = 3,
                                    int padAbove = 0,
                                    int padBelow = 0,
                                    int alignment = (int)HorizontalAlignment.Center,
                                    Func<Color>? getColor = null,
                                    Func<Color>? getShadowColor = null);

        /// <summary>
        ///   Add a horizontal separator.  This is a simplified version of
        ///   <c cref="AddHorizontalSeparator(IManifest, Func{double}, int, int, int, int, Func{Color}, Func{Color})">AddHorizontalSeparator</c>.
        /// </summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="widthFraction">
        ///   The fraction of the GMCM window that the separator
        ///   should occupy.  1.0 is the entire window.  Defaults to 0.85.</param>
        /// <param name="height">The height of the separator (in pixels)</param>
        /// <param name="padAbove">How much padding (in pixels) to place above the separator</param>
        /// <param name="padBelow">How much padding (in pixels) to place below the separator</param>
        /// <param name="alignment">
        ///   The horizontal alignment of the separator.
        ///   Use a value from the <c cref="HorizontalAlignment">HorizontalAlignment enumeration</c>.
        /// </param>
        /// <param name="color">The color to use for the separator.  Defaults to the game's text color.</param>
        /// <param name="shadowColor">
        ///   The color to use for the shadow drawn under the separator.  Defaults to the
        ///   game's text shadow color.  Use <c>Color.Transparent</c> to remove the shadow completely.
        /// </param>
        void AddSimpleHorizontalSeparator(IManifest mod,
                                          double widthFraction = 0.85,
                                          int height = 3,
                                          int padAbove = 0,
                                          int padBelow = 0,
                                          int alignment = (int)HorizontalAlignment.Center,
                                          Color? color = null,
                                          Color? shadowColor = null);

        /// <summary>
        /// Valid values for the <c>alignment</c> parameter of <c>AddHorizontalSeparator</c> and <c>AddSimpleHorizontalSeparator</c>
        /// </summary>
        public enum HorizontalAlignment
        {
            Left = -1,
            Center = 0,
            Right = 1
        }

        /// <summary>
        ///   Add a dynamic paragraph.  A dynamic paragraph reflects changes in the text returned by
        ///   <paramref name="text"/> even while the GMCM window is open.  It also supports styled text.
        ///   <para>
        ///     Styled text supports simple HTML-like markup for specifying text formatting.  The text must
        ///     be valid XML fragment(s).  (I.e., if the text were enclosed in an XML tag, the result must be
        ///     a valid XML document.)  See https://github.com/jltaylor-us/StardewGMCMOptions/blob/default/README.md#dynamic-paragraph
        ///     for details about the tags supported.
        ///   </para>
        /// </summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="logName">
        ///   A name to identify <em>this</em> dynamic paragraph in the SMAPI log, should there be any errors in
        ///   the text returned by <paramref name="text"/>.  This string may appear in the log, but will not appear
        ///   in-game.
        /// </param>
        /// <param name="text">The paragraph text.</param>
        /// <param name="isStyledText">
        ///   If <c>true</c>, then the text returned by <paramref name="text"/> will be treated as styled text.
        /// </param>
        void AddDynamicParagraph(IManifest mod,
                                 string logName,
                                 Func<string> text,
                                 bool isStyledText);

    }

}
