using System;
using System.Collections.Generic;
using System.IO;
using Rochas.ImageGenerator.Enumerators;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace Rochas.ImageGenerator.Filters
{
	public class ImageFilter : IDisposable
	{
		#region Declarations

		private readonly IImageEncoder _imageEncoder;
		private readonly IImageDecoder _imageDecoder;

		#endregion

		#region Constructors

		public ImageFilter()
		{
		}

		public ImageFilter(ImageFormatEnum imageFormat)
		{
			switch (imageFormat)
			{
				case ImageFormatEnum.Jpg:
					_imageEncoder = new JpegEncoder()
					{
						Quality = 90
					};
					_imageDecoder = new JpegDecoder();
					break;
				case ImageFormatEnum.Png:
					_imageEncoder = new PngEncoder() { };
					_imageDecoder = new PngDecoder();
					break;
				case ImageFormatEnum.Bmp:
					_imageEncoder = new BmpEncoder() { };
					_imageDecoder = new BmpDecoder();
					break;
				case ImageFormatEnum.Gif:
					_imageEncoder = new GifEncoder() { };
					_imageDecoder = new GifDecoder();
					break;
				case ImageFormatEnum.WebP:
					_imageEncoder = new WebpEncoder() { };
					_imageDecoder = new WebpDecoder();
					break;
				default:
					_imageEncoder = new PngEncoder() { };
					_imageDecoder = new PngDecoder();
					break;
			}
		}

		#endregion

		#region Public Methods
		public byte[]? BlurImage(byte[] imageContent, int horizontalPosition, int verticalPosition, int width, int height, ImageFilterLevelEnum level)
		{
			using var stream = new MemoryStream(imageContent);
			var format = Image.DetectFormat(imageContent);
			if (format == null)
				throw new InvalidImageContentException("Invalid file format. Only accepts BMP, PNG, JPG, GIF and WEBP");

			var multiplex = Convert.ToInt32(level);
			var amount = ((float)level * 2) / (multiplex * 0.7F);

			var image = Image.Load(stream, out format);
			var rect = new Rectangle(horizontalPosition, verticalPosition, width, height);

			image.Mutate(img => img.GaussianBlur(amount, rect));

			var destinationStream = new MemoryStream();
			image.Save(destinationStream, _imageEncoder);

			return destinationStream?.ToArray();
		}

		public byte[]? BlurImage(byte[] imageContent, double horizontalPercent, double verticalPercent, ImageFilterLevelEnum level)
		{
			if (imageContent == null)
				return null;

			using (var stream = new MemoryStream(imageContent))
			{
				var format = Image.DetectFormat(imageContent);
				if (format == null)
					throw new InvalidImageContentException("Invalid file format. Only accepts BMP, PNG, JPG, GIF and WEBP");

				var image = Image.Load(stream, out format);

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

		public byte[]? RenderWaterMarkedImage(byte[] imageContent, byte[] waterMarkContent, double percPosX, double percPosY)
		{
			if ((imageContent == null) || (waterMarkContent == null))
				return null;

			var imageFormat = Image.DetectFormat(imageContent);
			var markFormat = Image.DetectFormat(waterMarkContent);
			if ((imageFormat == null) || (markFormat == null))
				throw new InvalidImageContentException("Invalid file format. Only accepts BMP, PNG, JPG, GIF and WEBP");

			using var stream = new MemoryStream(imageContent);
			var image = Image.Load(stream, out imageFormat);

			using var squareStream = new MemoryStream(waterMarkContent);
			var square = Image.Load(squareStream, out markFormat);
			var coordinate = new Point(Convert.ToInt32(image.Width * (percPosX / 100.0)),
									   Convert.ToInt32(image.Height * (percPosY / 100.0)));

			image.Mutate(img => img.DrawImage(square, coordinate, 1F));

			var destinationStream = new MemoryStream();
			image.Save(destinationStream, _imageEncoder);

			return destinationStream?.ToArray();
		}

		public string? RenderWaterMarkedImage(string base64ImageContent, string base64WaterMarkContent, int percPosX, int percPosY)
		{
			if (string.IsNullOrWhiteSpace(base64ImageContent)
				|| string.IsNullOrWhiteSpace(base64WaterMarkContent))
				return null;

			var imageBinContent = Convert.FromBase64String(base64ImageContent);
			var squareBinContent = Convert.FromBase64String(base64WaterMarkContent);

			var preResult = RenderWaterMarkedImage(imageBinContent, squareBinContent, percPosX, percPosY);
			if (preResult != null)
				return Convert.ToBase64String(preResult);
			else
				return null;
		}

		public byte[]? ApplySingleFilter(byte[] imageContent, ImageFilterTypeEnum filterType, ImageFilterLevelEnum? level = default)
		{
			using (var stream = new MemoryStream(imageContent))
			{
				var format = Image.DetectFormat(imageContent);
				if (format == null)
					throw new InvalidImageContentException("Invalid file format. Only accepts BMP, PNG, JPG, GIF and WEBP");

				var image = Image.Load(stream, out format);

				float amount = 0;
				if (level.HasValue)
					amount = (float)level / 10;

				switch (filterType)
				{
					case ImageFilterTypeEnum.Grayscale:
						image.Mutate(img => img.Grayscale(amount));
						break;
					case ImageFilterTypeEnum.Sefia:
						image.Mutate(img => img.Sepia(amount));
						break;
					case ImageFilterTypeEnum.ColorInvert:
						image.Mutate(img => img.Invert());
						break;
					case ImageFilterTypeEnum.OilPaint:
						image.Mutate(img => img.OilPaint());
						break;
					case ImageFilterTypeEnum.Lightness:
						image.Mutate(img => img.Lightness(amount));
						break;
					default:
						image.Mutate(img => img.Kodachrome());
						break;

				}

				var destinationStream = new MemoryStream();
				image.Save(destinationStream, _imageEncoder);

				return destinationStream?.ToArray();
			}
		}

		public void Dispose()
		{
			GC.ReRegisterForFinalize(this);
		}

		#endregion
	}
}
