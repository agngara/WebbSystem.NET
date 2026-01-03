using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace BL.Services
{
    
    public static class ImageUploadService
    {
        /// <summary>
        /// Checks if filename has any of the strings passed in as allowed types, in it. 
        /// </summary>
        /// <param name="allowedTypes"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool ValidateFiletype(string[] allowedTypes, string filename)
        {
            if (allowedTypes.Contains(filename))
            {
                return true;
            }
            else return false;
        }

        public static bool SaveFile(IFormFile file, string filePath)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), filePath);

            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }
            FileInfo fileInfo = new FileInfo(file.FileName);
            string fileName = file.FileName + fileInfo.Extension;
            string fileNameWithPath = path + fileName;
            try
            {
                using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return false;
            }

        }
        
    }
}
