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
        /// Saves a file to the wwwroot folder. 
        /// </summary>
        /// <param name="file">File to be saved.</param>
        /// <param name="relativePath">Relative path to the file directory. One will be created if it does not already exist..</param>
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
        /// <summary>
        /// Delete a file from the wwwroot folder.
        /// </summary>
        /// <param name="fileDirectory">Relative path to the directory of the file.</param>
        /// <param name="fileName"></param>
        /// <returns>True if file is deleted correctly, otherwise false.</returns>
        public static bool DeleteFile(string fileDirectory, string fileName)
        {
            string wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string fullPath = Path.Combine(wwwRoot, fileDirectory.TrimStart('/', '\\'), fileName);

            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                else 
                {
                    return false;
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
