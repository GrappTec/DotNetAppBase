#region License

// Copyright(c) 2020 GrappTec
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion

using System.ComponentModel.DataAnnotations;
using DotNetAppBase.Std.Library.Properties;

namespace DotNetAppBase.Std.Library.Files
{
    public enum EFileExtension
    {
        [Display(ResourceType = typeof(DbEnums), Name = "EFileExtension_Unknown")]
        Unknown,

        [Display(ResourceType = typeof(DbEnums), Name = "EFileExtension_Xls")]
        Xls,

        [Display(ResourceType = typeof(DbEnums), Name = "EFileExtension_Xlsx")]
        Xlsx,

        [Display(ResourceType = typeof(DbEnums), Name = "EFileExtension_Pdf")]
        Pdf,

        [Display(ResourceType = typeof(DbEnums), Name = "EFileExtension_Txt")]
        Txt,

        [Display(ResourceType = typeof(DbEnums), Name = "EFileExtension_Doc")]
        Doc,

        [Display(ResourceType = typeof(DbEnums), Name = "EFileExtension_Docx")]
        Docx,

        [Display(ResourceType = typeof(DbEnums), Name = "EFileExtension_Png")]
        Png,

        [Display(ResourceType = typeof(DbEnums), Name = "EFileExtension_Xml")]
        Xml,

        [Display(ResourceType = typeof(DbEnums), Name = "EFileExtension_Eml")]
        Eml,

        [Display(ResourceType = typeof(DbEnums), Name = "EFileExtension_Any")]
        Any,

        [Display(ResourceType = typeof(DbEnums), Name = "EFileExtension_MultiExtensions")]
        MultiExtensions,

        [Display(ResourceType = typeof(DbEnums), Name = "EFileExtension_Csv")]
        Csv
    }
}