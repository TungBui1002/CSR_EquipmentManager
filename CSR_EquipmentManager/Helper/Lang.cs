using System;
using System.Collections.Generic;
using System.Web;

public static class Lang
{
    private static Dictionary<string, Dictionary<string, string>> data =
        new Dictionary<string, Dictionary<string, string>>()
    {
        { "vi", new Dictionary<string, string>()
            {
                { "login_header", "Đăng Nhập Hệ Thống" },
                { "login", "Đăng nhập" },
                { "logout", "Đăng xuất" },
                { "username", "Tên đăng nhập" },
                { "password", "Mật khẩu" },
                { "date", "≤30 Ngày" },
                { "device", "Thiết bị" },
                { "email_Device", "Email Thiết bị" },
                { "sum_devices", "Tổng thiết bị" },
                { "good_devices", "Còn hạn tốt" },
                { "expiringsoon_devices", "Sắp hết hạn" },
                { "expired_devices", "Quá hạn" },
                { "dashboard_device_status", "Phân loại thiết bị theo trạng thái" },
                { "dashboard_device_ratio", "Tỷ lệ trạng thái thiết bị" },
                { "btn_send_emails", "Gửi Email Cảnh Báo Thiết Bị Sắp Hết Hạn" },
                { "label_search", "Tìm kiếm..." },
                { "btn_add_device", "Thêm thiết bị mới" },
                { "ID_FileDevice", "Mã hồ sơ" },
                { "Factory_Device", "Xưởng" },
                { "Building_Floor_Device", "Tòa nhà - Tầng" },
                { "STT_PositionCode_Device", "STT - Vị trí" },
                { "ID_Device", "Mã thiết bị" },
                { "Name_Device", "Tên thiết bị" },
                { "Model_Device", "Model" },
                { "Positon_Device", "Vị trí thiết bị" },
                { "Inspection_Date_Device", "Ngày kiểm định" },
                { "Next_Inspection_Date_Device", "Ngày kiểm định tiếp theo" },
                { "Expried_Device", "Hết hạn" },
                { "Col_Action", "Thao tác" },
                { "btn_DownloadExcel", "Tải Mẫu Excel" },
                { "btn_ImportExcel", "Import Excel" },
                { "lable_Instruction", "Hướng dẫn" },
                { "li_1", "Tên thiết bị là bắt buộc" },
                { "li_2", "Ngày kiểm định nên nhập theo định dạng yyyy/MM/dd" },
                { "li_3", "Sau khi tạo xong bạn có thể thêm Email cho thiết bị" },
                { "Title_Email", "Danh sách Email Thiết Bị" },
                { "btn_AddEmail", "Thêm Email Mới" },
                { "Title_EquipmentList", "Danh sách thiết bị" },
                { "Title_AddNewEmail", "Thêm Email Cho Thiết Bị" },
                { "dashboard", "Trang tổng quan" }
            }
        },
        { "zh", new Dictionary<string, string>()
            {
                { "login_header", "登入系統" },
                { "login", "登入" },
                { "logout", "登出" },
                { "username", "帳號" },
                { "password", "密碼" },
                { "date", "≤30天" },
                { "device", "裝置" },
                { "email_Device", "Email" },
                { "sum_devices", "總設備" },
                { "good_devices", "正常使用中" },
                { "expiringsoon_devices", "即將到期" },
                { "expired_devices", "已過期" },
                { "dashboard_device_status", "依狀態對設備進行分類。" },
                { "dashboard_device_ratio", "裝置狀態比率" },
                { "btn_send_emails", "當您的設備許可證即將到期時，發送電子郵件提醒" },
                { "label_search", "搜尋..." },
                { "btn_add_device", "新增設備" },
                { "ID_FileDevice", "文件程式碼" },
                { "Factory_Device", "廠" },
                { "Building_Floor_Device", "棟 - 層" },
                { "STT_PositionCode_Device", "編號 - 位置" },
                { "ID_Device", "裝置代碼" },
                { "Name_Device", "設備名稱" },
                { "Model_Device", "Model" },
                { "Positon_Device", "設備位置" },
                { "Inspection_Date_Device", "檢查日期" },
                { "Next_Inspection_Date_Device", "下次檢查日期" },
                { "Expried_Device", "已到期" },
                { "Col_Action", "手術" },
                { "btn_DownloadExcel", "下載 Excel 模板" },
                { "btn_ImportExcel", "導入 Excel" },
                { "lable_Instruction", "指示" },
                { "li_1", "設備名稱為必填項。" },
                { "li_2", "檢查日期應以 yyyy/MM/dd 的格式輸入。" },
                { "li_3", "建立完成後，您可以將電子郵件地址新增至裝置。" },
                { "Title_Email", "設備電子郵件列表" },
                { "btn_AddEmail", "新增電子郵件" },
                { "Title_EquipmentList", "設備清單" },
                { "Title_AddNewEmail", "將電子郵件新增至設備" },
                { "dashboard", "儀表板" }
            }
        }
    };

    // lấy ngôn ngữ hiện tại
    public static string CurrentLang
    {
        get
        {
            var context = HttpContext.Current;

            var lang = context.Session["lang"]?.ToString();

            if (string.IsNullOrEmpty(lang))
            {
                var cookie = context.Request.Cookies["lang"];
                if (cookie != null)
                {
                    lang = cookie.Value;
                    context.Session["lang"] = lang;
                }
            }

            return string.IsNullOrEmpty(lang) ? "vi" : lang;
        }
    }

    public static string T(string key)
    {
        var lang = CurrentLang;

        if (data.ContainsKey(lang) && data[lang].ContainsKey(key))
        {
            return data[lang][key];
        }

        return key; // fallback nếu thiếu key
    }
}
