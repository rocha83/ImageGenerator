﻿using System;
using System.Drawing;
using QRCoder;
using Rochas.ImageGenerator.Enumerators;
using SixLabors.ImageSharp;

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

        public string GetQRCodeBase64Content(string textContent, int pixels = 5)
        {
            var qrCodeBitmap = RenderQRCodeBitmap(textContent, pixels);

            return _imageRender.GetImageBase64Content(qrCodeBitmap);
        }

        public string GetQRCodeBinaryContent(string textContent, int size = 5)
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

        private Image RenderQRCodeBitmap(string textContent, int pixels = 5)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(textContent, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            return qrCode.GetGraphic(pixels);
        }

        public void Dispose()
        {
            _imageRender.Dispose();
            GC.ReRegisterForFinalize(this);
        }

        #endregion
    }
}
