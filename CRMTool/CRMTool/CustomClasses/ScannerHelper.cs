using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WIA;
namespace Licencjat_new.CustomClasses
{
    static class ScannerHelper
    {
        #region Variables
        const string WIA_SCAN_COLOR_MODE = "6146";
        const string WIA_HORIZONTAL_SCAN_RESOLUTION_DPI = "6147";
        const string WIA_VERTICAL_SCAN_RESOLUTION_DPI = "6148";
        const string WIA_HORIZONTAL_SCAN_START_PIXEL = "6149";
        const string WIA_VERTICAL_SCAN_START_PIXEL = "6150";
        const string WIA_HORIZONTAL_SCAN_SIZE_PIXELS = "6151";
        const string WIA_VERTICAL_SCAN_SIZE_PIXELS = "6152";
        const string WIA_SCAN_BRIGHTNESS_PERCENTS = "6154";
        const string WIA_SCAN_CONTRAST_PERCENTS = "6155";
        #endregion

        #region Methods
        public static Image Scan()
        {
            try
            {
                CommonDialog commonDialog = new CommonDialog();
                Device scannerDevice = commonDialog.ShowSelectDevice(WiaDeviceType.ScannerDeviceType);
                if (scannerDevice != null)
                {
                    Item scannerItem = scannerDevice.Items[1];
                    SetDefaultScannerProperties(scannerItem);
                    ImageFile scanResult = scannerItem.Transfer("{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}");
                    //ImageFile scanResult = (ImageFile)commonDialog.ShowTransfer(scannnerItem, "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}", false);
                    if (scanResult != null)
                    {
                        Image img;
                        lock (scanResult)
                        {

                            ImageFile image = (ImageFile) scanResult;
                            MemoryStream stream = new MemoryStream((byte[]) image.FileData.get_BinaryData());
                            img = Image.FromStream(stream);
                        }

                        //ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

                        //System.Drawing.Imaging.Encoder myEncoder =
                        //    System.Drawing.Imaging.Encoder.Quality;

                        //EncoderParameters myEncoderParameters = new EncoderParameters(1);

                        //EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
                        //myEncoderParameters.Param[0] = myEncoderParameter;

                        return img;

                        //img.Save(@"C:\Users\Marcin\Desktop\TestTest.jpg", jpgEncoder, myEncoderParameters);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static void SetDefaultScannerProperties(Item scanner)
        {
            SetWIAProperty(scanner.Properties, WIA_HORIZONTAL_SCAN_RESOLUTION_DPI, 300);
            SetWIAProperty(scanner.Properties, WIA_VERTICAL_SCAN_RESOLUTION_DPI, 300);
            SetWIAProperty(scanner.Properties, WIA_HORIZONTAL_SCAN_START_PIXEL, 0);
            SetWIAProperty(scanner.Properties, WIA_VERTICAL_SCAN_START_PIXEL, 0);
            SetWIAProperty(scanner.Properties, WIA_HORIZONTAL_SCAN_SIZE_PIXELS, 8.3* 300);
            SetWIAProperty(scanner.Properties, WIA_VERTICAL_SCAN_SIZE_PIXELS, 11.67* 300);
            SetWIAProperty(scanner.Properties, WIA_SCAN_BRIGHTNESS_PERCENTS, 0);
            SetWIAProperty(scanner.Properties, WIA_SCAN_CONTRAST_PERCENTS, 0);
            SetWIAProperty(scanner.Properties, WIA_SCAN_COLOR_MODE, 1);
        }

        private static void SetWIAProperty(IProperties properties, object propName, object propValue)
        {
            Property prop = properties[propName];
            prop.set_Value(ref propValue);
        }
        #endregion
    }
}
