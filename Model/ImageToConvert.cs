using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Http;
namespace Img2Txt.Models
{
    public class ImageToConvert
    {
        [FileExtensions(Extensions = "jpg,jpeg,png,gif")]
        public IFormFile File { get; set; }
		
		[DataType(DataType.Url)]
		public string UrlOfTheImage { get; set; }
    }
}