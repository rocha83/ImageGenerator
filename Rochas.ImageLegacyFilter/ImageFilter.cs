﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Accord.Imaging.Filters;
using Rochas.ImageLegacyFilter.Enumerators;

namespace Rochas.ImageLegacyFilter
{
	public class ImageFilter
	{
		public ImageFilter() { 
		
		}

		public byte[] BlurImage(byte[] imageContent, int horizontalPosition, int verticalPosition, int width, int height, ImageBlurLevelEnum level)
		{
			using (var stream = new MemoryStream(imageContent))
			{
				var image = new Bitmap(Image.FromStream(stream));

				KeyValuePair<double, int> blurLevelConfig;
				switch (level)
				{
					case ImageBlurLevelEnum.Light:
						blurLevelConfig = new KeyValuePair<double, int>(1, 7);
						break;
					case ImageBlurLevelEnum.Medium:
						blurLevelConfig = new KeyValuePair<double, int>(3, 14);
						break;
					case ImageBlurLevelEnum.Hard:
						blurLevelConfig = new KeyValuePair<double, int>(5, 21);
						break;
					default:
						blurLevelConfig = new KeyValuePair<double, int>(3, 14);
						break;
				}

				var filter = new GaussianBlur(blurLevelConfig.Key, blurLevelConfig.Value);

				var rect = new Rectangle(horizontalPosition, verticalPosition, width, height);
				filter.ApplyInPlace(image, rect);

				return new ImageConverter().ConvertTo(image, typeof(byte[])) as byte[];
			}
		}

		public byte[] BlurImage(byte[] imageContent, double horizontalPercent, double verticalPercent, ImageBlurLevelEnum level)
		{
			if (imageContent == null)
				return null;

			using (var stream = new MemoryStream(imageContent))
			{
				var image = Image.FromStream(stream);

				var srcWidthSlice = Convert.ToInt32(image.Width * (horizontalPercent / 100));
				var srcHeightSlice = Convert.ToInt32(image.Height * (verticalPercent / 100));
				var horizontalFillSize = Convert.ToInt32(100.0 / horizontalPercent) - 1;
				var verticalFillSize = Convert.ToInt32(100.0 / verticalPercent) - 1;
				var horizontalPosition = srcWidthSlice * horizontalFillSize;
				var verticalPosition = srcHeightSlice * verticalFillSize;

				return BlurImage(imageContent, horizontalPosition, verticalPosition,
								 srcWidthSlice, srcHeightSlice, level);
			}
		}
	}
}
