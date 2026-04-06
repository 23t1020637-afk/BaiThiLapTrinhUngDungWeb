using Microsoft.AspNetCore.Html;
using System.Drawing;
using System.Globalization;
using static System.Net.WebRequestMethods;

namespace SV23T1020637.Admin.AppCodes
{
    public static class Converter
    {
        /// <summary>
        /// Chuyển chuỗi s sang giá trị kiểu DateTime (nếu chuyển không thành công
        /// thì trả về giá trị null)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="formats"></param>
        /// <returns></returns>
        public static DateTime? ToDateTime(this string s, string formats = "d/M/yyyy;d-M-yyyy;d.M.yyyy")
        {
            try
            {
                return DateTime.ParseExact(s, formats.Split(';'), CultureInfo.InvariantCulture);

            }
            catch
            {
                return null;
            }
        }
        public static string FormatVND(this decimal amount)
        {
            return string.Format(new CultureInfo("vi-VN"), "{0:N0} ₫", amount);
        }
        public static HtmlString RequiredAsterisk()
        {
            string html = $"(<i class=\"bi bi-asterisk text-danger\" style=\"font-size: 0.7rem;\" title=\"Bắt buộc nhập\"></i>)";
            return new HtmlString(html);
        }

    }
}
