﻿using Patagames.Ocr;
using Patagames.Ocr.Enums;
using System;
using System.Drawing;

namespace ValidateCodeProcessor
{
    public class OcrProcessor : IDisposable
    {
        public static OcrProcessor Instance { get; } = new OcrProcessor();

        private OcrProcessor()
        {
            ocrApi = OcrApi.Create();

            ocrApi.Init(Languages.English);
            ocrApi.SetVariable("tessedit_char_whitelist", "01234567890");
        }

        private OcrApi ocrApi;

        public string GetTextFromImage(string filename)
        {
            return ocrApi.GetTextFromImage(filename);
        }

        public string GetTextFromImage(Bitmap bitmap)
        {
            return ocrApi.GetTextFromImage(bitmap);
        }

        public void Dispose()
        {
            ocrApi.Dispose();
        }
    }
}
