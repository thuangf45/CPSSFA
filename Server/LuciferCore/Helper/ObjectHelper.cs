using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LuciferCore.Core;
using LuciferCore.Manager;

namespace LuciferCore.Helper
{
    /// <summary>
    /// Cung cấp các phương thức tiện ích để làm việc với các đối tượng:
    /// - Lấy thông tin thuộc tính
    /// - Chuyển đổi đối tượng thành từ điển
    /// - Sao chép thuộc tính giữa các đối tượng
    /// - Kiểm tra kiểu và giá trị
    /// </summary>
    public static class ObjectHelper
    {
        /// <summary>
        /// Lấy danh sách tên thuộc tính công khai của một đối tượng.
        /// </summary>
        /// <param name="obj">Đối tượng cần lấy thuộc tính.</param>
        /// <param name="bindingFlags">Cờ để lọc thuộc tính (mặc định: Public | Instance).</param>
        /// <returns>Danh sách tên thuộc tính.</returns>
        public static List<string> GetPropertyNames(object obj, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (obj == null)
            {
                return new List<string>();
            }

            return obj.GetType().GetProperties(bindingFlags)
                     .Select(prop => prop.Name)
                     .ToList();
        }

        /// <summary>
        /// Lấy danh sách thuộc tính và giá trị của một đối tượng dưới dạng từ điển.
        /// </summary>
        /// <param name="obj">Đối tượng cần lấy thuộc tính.</param>
        /// <param name="prefix">Tiền tố thêm vào tên thuộc tính (nếu có, ví dụ: "@").</param>
        /// <param name="bindingFlags">Cờ để lọc thuộc tính.</param>
        /// <returns>Từ điển chứa tên thuộc tính và giá trị.</returns>
        public static Dictionary<string, object> ToDictionary(object obj, string prefix = "", BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            var dict = new Dictionary<string, object>();
            if (obj == null)
            {
                return dict;
            }

            var properties = obj.GetType().GetProperties(bindingFlags);
            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                var key = prefix + prop.Name;
                dict[key] = value ?? DBNull.Value;
            }

            return dict;
        }

        /// <summary>
        /// Kiểm tra xem một đối tượng có phải là kiểu cụ thể hay không (bao gồm các lớp con).
        /// </summary>
        /// <typeparam name="T">Kiểu cần kiểm tra.</typeparam>
        /// <param name="obj">Đối tượng cần kiểm tra.</param>
        /// <returns>True nếu đối tượng thuộc kiểu T hoặc lớp con của T.</returns>
        public static bool IsType<T>(object obj)
        {
            return obj != null && obj is T;
        }

        /// <summary>
        /// Sao chép các thuộc tính từ một đối tượng nguồn sang đối tượng đích nếu tên và kiểu tương thích.
        /// </summary>
        /// <param name="source">Đối tượng nguồn.</param>
        /// <param name="destination">Đối tượng đích.</param>
        /// <param name="bindingFlags">Cờ để lọc thuộc tính.</param>
        public static void CopyProperties(object source, object destination, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (source == null || destination == null)
            {
                return;
            }

            var sourceProps = source.GetType().GetProperties(bindingFlags)
                                   .Where(p => p.CanRead)
                                   .ToDictionary(p => p.Name, p => p);
            var destProps = destination.GetType().GetProperties(bindingFlags)
                                      .Where(p => p.CanWrite)
                                      .ToDictionary(p => p.Name, p => p);

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.TryGetValue(sourceProp.Key, out var destProp) &&
                    destProp.PropertyType == sourceProp.Value.PropertyType)
                {
                    try
                    {
                        var value = sourceProp.Value.GetValue(source);
                        destProp.SetValue(destination, value);
                    }
                    catch (Exception)
                    {
                        // Bỏ qua lỗi khi sao chép thuộc tính
                    }
                }
            }
        }

        /// <summary>
        /// Lấy giá trị của một thuộc tính cụ thể từ một đối tượng.
        /// </summary>
        /// <param name="obj">Đối tượng cần lấy giá trị.</param>
        /// <param name="propertyName">Tên thuộc tính.</param>
        /// <param name="bindingFlags">Cờ để lọc thuộc tính.</param>
        /// <returns>Giá trị của thuộc tính hoặc null nếu không tìm thấy.</returns>
        public static object GetPropertyValue(object obj, string propertyName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            var prop = obj.GetType().GetProperty(propertyName, bindingFlags);
            return prop?.GetValue(obj);
        }

        /// <summary>
        /// Ghi log tất cả thông tin thuộc tính của một đối tượng để debug.
        /// </summary>
        /// <param name="obj">Đối tượng cần ghi log.</param>
        /// <param name="bindingFlags">Cờ để lọc thuộc tính.</param>
        public static void LogObjectProperties(object obj, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (obj == null)
            {
                Simulation.GetModel<LogManager>().Log("ObjectHelper.LogObjectProperties: Object is null");
                return;
            }

            var properties = obj.GetType().GetProperties(bindingFlags);
            Simulation.GetModel<LogManager>().Log($"ObjectHelper.LogObjectProperties: Type = {obj.GetType().Name}, Found {properties.Length} properties");
            foreach (var prop in properties)
            {
                if (prop.GetIndexParameters().Length > 0)
                {
                    // Skip indexed property
                    continue;
                }

                var value = prop.GetValue(obj);
                Simulation.GetModel<LogManager>().Log($"[Object] {prop.Name} = {value ?? "null"} ({prop.PropertyType.Name})");
            }
        }
    }
}