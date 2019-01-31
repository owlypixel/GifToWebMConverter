using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GifToWebMConverter.Pages
{
    public class IndexModel : PageModel
    {
        const string inputFileName = "input.gif";
        const string outputFileName = "output.webm";
        const string ffmpegPath = @"/usr/bin/ffmpeg";

        public ActionResult OnPost(string format, IFormFile gifFile)
        {
            var tempPath = _CreateTempAndClear();
            var inputFilePath = _SaveFile(gifFile, tempPath);
            var outputFilePath = Path.Combine(tempPath, outputFileName);

            using (var process = Process.Start(ffmpegPath, string.Format("-i \"{0}\" -b:v 0 -crf 25 \"{1}\"", inputFilePath, outputFilePath)))
            {
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    var stream = System.IO.File.OpenRead(outputFilePath);
                    return File(stream, "application/octet-stream", outputFileName);
                }
            }
            return Content("Unable to convert your file");
        }

        string _SaveFile(IFormFile gifFile, string tempPath)
        {
            var inputFilePath = Path.Combine(tempPath, inputFileName); // Unique filename
            using (var fileStream = gifFile.OpenReadStream())
            using (var stream = new FileStream(inputFilePath, FileMode.CreateNew))
            {
                fileStream.CopyTo(stream);
            }
            return inputFilePath;
        }

        string _CreateTempAndClear()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "GifConverter");
            try
            {
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);
                foreach (var file in Directory.GetFiles(tempPath))
                    System.IO.File.Delete(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: '{0}'", ex);
            }
            return tempPath;
        }
    }
}