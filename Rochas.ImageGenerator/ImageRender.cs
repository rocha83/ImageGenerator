using System;
using System.Net;
using System.IO;
using SixLabors.ImageSharp;
using Rochas.ImageGenerator.Enumerators;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats;

namespace Rochas.ImageGenerator
{
	public class ImageRender : IDisposable
	{
		#region Declarations

		private readonly IImageEncoder? _imageFormat;

		#endregion

		#region Constructors

		public ImageRender(ImageFormatEnum imageFormat)
		{
			switch (imageFormat)
			{
				case ImageFormatEnum.Jpg:
					_imageFormat = new JpegEncoder()
					{
						Quality = 90
					};
					break;
				case ImageFormatEnum.Png:
					_imageFormat = new PngEncoder() { };
					break;
				case ImageFormatEnum.Bmp:
					_imageFormat = new BmpEncoder() {};
					break;
				case ImageFormatEnum.Gif:
					_imageFormat = new GifEncoder() { };
					break;
				case ImageFormatEnum.WebP:
					_imageFormat = new WebpEncoder() { };
					break;
			}
		}

		#endregion

		#region Public Methods

		#region Base64 Image Methods

		public string GetImageBase64Content(byte[] imageBytes, int maxWidth = 0)
		{
			using var memStream = new MemoryStream();
			var result = GetImageScaleResult(memStream, imageBytes, maxWidth);

			return RenderBase64Image(result);
		}

		public string GetImageBase64Content(Stream imageStream, int maxWidth = 0)
		{
			using var memStream = new MemoryStream();
			var result = GetImageScaleResult(memStream, imageStream, maxWidth);

			return RenderBase64Image(result);
		}

		public string GetImageBase64Content(Image imageBitmap)
		{
			using var memStream = new MemoryStream();
			imageBitmap.Save(memStream, _imageFormat);

			var result = memStream.ToArray();

			return RenderBase64Image(result);
		}

		public string GetImageUrlBase64Content(string imageUrl, int maxWidth = 0)
		{
			var result = GetImageUrlBinaryContent(imageUrl, maxWidth);

			return RenderBase64Image(result);
		}

		public string GetImageFileBase64Content(string filePath, int maxWidth = 0)
		{
			var result = GetImageFileBinaryContent(filePath, maxWidth);

			return RenderBase64Image(result);
		}

		#endregion

		#region Binary Image Methods

		public byte[] GetImageBinaryContent(string base64Image, int maxWidth = 0)
		{
			var imageBytes = RenderBinaryImage(base64Image);

			using var memStream = new MemoryStream();
			return GetImageScaleResult(memStream, imageBytes, maxWidth);
		}

		public byte[] GetImageBinaryContent(Image imageBitmap)
		{
			using var memStream = new MemoryStream();
			imageBitmap.Save(memStream, new JpegEncoder() { Quality = 90 });

			return memStream.ToArray();
		}

		public byte[] GetImageUrlBinaryContent(string imageUrl, int maxWidth = 0)
		{
			byte[]? result = null;

			using (var memStream = new MemoryStream())
			{
				using var webClient = new WebClient();
				var imageBytes = webClient.DownloadData(imageUrl);

				result = GetImageScaleResult(memStream, imageBytes, maxWidth);
			}

			return result;
		}

		public byte[] GetImageFileBinaryContent(string filePath, int maxWidth = 0)
		{
			var imageBytes = GetImageFileContent(filePath);

			using var memStream = new MemoryStream();
			return GetImageScaleResult(memStream, imageBytes, maxWidth);
		}

		#endregion

		#region Image File Methods

		public void SaveBase64ImageFile(string filePath, string base64Image)
		{
			var imageBytes = RenderBinaryImage(base64Image);

			File.WriteAllBytes(filePath, imageBytes);
		}

		public void SaveBinaryImageFile(string filePath, byte[] imageBytes)
		{
			var base64Image = RenderBase64Image(imageBytes);

			File.WriteAllText(filePath, base64Image);
		}

		#endregion

		#endregion

		#region Helper Methods

		private string RenderBase64Image(byte[] imageContent)
		{
			return Convert.ToBase64String(imageContent);
		}

		private byte[] RenderBinaryImage(string base64Content)
		{
			return Convert.FromBase64String(base64Content);
		}

		private byte[] GetImageFileContent(string filePath)
		{
			using var memStream = new MemoryStream();
			var fileStream = File.Open(filePath, FileMode.Open);

			fileStream.CopyTo(memStream);

			return memStream.ToArray();
		}

		private byte[] GetImageScaleResult(MemoryStream memStream, byte[] imageBytes, int maxWidth)
		{
			var image = ScaleImage(imageBytes, maxWidth);

			image.SaveAsJpeg(memStream, new JpegEncoder() { Quality = 90 });

			return memStream.ToArray();
		}

		private byte[] GetImageScaleResult(MemoryStream memStream, Stream imageStream, int maxWidth)
		{
			var image = ScaleImage(imageStream, maxWidth);

			image.SaveAsJpeg(memStream, new JpegEncoder() { Quality = 90 });

			return memStream.ToArray();
		}

		private Image ScaleImage(byte[] imageBytes, int maxWidth)
		{
			Image? result;

			var imageStream = new MemoryStream(imageBytes);

			result = ScaleImage(imageStream, maxWidth);

			return result;
		}

		private Image ScaleImage(Stream imageStream, int maxWidth)
		{
			Image? result;

			var image = Image.Load(imageStream);

			if ((maxWidth > 0) && (image.Width > maxWidth))
			{
				var imageWidth = double.Parse(image.Width.ToString());
				var imageHeight = double.Parse(image.Height.ToString());

				var imageAspect = imageWidth / maxWidth;
				var newWidth = (imageWidth / imageAspect);
				var newHeight = (imageHeight / imageAspect);

				var intWidth = int.Parse(Math.Round(newWidth, 0).ToString());
				var intHeight = int.Parse(Math.Round(newHeight, 0).ToString());

				image.Mutate(img => img.Resize(intWidth, intHeight));

				return image;
			}
			else
				result = image;

			return result;
		}

		public void Dispose()
		{
			GC.ReRegisterForFinalize(this);
		}

		#endregion
	}
}
