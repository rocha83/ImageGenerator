using System;
using System.Drawing;
using QRCoder;
using Rochas.ImageGenerator.Enumerators;

namespace Rochas.ImageGenerator
{
    public class QRCodeRender : IDisposable
    {
        #region Declarations

        private readonly ImageRender _imageRender;

        #endregion

        #region Constructors

        public QRCodeRender()
        {
            _imageRender = new ImageRender(ImageFormatEnum.Png);
        }

        #endregion

        #region Public Methods

        public string GetQRCodeBase64Content(string textContent, int size = 240)
        {
            var qrCodeBitmap = RenderQRCodeBitmap(textContent, size);

            return _imageRender.GetImageBase64Content(qrCodeBitmap);
        }

        public string GetQRCodeBinaryContent(string textContent, int size = 240)
        {
            var qrCodeBitmap = RenderQRCodeBitmap(textContent, size);

            return _imageRender.GetImageBase64Content(qrCodeBitmap);
        }

        public void SaveQRCodeBase64File(string filePath, string base64Content)
        {
            _imageRender.SaveBase64ImageFile(filePath, base64Content);
        }

        public void SaveQRCodeBinaryFile(string filePath, byte[] binaryContent)
        {
            _imageRender.SaveBinaryImageFile(filePath, binaryContent);
        }

        #endregion

        #region Helper Methods

        private Bitmap RenderQRCodeBitmap(string textContent, int size)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(textContent, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            return qrCode.GetGraphic(size);
        }

        public void Dispose()
        {
            _imageRender.Dispose();
            GC.ReRegisterForFinalize(this);
        }

        #endregion
    }
}
