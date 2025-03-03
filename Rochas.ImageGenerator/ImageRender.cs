using System;
using System.Net;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Rochas.ImageGenerator.Enumerators;

namespace Rochas.ImageGenerator
{
    public class ImageRender : IDisposable
    {
        #region Declarations

        private readonly ImageFormat? _imageFormat;
        
        #endregion

        #region Constructors

        public ImageRender(ImageFormatEnum imageFormat)
        {
            switch(imageFormat)
            {
                case ImageFormatEnum.Jpg:
                    _imageFormat = ImageFormat.Jpeg;
                    break;
                case ImageFormatEnum.Png:
                    _imageFormat = ImageFormat.Png;
                    break;
                case ImageFormatEnum.Bmp:
                    _imageFormat = ImageFormat.Bmp;
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

        public string GetImageBase64Content(Bitmap imageBitmap)
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

        public byte[] GetImageBinaryContent(Bitmap imageBitmap)
        {
            using var memStream = new MemoryStream();
            imageBitmap.Save(memStream, _imageFormat);

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

		public byte[] RenderWaterMarkedImage(byte[] imageContent, byte[] waterMarkContent, double percPosX, double percPosY)
		{
			if ((imageContent == null) || (waterMarkContent == null))
				return null;

			using var stream = new MemoryStream(imageContent);
			var image = Image.FromStream(stream);

			using var squareStream = new MemoryStream(waterMarkContent);
			var square = Image.FromStream(squareStream);

			var render = Graphics.FromImage(image);
			render.DrawImage(square, Convert.ToInt32(image.Width * (percPosX / 100.0)),
									 Convert.ToInt32(image.Height * (percPosY / 100.0)));

			var result = new ImageConverter().ConvertTo(image, typeof(byte[])) as byte[];

			return result;
		}

		public string RenderWaterMarkedImage(string base64ImageContent, string base64WaterMarkContent, int percPosX, int percPosY, int maxWidth = 0)
		{
			if (string.IsNullOrWhiteSpace(base64ImageContent)
				|| string.IsNullOrWhiteSpace(base64WaterMarkContent))
				return null;

			var imageBinContent = Convert.FromBase64String(base64ImageContent);
			var squareBinContent = Convert.FromBase64String(base64WaterMarkContent);

			var preResult = RenderWaterMarkedImage(imageBinContent, squareBinContent, percPosX, percPosY);

			return GetImageBase64Content(preResult, maxWidth);
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
            var fileStream  = File.Open(filePath, FileMode.Open);

            fileStream.CopyTo(memStream);

            return memStream.ToArray();
        }
        
        private byte[] GetImageScaleResult(MemoryStream memStream,  byte[] imageBytes, int maxWidth)
        {
            var image = ScaleImage(imageBytes, maxWidth);

            image.Save(memStream, _imageFormat);

            return memStream.ToArray();
        }

        private byte[] GetImageScaleResult(MemoryStream memStream, Stream imageStream, int maxWidth)
        {
            var image = ScaleImage(imageStream, maxWidth);

            image.Save(memStream, _imageFormat);

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

            var image = Image.FromStream(imageStream);

            if ((maxWidth > 0) && (image.Width > maxWidth))
            {
                var imageWidth = double.Parse(image.Width.ToString());
                var imageHeight = double.Parse(image.Height.ToString());

                var imageAspect = imageWidth / maxWidth;
                var newWidth = (imageWidth / imageAspect);
                var newHeight = (imageHeight / imageAspect);

                var intWidth = int.Parse(Math.Round(newWidth, 0).ToString());
                var intHeight = int.Parse(Math.Round(newHeight, 0).ToString());

                result = new Bitmap(intWidth, intHeight);
                
                using var graph = Graphics.FromImage(result);
                graph.DrawImage(image, 0, 0, intWidth, intHeight);
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
