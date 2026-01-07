using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace BL.Services
{

    public static class FileService
    {
        /// <summary>
        /// Checks if filename has any of the strings passed in as allowed types, in it. 
        /// </summary>
        /// <param name="allowedTypes"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool ValidateFiletype(string[] allowedTypes, string filename)
        {
            string extension = Path.GetExtension(filename);

            return allowedTypes.Contains(extension);
        }
        /// <summary>
        /// Saves a file to a wwwroot folder. 
        /// </summary>
        /// <param name="file">File to be saved.</param>
        /// <param name="relativePath">Relative path, in other words, not including wwwroot or disk.</param>
        /// <returns>Relative path to saved file. Returns an empty string if an exception occurs.</returns>
        public static string SaveFile(IFormFile file, string relativePath)
        {

            string wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string fullPath = Path.Combine(wwwRoot, relativePath);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string exactPath = Path.Combine(fullPath, fileName);
            try
            {
                using (var stream = new FileStream(exactPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                    return "/" + Path.Combine(relativePath, fileName).Replace("\\", "/");
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return "";
            }
        }

        //public static IFormFile GetFile(string relativePath)
        //{
        //    string wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        //    string fullPath = Path.Combine(wwwRoot, relativePath);
        //    using BinaryReader binaryReader = new BinaryReader(new  FileStream(fullPath, FileMode.Open, FileAccess.Read));

        //    IFormFile = binaryReader.Read();

        //}
    }
}
