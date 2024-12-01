using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Schema;
using static System.Net.Mime.MediaTypeNames;


namespace ToolRenderCode
{
    public partial class RenderForm : Form
    {
        public RenderForm()
        {
            InitializeComponent();
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            // Khởi tạo OpenFileDialog và cho phép chọn nhiều tệp
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.cs)|*.cs|All files (*.*)|*.*";
            openFileDialog.Multiselect = true;  // Cho phép chọn nhiều tệp

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Lặp qua tất cả các tệp được chọn
                foreach (string filePath in openFileDialog.FileNames)
                {
                    // Gọi hàm để xử lý từng tệp
                    ReadFileCS(filePath);
                }
            }
        }

        private void ReadFileCS(string filePath)
        {
            if (File.Exists(filePath))
            {
                // Đọc nội dung tệp
                string fileContent = File.ReadAllText(filePath);

                // Hiển thị nội dung tệp vào điều khiển (txtClass)
                txtClass.Text += fileContent + $"\r\n\r\n//#className:{System.IO.Path.GetFileName(filePath)}// \r\n\r\n//Read//\r\n\r\n";  // Thêm nội dung của từng tệp vào

                // Bạn có thể xử lý các phần tử khác nếu cần, ví dụ tìm kiếm class hoặc các xử lý khác
                //FindClassAndGetSet(fileContent);
            }
            else
            {
                Console.WriteLine("Tệp không tồn tại: " + filePath);
            }
        }

        private void btnFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Chọn một thư mục";
                folderBrowserDialog.ShowNewFolderButton = true; // Cho phép tạo thư mục mới

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = folderBrowserDialog.SelectedPath;
                    txtFolder.Text = selectedPath;

                    File.WriteAllText("temp.txt", selectedPath);
                }
            }
        }

        private void FindClassAndGetSet(string text)
        {
            try
            {
                // Loại trừ các dòng bắt đầu bằng //
                string filteredText = string.Join("\n", text.Split('\n').Where(line => !line.TrimStart().StartsWith("//")));

                // Biểu thức chính quy để tìm các dòng using
                string usingPattern = @"^\s*using\s+[^\s;]+;\s*$";
                Regex usingRegex = new Regex(usingPattern, RegexOptions.Multiline);

                // Lấy tất cả các dòng using
                MatchCollection usingMatches = usingRegex.Matches(filteredText);
                List<string> usings = usingMatches.Cast<Match>().Select(match => match.Value).ToList();

                var usingNames = string.Empty;
                // In ra danh sách using (nếu cần)
                foreach (string usingLine in usings)
                {
                    usingNames += usingLine + "\r\n";
                }

                // Biểu thức chính quy để tìm lớp
                string classPattern = @"public\s+(partial\s+)?class\s+(\w+)\s*(?::\s*[\w<>\s,]+)?\s*{((?:[^{}]|\{(?:[^{}]|\{[^{}]*\})*\})*)}";

                Regex classRegex = new Regex(classPattern, RegexOptions.Singleline);

                // Biểu thức chính quy để tìm các trường trong lớp
                string propertyPattern = @"public\s+(virtual\s+)?[\w<>\[\],?]+\s+\w+\s*\{\s*(get;?\s*(=>.*;|{.*})?)?\s*(set;?\s*(=>.*;|{.*})?)?\s*\}(\s*=\s*[^;]+;)?";

                Regex propertyRegex = new Regex(propertyPattern);

                // Dictionary lưu trữ kết quả
                Dictionary<string, List<string>> classProperties = new Dictionary<string, List<string>>();

                // Tìm các lớp
                MatchCollection classMatches = classRegex.Matches(filteredText);

                foreach (Match classMatch in classMatches)
                {
                    string className = classMatch.Groups[2].Value; // Tên lớp
                    string classBody = classMatch.Groups[3].Value; // Nội dung lớp

                    // Biểu thức chính quy tìm 'Id', 'id', 'ID' mà không trùng với 'UserId' hoặc các từ khác chứa 'Id'
                    string pattern = @"\b[idID]{2}\b(?!\w)";

                    // Tạo regex và tìm kiếm, sử dụng RegexOptions.IgnoreCase để bỏ qua chữ hoa chữ thường
                    var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                    var matches = regex.Matches(text);

                    // kiểm tra xem tron text có chứa từ [Key] hoặc Id không 
                    if (matches.Count == 0 && !classBody.Contains("[Key]") && !classMatch.Value.Contains("IdentityUser"))
                    {
                        continue;
                    }

                    // Ghi file liên quan đến lớp
                    //CreateFileModel(usingNames, classBody, className, "Data/Entities", "{ClassModel}.txt");
                    CreateFileModelV2(usingNames, classMatch.Value, className, "Data/Entities", "{ClassModel}.txt");
                    WriteFileDbContext(className, "Data/Entities", "_{namespaceCut}DbContext.txt");

                    // Tìm các trường trong nội dung lớp
                    MatchCollection propertyMatches = propertyRegex.Matches(classBody);
                    List<string> properties = new List<string>();

                    foreach (Match propertyMatch in propertyMatches)
                    {
                        // Bỏ qua các dòng không cần thiết (VD: Constructor, phương thức)
                        if (!propertyMatch.Value.Contains("{") || !propertyMatch.Value.Contains("}")) continue;
                        properties.Add(propertyMatch.Value.Trim());
                    }

                    // Lưu danh sách thuộc tính của lớp
                    classProperties[className] = properties;
                }


                // Hiển thị kết quả
                foreach (var entry in classProperties)
                {
                    CreateFile(entry.Key, classProperties[entry.Key], "Controllers", "Controller.txt");

                    CreateFile(entry.Key, classProperties[entry.Key], "Dtos", "CreateDto.txt", "CreateDto");
                    CreateFile(entry.Key, classProperties[entry.Key], "Dtos", "DetailDto.txt", "DetailDto");
                    CreateFile(entry.Key, classProperties[entry.Key], "Dtos", "GridDto.txt", "GridDto");
                    CreateFile(entry.Key, classProperties[entry.Key], "Dtos", "GridPaging.txt");
                    CreateFile(entry.Key, classProperties[entry.Key], "Dtos", "UpdateDto.txt", "UpdateDto");

                    CreateFile(entry.Key, classProperties[entry.Key], "Profiles", "Profile.txt");
                }

                // Pattern để tìm nội dung giữa //#className: và //
                string patternClass = @"//#className:(.*?)//";

                // Tạo Regex và tìm các match
                MatchCollection matchesClass = Regex.Matches(text, patternClass);

                foreach (Match match in matchesClass)
                {
                    if (match.Success)
                    {
                        string classNameNow = match.Groups[1].Value; // Lấy giá trị trong nhóm (.*?)
                        Console.WriteLine(classNameNow); // Output: Abc, EPD
                        MessageBox.Show($"{classNameNow} đã tạo xong {classProperties.Keys.Count()} file");

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo file: " + ex.Message);
            }
        }

        /// <summary>
        /// (CamelCase hoặc PascalCase) và chuyển đổi chúng sang dạng có dấu - giữa các từ.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string ConvertToHyphenated(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Sử dụng Regex để thêm dấu '-' trước các ký tự viết hoa
            string result = Regex.Replace(input, @"(?<!^)([A-Z])", "-$1");

            // Đảm bảo ký tự đầu tiên không bị viết thường nếu là PascalCase
            return result;
        }

        private void CreateFile(string className, List<string> listParams, string folderName, string fileName, string param = "")
        {
            if (string.IsNullOrEmpty(txtFolder.Text))
            {
                MessageBox.Show("Đường dẫn folder không được để trống!");
                return;
            }

            // Đường dẫn thư mục và tệp
            string folderPath = Path.Combine(txtFolder.Text, folderName);
            string filePath = Path.Combine(folderPath, className + fileName.Replace(".txt", ".cs"));

            // Kiểm tra và tạo thư mục
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"Thư mục chưa tồn tại. Đang tạo: {folderPath}");
                Directory.CreateDirectory(folderPath);
            }
            else
            {
                Console.WriteLine($"Thư mục đã tồn tại: {folderPath}");
            }

            if (folderName == "Dtos")
            {
                // Tạo thư mục con nếu chưa tồn tại
                string subFolderPath = Path.Combine(folderPath, className);
                if (!Directory.Exists(subFolderPath))
                {
                    Console.WriteLine($"Thư mục con '{className}' chưa tồn tại. Đang tạo...");
                    Directory.CreateDirectory(subFolderPath); // Tạo thư mục con
                }

                filePath = Path.Combine(folderPath, className, className + fileName.Replace(".txt", ".cs"));
            }

            // Kiểm tra và tạo tệp
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Tệp chưa tồn tại. Đang tạo: {filePath}");

                string jsonContent = File.ReadAllText(Path.Combine(folderName, fileName));

                // Lấy tên thư mục cuối cùng từ đường dẫn
                string namespacetxt = Path.GetFileName(txtFolder.Text);

                var getSets = string.Empty;

                // Xử lý các dòng get-set
                int i = 0;

                List<string> listAttrs = new List<string> { 
                      "sbyte",
                      "byte",
                      "short",
                      "ushort",
                      "int",
                      "uint",
                      "long",
                      "ulong",
                      "float",
                      "double",
                      "decimal",
                      "char",
                      "string",
                      "bool",
                      "DateTime",
                      "DateTimeOffset",
                      "object",
                      "dynamic",
                      "Guid",
                      "IntPtr",
                      "UIntPtr",

                      "sbyte?",
                      "byte?",
                      "short?",
                      "ushort?",
                      "int?",
                      "uint?",
                      "long?",
                      "ulong?",
                      "float?",
                      "double?",
                      "decimal?",
                      "char?",
                      "string?",
                      "bool?",
                      "DateTime?",
                      "DateTimeOffset?",
                      "object?",
                      "dynamic?",
                      "Guid?",
                      "IntPtr?",
                      "UIntPt?r"
                    };

                var usingAdds = string.Empty;

                foreach (var item in listParams)
                {
                    // Kiểm tra xem dòng có phải danh sách, đối tượng model hoặc kiểu dữ liệu phức tạp khác
                    if (Regex.IsMatch(item, @"public\s+(virtual\s+)?((?!int|string|double|long|float|bool|DateTime)\w+)(<\w+>)?"))
                    {
                        // Chèn thêm tham số mới vào kiểu dữ liệu
                        string modifiedItem = Regex.Replace(
                            item,
                            @"public\s+(virtual\s+)?((?!int|string|double|long|float|bool|DateTime)\w+)(<\w+>)?",
                            match =>
                            {
                                // Lấy nội dung kiểu dữ liệu và thêm param
                                string content = match.Value;

                                // Tìm kiểu dữ liệu trong dấu <>
                                int start = content.IndexOf('<') + 1;
                                int end = content.IndexOf('>');
                                var isCheck = false;

                                // kiểm tra thuộc tính
                                var arrs = Regex.Split(content, @"\s+").ToList();
                                if (arrs.Count > 0 && !content.Contains("<"))
                                {
                                    var textModel = arrs[arrs.Count - 1];
                                    if (listAttrs.Contains(textModel))
                                    {
                                        isCheck = true;
                                    }
                                    else
                                    {
                                        content = content.Replace(textModel, $"{namespacetxt}.Dtos.{textModel}.{textModel}" );

                                        return content + param;
                                    }
                                }

                                // Kiểm tra nếu có kiểu dữ liệu trong dấu <>
                                if (start != -1 && end != -1)
                                {
                                    string typeInAngleBrackets = content.Substring(start, end - start);

                                    // Kiểm tra kiểu dữ liệu có trong danh sách hay không
                                    if (listAttrs.Contains(typeInAngleBrackets))
                                    {
                                        isCheck = true;
                                    }
                                    else
                                    {
                                        isCheck = false;
                                    }
                                }

                                if (!isCheck)
                                {
                                    if (content.Contains("<") && content.Contains(">"))
                                    {
                                        // Thêm param nếu là generic type
                                        return Regex.Replace(content, @"<(\w+)>", $"<{namespacetxt}.Dtos.$1.$1{param}>");
                                    }
                                }

                                return content; // Giữ nguyên nếu không phải generic type
                            }
                        );
                        getSets += (i != 0 ? "        " : "") + modifiedItem + Environment.NewLine + Environment.NewLine;
                    }
                    else
                    {
                        getSets += (i != 0 ? "        " : "") + item + Environment.NewLine + Environment.NewLine;
                    }
                    i++;
                }

                string namespaceCut = namespacetxt.Contains(".") ? namespacetxt.Split('.')[0] : namespacetxt;
                jsonContent = jsonContent.Replace("{namespace}", namespacetxt);
                jsonContent = jsonContent.Replace("{ClassModel}", className);
                jsonContent = jsonContent.Replace("{ClassModelRoute}", ConvertToHyphenated(className).ToLower());
                jsonContent = jsonContent.Replace("{ClassModelLower}", className.ToLower());
                jsonContent = jsonContent.Replace("{getset}", getSets);
                jsonContent = jsonContent.Replace("{namespaceCut}", namespaceCut);

                File.WriteAllText(filePath, jsonContent);
            }
            else
            {
                Console.WriteLine($"Tệp đã tồn tại: {filePath}");
            }
        }


        private void CreateFileBase(string folderNameOut, string fileName, bool isAddParamFileName = false)
        {
            if (string.IsNullOrEmpty(txtFolder.Text))
            {
                MessageBox.Show("Đường dẫn folder không được để trống!");
                return;
            }

            if(folderNameOut.Replace("/","\\").Split('\\').ToArray().Length > 0)
            {
                var fd = string.Empty;
                foreach (var item in folderNameOut.Replace("/", "\\").Split('\\').ToArray())
                {
                    fd += "\\" + item ;
                    // Đường dẫn thư mục và tệp
                    string folderPathOut = Path.Combine(txtFolder.Text, fd.TrimStart('\\').TrimEnd('\\'));

                    // Kiểm tra và tạo thư mục
                    if (!Directory.Exists(folderPathOut))
                    {
                        Directory.CreateDirectory(folderPathOut);
                    }
                }
            }

            // Đường dẫn thư mục và tệp
            string folderPath = Path.Combine(txtFolder.Text, folderNameOut);
            // Lấy tên thư mục cuối cùng từ đường dẫn
            string namespacetxt = Path.GetFileName(txtFolder.Text);
            string namespaceCut = namespacetxt.Contains(".") ? namespacetxt.Split('.')[0] : namespacetxt;

            var fName = fileName.Replace("{namespaceCut}", namespaceCut).Replace(".txt", ".cs");
            
            string filePath = Path.Combine(folderPath, fName);

            // Kiểm tra và tạo tệp
            if (!File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(Path.Combine(folderNameOut, fileName));

                var getSets = string.Empty;

              
                jsonContent = jsonContent.Replace("{namespace}", namespacetxt);

                jsonContent = jsonContent.Replace("{namespaceCut}", namespaceCut);

                File.WriteAllText(filePath, jsonContent);
            }
            else
            {
                Console.WriteLine($"Tệp đã tồn tại: {filePath}");
            }
        }

        private void CreateFileModel(string usingNames, string textClassModel, string classModel,  string folderNameOut, string fileName)
        {
            if (string.IsNullOrEmpty(txtFolder.Text))
            {
                MessageBox.Show("Đường dẫn folder không được để trống!");
                return;
            }

            if (folderNameOut.Replace("/", "\\").Split('\\').ToArray().Length > 0)
            {
                var fd = string.Empty;
                foreach (var item in folderNameOut.Replace("/", "\\").Split('\\').ToArray())
                {
                    fd += "\\" + item;
                    // Đường dẫn thư mục và tệp
                    string folderPathOut = Path.Combine(txtFolder.Text, fd.TrimStart('\\').TrimEnd('\\'));

                    // Kiểm tra và tạo thư mục
                    if (!Directory.Exists(folderPathOut))
                    {
                        Directory.CreateDirectory(folderPathOut);
                    }
                }
            }

            // Đường dẫn thư mục và tệp
            string folderPath = Path.Combine(txtFolder.Text, folderNameOut);
            // Lấy tên thư mục cuối cùng từ đường dẫn
            string namespacetxt = Path.GetFileName(txtFolder.Text);
            string namespaceCut = namespacetxt.Contains(".") ? namespacetxt.Split('.')[0] : namespacetxt;

            var fName = fileName.Replace("{namespaceCut}", namespaceCut).Replace("{ClassModel}", classModel).Replace(".txt", ".cs");

            string filePath = Path.Combine(folderPath, fName);

            // Kiểm tra và tạo tệp
            if (!File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(Path.Combine(folderNameOut, fileName));

                var getSets = string.Empty;


                jsonContent = jsonContent.Replace("{namespace}", namespacetxt);

                jsonContent = jsonContent.Replace("{namespaceCut}", namespaceCut);

                jsonContent = jsonContent.Replace("{ClassModel}", classModel);

                jsonContent = jsonContent.Replace("{renderModel}", textClassModel);

                jsonContent = jsonContent.Replace("{usingNames}", usingNames);

                File.WriteAllText(filePath, jsonContent);
            }
            else
            {
                Console.WriteLine($"Tệp đã tồn tại: {filePath}");
            }
        }

        private void CreateFileModelV2(string usingNames, string textClassModelAll, string classModel, string folderNameOut, string fileName)
        {
            if (string.IsNullOrEmpty(txtFolder.Text))
            {
                MessageBox.Show("Đường dẫn folder không được để trống!");
                return;
            }

            if (folderNameOut.Replace("/", "\\").Split('\\').ToArray().Length > 0)
            {
                var fd = string.Empty;
                foreach (var item in folderNameOut.Replace("/", "\\").Split('\\').ToArray())
                {
                    fd += "\\" + item;
                    // Đường dẫn thư mục và tệp
                    string folderPathOut = Path.Combine(txtFolder.Text, fd.TrimStart('\\').TrimEnd('\\'));

                    // Kiểm tra và tạo thư mục
                    if (!Directory.Exists(folderPathOut))
                    {
                        Directory.CreateDirectory(folderPathOut);
                    }
                }
            }

            // Đường dẫn thư mục và tệp
            string folderPath = Path.Combine(txtFolder.Text, folderNameOut);
            // Lấy tên thư mục cuối cùng từ đường dẫn
            string namespacetxt = Path.GetFileName(txtFolder.Text);
            string namespaceCut = namespacetxt.Contains(".") ? namespacetxt.Split('.')[0] : namespacetxt;

            var fName = fileName.Replace("{namespaceCut}", namespaceCut).Replace("{ClassModel}", classModel).Replace(".txt", ".cs");

            string filePath = Path.Combine(folderPath, fName);

            // Kiểm tra và tạo tệp
            if (!File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(Path.Combine(folderNameOut, fileName));

                var getSets = string.Empty;


                jsonContent = jsonContent.Replace("{namespace}", namespacetxt);

                jsonContent = jsonContent.Replace("{namespaceCut}", namespaceCut);

                jsonContent = jsonContent.Replace("{ClassModel}", classModel);

                jsonContent = jsonContent.Replace("{renderModelAll}", textClassModelAll);

                jsonContent = jsonContent.Replace("{usingNames}", usingNames);

                File.WriteAllText(filePath, jsonContent);
            }
            else
            {
                Console.WriteLine($"Tệp đã tồn tại: {filePath}");
            }
        }

        /// <summary>
        /// Tạo dữ liệu trong file dbContext
        /// </summary>
        /// <param name="classModel"></param>
        /// <param name="folderNameOut"></param>
        /// <param name="fileName"></param>
        private void WriteFileDbContext(string classModel, string folderNameOut, string fileName)
        {
            if (string.IsNullOrEmpty(txtFolder.Text))
            {
                MessageBox.Show("Đường dẫn folder không được để trống!");
                return;
            }

            if (folderNameOut.Replace("/", "\\").Split('\\').ToArray().Length > 0)
            {
                var fd = string.Empty;
                foreach (var item in folderNameOut.Replace("/", "\\").Split('\\').ToArray())
                {
                    fd += "\\" + item;
                    // Đường dẫn thư mục và tệp
                    string folderPathOut = Path.Combine(txtFolder.Text, fd.TrimStart('\\').TrimEnd('\\'));

                    // Kiểm tra và tạo thư mục
                    if (!Directory.Exists(folderPathOut))
                    {
                        Directory.CreateDirectory(folderPathOut);
                    }
                }
            }

            // Đường dẫn thư mục và tệp
            string folderPath = Path.Combine(txtFolder.Text, folderNameOut);
            // Lấy tên thư mục cuối cùng từ đường dẫn
            string namespacetxt = Path.GetFileName(txtFolder.Text);
            string namespaceCut = namespacetxt.Contains(".") ? namespacetxt.Split('.')[0] : namespacetxt;

            var fName = fileName.Replace("{namespaceCut}", namespaceCut).Replace("{ClassModel}", classModel).Replace(".txt", ".cs");

            string filePath = Path.Combine(folderPath, fName);

            string nameEnity = classModel;

            if (nameEnity.EndsWith("y"))
            {
                nameEnity = classModel.Substring(0, classModel.Length - 1) + "ies";
            }
            else
            {
                nameEnity = classModel + "s";
            }
            // Nội dung cần thêm
            string newLineToAdd = $"public virtual DbSet<{namespacetxt}.Data.Entities.{classModel}> {nameEnity} {{ get; set; }}";

            if (!File.Exists(filePath))
            {
                // Đọc mẫu file nếu file chưa tồn tại
                string templateContent = File.ReadAllText(Path.Combine(folderNameOut, fileName));

                templateContent = templateContent.Replace("{namespace}", namespacetxt);
                templateContent = templateContent.Replace("{namespaceCut}", namespaceCut);
                templateContent = templateContent.Replace("{ClassModel}", classModel);
                //templateContent = templateContent.Replace("{usingNames}", usingNames);

                if (!templateContent.Contains($"DbSet<{namespacetxt}.Data.Entities.{classModel}>")
                    && !templateContent.Contains($"DbSet<{classModel}>"))
                {
                    // Thêm dòng mới trước đoạn `//{KhongXoaDoanCommentNay}`
                    if (templateContent.Contains("//{KhongXoaDoanCommentNay}"))
                    {
                        templateContent = templateContent.Replace("//{KhongXoaDoanCommentNay}",
                            $"{newLineToAdd}\r\n        //{{KhongXoaDoanCommentNay}}");
                    }

                    // Ghi file
                    File.WriteAllText(filePath, templateContent);
                }
            }
            else
            {
                // Nếu file tồn tại, chỉ sửa đổi nội dung
                string fileContent = File.ReadAllText(filePath);

                if (!fileContent.Contains(newLineToAdd))
                {
                    if (fileContent.Contains("//{KhongXoaDoanCommentNay}"))
                    {
                        // Ghi đè dòng mới trước `//{KhongXoaDoanCommentNay}`
                        fileContent = fileContent.Replace("//{KhongXoaDoanCommentNay}",
                            $"{newLineToAdd}\r\n        //{{KhongXoaDoanCommentNay}}");
                        File.WriteAllText(filePath, fileContent);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy đoạn `//{KhongXoaDoanCommentNay}` trong tệp.");
                    }
                }
                
            }
        }


        private void btnTaoFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtClass.Text))
            {
                MessageBox.Show("Không được để trống model");
                return;
            }

            if (string.IsNullOrEmpty(txtFolder.Text))
            {
                MessageBox.Show("Không được để trống Link Dự án");
                return;
            }

            File.WriteAllText("temp.txt", txtFolder.Text);

            var listClass = Regex.Split(txtClass.Text, "//Read//");

            foreach(var str in listClass)
            {
                if (string.IsNullOrEmpty(str.Trim()))
                {
                    continue;
                }
                FindClassAndGetSet(str);

            }

            // tạo authorize
            CreateFileBase("Authorize", "BaseAuthorizeAttribute.txt");
            CreateFileBase("Data/Enums", "PrivilegeListEnum.txt");

            CreateFileBase("BaseExt", "BaseController.txt");
            CreateFileBase("BaseExt", "BaseService.txt", true);
            CreateFileBase("BaseExt", "Lazier.txt");
            CreateFileBase("BaseExt/Interface", "IBaseService.txt", true);
            CreateFileBase("BaseExt/Interface", "IIdentifier.txt");

            CreateFileBase("Pages", "PageHelper.txt");
            CreateFileBase("Pages", "PagingParams.txt");
            CreateFileBase("Pages", "PagingResult.txt");

            CreateFileBase("Repositories/Interface", "ICascadeDelete.txt");
            CreateFileBase("Repositories/Interface", "ICreateInfo.txt");
            CreateFileBase("Repositories/Interface", "IDeleteInfo.txt");
            CreateFileBase("Repositories/Interface", "ICommonRepository.txt", true);
            CreateFileBase("Repositories/Interface", "IUpdateInfo.txt");
            CreateFileBase("Repositories/Interface", "IUserIdentity.txt");
            CreateFileBase("Repositories", "CustomResolver.txt");
            CreateFileBase("Repositories", "CommonRepository.txt", true);
            CreateFileBase("Repositories", "FullTableInfo.txt");


            CreateFileBase("Services", "CommonService.txt", true);
            CreateFileBase("Services", "ServiceExtensions.txt");


            CreateServiceInProgram();
            txtClass.Text = "";
        }

        private void CreateServiceInProgram()
        {
            if (string.IsNullOrEmpty(txtFolder.Text))
            {
                MessageBox.Show("Đường dẫn folder không được để trống!");
                return;
            }

            string filePath = Path.Combine(txtFolder.Text, "Program.cs");

            // Kiểm tra sự tồn tại của file Program.cs
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Tệp Program.cs không tồn tại trong đường dẫn được cung cấp!");
                return;
            }

            // Đọc nội dung tệp
            string fileContent = File.ReadAllText(filePath);

            // Kiểm tra nếu đã có dòng AddCustomScopedServices
            if (fileContent.Contains("builder.Services.AddCustomScopedServices();"))
            {
                //MessageBox.Show("Dòng AddCustomScopedServices đã tồn tại trong tệp Program.cs.");
                return;
            }

            // Tìm vị trí để chèn dòng mới
            string addDbContextLine = "builder.Services.AddDbContext";
            int insertIndex = fileContent.IndexOf(addDbContextLine);

            if (insertIndex != -1)
            {
                // Tìm vị trí kết thúc của dòng AddDbContext
                int lineEndIndex = fileContent.IndexOf(");", insertIndex) + 3;

                // Thêm dòng mới sau AddDbContext
                string newLine = "\r\nbuilder.Services.AddCustomScopedServices();";
                fileContent = fileContent.Insert(lineEndIndex, newLine);

                // Ghi lại nội dung vào file
                File.WriteAllText(filePath, fileContent);

                MessageBox.Show("Đã thêm dòng AddCustomScopedServices vào tệp Program.cs.");
            }
            else
            {
                //MessageBox.Show("Không tìm thấy dòng AddDbContext trong tệp Program.cs.");
            }
        }

        private void RenderForm_Load(object sender, EventArgs e)
        {
            if (File.Exists("temp.txt"))
            {
                // Đọc mẫu file nếu file chưa tồn tại
                string templateContent = File.ReadAllText("temp.txt");

                txtFolder.Text = templateContent;
            }
        }
    }
}
