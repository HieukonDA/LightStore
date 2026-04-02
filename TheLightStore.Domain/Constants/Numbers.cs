using System;

namespace TheLightStore.Domain.Constants;

public class Numbers
{
    public struct Pagination
    {
        public const int DefaultPageSize = 20;
        public const int DefaultPageNumber = 1;
        public static readonly int[] DefaultRecordLimit = [10, 25, 50, 100, 150, 200];
    }
}
