using System;
using System.Collections.Generic;

namespace GonDraz.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        ///     Checks if the collection is null or empty.
        ///     (Kiểm tra collection null hoặc rỗng)
        /// </summary>
        // Kiểm tra null hoặc rỗng
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        /// <summary>
        ///     Shuffles the elements of the list randomly.
        ///     (Trộn ngẫu nhiên các phần tử trong list)
        /// </summary>
        // Trộn ngẫu nhiên list
        public static void Shuffle<T>(this IList<T> list)
        {
            var rng = new Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        /// <summary>
        ///     Returns a random element from the list.
        ///     (Lấy phần tử ngẫu nhiên từ list)
        /// </summary>
        // Lấy phần tử ngẫu nhiên
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0) return default;
            var rng = new Random();
            return list[rng.Next(list.Count)];
        }

        /// <summary>
        ///     Removes duplicate elements from the list.
        ///     (Xoá các phần tử trùng lặp trong list)
        /// </summary>
        // Xoá phần tử trùng lặp
        public static void RemoveDuplicates<T>(this IList<T> list)
        {
            var set = new HashSet<T>();
            var i = 0;
            while (i < list.Count)
                if (!set.Add(list[i]))
                    list.RemoveAt(i);
                else
                    i++;
        }

        /// <summary>
        ///     Adds a range of elements to the collection.
        ///     (Thêm một dải phần tử vào collection)
        /// </summary>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
                collection.Add(item);
        }

        /// <summary>
        ///     Removes all elements that match the predicate from the collection.
        ///     (Xoá tất cả phần tử thoả mãn điều kiện khỏi collection)
        /// </summary>
        public static void RemoveWhere<T>(this ICollection<T> collection, Predicate<T> match)
        {
            var itemsToRemove = new List<T>();
            foreach (var item in collection)
                if (match(item))
                    itemsToRemove.Add(item);
            foreach (var item in itemsToRemove)
                collection.Remove(item);
        }

        /// <summary>
        ///     Returns the index of the first element that matches the predicate, or -1 if not found.
        ///     (Trả về chỉ số phần tử đầu tiên thoả mãn điều kiện, -1 nếu không có)
        /// </summary>
        public static int FindIndex<T>(this IList<T> list, Predicate<T> match)
        {
            for (var i = 0; i < list.Count; i++)
                if (match(list[i]))
                    return i;
            return -1;
        }

        /// <summary>
        ///     Returns the last element of the list, or default if empty.
        ///     (Trả về phần tử cuối cùng của list, hoặc mặc định nếu rỗng)
        /// </summary>
        public static T LastOrDefault<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0) return default;
            return list[^1];
        }

        /// <summary>
        ///     Swaps two elements in the list by their indices.
        ///     (Hoán đổi hai phần tử trong list theo chỉ số)
        /// </summary>
        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= list.Count || indexB < 0 || indexB >= list.Count || indexA == indexB) return;
            (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
        }

        /// <summary>
        ///     Returns a random element from the collection (IEnumerable).
        ///     (Lấy phần tử ngẫu nhiên từ collection IEnumerable)
        /// </summary>
        public static T GetRandom<T>(this IEnumerable<T> collection)
        {
            var list = collection as IList<T> ?? new List<T>(collection);
            if (list.Count == 0) return default;
            var rng = new Random();
            return list[rng.Next(list.Count)];
        }
    }
}