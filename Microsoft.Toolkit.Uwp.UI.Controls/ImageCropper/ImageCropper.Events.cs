// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    /// <summary>
    /// The <see cref="ImageCropper"/> control allows user to crop image freely.
    /// </summary>
    public partial class ImageCropper
    {
        private void ImageCropperThumb_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var changed = false;
            var diffPos = default(Point);
            if (e.Key == VirtualKey.Left)
            {
                diffPos.X--;
                var upKeyState = Window.Current.CoreWindow.GetAsyncKeyState(VirtualKey.Up);
                var downKeyState = Window.Current.CoreWindow.GetAsyncKeyState(VirtualKey.Down);
                if (Window.Current.CoreWindow.GetAsyncKeyState(VirtualKey.Up) == CoreVirtualKeyStates.Down)
                {
                    diffPos.Y--;
                }

                if (Window.Current.CoreWindow.GetAsyncKeyState(VirtualKey.Down) == CoreVirtualKeyStates.Down)
                {
                    diffPos.Y++;
                }

                changed = true;
            }
            else if (e.Key == VirtualKey.Right)
            {
                diffPos.X++;
                var upKeyState = Window.Current.CoreWindow.GetAsyncKeyState(VirtualKey.Up);
                var downKeyState = Window.Current.CoreWindow.GetAsyncKeyState(VirtualKey.Down);
                if (upKeyState == CoreVirtualKeyStates.Down)
                {
                    diffPos.Y--;
                }

                if (downKeyState == CoreVirtualKeyStates.Down)
                {
                    diffPos.Y++;
                }

                changed = true;
            }
            else if (e.Key == VirtualKey.Up)
            {
                diffPos.Y--;
                var leftKeyState = Window.Current.CoreWindow.GetAsyncKeyState(VirtualKey.Left);
                var rightKeyState = Window.Current.CoreWindow.GetAsyncKeyState(VirtualKey.Right);
                if (leftKeyState == CoreVirtualKeyStates.Down)
                {
                    diffPos.X--;
                }

                if (rightKeyState == CoreVirtualKeyStates.Down)
                {
                    diffPos.X++;
                }

                changed = true;
            }
            else if (e.Key == VirtualKey.Down)
            {
                diffPos.Y++;
                var leftKeyState = Window.Current.CoreWindow.GetAsyncKeyState(VirtualKey.Left);
                var rightKeyState = Window.Current.CoreWindow.GetAsyncKeyState(VirtualKey.Right);
                if (leftKeyState == CoreVirtualKeyStates.Down)
                {
                    diffPos.X--;
                }

                if (rightKeyState == CoreVirtualKeyStates.Down)
                {
                    diffPos.X++;
                }

                changed = true;
            }

            if (changed)
            {
                var imageCropperThumb = (ImageCropperThumb)sender;
                UpdateCroppedRect(imageCropperThumb.Position, diffPos);
            }
        }

        private void ImageCropperThumb_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var inverseImageTransform = _imageTransform.Inverse;
            if (inverseImageTransform != null)
            {
                var selectedRect = new Rect(new Point(_startX, _startY), new Point(_endX, _endY));
                var croppedRect = inverseImageTransform.TransformBounds(selectedRect);
                if (croppedRect.Width > MinCropSize.Width && croppedRect.Height > MinCropSize.Height)
                {
                    croppedRect.Intersect(_restrictedCropRect);
                    _currentCroppedRect = croppedRect;
                }

                UpdateImageLayout(true);
            }
        }

        private void ImageCropperThumb_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var inverseImageTransform = _imageTransform.Inverse;
            if (inverseImageTransform != null)
            {
                var selectedRect = new Rect(new Point(_startX, _startY), new Point(_endX, _endY));
                var croppedRect = inverseImageTransform.TransformBounds(selectedRect);
                if (croppedRect.Width > MinCropSize.Width && croppedRect.Height > MinCropSize.Height)
                {
                    croppedRect.Intersect(_restrictedCropRect);
                    _currentCroppedRect = croppedRect;
                }

                UpdateImageLayout(true);
            }
        }

        private void ImageCropperThumb_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var imageCropperThumb = (ImageCropperThumb)sender;
            var currentPointerPosition = new Point(
                imageCropperThumb.X + e.Position.X + e.Delta.Translation.X - (imageCropperThumb.ActualWidth / 2),
                imageCropperThumb.Y + e.Position.Y + e.Delta.Translation.Y - (imageCropperThumb.ActualHeight / 2));
            var safePosition = GetSafePoint(_restrictedSelectRect, currentPointerPosition);
            var safeDiffPoint = new Point(safePosition.X - imageCropperThumb.X, safePosition.Y - imageCropperThumb.Y);
            UpdateCroppedRect(imageCropperThumb.Position, safeDiffPoint);
        }

        private void SourceImage_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var diffPos = e.Delta.Translation;
            var inverseImageTransform = _imageTransform.Inverse;
            if (inverseImageTransform != null)
            {
                var startPoint = new Point(_startX - diffPos.X, _startY - diffPos.Y);
                var endPoint = new Point(_endX - diffPos.X, _endY - diffPos.Y);
                if (IsSafePoint(_restrictedSelectRect, startPoint) && IsSafePoint(_restrictedSelectRect, endPoint))
                {
                    var selectedRect = new Rect(startPoint, endPoint);
                    if ((selectedRect.Width - MinSelectSize.Width) < -0.001 || (selectedRect.Height - MinSelectSize.Height) < -0.001)
                    {
                        return;
                    }

                    var movedRect = inverseImageTransform.TransformBounds(selectedRect);
                    movedRect.Intersect(_restrictedCropRect);
                    _currentCroppedRect = movedRect;
                    UpdateImageLayout();
                }
            }
        }

        private void ImageCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Source == null)
            {
                return;
            }

            UpdateImageLayout();
            UpdateMaskArea();
        }
    }
}